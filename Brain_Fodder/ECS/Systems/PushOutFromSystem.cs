
using Brain_Fodder;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using SpaceEngine.RenderEngine;
using System;

namespace Dino_Engine.ECS.Systems
{
    public class PushOutFromSystem : SystemBase
    {
        public PushOutFromSystem() : base(new BitMask())
        {
            Priority = 11;
        }

        public override void Update(ECSWorld world, float deltaTime)
        {
            var buffer = world.GetComponent<CollisionBufferComponent>(world.GetSingleton<CollisionBufferComponent>());

            foreach (var manifold in buffer.Manifolds)
            {
                proccessEntity(world.GetEntityView(manifold.EntityA), manifold);
                proccessEntity(world.GetEntityView(manifold.EntityB), manifold);
            }
        }

        private void proccessEntity(EntityView entity, CollisionManifold manifold)
        {
            if (!entity.Has<PushOutFromOnCollision>()) return;
            PushOutFromOnCollision pushComponent = entity.Get<PushOutFromOnCollision>();

            var position = entity.Get<PositionComponent>();

            Vector2 start = position.value;
            var end = start+ (position.value - pushComponent.center).Normalized() * 50f;
            AnimationComponent animation = entity.Get<AnimationComponent>();

            animation.start = start;
            animation.goal = end;
            animation.t = 0f;
            animation.duration= 0.3f;

            entity.Set(animation);

            Console.WriteLine($"Animation t: {animation.t}, position: {position.value}");
            ColourComponent colourComponent = entity.Get<ColourComponent>();
            colourComponent.colour.Y += 0.2f;
            entity.Set(colourComponent);
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase as we use the Singleton Buffer
        }
    }
}