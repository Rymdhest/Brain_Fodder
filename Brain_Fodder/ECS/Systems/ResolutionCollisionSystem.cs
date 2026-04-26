
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Systems
{
    public class ResolutionCollisionSystem : SystemBase
    {
        public ResolutionCollisionSystem() : base(new BitMask())
        {
            Priority = 10;
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            throw new NotImplementedException();
        }

        internal override void UpdateInternal(ECSWorld world, float deltaTime)
        {
            CollisionBufferComponent buffer = world.GetComponent<CollisionBufferComponent>(world.GetSingleton<CollisionBufferComponent>());
            if (buffer.Manifolds.Count == 0) return;

            // --- TUNING PARAMETERS ---
            float slop = 0.03f;      // Allow 0.05 units of overlap before pushing
            float percent = 0.6f;    // Only fix 40% of the overlap per frame (Baumgarte)
            float minBounceVel = 10.0f; // Velocity threshold to stop tiny bounces

            for (int i = 0; i < buffer.Manifolds.Count; i++)
            {
                var manifold = buffer.Manifolds[i];

                EntityView viewA = world.GetEntityView(manifold.EntityA);
                EntityView viewB = world.GetEntityView(manifold.EntityB);
                if (!(viewA.Has<PhysicsComponent>() && viewB.Has<PhysicsComponent>()) ) continue;
                var physA = viewA.Get<PhysicsComponent>();
                var physB = viewB.Get<PhysicsComponent>();

                float totalInvMass = physA.InvMass + physB.InvMass;
                if (totalInvMass <= 0) continue;

                // --- PHASE 1: STABILIZED POSITIONAL CORRECTION ---
                // Instead of fixing 100%, we fix a percentage and ignore tiny overlaps
                float correctionMagnitude = Math.Max(manifold.Penetration - slop, 0.0f) / totalInvMass * percent;
                Vector2 correction = manifold.Normal * correctionMagnitude;

                var posA = viewA.Get<PositionComponent>();
                var posB = viewB.Get<PositionComponent>();

                posA.value += correction * physA.InvMass;
                posB.value -= correction * physB.InvMass;

                viewA.Set(posA);
                viewB.Set(posB);

                // --- PHASE 2: VELOCITY RESOLUTION ---
                var velA = viewA.Get<VelocityComponent>();
                var velB = viewB.Get<VelocityComponent>();

                Vector2 relativeVelocity = velA.value - velB.value;
                float velAlongNormal = Vector2.Dot(relativeVelocity, manifold.Normal);

                if (velAlongNormal > 0) continue;

                // Use the threshold to kill "micro-bounces"
                float e = (MathF.Abs(velAlongNormal) < minBounceVel) ? 0.0f : Math.Min(physA.Restitution, physB.Restitution);

                float j = -(1 + e) * velAlongNormal;
                j /= totalInvMass;

                Vector2 impulse = manifold.Normal * j;

                velA.value += impulse * physA.InvMass;
                velB.value -= impulse * physB.InvMass;

                viewA.Set(velA);
                viewB.Set(velB);

                manifold.Impulse = MathF.Abs(j);
                buffer.Manifolds[i] = manifold;
            }

            //buffer.Clear();
        }
    }
}
