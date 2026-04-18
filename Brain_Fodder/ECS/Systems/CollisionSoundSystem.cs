using Brain_Fodder;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using SpaceEngine.RenderEngine;
using System;

namespace Dino_Engine.ECS.Systems
{
    public class CollisionSoundSystem : SystemBase
    {
        public CollisionSoundSystem() : base(new BitMask())
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
            if (!entity.Has<CollisionSound>()) return;
            CollisionSound soundComponent = entity.Get<CollisionSound>();
            if (WindowHandler.getTotalTime() - soundComponent.timeLastPlayed < soundComponent.cooldownSeconds) return;
            if (manifold.Impulse < soundComponent.minImpulse) return;

            Console.WriteLine($"Playing collision sound with impulse {manifold.Impulse}");



            SoundManager.Play(SoundManager.GenerateSound((int)(manifold.Impulse/100)));
            entity.Set(new CollisionSound
            {
                timeLastPlayed = WindowHandler.getTotalTime(),
                cooldownSeconds = entity.Get<CollisionSound>().cooldownSeconds
            });
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase as we use the Singleton Buffer
        }
    }
}