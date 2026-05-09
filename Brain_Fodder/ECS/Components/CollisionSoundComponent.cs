using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace Dino_Engine.ECS.Components
{
    public struct CollisionSoundComponent : IComponent
    {


        public float cooldownSeconds = 0.03f;
        public float timeLastPlayed = 0.0f;
        public float minImpulse = 30.0f;
        public int note = 0;

        public CollisionSoundComponent(int note)
        {
            this.note = note;
        }

    }
}
