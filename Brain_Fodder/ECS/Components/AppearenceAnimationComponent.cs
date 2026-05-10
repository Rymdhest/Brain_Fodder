using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct AppearenceAnimationComponent : IComponent
    {
        public Vector3 startColor = new Vector3();

        public Vector3 goalColor = new Vector3();
        public float goalSize;
        public float startSize;
        public float t = 99;
        public float duration = 1f;

        public AppearenceAnimationComponent(Vector3 startColor, Vector3 goalColor, float startSize, float goalSize, float duration)
        {
            this.startColor = startColor;
            this.goalColor = goalColor;
            this.startSize = startSize;
            this.goalSize = goalSize;
            this.duration = duration;
        }
    }
}
