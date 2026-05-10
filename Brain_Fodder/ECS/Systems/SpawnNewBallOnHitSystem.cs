using Brain_Fodder;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using SpaceEngine.RenderEngine;
using SpaceEngine.Util;
using System;

namespace Dino_Engine.ECS.Systems
{
    public class SpawnNewBallOnHitSystem : SystemBase
    {
        public SpawnNewBallOnHitSystem() : base(new BitMask())
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
            if (!entityA.Has<SpawnNewBallOnHitComponent>()) return;
            if (!entityA.Has<CircleComponent>()) return;
            if (!entityB.Has<TriangleComponent>()) return;
            if (Engine.Instance.ecsWorld.GetEntityView(Engine.Instance.ecsWorld.GetSingleton<GameStateComponent>()).Get<GameStateComponent>().IsVictory) return;

            Engine.Instance.ecsWorld.spawnBall();

        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase as we use the Singleton Buffer
        }
    }
}