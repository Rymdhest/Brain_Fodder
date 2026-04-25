
using Brain_Fodder;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SpaceEngine.Modelling;
using SpaceEngine.RenderEngine;
using SpaceEngine.Util;
using System.Net.NetworkInformation;

namespace Brain_Fodder.Rendering
{
    public struct CircleRenderCommand
    {
        public Vector2 position;
        public float radius;
        public Vector3 color;
    }
    public struct RingRenderCommand
    {
        public Vector2 position;
        public float radius;
        public float width;
        public Vector3 color;
    }
    public struct RectangleRenderCommand
    {
        public Vector2 position;
        public Vector2 size;
        public Vector3 color;
        public float rotation;
    }

    internal class MasterRenderer
    {
        public static ShaderProgram rectangleShader = new ShaderProgram("Simple_Vertex", "Rectangle_Fragment");
        public static ShaderProgram circleShader = new ShaderProgram("Simple_Vertex", "Circle_Fragment");
        public static ShaderProgram RingShader = new ShaderProgram("Simple_Vertex", "Ring_Fragment");
        private glModel unitSquare;
        private glModel lineBase;
        private Matrix4 projection;

        public static List<RingRenderCommand> rings = new List<RingRenderCommand>();
        public static List<CircleRenderCommand> circles = new List<CircleRenderCommand>();
        public static List<RectangleRenderCommand> rectangles = new List<RectangleRenderCommand>();


        class BouncyBall{
            private Vector2 velocity;
            private Circle model;

            public BouncyBall(Vector2 center, Vector2 velocity, float radius)
            {
                Model = new Circle(center, radius);
                this.Velocity = velocity;
            }

            public Vector2 Velocity { get => velocity; set => velocity = value; }
            public Circle Model { get => model; set => model = value; }
        }

        public MasterRenderer() {
            float[] positions = { -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f, 0.5f, 0.5f };
            int[] indices = { 0, 1, 2, 3, 0, 2 };
            unitSquare = glLoader.loadToVAO(positions, indices);

            float[] positionsLine = { 0, 0.5f, 0, -0.5f, 1, -0.5f, 1, 0.5f };
            int[] indicesLine = { 0, 1, 2, 3, 0, 2 };
            lineBase = glLoader.loadToVAO(positionsLine, indicesLine);

        }



        public void prepareFrame()
        {
            projection = Matrix4.CreateOrthographicOffCenter(0.0f, WindowHandler.gameWindow.ClientSize.X, 0.0f, WindowHandler.gameWindow.ClientSize.Y, -1.0f, 1.0f);
            GL.DepthMask(true);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ClearColor(0.02f, 0.02f, 0.1f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void finishFrame()
        {
            WindowHandler.getWindow().SwapBuffers();
        }
   
        public void render()
        {
            prepareFrame();

            renderRings();
            rendercircles();
            renderRectangles();

            finishFrame();
        }

        private void renderRings()
        {
            RingShader.bind();
            foreach (RingRenderCommand ring in rings)
            {
                RingShader.loadUniformMatrix4f("uProjection", projection);
                RingShader.loadUniformVector3f("color", ring.color);
                RingShader.loadUniformFloat("width", ring.width);
                circleShader.loadUniformMatrix4f("modelMatrix", MyMath.createTransformationMatrix(ring.position, 0f, new Vector2(ring.radius * 2)));

                glModel glmodel = unitSquare;
                GL.BindVertexArray(glmodel.getVAOID());
                GL.EnableVertexAttribArray(0);
                GL.DrawElements(PrimitiveType.Triangles, glmodel.getVertexCount(), DrawElementsType.UnsignedInt, 0);
            }
            RingShader.unBind();
            rings.Clear();
        }

        private void rendercircles()
        {
            circleShader.bind();
            foreach (CircleRenderCommand circle in circles)
            {
                circleShader.loadUniformMatrix4f("uProjection", projection);
                circleShader.loadUniformVector3f("color", circle.color);
                circleShader.loadUniformMatrix4f("modelMatrix", MyMath.createTransformationMatrix(circle.position, 0f, new Vector2(circle.radius*2)));

                glModel glmodel = unitSquare;
                GL.BindVertexArray(glmodel.getVAOID());
                GL.EnableVertexAttribArray(0);
                GL.DrawElements(PrimitiveType.Triangles, glmodel.getVertexCount(), DrawElementsType.UnsignedInt, 0);
            }
            circleShader.unBind();
            circles.Clear();
        }

        private void renderRectangles()
        {
            rectangleShader.bind();
            foreach (RectangleRenderCommand rectangle in rectangles)
            {
                rectangleShader.loadUniformMatrix4f("uProjection", projection);
                rectangleShader.loadUniformVector3f("color", rectangle.color);
                rectangleShader.loadUniformMatrix4f("modelMatrix", MyMath.createTransformationMatrix(rectangle.position, rectangle.rotation, rectangle.size));

                glModel glmodel = unitSquare;
                GL.BindVertexArray(glmodel.getVAOID());
                GL.EnableVertexAttribArray(0);
                GL.DrawElements(PrimitiveType.Triangles, glmodel.getVertexCount(), DrawElementsType.UnsignedInt, 0);
            }
            rectangleShader.unBind();
            rectangles.Clear();
        }

        public void update(float delta)
        {

        }
        public void onResize(ResizeEventArgs eventArgs)
        {
            GL.Viewport(0, 0, WindowHandler.gameWindow.ClientSize.X, WindowHandler.gameWindow.ClientSize.Y);
        }
    }
}
