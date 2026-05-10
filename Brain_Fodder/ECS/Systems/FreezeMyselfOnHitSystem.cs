using Brain_Fodder;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using SpaceEngine.RenderEngine;
using System;

namespace Dino_Engine.ECS.Systems
{
    public class FreezeMyselfOnHitSystem : SystemBase
    {
        public FreezeMyselfOnHitSystem() : base(new BitMask())
        {
            Priority = 10;
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
            if (!entityA.Has<FreezeMyselfOnHitComponent>()) return;
            if (!entityA.Has<CircleComponent>()) return;
            if (!entityB.Has<TriangleComponent>()) return;
            if (Engine.Instance.ecsWorld.GetEntityView(Engine.Instance.ecsWorld.GetSingleton<GameStateComponent>()).Get<GameStateComponent>().IsVictory) return;

            var physicsComponent = entityA.Get<PhysicsComponent>();
            physicsComponent.InvMass = 0.0f;
            entityA.Set(physicsComponent);

            var velocity = entityA.Get<VelocityComponent>();
            velocity.value = new Vector2(0);
            entityA.Set(velocity);

            var color = entityA.Get<ColourComponent>();
            color.colour = new Vector3(0.6f, 0.6f, 0.6f);
            entityA.Set(color);
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase as we use the Singleton Buffer
        }
    }
}