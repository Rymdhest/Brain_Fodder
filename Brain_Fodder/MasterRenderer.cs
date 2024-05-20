
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using Brain_Fodder;
using SpaceEngine.Modelling;
using OpenTK.Windowing.Desktop;
using SpaceEngine.Util;

namespace SpaceEngine.RenderEngine
{
    internal class MasterRenderer
    {
        public static ShaderProgram simpleShader = new ShaderProgram("Simple_Vertex", "Simple_Fragment");
        public static ShaderProgram circleShader = new ShaderProgram("Simple_Vertex", "Circle_Fragment");
        public static ShaderProgram RingShader = new ShaderProgram("Simple_Vertex", "Ring_Fragment");
        private glModel unitSquare;
        private glModel lineBase;
        private Matrix4 projection;

        private List<Circle> circles = new List<Circle>();
        private List<Line> lines = new List<Line>();
        private List<Ring> rings = new List<Ring>();

        private List<BouncyBall> balls = new List<BouncyBall>();

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
            float[] positions = { -1, 1, -1, -1, 1, -1, 1, 1 };
            int[] indices = { 0, 1, 2, 3, 0, 2 };
            unitSquare = glLoader.loadToVAO(positions, indices);

            float[] positionsLine = { 0, 0.5f, 0, -0.5f, 1, -0.5f, 1, 0.5f };
            int[] indicesLine = { 0, 1, 2, 3, 0, 2 };
            lineBase = glLoader.loadToVAO(positionsLine, indicesLine);

            circles.Add(new Circle(WindowHandler.getCenter(), 10));

            circles.Add(new Circle(new Vector2(100, 100), 10));
            circles.Add(new Circle(new Vector2(200, 300), 10));

            rings.Add(new Ring(WindowHandler.getCenter(), WindowHandler.getResolution().X/2f, 2.0f));
        }

        private void addBall()
        {
            BouncyBall ball = new BouncyBall(WindowHandler.getCenter(), new Vector2(), 10);
            balls.Add(ball);
            circles.Add(ball.Model);
        }

        public void prepareFrame()
        {
            projection = Matrix4.CreateOrthographicOffCenter(0.0f, WindowHandler.gameWindow.ClientSize.X, 0.0f, WindowHandler.gameWindow.ClientSize.Y, -1.0f, 1.0f);
            GL.DepthMask(true);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.Blend);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void finishFrame()
        {
            WindowHandler.getWindow().SwapBuffers();
        }
   
        public void render()
        {


            Vector2 from = new Vector2(100, 100);
            Vector2 to = new Vector2(200, 200);
            float width = 10.0f;
            prepareFrame();

            renderRings();
            rendercircles();
            renderLines();




            finishFrame();
        }

        private void renderRings()
        {
            RingShader.bind();
            foreach (Ring ring in rings)
            {
                RingShader.loadUniformMatrix4f("uProjection", projection);
                RingShader.loadUniformVector2f("center", ring.Transformation.position);
                RingShader.loadUniformVector3f("color", ring.Color);
                RingShader.loadUniformFloat("radius", ring.Transformation.scale.X);
                RingShader.loadUniformFloat("width", ring.Width);
                RingShader.loadUniformMatrix4f("modelMatrix", MyMath.createTransformationMatrix(ring.Transformation));

                glModel glmodel = unitSquare;
                GL.BindVertexArray(glmodel.getVAOID());
                GL.EnableVertexAttribArray(0);
                GL.DrawElements(PrimitiveType.Triangles, glmodel.getVertexCount(), DrawElementsType.UnsignedInt, 0);
            }
            RingShader.unBind();
        }

        private void rendercircles()
        {
            circleShader.bind();
            foreach (Circle circle in circles)
            {
                circleShader.loadUniformMatrix4f("uProjection", projection);
                circleShader.loadUniformVector2f("center", circle.Transformation.position);
                circleShader.loadUniformVector3f("color", circle.Color);
                circleShader.loadUniformFloat("radius", circle.Transformation.scale.X);
                circleShader.loadUniformMatrix4f("modelMatrix", MyMath.createTransformationMatrix(circle.Transformation));

                glModel glmodel = unitSquare;
                GL.BindVertexArray(glmodel.getVAOID());
                GL.EnableVertexAttribArray(0);
                GL.DrawElements(PrimitiveType.Triangles, glmodel.getVertexCount(), DrawElementsType.UnsignedInt, 0);
            }
            circleShader.unBind();
        }

        private void renderLines()
        {
            simpleShader.bind();
            foreach (Line line in lines)
            {
                simpleShader.loadUniformMatrix4f("uProjection", projection);
                simpleShader.loadUniformVector3f("color", line.Color);
                simpleShader.loadUniformMatrix4f("modelMatrix", MyMath.createTransformationMatrix(line.Transformation));

                glModel glmodel = lineBase;
                GL.BindVertexArray(glmodel.getVAOID());
                GL.EnableVertexAttribArray(0);
                GL.DrawElements(PrimitiveType.Triangles, glmodel.getVertexCount(), DrawElementsType.UnsignedInt, 0);
            }
            simpleShader.unBind();
        }

        public void update(float delta)
        {
            foreach (Circle circle in circles)
            {
                circle.Transformation.position.Y += MyMath.rngMinusPlus() * delta * 500f;
                circle.Transformation.position.X += MyMath.rngMinusPlus() * delta * 500f; ;
            }
            foreach (Line line in lines)
            {
                line.setEnd(circles[0].Transformation.position);
            }
        }
        public void onResize(ResizeEventArgs eventArgs)
        {
            GL.Viewport(0, 0, WindowHandler.gameWindow.ClientSize.X, WindowHandler.gameWindow.ClientSize.Y);
        }
    }
}
