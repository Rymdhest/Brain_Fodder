using Brain_Fodder;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using SpaceEngine.RenderEngine;
using SpaceEngine.Util;
using System;

namespace Dino_Engine.ECS.Systems
{
    public class KillerSystem : SystemBase
    {
        public KillerSystem() : base(new BitMask())
        {
            Priority = 11;
        }

        public override void Update(ECSWorld world, float deltaTime)
        {
            var buffer = world.GetComponent<CollisionBufferComponent>(world.GetSingleton<CollisionBufferComponent>());

            foreach (var manifold in buffer.Manifolds)
            {
                proccessEntity(world.GetEntityView(manifold.EntityA), world.GetEntityView(manifold.EntityB));
                proccessEntity(world.GetEntityView(manifold.EntityB), world.GetEntityView(manifold.EntityA));
            }
        }

        private void proccessEntity(EntityView entityA, EntityView entityB)
        {
            if (!(entityA.Has<KillableTag>() && entityB.Has<KillerTag>())) return;

            Engine.Instance.ecsWorld.DestroyEntity(entityA.Entity);

            var world = Engine.Instance.ecsWorld;
            Vector2 center = Engine.Instance.outerResolution / 2;


            for (int i = 0; i<2; i++)
            {
                Entity newEntity = world.CreateEntity(
                    new PositionComponent(center),
                    new CircleComponent(10),
                    new ColourComponent(new Vector3(1.0f, 1.0f, 1.0f)),
                    new collidableTag(),
                    new PhysicsComponent(1.0f, 1.05f),
                    new VelocityComponent(MyMath.rng2DMinusPlus().Normalized()*200f),
                    new GravityComponent(0f),
                    new KillableTag()
                );
            }

            var circle = entityB.Get<CircleComponent>();
            circle.radius += 0.50f;

            entityB.Set(circle);

        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase as we use the Singleton Buffer
        }
    }
}