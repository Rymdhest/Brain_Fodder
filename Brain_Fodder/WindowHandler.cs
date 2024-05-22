
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using OpenTK.Input;
using OpenTK.Audio.OpenAL;
namespace SpaceEngine.RenderEngine
{
    internal class WindowHandler
    {
        private string title = "Brain Fodder";
        public static GameWindow? gameWindow = null;

        private Stopwatch frameStopWatch = new Stopwatch();
        private Stopwatch secondStopWatchUpdate = new Stopwatch();
        private Stopwatch secondStopWatchRender = new Stopwatch();
        private float delta = 0f;
        private int framesLastSecondUpdate = 0;
        private int framesLastSecondRender = 0;
        private int framesCurrentSecondUpate = 0;
        private int framesCurrentSecondRender = 0;

        public WindowHandler(Vector2i resolution)
        {
            GameWindowSettings gws = GameWindowSettings.Default;
            NativeWindowSettings nws = NativeWindowSettings.Default;
            nws.API = ContextAPI.OpenGL;
            //nws.APIVersion = Version.Parse("3.3");
            nws.AutoLoadBindings = true;
            nws.Title = title;
            nws.ClientSize = resolution;
            nws.Location = new Vector2i(20, 100);

            gws.UpdateFrequency = 60;

            gameWindow = new GameWindow(gws, nws);

            secondStopWatchUpdate.Start();
            secondStopWatchRender.Start();
            frameStopWatch.Start();
        }

        public static Vector2i getCenter()
        {
            return new Vector2i(gameWindow.ClientSize.X / 2, gameWindow.ClientSize.Y / 2);
        }

        public static Vector2i getResolution()
        {
            return gameWindow.ClientSize;
        }

        public static GameWindow? getWindow()
        {
            return gameWindow;
        }
        public void update(float delta)
        {
            this.delta = (float)frameStopWatch.Elapsed.TotalSeconds;
            frameStopWatch.Restart();

            if (secondStopWatchUpdate.Elapsed.TotalMilliseconds >= 1000.0)
            {
                framesLastSecondUpdate = framesCurrentSecondUpate;
                framesCurrentSecondUpate = 0;
                gameWindow.Title = title + " " + framesLastSecondUpdate + " FPS Update : "+ framesLastSecondRender+" FPS";
                secondStopWatchUpdate.Restart();

            }
            framesCurrentSecondUpate++;
        }
        public void render()
        {
            if (secondStopWatchRender.Elapsed.TotalMilliseconds >= 1000.0)
            {
                framesLastSecondRender = framesCurrentSecondRender;
                framesCurrentSecondRender = 0;
                secondStopWatchRender.Restart();

            }
            framesCurrentSecondRender++;
        }
        public void onResize(ResizeEventArgs eventArgs)
        {
            gameWindow.ClientSize = eventArgs.Size;
        }
        public float getDelta()
        {
            return delta;
        }
        public static void setMouseGrabbed(bool setTo)
        {
            if (setTo)
            {
                gameWindow.CursorState = CursorState.Grabbed;
            } else
            {
                gameWindow.CursorState = CursorState.Normal;
            }
            
        }
    }

}
