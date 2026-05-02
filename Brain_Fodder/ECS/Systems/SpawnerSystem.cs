
using Brain_Fodder;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using SpaceEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino_Engine.ECS.Systems
{
    public class SpawnerSystem : SystemBase
    {
        public SpawnerSystem()
            : base(new BitMask(typeof(SpawnerComponent), typeof(PositionComponent)))
        {
        }
        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            SpawnerComponent spawner = entity.Get<SpawnerComponent>();
            if (spawner.lastSpawnTime >= spawner.SpawnInterval)
            {
                // Spawn logic here
                spawner.lastSpawnTime = 0;
                SpawnEntity(Engine.Instance.ecsWorld, spawner, entity.Get<PositionComponent>());
            }
            else
            {
                spawner.lastSpawnTime += deltaTime;
            }
            entity.Set(spawner);
        }

        private void SpawnEntity(ECSWorld world, SpawnerComponent spawner, PositionComponent position)
        {
            if (world.GetEntityView(world.GetSingleton<GameStateComponent>()).Get<GameStateComponent>().IsVictory ) {
                return;
            }

            // Create a new entity
            /*
            Entity newEntity = world.CreateEntity(
                    new PositionComponent(position.value+new Vector2(MyMath.rngMinusPlus()*100, 0)),
                    new RectangleComponent(new Vector2(100 + MyMath.rng() * 300, 10), MyMath.rngMinusPlus()*0.1f),
                    new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                    new ColourComponent(new Vector3(1.0f, 0.0f, 0.5f)),
                    new collidableTag(),
                    new PhysicsComponent(0.0f, 1),
                    new VelocityComponent(new Vector2(0f, 100.0f)),
                    new KillableTag()
                );
            */
            
            Entity newEntity = world.CreateEntity(
                new PositionComponent(position.value),
                new RingComponent(1080/4f, 10),
                new ColourComponent(new Vector3(1.0f, 0.0f, 0.5f)),
                new collidableTag(),
                new PhysicsComponent(0.0f, 1.05f),
                new VelocityComponent(new Vector2(0f, 0f)),
                new SizeChangerComponent(-60),
                new KillableTag()
            );
            
        }
    }
}
