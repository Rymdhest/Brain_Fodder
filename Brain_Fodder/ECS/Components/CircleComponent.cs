using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct CircleComponent : IComponent
    {
        public float radius;

        public CircleComponent(float radius)
        {
            this.radius = radius;
        }

    }
}
