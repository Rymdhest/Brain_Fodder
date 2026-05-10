using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct PushOutFromOnCollision : IComponent
    {
        public Vector2 center;
        public float pushDistance;

        public PushOutFromOnCollision(Vector2 center, float pushDistance)
        {
            this.center = center;
            this.pushDistance = pushDistance;
        }
    }
}
