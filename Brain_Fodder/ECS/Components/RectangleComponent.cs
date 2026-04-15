using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct RectangleComponent : IComponent
    {
        public Vector2 size;
        public float rotation;

        public RectangleComponent(Vector2 size, float rotation = 0.0f)
        {
            this.size = size;
            this.rotation = rotation;
        }

    }
}
