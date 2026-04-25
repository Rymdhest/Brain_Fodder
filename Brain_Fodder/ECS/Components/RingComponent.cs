using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct RingComponent : IComponent
    {
        public float radius;
        public float width;

        public RingComponent(float radius, float width)
        {
            this.radius = radius;
            this.width = width;
        }

    }
}
