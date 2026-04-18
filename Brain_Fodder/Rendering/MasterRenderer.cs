
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

    public struct RectangleRenderCommand
    {
        public Vector2 position;
        public Vector2 size;
        public Vector3 color;
        public float rotation;
    }

    internal class MasterRenderer
    {
        public static ShaderProgram simpleShader = new ShaderProgram("Simple_Vertex", "Simple_Fragment");
        public static ShaderProgram circleShader = new ShaderProgram("Simple_Vertex", "Circle_Fragment");
        public static ShaderProgram RingShader = new ShaderProgram("Simple_Vertex", "Ring_Fragment");
        private glModel unitSquare;
        private glModel lineBase;
        private glModel rectangleBase;
        private Matrix4 projection;

        public static List<CircleRenderCommand> circles = new List<CircleRenderCommand>();
        public static List<RectangleRenderCommand> rectangles = new List<RectangleRenderCommand>();
        private List<Line> lines = new List<Line>();

        private List<BouncyBall> balls = new List<BouncyBall>();

        private BouncyBall lineBall;

        private Ring border;

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

            float[] positionsRectangle = { 0, 0.0f, 1, 0.0f, 1, 1.0f, 0, 1f };
            int[] indicesRectangle = { 0, 1, 2, 3, 0, 2 };
            rectangleBase = glLoader.loadToVAO(positionsLine, indicesLine);
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

            rendercircles();
            renderRectangles();

            finishFrame();
        }


        private void rendercircles()
        {
            circleShader.bind();
            foreach (CircleRenderCommand circle in circles)
            {
                circleShader.loadUniformMatrix4f("uProjection", projection);
                circleShader.loadUniformVector3f("color", circle.color);
                circleShader.loadUniformFloat("radius", circle.radius);
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
            simpleShader.bind();
            foreach (RectangleRenderCommand rectangle in rectangles)
            {
                simpleShader.loadUniformMatrix4f("uProjection", projection);
                simpleShader.loadUniformVector3f("color", rectangle.color);
                simpleShader.loadUniformMatrix4f("modelMatrix", MyMath.createTransformationMatrix(rectangle.position, rectangle.rotation, rectangle.size));

                glModel glmodel = unitSquare;
                GL.BindVertexArray(glmodel.getVAOID());
                GL.EnableVertexAttribArray(0);
                GL.DrawElements(PrimitiveType.Triangles, glmodel.getVertexCount(), DrawElementsType.UnsignedInt, 0);
            }
            simpleShader.unBind();
            rectangles.Clear();
        }

        public void update(float delta)
        {
            foreach (BouncyBall ball in balls)
            {
                ball.Model.Transformation.position += ball.Velocity * delta;

                if (Vector2.Distance( ball.Model.Transformation.position, border.Transformation.position)+ball.Model.Radius*0.8f > border.getInnerRadius())
                {
                    float angle = MyMath.AngleBetween(ball.Model.Transformation.position, border.Transformation.position);
                    float x = MathF.Cos(angle)*-1f;
                    float y = MathF.Sin(angle)*-1f;
                    Vector2 collisionPoint = border.Transformation.position + new Vector2(x, y)*border.Radius;
                    ball.Velocity = ReflectCircleCollision(ball.Velocity, ball.Model.Transformation.position, border.Transformation.position);
                    ball.Velocity *= 1.06f;
                    //SoundManager.Play(MathF.Sqrt( ball.Velocity.Length)*0.02f);
                    Line line = new Line(collisionPoint, ball.Model.Transformation.position,3.0f);
                    line.Color = ball.Model.Color;

                    float diameter = border.Radius * 2f;
                    float r = 0f;
                    float g = (collisionPoint.Y - border.Transformation.position.Y) / diameter + 0.5f;
                    float b = (collisionPoint.X - border.Transformation.position.X) / diameter + 0.5f;
                    line.Color = new Vector3(r, g, b);
                    lines.Add(line);
                }
            }
            foreach(Line line in lines)
            {
                line.setEnd(lineBall.Model.Transformation.position);
            }
        }
        public void onResize(ResizeEventArgs eventArgs)
        {
            GL.Viewport(0, 0, WindowHandler.gameWindow.ClientSize.X, WindowHandler.gameWindow.ClientSize.Y);
        }

        private Vector2 ReflectCircleCollision(Vector2 velocity, Vector2 point, Vector2 center)
        {
            // Normal vector at the point of collision
            float Nx = point.X - center.X;
            float Ny = point.Y - center.Y;

            // Normalize the normal vector
            float norm = MathF.Sqrt(Nx * Nx + Ny * Ny);
            Nx /= norm;
            Ny /= norm;

            // Velocity vector
            float dotProduct = velocity.X * Nx + velocity.Y * Ny;

            // Reflected velocity
            float Vnew_x = velocity.X - 2 * dotProduct * Nx;
            float Vnew_y = velocity.Y - 2 * dotProduct * Ny;

            return new Vector2(Vnew_x, Vnew_y);
        }
    }
}
