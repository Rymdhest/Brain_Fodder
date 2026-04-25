
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino_Engine.ECS.Systems
{
    public class SizeChangerSystem : SystemBase
    {
        public SizeChangerSystem()
            : base(new BitMask(typeof(SizeChangerComponent)))
        {
        }
        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            if (entity.Has<RingComponent>())
            {
                var ring = entity.Get<RingComponent>();
                ring.radius += entity.Get<SizeChangerComponent>().change * deltaTime;
                entity.Set(ring);
            }
            if (entity.Has<CircleComponent>())
            {
                var circle = entity.Get<CircleComponent>();
                circle.radius += entity.Get<SizeChangerComponent>().change * deltaTime;
                entity.Set(circle);
            }
        }
    }
}
