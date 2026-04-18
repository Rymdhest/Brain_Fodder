using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace Dino_Engine.ECS.Components
{
    public struct CollisionSound : IComponent
    {


        public float cooldownSeconds = 0.1f;
        public float timeLastPlayed = 0.0f;
        public float minImpulse = 100.0f;

        public CollisionSound()
        {
        }

    }
}
