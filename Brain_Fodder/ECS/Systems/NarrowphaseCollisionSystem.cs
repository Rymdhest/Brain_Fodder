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

                if (entityA.Has<ShapeComponent>() && entityB.Has<ShapeComponent>())
                {
                    var shapeA = entityA.Get<ShapeComponent>();
                    var posA = entityA.Get<PositionComponent>().value;

                    var shapeB = entityB.Get<ShapeComponent>();
                    var posB = entityB.Get<PositionComponent>().value;

                    // --- THE FIX: Smart Hollow vs Hollow Routing ---
                    if (!shapeA.filled && !shapeB.filled)
                    {
                        // Calculate rough area to determine which container is bigger
                        float areaA = shapeA.size.X * shapeA.size.Y;
                        float areaB = shapeB.size.X * shapeB.size.Y;

                        if (areaA >= areaB)
                        {
                            // A is the giant container, B is the small occupant treated as solid
                            CheckHollowVsSolid(entityA, shapeA, posA, entityB, shapeB, posB, buffer, false);
                        }
                        else
                        {
                            // B is the giant container, A is the small occupant treated as solid
                            CheckHollowVsSolid(entityB, shapeB, posB, entityA, shapeA, posA, buffer, true);
                        }
                    }
                    // --- Standard Hollow vs Solid Routing ---
                    else if (!shapeA.filled)
                    {
                        CheckHollowVsSolid(entityA, shapeA, posA, entityB, shapeB, posB, buffer, false);
                    }
                    else if (!shapeB.filled)
                    {
                        CheckHollowVsSolid(entityB, shapeB, posB, entityA, shapeA, posA, buffer, true);
                    }
                    else
                    {
                        CheckSolidVsSolid(entityA, shapeA, posA, entityB, shapeB, posB, buffer);
                    }
                }
            }
        }

        // --- MAIN ROUTERS ---

        private void CheckSolidVsSolid(EntityView entA, ShapeComponent shapeA, Vector2 posA,
                                       EntityView entB, ShapeComponent shapeB, Vector2 posB,
                                       CollisionBufferComponent buffer)
        {
            bool collision = false;
            Vector2 normal = Vector2.Zero;
            float penetration = 0;

            if (shapeA.sides <= 0 && shapeB.sides <= 0)
            {
                collision = CheckCircleVsCircle(posA, shapeA.size.X * 0.5f, posB, shapeB.size.X * 0.5f, out normal, out penetration);
            }
            else if (shapeA.sides <= 0)
            {
                Vector2[] ptsB = GetShapeVertices(posB, shapeB.size, shapeB.rotation, shapeB.sides);
                collision = CheckPolygonVsCircle(ptsB, posB, posA, shapeA.size.X * 0.5f, out normal, out penetration);
                normal = -normal; // Ensure normal points A -> B
            }
            else if (shapeB.sides <= 0)
            {
                Vector2[] ptsA = GetShapeVertices(posA, shapeA.size, shapeA.rotation, shapeA.sides);
                collision = CheckPolygonVsCircle(ptsA, posA, posB, shapeB.size.X * 0.5f, out normal, out penetration);
            }
            else
            {
                Vector2[] ptsA = GetShapeVertices(posA, shapeA.size, shapeA.rotation, shapeA.sides);
                Vector2[] ptsB = GetShapeVertices(posB, shapeB.size, shapeB.rotation, shapeB.sides);
                collision = CheckPolygonsSAT(ptsA, ptsB, out normal, out penetration);
            }

            if (collision)
            {
                // Ensure normal points from B to A
                Vector2 d = posA - posB;
                if (Vector2.Dot(d, normal) < 0) normal = -normal;

                buffer.Manifolds.Add(new CollisionManifold
                {
                    EntityA = entA.Entity,
                    EntityB = entB.Entity,
                    Normal = normal,
                    Penetration = penetration
                });
            }
        }

        private void CheckHollowVsSolid(EntityView hollowEnt, ShapeComponent hollowShape, Vector2 hollowPos,
                                                EntityView solidEnt, ShapeComponent solidShape, Vector2 solidPos,
                                                CollisionBufferComponent buffer, bool inverted)
        {
            // --- THE FIX ---
            // If the hollow shape is a circle (sides <= 0), approximate it with 32 sides for the physics walls
            int hollowSides = hollowShape.sides <= 0 ? 32 : hollowShape.sides;

            // 1. Generate outer and inner vertices using hollowSides instead of hollowShape.sides
            Vector2[] outerPts = GetShapeVertices(hollowPos, hollowShape.size, hollowShape.rotation, hollowSides);

            float thickness2 = hollowShape.thickness * 2f;
            Vector2 innerSize = new Vector2(
                MathF.Max(0.1f, hollowShape.size.X - thickness2),
                MathF.Max(0.1f, hollowShape.size.Y - thickness2)
            );
            Vector2[] innerPts = GetShapeVertices(hollowPos, innerSize, hollowShape.rotation, hollowSides);

            bool isSolidCircle = solidShape.sides <= 0;
            Vector2[] solidPts = null;
            float solidRadius = 0f;

            if (isSolidCircle)
                solidRadius = solidShape.size.X * 0.5f;
            else
                solidPts = GetShapeVertices(solidPos, solidShape.size, solidShape.rotation, solidShape.sides);

            // 2. Loop through each wall segment
            for (int i = 0; i < outerPts.Length; i++)
            {
                int j = (i + 1) % outerPts.Length;

                // Build the 4-point convex wall piece
                Vector2[] wallPts = new Vector2[]
                {
                    outerPts[i],
                    outerPts[j],
                    innerPts[j],
                    innerPts[i]
                };

                bool collision = false;
                Vector2 normal = Vector2.Zero;
                float penetration = 0;

                if (isSolidCircle)
                {
                    collision = CheckPolygonVsCircle(wallPts, hollowPos, solidPos, solidRadius, out normal, out penetration);
                }
                else
                {
                    collision = CheckPolygonsSAT(wallPts, solidPts, out normal, out penetration);
                }

                if (collision)
                {
                    // Calculate the geometric center of this specific wall piece
                    Vector2 wallCenter = (outerPts[i] + outerPts[j] + innerPts[j] + innerPts[i]) * 0.25f;

                    // Set up Entity A and B targets
                    EntityView entA = inverted ? solidEnt : hollowEnt;
                    EntityView entB = inverted ? hollowEnt : solidEnt;

                    // Standardize normal relative to the WALL piece, not the global hollow center
                    Vector2 realPosA = inverted ? solidPos : wallCenter;
                    Vector2 realPosB = inverted ? wallCenter : solidPos;

                    Vector2 d = realPosA - realPosB;
                    if (Vector2.Dot(d, normal) < 0) normal = -normal;

                    buffer.Manifolds.Add(new CollisionManifold
                    {
                        EntityA = entA.Entity,
                        EntityB = entB.Entity,
                        Normal = normal,
                        Penetration = penetration,
                        IsInnerBoundary = false
                    });
                }
            }
        }


        // --- MATHEMATICAL VERTEX GENERATION (Mirrors Shader Exactly) ---

        private Vector2[] GetShapeVertices(Vector2 pos, Vector2 size, float rotation, int sides)
        {
            if (sides == 4) return GetCorners(pos, size, rotation); // Perfect Rect Bypass

            float minY = 1.0f, maxY = -1.0f, maxX = 0.0f;
            Vector2[] pts = new Vector2[sides];

            // 1. Math Boundaries
            for (int i = 0; i < sides; i++)
            {
                float a = i * (2.0f * MathF.PI / sides);
                pts[i] = new Vector2(MathF.Sin(a), MathF.Cos(a));
                minY = MathF.Min(minY, pts[i].Y);
                maxY = MathF.Max(maxY, pts[i].Y);
                maxX = MathF.Max(maxX, MathF.Abs(pts[i].X));
            }

            // 2. Exact scale matching shader's [-0.5, 0.5] quad fit
            float wScale = size.X / (maxX * 2.0f);
            float hScale = size.Y / (maxY - minY);
            float yOff = -(maxY + minY) / 2.0f;

            float cos = MathF.Cos(rotation);
            float sin = MathF.Sin(rotation);

            // 3. Transform to World Space
            for (int i = 0; i < sides; i++)
            {
                float localX = pts[i].X * wScale;
                float localY = (pts[i].Y + yOff) * hScale;

                pts[i].X = pos.X + (localX * cos - localY * sin);
                pts[i].Y = pos.Y + (localX * sin + localY * cos);
            }

            return pts;
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

        // --- CORE ALGORITHMS ---

        private bool CheckCircleVsCircle(Vector2 posA, float radA, Vector2 posB, float radB, out Vector2 normal, out float penetration)
        {
            normal = Vector2.Zero;
            penetration = 0;

            Vector2 delta = posA - posB;
            float distanceSq = delta.LengthSquared;
            float radiusSum = radA + radB;

            if (distanceSq <= radiusSum * radiusSum)
            {
                float distance = MathF.Sqrt(distanceSq);
                normal = (distance != 0) ? delta / distance : new Vector2(1, 0);
                penetration = radiusSum - distance;
                return true;
            }
            return false;
        }

        private bool CheckPolygonVsCircle(Vector2[] polyPts, Vector2 polyPos, Vector2 circlePos, float radius, out Vector2 normal, out float penetration)
        {
            penetration = float.MaxValue;
            normal = Vector2.Zero;

            // 1. Edge Normals
            for (int i = 0; i < polyPts.Length; i++)
            {
                int j = (i + 1) % polyPts.Length;
                Vector2 edge = polyPts[j] - polyPts[i];
                Vector2 axis = new Vector2(-edge.Y, edge.X).Normalized();

                Project(polyPts, axis, out float minA, out float maxA);

                float cProj = Vector2.Dot(circlePos, axis);
                float minB = cProj - radius;
                float maxB = cProj + radius;

                float overlap = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);
                if (overlap <= 0) return false;

                if (overlap < penetration)
                {
                    penetration = overlap;
                    normal = axis;
                }
            }

            // 2. Closest Vertex Normal
            Vector2 closestVertex = polyPts[0];
            float minDistSq = (circlePos - polyPts[0]).LengthSquared;
            for (int i = 1; i < polyPts.Length; i++)
            {
                float distSq = (circlePos - polyPts[i]).LengthSquared;
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closestVertex = polyPts[i];
                }
            }

            Vector2 axisToVertex = circlePos - closestVertex;
            if (axisToVertex != Vector2.Zero)
            {
                axisToVertex.Normalize();
                Project(polyPts, axisToVertex, out float minA, out float maxA);

                float cProj = Vector2.Dot(circlePos, axisToVertex);
                float minB = cProj - radius;
                float maxB = cProj + radius;

                float overlap = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);
                if (overlap <= 0) return false;

                if (overlap < penetration)
                {
                    penetration = overlap;
                    normal = axisToVertex;
                }
            }

            return true;
        }

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

                    Vector2 axis = new Vector2(-edge.Y, edge.X);
                    if (axis != Vector2.Zero) axis.Normalize();

                    Project(cornersA, axis, out float minA, out float maxA);
                    Project(cornersB, axis, out float minB, out float maxB);

                    float overlap = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);
                    if (overlap <= 0) return false;

                    if (overlap < penetration)
                    {
                        penetration = overlap;
                        normal = axis;
                    }
                }
            }
            return true;
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

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime) { }
    }
}