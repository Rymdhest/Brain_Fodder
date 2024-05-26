
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

            float width = 90;
            border = new Ring(WindowHandler.getCenter(), WindowHandler.getResolution().X / 2f-width/2.0f, width);
            rings.Add(border);
            int numballs = 15;
            float fill = .7f;
            for(int i = 0; i<numballs; i++)
            {
                float spawnLength = border.getInnerRadius()*2f*fill;
                float spawnStartX = -border.getInnerRadius()*fill;
                float percent = i/(float)(numballs-1);
                float x =spawnStartX+ percent*spawnLength;
                Vector2 spawn = WindowHandler.getCenter() + new Vector2(x, 0);

                float r = 0.1f;
                float g = MathF.Abs(percent-0.5f)*2f;
                float b = 1.0f-MathF.Abs(percent - 0.5f)*2f;
                
                addBall(spawn, new Vector3(r, g, b));
            }
        }

        private void addBall(Vector2 spawnPos, Vector3 color)
        {
            float speed = 150;
            //Vector2 spawnPos = WindowHandler.getCenter() + MyMath.rng2DMinusPlus()*0;
            //spawnPos.X += MyMath.rngMinusPlus() * 100;
            Vector2 velocity = new Vector2(speed * MyMath.rngMinusPlus(), speed * MyMath.rngMinusPlus());
            velocity = new Vector2(0, speed);
            BouncyBall ball = new BouncyBall(spawnPos, velocity, 50);

            ball.Model.Color = color;

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
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
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
                RingShader.loadUniformFloat("radius", ring.Radius);
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
            foreach (BouncyBall ball in balls)
            {
                ball.Model.Transformation.position += ball.Velocity * delta;

                if (Vector2.Distance( ball.Model.Transformation.position, border.Transformation.position)+ball.Model.Radius > border.getInnerRadius())
                {
                    ball.Velocity = ReflectCircleCollision(ball.Velocity, ball.Model.Transformation.position, border.Transformation.position);
                    ball.Velocity *= 1.05f;
                    SoundManager.Play(ball.Velocity.Length*0.001f);
                }
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
