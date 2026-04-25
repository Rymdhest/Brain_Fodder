using Brain_Fodder;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using SpaceEngine.RenderEngine;
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

        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase as we use the Singleton Buffer
        }
    }
}