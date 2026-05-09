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
            if (!entity.Has<CollisionSoundComponent>()) return;
            CollisionSoundComponent soundComponent = entity.Get<CollisionSoundComponent>();
            if (WindowHandler.getTotalTime() - soundComponent.timeLastPlayed < soundComponent.cooldownSeconds) return;
            if (manifold.Impulse < soundComponent.minImpulse) return;
            if (Engine.Instance.ecsWorld.GetEntityView(Engine.Instance.ecsWorld.GetSingleton<GameStateComponent>()).Get<GameStateComponent>().IsVictory) return;
            //Console.WriteLine($"Playing collision sound with impulse {manifold.Impulse}");


            SoundManager.OnBallBounce();

            //SoundManager.Play(SoundManager.GenerateSound((int)(manifold.Impulse/200)));

            //SoundManager.Play(SoundManager.GenerateSound(soundComponent.note));
            //SoundManager.Play(SoundManager.GenerateSound(40));

            //SoundManager.Play(SoundManager.GenerateCelebrationSound());


            entity.Set(new CollisionSoundComponent
            {
                timeLastPlayed = WindowHandler.getTotalTime(),
                cooldownSeconds = entity.Get<CollisionSoundComponent>().cooldownSeconds,
                note = entity.Get<CollisionSoundComponent>().note
            });
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase as we use the Singleton Buffer
        }
    }
}