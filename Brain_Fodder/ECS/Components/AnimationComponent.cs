using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct AnimationComponent : IComponent
    {
        public Vector2 start = new Vector2();
        public Vector2 goal = new Vector2();
        public float t = 99;
        public float duration = 1f;
        public AnimationComponent(Vector2 start, Vector2 goal, float duration)
        {
            this.start = start;
            this.goal = goal;
            this.duration = duration;
        }
        public AnimationComponent()
        {
        }
    }
}
