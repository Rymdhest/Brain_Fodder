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
        private int[] pbos = new int[2]; // Ring buffer: 2 PBOs for async reads
        private int currentPboIndex = 0;
        private byte[]? frameData;
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

                    recorder.StartRecording(innerResolution, 60);
                }
            };



            FrameBufferSettings frameBufferSettings = new FrameBufferSettings(innerResolution);
            frameBufferSettings.drawBuffers.Add(new DrawBufferSettings(FramebufferAttachment.ColorAttachment0));
            frameBuffer = new FrameBuffer(frameBufferSettings);





            int width = innerResolution.X;
            int height = innerResolution.Y;
            GL.GenBuffers(2, pbos);
            for (int i = 0; i < 2; i++)
            {
                GL.BindBuffer(BufferTarget.PixelPackBuffer, pbos[i]);
                GL.BufferData(BufferTarget.PixelPackBuffer, width * height * 4, IntPtr.Zero, BufferUsageHint.StreamRead);
            }
            frameData = new byte[width * height * 4];
        }

        public void run()
        {

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

            // Get the PBO we'll write to (the OTHER one from last frame)
            int writePbo = pbos[currentPboIndex];
            int readPbo = pbos[1 - currentPboIndex];

            // 1. Read from the read PBO (data from 1-2 frames ago)
            GL.BindBuffer(BufferTarget.PixelPackBuffer, readPbo);
            IntPtr ptr = GL.MapBuffer(BufferTarget.PixelPackBuffer, BufferAccess.ReadOnly);
            if (ptr != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.Copy(ptr, frameData, 0, frameData.Length);
                GL.UnmapBuffer(BufferTarget.PixelPackBuffer);

                // Direct write to disk - no queuing, no copying
                recorder.RenderFrame(frameData);
            }

            // 2. Bind write PBO and issue async GPU read (non-blocking)
            GL.BindBuffer(BufferTarget.PixelPackBuffer, writePbo);
            GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            // Swap to next PBO for next frame
            currentPboIndex = 1 - currentPboIndex;

            frameBuffer.resolveToScreen();
        }
    }
}
