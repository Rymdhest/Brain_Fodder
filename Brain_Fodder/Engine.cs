using Brain_Fodder.Rendering;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using SpaceEngine.RenderEngine;
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
        public VideoRecorder videoRecorder = new VideoRecorder();
        private int pbo;

        public static Engine? Instance { get => _instance; }

        public Engine()
        {
            _instance = this;
            windowHandler = new WindowHandler(new Vector2i(1080, 1920) / 2  );
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
                    videoRecorder.Stop();

                    ecsWorld.clearLevel();
                    ecsWorld.SpawnLevel();

                }
            };

            int width = WindowHandler.getResolution().X;
            int height = WindowHandler.getResolution().Y;
            pbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.PixelPackBuffer, pbo);
            GL.BufferData(BufferTarget.PixelPackBuffer, width * height * 4, IntPtr.Zero, BufferUsageHint.StreamRead);
        }

        public void run()
        {
            string userVideos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            string myEngineFolder = Path.Combine(userVideos, "BrainFodder");
            Directory.CreateDirectory(myEngineFolder);
            string outputPath = Path.Combine(myEngineFolder, "recording.mp4");
            videoRecorder.Start(WindowHandler.getResolution().X, WindowHandler.getResolution().Y, 60, outputPath);

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
            windowHandler.render();
            masterRenderer.render();


            int width = WindowHandler.getResolution().X;
            int height = WindowHandler.getResolution().Y;
            // 1. Tell GPU to copy framebuffer to PBO
            GL.BindBuffer(BufferTarget.PixelPackBuffer, pbo);
            GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            // 2. Map buffer to CPU memory
            IntPtr ptr = GL.MapBuffer(BufferTarget.PixelPackBuffer, BufferAccess.ReadOnly);
            byte[] frameData = new byte[width * height * 4];
            System.Runtime.InteropServices.Marshal.Copy(ptr, frameData, 0, frameData.Length);
            GL.UnmapBuffer(BufferTarget.PixelPackBuffer);

            // 3. Send to FFmpeg
            videoRecorder.WriteFrame(frameData);
        }
    }
}
