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
        private Vector2[] GetCorners(Vector2 center, Vector2 size, float rotation)
        {
            Vector2 h = size * 0.5f;
            Vector2[] corners = new Vector2[4] {
        new Vector2(-h.X, -h.Y), new Vector2(h.X, -h.Y),
        new Vector2(h.X, h.Y),   new Vector2(-h.X, h.Y)
    };

            float cos = MathF.Cos(rotation);
            float sin = MathF.Sin(rotation);

            for (int i = 0; i < 4; i++)
            {
                float x = corners[i].X;
                float y = corners[i].Y;
                corners[i].X = center.X + (x * cos - y * sin);
                corners[i].Y = center.Y + (x * sin + y * cos);
            }
            return corners;
        }

        private void Project(Vector2[] corners, Vector2 axis, out float min, out float max)
        {
            min = max = Vector2.Dot(corners[0], axis);
            for (int i = 1; i < 4; i++)
            {
                float p = Vector2.Dot(corners[i], axis);
                min = MathF.Min(min, p);
                max = MathF.Max(max, p);
            }
        }

        private void CheckRectVsRect(EntityView a, EntityView b, CollisionBufferComponent buffer)
        {
            Vector2[] cornersA = GetCorners(a.Get<PositionComponent>().value, a.Get<RectangleComponent>().size, a.Get<RectangleComponent>().rotation);
            Vector2[] cornersB = GetCorners(b.Get<PositionComponent>().value, b.Get<RectangleComponent>().size, b.Get<RectangleComponent>().rotation);

            // Axes to check: Normals of the edges of A and B
            Vector2[] axes = new Vector2[] {
        (cornersA[1] - cornersA[0]).Normalized(), // A Right
        (cornersA[3] - cornersA[0]).Normalized(), // A Up
        (cornersB[1] - cornersB[0]).Normalized(), // B Right
        (cornersB[3] - cornersB[0]).Normalized()  // B Up
    };

            float minOverlap = float.MaxValue;
            Vector2 smallestAxis = Vector2.Zero;

            foreach (var axis in axes)
            {
                Project(cornersA, axis, out float minA, out float maxA);
                Project(cornersB, axis, out float minB, out float maxB);

                float overlap = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);

                if (overlap <= 0) return; // GAP FOUND! No collision.

                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    smallestAxis = axis;
                }
            }

            // Ensure normal points from B to A
            Vector2 d = a.Get<PositionComponent>().value - b.Get<PositionComponent>().value;
            if (Vector2.Dot(d, smallestAxis) < 0) smallestAxis = -smallestAxis;

            buffer.Manifolds.Add(new CollisionManifold
            {
                EntityA = a.Entity,
                EntityB = b.Entity,
                Normal = smallestAxis,
                Penetration = minOverlap
            });
        }

        private void CheckCircleVsRect(EntityView circEnt, EntityView rectEnt, CollisionBufferComponent buffer)
        {
            // 1. Get world data
            Vector2 cPos = circEnt.Get<PositionComponent>().value;
            float radius = circEnt.Get<CircleComponent>().radius;

            Vector2 rPos = rectEnt.Get<PositionComponent>().value;
            var rectComp = rectEnt.Get<RectangleComponent>();
            Vector2 h = rectComp.size * 0.5f;
            float rotation = rectComp.rotation;

            // 2. Transform Circle Center to Rectangle's Local Space
            // Relative position
            Vector2 relPos = cPos - rPos;

            // Un-rotate the relative position
            float cos = MathF.Cos(-rotation);
            float sin = MathF.Sin(-rotation);
            Vector2 localCirclePos = new Vector2(
                relPos.X * cos - relPos.Y * sin,
                relPos.X * sin + relPos.Y * cos
            );

            // 3. Perform standard AABB-style closest point check in local space
            Vector2 localClosest = new Vector2(
                Math.Clamp(localCirclePos.X, -h.X, h.X),
                Math.Clamp(localCirclePos.Y, -h.Y, h.Y)
            );

            Vector2 localDistVec = localCirclePos - localClosest;
            float distanceSq = localDistVec.LengthSquared;

            if (distanceSq < radius * radius)
            {
                float distance = MathF.Sqrt(distanceSq);
                Vector2 worldNormal;

                if (distance == 0)
                {
                    // Circle center is inside. Find the shallowest local axis to push out.
                    Vector2 absDist = new Vector2(MathF.Abs(localCirclePos.X / h.X), MathF.Abs(localCirclePos.Y / h.Y));
                    Vector2 localNormal = (absDist.X > absDist.Y)
                        ? new Vector2(MathF.Sign(localCirclePos.X), 0)
                        : new Vector2(0, MathF.Sign(localCirclePos.Y));

                    // Rotate normal back to world space
                    float wCos = MathF.Cos(rotation);
                    float wSin = MathF.Sin(rotation);
                    worldNormal = new Vector2(
                        localNormal.X * wCos - localNormal.Y * wSin,
                        localNormal.X * wSin + localNormal.Y * wCos
                    );

                    buffer.Manifolds.Add(new CollisionManifold
                    {
                        EntityA = circEnt.Entity,
                        EntityB = rectEnt.Entity,
                        Normal = worldNormal,
                        Penetration = radius
                    });
                }
                else
                {
                    // Rotate the local distance vector back to world space for the normal
                    Vector2 localNormal = localDistVec / distance;

                    float wCos = MathF.Cos(rotation);
                    float wSin = MathF.Sin(rotation);
                    worldNormal = new Vector2(
                        localNormal.X * wCos - localNormal.Y * wSin,
                        localNormal.X * wSin + localNormal.Y * wCos
                    );

                    buffer.Manifolds.Add(new CollisionManifold
                    {
                        EntityA = circEnt.Entity,
                        EntityB = rectEnt.Entity,
                        Normal = worldNormal,
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