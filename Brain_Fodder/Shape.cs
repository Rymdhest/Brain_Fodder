using OpenTK.Mathematics;

namespace Brain_Fodder
{
    public abstract class Shape
    {
        private Transformation2D transformation;
        private Vector3 color;
        public Shape()
        {
            Color = new Vector3(1.0f, 0.0f, 0.0f);
        }

        public Transformation2D Transformation { get => transformation; set => transformation = value; }
        public Vector3 Color { get => color; set => color = value; }
    }
    public class Circle : Shape
    {
        public Circle(Vector2 center, float radius)
        {
            base.Transformation = new Transformation2D(center, 0f, new Vector2(radius));
        }
    }

    public class Ring : Circle
    {
        private float width;
        public Ring(Vector2 center, float radius, float width) : base(center, radius)
        {
            this.Width = width;
        }

        public float getInnerRadius()
        {
            return base.Transformation.scale.X - width;
        }

        public float Width { get => width; set => width = value; }
    }

    public class Line : Shape
    {
        private Vector2 start;
        private Vector2 end;
        private float width;

        public Line(Vector2 start, Vector2 end, float width)
        {
            this.start = start;
            this.end = end;
            this.width = width;
            updateTransformation();
        }

        private void updateTransformation()
        {
            float angle = MathF.Atan2(end.Y - start.Y, end.X - start.X);
            float length = Vector2.Distance(start, end);
            base.Transformation = new Transformation2D(start, angle, new Vector2(length, width));
        }

        public void setEnd(Vector2 newEnd)
        {
            this.end = newEnd;
            updateTransformation();
        }
    }
}
