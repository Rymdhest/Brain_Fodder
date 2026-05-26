using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct ShapeComponent : IComponent
    {
        public Vector2 size;
        public float rotation;
        public int sides;
        public float thickness;
        public bool filled;

        public ShapeComponent(Vector2 size, int sides, float rotation = 0.0f, float thickness = 0.0f, bool filled = true)
        {
            this.size = size;
            this.rotation = rotation;
            this.sides = sides;
            this.thickness = thickness;
            this.filled = filled;
        }

    }
}
