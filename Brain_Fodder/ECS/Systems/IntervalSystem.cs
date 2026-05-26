
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
    public class IntervalSystem : SystemBase
    {
        public IntervalSystem()
            : base(new BitMask(typeof(IntervalActionComponent)))
        {
        }
        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            if (world.GetEntityView(world.GetSingleton<GameStateComponent>()).Get<GameStateComponent>().IsVictory) return;
            IntervalActionComponent spawner = entity.Get<IntervalActionComponent>();
            if (spawner.lastSpawnTime >= spawner.SpawnInterval)
            {
                // Spawn logic here
                entity.Get<IntervalActionComponent>().Action?.Invoke(world);
                spawner = entity.Get<IntervalActionComponent>();
                spawner.lastSpawnTime = 0;
            }
            else
            {
                spawner.lastSpawnTime += deltaTime;
            }
            entity.Set(spawner);
        }

    }
}
