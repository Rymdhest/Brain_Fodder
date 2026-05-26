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

            foreach (var collisionEvent in buffer.EnterEvents)
            {
                proccessEntity(world.GetEntityView(collisionEvent.EntityA), world.GetEntityView(collisionEvent.EntityB));
                proccessEntity(world.GetEntityView(collisionEvent.EntityB), world.GetEntityView(collisionEvent.EntityA));
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