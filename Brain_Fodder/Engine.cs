using Brain_Fodder.Rendering;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using SpaceEngine.RenderEngine;

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
                    ecsWorld.clearLevel();
                    ecsWorld.SpawnLevel();
                }
            };
        }

        public void run()
        {
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
        }
    }
}
