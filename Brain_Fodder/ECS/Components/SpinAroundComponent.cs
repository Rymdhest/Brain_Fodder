using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct SpinAroundComponent : IComponent
    {
        public Vector2 center;
        public float radius;
        public float Duration;
        public float t = 0f;

        public SpinAroundComponent(Vector2 center, float radius, float Duration)
        {
            this.radius = radius;
            this.Duration = Duration;
            this.center = center;

        }
    }
}
