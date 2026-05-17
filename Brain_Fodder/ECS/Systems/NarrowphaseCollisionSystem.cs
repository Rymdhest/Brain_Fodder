using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using System;

namespace Dino_Engine.ECS.Systems
{
    public class NarrowphaseCollisionSystem : SystemBase
    {
        public NarrowphaseCollisionSystem() : base(new BitMask())
        {
            Priority = -4;
        }

        public override void Update(ECSWorld world, float deltaTime)
        {
            var buffer = world.GetComponent<CollisionBufferComponent>(world.GetSingleton<CollisionBufferComponent>());

            foreach (var pair in buffer.PotentialPairs)
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
                // 4. Rectangle vs Circle 
                else if (entityA.Has<RectangleComponent>() && entityB.Has<CircleComponent>())
                {
                    CheckCircleVsRect(entityB, entityA, buffer);
                }
                // 5. Circle vs Ring
                else if (entityA.Has<CircleComponent>() && entityB.Has<RingComponent>())
                {
                    CheckCircleVsRing(entityA, entityB, buffer);
                }
                // 6. Ring vs Circle
                else if (entityA.Has<RingComponent>() && entityB.Has<CircleComponent>())
                {
                    CheckCircleVsRing(entityB, entityA, buffer);
                }
                // 7. Triangle vs Triangle
                else if (entityA.Has<TriangleComponent>() && entityB.Has<TriangleComponent>())
                {
                    CheckTriVsTri(entityA, entityB, buffer);
                }
                // 8. Triangle vs Rectangle
                else if (entityA.Has<TriangleComponent>() && entityB.Has<RectangleComponent>())
                {
                    CheckTriVsRect(entityA, entityB, buffer);
                }
                // 9. Rectangle vs Triangle
                else if (entityA.Has<RectangleComponent>() && entityB.Has<TriangleComponent>())
                {
                    CheckTriVsRect(entityB, entityA, buffer);
                }
                // 10. Triangle vs Circle
                else if (entityA.Has<TriangleComponent>() && entityB.Has<CircleComponent>())
                {
                    CheckTriVsCircle(entityA, entityB, buffer);
                }
                // 11. Circle vs Triangle
                else if (entityA.Has<CircleComponent>() && entityB.Has<TriangleComponent>())
                {
                    CheckTriVsCircle(entityB, entityA, buffer);
                }
            }
        }

        // --- NEW TRIANGLE METHODS ---

        private void CheckTriVsTri(EntityView a, EntityView b, CollisionBufferComponent buffer)
        {
            Vector2[] cornersA = GetTriangleCorners(a);
            Vector2[] cornersB = GetTriangleCorners(b);

            if (CheckPolygonsSAT(cornersA, cornersB, out Vector2 normal, out float penetration))
            {
                // Ensure normal points from B to A
                Vector2 d = a.Get<PositionComponent>().value - b.Get<PositionComponent>().value;
                if (Vector2.Dot(d, normal) < 0) normal = -normal;

                buffer.Manifolds.Add(new CollisionManifold
                {
                    EntityA = a.Entity,
                    EntityB = b.Entity,
                    Normal = normal,
                    Penetration = penetration
                });
            }
        }

        private void CheckTriVsRect(EntityView tri, EntityView rect, CollisionBufferComponent buffer)
        {
            Vector2[] cornersTri = GetTriangleCorners(tri);
            Vector2[] cornersRect = GetCorners(
                rect.Get<PositionComponent>().value,
                rect.Get<RectangleComponent>().size,
                rect.Get<RectangleComponent>().rotation
            );

            if (CheckPolygonsSAT(cornersTri, cornersRect, out Vector2 normal, out float penetration))
            {
                // Ensure normal points from B (Rect) to A (Tri)
                Vector2 d = tri.Get<PositionComponent>().value - rect.Get<PositionComponent>().value;
                if (Vector2.Dot(d, normal) < 0) normal = -normal;

                buffer.Manifolds.Add(new CollisionManifold
                {
                    EntityA = tri.Entity,
                    EntityB = rect.Entity,
                    Normal = normal,
                    Penetration = penetration
                });
            }
        }

