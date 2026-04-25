using Brain_Fodder.Recording;
using Brain_Fodder.Rendering;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using Dino_Engine.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using SpaceEngine.RenderEngine;
using System.Diagnostics;
using System.Text;

namespace Brain_Fodder
{
    public class Engine
    {
        SoundManager soundManager;
        WindowHandler windowHandler;
        MasterRenderer masterRenderer;
        public ECSWorld ecsWorld;
        private static Engine? _instance;
        public static float EngineDeltaClock = 0f;
        private int pbo;
        private Recorder recorder = new Recorder();
        private FrameBuffer frameBuffer;
        public Vector2i innerResolution = new Vector2i(1080, 1920)*1;
        public Vector2i outerResolution = new Vector2i(1080, 1920) / 2;
        public static Engine? Instance { get => _instance; }

        public Engine()
        {

            _instance = this;
            windowHandler = new WindowHandler(outerResolution);
            masterRenderer = new MasterRenderer();

            ComponentTypeRegistry.AutoRegisterAllComponents();
            SystemRegistry.AutoRegisterAllSystems();
            ecsWorld = new ECSWorld();

            ecsWorld.ApplyDeferredCommands();
            soundManager = new SoundManager();

            WindowHandler.getWindow().UpdateFrame += delegate (FrameEventArgs eventArgs)
            {
                update((float)eventArgs.Time);
            };
            WindowHandler.getWindow().RenderFrame += delegate (FrameEventArgs eventArgs)
            {
                render();
            };
            WindowHandler.getWindow().Resize += delegate (ResizeEventArgs eventArgs)
            {
                windowHandler.onResize(eventArgs);
                masterRenderer.onResize(eventArgs);
            };
            WindowHandler.getWindow().KeyUp += delegate (KeyboardKeyEventArgs eventArgs)
            {
                if (eventArgs.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.R)
                {
                    recorder.StopRecording();

                    ecsWorld.clearLevel();
                    ecsWorld.SpawnLevel();

                }
            };



            FrameBufferSettings frameBufferSettings = new FrameBufferSettings(innerResolution);
            frameBufferSettings.drawBuffers.Add(new DrawBufferSettings(FramebufferAttachment.ColorAttachment0));
            frameBuffer = new FrameBuffer(frameBufferSettings);





            int width = innerResolution.X;
            int height = innerResolution.Y;
            pbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.PixelPackBuffer, pbo);
            GL.BufferData(BufferTarget.PixelPackBuffer, width * height * 4, IntPtr.Zero, BufferUsageHint.StreamRead);
        }

        public static void CombineVideoAndAudio(
           string videoPath,
           string audioWavPath,
           string outputPath)
        {
            // 1. Check that the file exists and is not trivially empty

            var videoInfo = new FileInfo(videoPath);
            if (!videoInfo.Exists || videoInfo.Length < 100)
                throw new FileNotFoundException("Video file missing or too small", videoPath);

            var audioInfo = new FileInfo(audioWavPath);
            if (!audioInfo.Exists || audioInfo.Length <= 44) // 44-byte WAV header
                throw new FileNotFoundException("Audio file missing or just header", audioWavPath);

            var outputInfo = new FileInfo(outputPath);
            if (outputInfo.Directory?.Exists == false)
                outputInfo.Directory.Create();

            // 2. Build FFmpeg arguments

            // Always use full encoding here, not copy, so it survives malformed raw inputs
            var args = new[]
            {
            "-y", // overwrite without asking

            "-hide_banner", // cleaner output
            "-loglevel", "info", // detailed enough to see problems

            "-i", videoPath,   // video input
            "-i", audioWavPath, // audio input

            "-c:v", "libx264",          // encode video
            "-preset", "veryfast",      // faster encoding
            "-crf", "18",               // good quality
            "-pix_fmt", "yuv420p",      // compatibility

            "-c:a", "aac",              // AAC audio
            "-b:a", "192k",             // 192 kbps

            "-map", "0:v:0",            // map first video stream from first file
            "-map", "1:a:0",            // map first audio stream from second file

            outputPath
        };

            string commandLine = string.Join(" ", args);
            Debug.WriteLine($"FFmpeg command: {commandLine}");

            // 3. Run FFmpeg and capture both stderr and stdout

            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = commandLine,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(outputPath) ?? "."
            };

            using var proc = new Process { StartInfo = startInfo };

            proc.Start();

            // Read all output (FFmpeg logs to stderr)
            string stderr = proc.StandardError.ReadToEnd();
            string stdout = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();

            // 4. Log everything and fail hard if something went wrong

            Debug.WriteLine("--- FFmpeg stdout ---");
            Debug.WriteLine(stdout);
            Debug.WriteLine("--- End FFmpeg stdout ---");

            Debug.WriteLine("--- FFmpeg stderr ---");
            Debug.WriteLine(stderr);
            Debug.WriteLine("--- End FFmpeg stderr ---");

            if (proc.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"FFmpeg failed with exit code {proc.ExitCode}.\n" +
                    $"Arguments:\n{commandLine}\n\n" +
                    $"stderr:\n{stderr}");
            }

            // 5. Verify that the output file exists and is not empty

            var outputFinal = new FileInfo(outputPath);

            if (!outputFinal.Exists)
                throw new InvalidOperationException(
                    $"FFmpeg finished successfully, but output file '{outputPath}' does not exist.");

            if (outputFinal.Length == 0)
                throw new InvalidOperationException(
                    $"FFmpeg finished successfully, but output file is 0 bytes.\n" +
                    $"This usually means the input streams are invalid or unsupported.\n" +
                    $"stderr:\n{stderr}");
        }

        public void run()
        {
            string userVideos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            string myEngineFolder = Path.Combine(userVideos, "BrainFodder");
            Directory.CreateDirectory(myEngineFolder);
            string videoPath = Path.Combine(myEngineFolder, "raw_video.mp4");
            string audioPath = Path.Combine(myEngineFolder, "audio.wav");

            recorder.StartRecording(innerResolution, 60);

            WindowHandler.getWindow().Run();
        }
        private void update(float delta)
        {
            EngineDeltaClock += delta;
            windowHandler.update(delta);
            ecsWorld.Update(delta);
            masterRenderer.update(delta);
            SoundManager.update(delta);
        }
        private void render()
        {

            frameBuffer.bind();

            windowHandler.render();
            masterRenderer.render();


            int width = innerResolution.X;
            int height = innerResolution.Y;
            // 1. Tell GPU to copy framebuffer to PBO
            GL.BindBuffer(BufferTarget.PixelPackBuffer, pbo);
            GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            // 2. Map buffer to CPU memory
            IntPtr ptr = GL.MapBuffer(BufferTarget.PixelPackBuffer, BufferAccess.ReadOnly);
            byte[] frameData = new byte[width * height * 4];
            System.Runtime.InteropServices.Marshal.Copy(ptr, frameData, 0, frameData.Length);
            GL.UnmapBuffer(BufferTarget.PixelPackBuffer);

            // 3. Send to FFmpeg
            recorder.RenderFrame(frameData);

            frameBuffer.resolveToScreen();
        }
    }
}
