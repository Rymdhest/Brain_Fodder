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
        EntityManager entityManager;
        public static float EngineDeltaClock = 0f;

        public Engine()
        {
            windowHandler = new WindowHandler(new Vector2i(1080, 1920) / 2);
            masterRenderer = new MasterRenderer();
            entityManager = new EntityManager();
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
        }

        public void run()
        {
            WindowHandler.getWindow().Run();
        }
        private void update(float delta)
        {
            EngineDeltaClock += delta;
            windowHandler.update(delta);
            entityManager.update(delta);
            masterRenderer.update(delta);
        }
        private void render()
        {
            windowHandler.render();
            masterRenderer.render();
        }
    }
}
