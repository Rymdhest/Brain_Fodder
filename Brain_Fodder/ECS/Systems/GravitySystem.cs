
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino_Engine.ECS.Systems
{
    public class GravitySystem : SystemBase
    {
        public GravitySystem()
            : base(new BitMask(typeof(GravityTag), typeof(VelocityComponent)))
        {
        }
        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            var vel = entity.Get<VelocityComponent>();

            vel.value -= new OpenTK.Mathematics.Vector2(0, 280) * deltaTime;
            entity.Set(vel);
        }
    }
}
