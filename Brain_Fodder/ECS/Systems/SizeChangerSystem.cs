
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
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
            if (entity.Has<ShapeComponent>())
            {
                var shape = entity.Get<ShapeComponent>();
                shape.size += new Vector2( entity.Get<SizeChangerComponent>().change * deltaTime);
                entity.Set(shape);
            }
        }
    }
}
