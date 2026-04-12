
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino_Engine.ECS.Systems
{
    public class CollisionSystem : SystemBase
    {
        public CollisionSystem()
            : base(new BitMask(typeof(PositionComponent), typeof(VelocityComponent), typeof(CircleComponent), typeof(collidableTag)))
        {
        }

        private bool CollisionCheck(EntityView entityA, EntityView entityB, out float insideDist)
        {
            var positionA = entityA.Get<PositionComponent>().value;
            var positionB = entityB.Get<PositionComponent>().value;
            var circleA = entityA.Get<CircleComponent>().radius;
            var circleB = entityB.Get<CircleComponent>().radius;
            float dx = positionA.X - positionB.X;
            float dy = positionA.Y - positionB.Y;
            float distance = MathF.Sqrt( dx * dx + dy * dy);
            float radiusSum = circleA + circleB;
            insideDist = radiusSum-distance;
            return distance <= radiusSum;
        }

        private Vector2 ReflectCircleCollision(Vector2 velocity, Vector2 CircleA, Vector2 CircleB)
        {
            // Normal vector at the point of collision
            float Nx = CircleA.X - CircleB.X;
            float Ny = CircleA.Y - CircleB.Y;

            // Normalize the normal vector
            float norm = MathF.Sqrt(Nx * Nx + Ny * Ny);
            Nx /= norm;
            Ny /= norm;

            // Velocity vector
            float dotProduct = velocity.X * Nx + velocity.Y * Ny;

            // Reflected velocity
            float Vnew_x = velocity.X - 2 * dotProduct * Nx;
            float Vnew_y = velocity.Y - 2 * dotProduct * Ny;

            return new Vector2(Vnew_x, Vnew_y);
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            foreach (var archetype in world.QueryArchetypes(WithMask, WithoutMask))
            {
                var accessor = new ComponentAccessor(archetype);

                foreach (var entityOther in accessor)
                {
                    if (entityOther.Equals(entity)) continue;

                    if (CollisionCheck(entity, entityOther, out float insideDist))
                    {
                        Console.WriteLine($"Collision detected between Entity {entity.Entity} and Entity {entityOther.Entity}");
                        Vector2 velocityOld = entity.Get<VelocityComponent>().value;

                        Vector2 circleA = entity.Get<PositionComponent>().value;
                        Vector2 circleB = entityOther.Get<PositionComponent>().value;

                        entity.Set<VelocityComponent>(new VelocityComponent { value = ReflectCircleCollision(velocityOld, circleA, circleB) });

                        Vector2 velocity = entity.Get<VelocityComponent>().value;

                        entity.Set<PositionComponent>(new PositionComponent { value = entity.Get<PositionComponent>().value + velocity.Normalized() * (insideDist) });
                    }
                }
            }
        }
    }
}
