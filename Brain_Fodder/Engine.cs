using Brain_Fodder.Rendering;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using SpaceEngine.RenderEngine;

namespace Brain_Fodder
{
    internal class Engine
    {
        SoundManager soundManager;
        WindowHandler windowHandler;
        MasterRenderer masterRenderer;
        ECSWorld ecsWorld;
        public static float EngineDeltaClock = 0f;

        public Engine()
        {
            windowHandler = new WindowHandler(new Vector2i(1080, 1920) / 2);
            masterRenderer = new MasterRenderer();

            ComponentTypeRegistry.AutoRegisterAllComponents();
            SystemRegistry.AutoRegisterAllSystems();
            ecsWorld = new ECSWorld();

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
