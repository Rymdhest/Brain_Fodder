
using Brain_Fodder;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino_Engine.ECS.Systems
{
    public class GravityOnVictorySystem : SystemBase
    {
        public GravityOnVictorySystem()
            : base(new BitMask(typeof(GravityOnVictoryTag), typeof(GravityComponent)))
        {
        }
        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            if (Engine.Instance.ecsWorld.GetEntityView(Engine.Instance.ecsWorld.GetSingleton<GameStateComponent>()).Get<GameStateComponent>().IsVictory)
            {

                var gravity = entity.Get<GravityComponent>();
                gravity.gravity = 400f;
                entity.Set<GravityComponent>(gravity);

            }
        }
    }
}
