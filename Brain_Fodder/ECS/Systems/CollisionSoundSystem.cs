using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using System;

namespace Dino_Engine.ECS.Systems
{
    public class CollisionSoundSystem : SystemBase
    {
        public CollisionSoundSystem() : base(new BitMask())
        {
            Priority = 7;
        }

        public override void Update(ECSWorld world, float deltaTime)
        {
            var buffer = world.GetComponent<CollisionBufferComponent>(world.GetSingleton<CollisionBufferComponent>());

            foreach (var manifold in buffer.Manifolds)
            {
                proccessEntity(world.GetEntityView(manifold.EntityA));
                proccessEntity(world.GetEntityView(manifold.EntityB));

            }
        }

        private void proccessEntity(EntityView entity)
        {
            if (entity.Has<CollisionSoundTag>())
            {
                SoundManager.Play(SoundManager.GenerateSound());
            }
           
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase as we use the Singleton Buffer
        }
    }
}