using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public enum EasingType
    {
        Linear,
        Cosine,
        SmoothStep,
        QuadraticIn,
        QuadraticOut
    }

    public struct OscillatorComponent : IComponent
    {
        public Vector2 positionA;
        public Vector2 positionB;
        public float Speed = 1.0f;
        public float Timer = 0.0f; // 0 to 1
        public bool Reverse = false; // Flips direction
        public EasingType Easing = EasingType.Linear;

        public OscillatorComponent(Vector2 positionA, Vector2 positionB)
        {
            this.positionA = positionA;
            this.positionB = positionB;
        }
    }
}
