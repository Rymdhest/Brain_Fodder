using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using System;

namespace Dino_Engine.ECS.Systems
{
    public class NarrowphaseCollisionSystem : SystemBase
    {
        public NarrowphaseCollisionSystem() : base(new BitMask()) { }

        public override void Update(ECSWorld world, float deltaTime)
        {
            var buffer = world.GetComponent<CollisionBufferComponent>(world.GetSingleton<CollisionBufferComponent>());

            foreach (var pair in buffer.Pairs)
            {
                var entityA = world.GetEntityView(pair.EntityA);
                var entityB = world.GetEntityView(pair.EntityB);

                // 1. Circle vs Circle
                if (entityA.Has<CircleComponent>() && entityB.Has<CircleComponent>())
                {
                    CheckCircleVsCircle(entityA, entityB, buffer);
                }
                // 2. Rectangle vs Rectangle
                else if (entityA.Has<RectangleComponent>() && entityB.Has<RectangleComponent>())
                {
                    CheckRectVsRect(entityA, entityB, buffer);
                }
                // 3. Circle vs Rectangle
                else if (entityA.Has<CircleComponent>() && entityB.Has<RectangleComponent>())
                {
                    CheckCircleVsRect(entityA, entityB, buffer);
                }
                // 4. Rectangle vs Circle (Flip entities to reuse logic)
                else if (entityA.Has<RectangleComponent>() && entityB.Has<CircleComponent>())
                {
                    CheckCircleVsRect(entityB, entityA, buffer);
                }
            }
        }

        private void CheckCircleVsCircle(EntityView a, EntityView b, CollisionBufferComponent buffer)
        {
            Vector2 posA = a.Get<PositionComponent>().value;
            Vector2 posB = b.Get<PositionComponent>().value;
            float radA = a.Get<CircleComponent>().radius;
            float radB = b.Get<CircleComponent>().radius;

            Vector2 delta = posA - posB;
            float distanceSq = delta.LengthSquared;
            float radiusSum = radA + radB;

            if (distanceSq <= radiusSum * radiusSum)
            {
                float distance = MathF.Sqrt(distanceSq);
                Vector2 normal = (distance != 0) ? delta / distance : new Vector2(1, 0);

                buffer.Manifolds.Add(new CollisionManifold
                {
                    EntityA = a.Entity,
                    EntityB = b.Entity,
                    Normal = normal,
                    Penetration = radiusSum - distance
                });
            }
        }

        private void CheckRectVsRect(EntityView a, EntityView b, CollisionBufferComponent buffer)
        {
            Vector2 posA = a.Get<PositionComponent>().value;
            Vector2 posB = b.Get<PositionComponent>().value;
            Vector2 sizeA = a.Get<RectangleComponent>().size;
            Vector2 sizeB = b.Get<RectangleComponent>().size;

            Vector2 delta = posA - posB;
            Vector2 hA = sizeA * 0.5f; // Half-extents
            Vector2 hB = sizeB * 0.5f;

            // Calculate overlap on X and Y
            float overlapX = (hA.X + hB.X) - MathF.Abs(delta.X);
            if (overlapX <= 0) return;

            float overlapY = (hA.Y + hB.Y) - MathF.Abs(delta.Y);
            if (overlapY <= 0) return;

            // Resolve on the axis of shallowest penetration
            Vector2 normal;
            float penetration;

            if (overlapX < overlapY)
            {
                normal = new Vector2(delta.X > 0 ? 1 : -1, 0);
                penetration = overlapX;
            }
            else
            {
                normal = new Vector2(0, delta.Y > 0 ? 1 : -1);
                penetration = overlapY;
            }

            buffer.Manifolds.Add(new CollisionManifold
            {
                EntityA = a.Entity,
                EntityB = b.Entity,
                Normal = normal,
                Penetration = penetration
            });
        }

        private void CheckCircleVsRect(EntityView circEnt, EntityView rectEnt, CollisionBufferComponent buffer)
        {
            Vector2 cPos = circEnt.Get<PositionComponent>().value;
            float radius = circEnt.Get<CircleComponent>().radius;
            Vector2 rPos = rectEnt.Get<PositionComponent>().value;
            Vector2 rSize = rectEnt.Get<RectangleComponent>().size;
            Vector2 h = rSize * 0.5f;

            // Vector from rect center to circle center
            Vector2 delta = cPos - rPos;

            // Closest point on the rectangle to the circle center
            Vector2 closest = new Vector2(
                Math.Clamp(delta.X, -h.X, h.X),
                Math.Clamp(delta.Y, -h.Y, h.Y)
            );

            Vector2 difference = delta - closest;
            float distanceSq = difference.LengthSquared;

            // Collision check
            if (distanceSq < radius * radius)
            {
                float distance = MathF.Sqrt(distanceSq);
                Vector2 normal;

                if (distance == 0) // Circle center is exactly inside or on the edge
                {
                    // For simplicity, push circle out along the axis of least penetration
                    // This is rare but prevents division by zero
                    normal = MathF.Abs(delta.X) > MathF.Abs(delta.Y) ? new Vector2(MathF.Sign(delta.X), 0) : new Vector2(0, MathF.Sign(delta.Y));
                    if (normal == Vector2.Zero) normal = new Vector2(0, 1);

                    buffer.Manifolds.Add(new CollisionManifold
                    {
                        EntityA = circEnt.Entity,
                        EntityB = rectEnt.Entity,
                        Normal = normal,
                        Penetration = radius
                    });
                }
                else
                {
                    normal = difference / distance;
                    buffer.Manifolds.Add(new CollisionManifold
                    {
                        EntityA = circEnt.Entity,
                        EntityB = rectEnt.Entity,
                        Normal = normal,
                        Penetration = radius - distance
                    });
                }
            }
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase as we use the Singleton Buffer
        }
    }
}