        private void CheckTriVsCircle(EntityView triEnt, EntityView circEnt, CollisionBufferComponent buffer)
        {
            Vector2[] corners = GetTriangleCorners(triEnt);
            Vector2 cPos = circEnt.Get<PositionComponent>().value;
            float radius = circEnt.Get<CircleComponent>().radius;

            float minOverlap = float.MaxValue;
            Vector2 smallestAxis = Vector2.Zero;

            // 1. Check the 3 edge normals of the triangle
            for (int i = 0; i < 3; i++)
            {
                int j = (i + 1) % 3;
                Vector2 edge = corners[j] - corners[i];
                Vector2 axis = new Vector2(-edge.Y, edge.X).Normalized();

                Project(corners, axis, out float minA, out float maxA);

                // Project circle onto axis
                float cProj = Vector2.Dot(cPos, axis);
                float minB = cProj - radius;
                float maxB = cProj + radius;

                float overlap = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);
                if (overlap <= 0) return; // Gap found, no collision

                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    smallestAxis = axis;
                }
            }

            // 2. Check the axis from the circle center to the closest triangle vertex
            Vector2 closestVertex = corners[0];
            float minDistSq = (cPos - corners[0]).LengthSquared;
            for (int i = 1; i < 3; i++)
            {
                float distSq = (cPos - corners[i]).LengthSquared;
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closestVertex = corners[i];
                }
            }

            Vector2 axisToVertex = cPos - closestVertex;
            if (axisToVertex != Vector2.Zero)
            {
                axisToVertex.Normalize();
                Project(corners, axisToVertex, out float minA, out float maxA);

                float cProj = Vector2.Dot(cPos, axisToVertex);
                float minB = cProj - radius;
                float maxB = cProj + radius;

                float overlap = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);
                if (overlap <= 0) return;

                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    smallestAxis = axisToVertex;
                }
            }

            // Ensure normal points from Circle to Triangle
            Vector2 tPos = triEnt.Get<PositionComponent>().value;
            Vector2 dir = tPos - cPos;
            if (Vector2.Dot(dir, smallestAxis) < 0) smallestAxis = -smallestAxis;

            buffer.Manifolds.Add(new CollisionManifold
            {
                EntityA = triEnt.Entity,
                EntityB = circEnt.Entity,
                Normal = smallestAxis,
                Penetration = minOverlap
            });
        }

        // --- POLYGON / SAT HELPERS ---

        // Generic SAT solver for any two convex polygons (used for Tri vs Tri, Rect vs Rect, Tri vs Rect)
        private bool CheckPolygonsSAT(Vector2[] cornersA, Vector2[] cornersB, out Vector2 normal, out float penetration)
        {
            penetration = float.MaxValue;
            normal = Vector2.Zero;

            Vector2[][] polygons = { cornersA, cornersB };
            foreach (var polygon in polygons)
            {
                for (int i = 0; i < polygon.Length; i++)
                {
                    int j = (i + 1) % polygon.Length;
                    Vector2 edge = polygon[j] - polygon[i];

                    // Left-hand normal
                    Vector2 axis = new Vector2(-edge.Y, edge.X);
                    if (axis != Vector2.Zero) axis.Normalize();

                    Project(cornersA, axis, out float minA, out float maxA);
                    Project(cornersB, axis, out float minB, out float maxB);

                    float overlap = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);
                    if (overlap <= 0) return false; // Separating axis found

                    if (overlap < penetration)
                    {
                        penetration = overlap;
                        normal = axis;
                    }
                }
            }
            return true;
        }

        private Vector2[] GetTriangleCorners(EntityView triEnt)
        {
            Vector2 pos = triEnt.Get<PositionComponent>().value;
            var tri = triEnt.Get<TriangleComponent>();

            // 1. Define local vertices based on your unit triangle scaled by the component's size
            Vector2[] corners = new Vector2[3] {
        new Vector2(-0.5f * tri.size.X, -0.5f * tri.size.Y), // Bottom left
        new Vector2( 0.5f * tri.size.X, -0.5f * tri.size.Y), // Bottom right
        new Vector2( 0.0f,               0.5f * tri.size.Y)  // Top center
    };

            // 2. Apply rotation
            float cos = MathF.Cos(tri.rotation);
            float sin = MathF.Sin(tri.rotation);

            for (int i = 0; i < 3; i++)
            {
                float x = corners[i].X;
                float y = corners[i].Y;

                // Rotate and then translate to world position
                corners[i].X = pos.X + (x * cos - y * sin);
                corners[i].Y = pos.Y + (x * sin + y * cos);
            }

            return corners;
        }

        private void Project(Vector2[] corners, Vector2 axis, out float min, out float max)
        {
            min = max = Vector2.Dot(corners[0], axis);
            for (int i = 1; i < corners.Length; i++)
            {
                float p = Vector2.Dot(corners[i], axis);
                min = MathF.Min(min, p);
                max = MathF.Max(max, p);
            }
        }

        // --- EXISTING RECT/CIRCLE/RING METHODS ---

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

        private void CheckRectVsRect(EntityView a, EntityView b, CollisionBufferComponent buffer)
        {
            Vector2[] cornersA = GetCorners(a.Get<PositionComponent>().value, a.Get<RectangleComponent>().size, a.Get<RectangleComponent>().rotation);
            Vector2[] cornersB = GetCorners(b.Get<PositionComponent>().value, b.Get<RectangleComponent>().size, b.Get<RectangleComponent>().rotation);

            // Using the newly implemented generic SAT method
            if (CheckPolygonsSAT(cornersA, cornersB, out Vector2 normal, out float penetration))
            {
                Vector2 d = a.Get<PositionComponent>().value - b.Get<PositionComponent>().value;
                if (Vector2.Dot(d, normal) < 0) normal = -normal;

                buffer.Manifolds.Add(new CollisionManifold
                {
                    EntityA = a.Entity,
                    EntityB = b.Entity,
                    Normal = normal,
                    Penetration = penetration
                });
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
            Vector2 relPos = cPos - rPos;
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
                    Vector2 absDist = new Vector2(MathF.Abs(localCirclePos.X / h.X), MathF.Abs(localCirclePos.Y / h.Y));
                    Vector2 localNormal = (absDist.X > absDist.Y)
                        ? new Vector2(MathF.Sign(localCirclePos.X), 0)
                        : new Vector2(0, MathF.Sign(localCirclePos.Y));

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

        private void CheckCircleVsRing(EntityView circEnt, EntityView ringEnt, CollisionBufferComponent buffer)
        {
            Vector2 circPos = circEnt.Get<PositionComponent>().value;
            float circRadius = circEnt.Get<CircleComponent>().radius;

            Vector2 ringPos = ringEnt.Get<PositionComponent>().value;
            float ringOuterRadius = ringEnt.Get<RingComponent>().radius + ringEnt.Get<RingComponent>().width * 0.5f;
            float ringInnerRadius = ringEnt.Get<RingComponent>().radius - ringEnt.Get<RingComponent>().width * 0.5f;

            Vector2 delta = circPos - ringPos;
            float distanceSq = delta.LengthSquared;
            float distance = MathF.Sqrt(distanceSq);

            float minDist = ringInnerRadius - circRadius;
            float maxDist = ringOuterRadius + circRadius;

            if (distance >= minDist && distance <= maxDist)
            {
                Vector2 normal = (distance != 0) ? delta / distance : new Vector2(1, 0);
                float distToOuter = ringOuterRadius - distance;
                float distToInner = distance - ringInnerRadius;

                float penetration;
                if (distToOuter < distToInner)
                {
                    penetration = distToOuter + circRadius;
                }
                else
                {
                    normal = -normal;
                    penetration = distToInner + circRadius;
                }

                buffer.Manifolds.Add(new CollisionManifold
                {
                    EntityA = circEnt.Entity,
                    EntityB = ringEnt.Entity,
                    Normal = normal,
                    Penetration = penetration
                });
            }
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase
        }
    }
}