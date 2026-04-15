using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct PhysicsComponent : IComponent
    {
        public float InvMass;
        public float Restitution;
        public PhysicsComponent(float mass, float restitution)
        {
            InvMass = mass > 0 ? 1.0f / mass : 0f;
            Restitution = Math.Clamp(restitution, 0f, 1f);
        }

    }
}
