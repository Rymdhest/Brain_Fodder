using Brain_Fodder;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using SpaceEngine.RenderEngine;
using System;
using static System.Collections.Specialized.BitVector32;

namespace Dino_Engine.ECS.Systems
{
    public class OnHitSystem : SystemBase
    {
        public OnHitSystem() : base(new BitMask())
        {
            Priority = 11;
        }

        public override void Update(ECSWorld world, float deltaTime)
        {
            if (world.GetEntityView(world.GetSingleton<GameStateComponent>()).Get<GameStateComponent>().IsVictory) return;
            var buffer = world.GetComponent<CollisionBufferComponent>(world.GetSingleton<CollisionBufferComponent>());

            foreach (var collisionEvent in buffer.EnterEvents)
            {
                ExecuteReaction(world.GetEntityView(collisionEvent.EntityA), world.GetEntityView(collisionEvent.EntityB), world);
                ExecuteReaction(world.GetEntityView(collisionEvent.EntityB), world.GetEntityView(collisionEvent.EntityA), world);
            }
        }

        private void ExecuteReaction(EntityView entityA, EntityView entityB, ECSWorld world)
        {
            if (!entityA.Has<OnHitComponent>()) return;

            Console.WriteLine(entityA.Entity.Id + " hit " + entityB.Entity.Id + " at frame " + Engine.Instance.frameCount);
            entityA.Get<OnHitComponent>().Action?.Invoke(entityA, entityB, world);
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase as we use the Singleton Buffer
        }
    }
}