using Brain_Fodder.Rendering;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Systems
{
    public class SpinSystem : SystemBase
    {
        public SpinSystem()
            : base(new BitMask(typeof(ShapeComponent), typeof(SpinComponent)))
        {
        }


        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            Vector2 size = entity.Get<ShapeComponent>().size;
            var shape = entity.Get<ShapeComponent>();
            float spin = entity.Get<SpinComponent>().spin;
            
            shape.rotation += deltaTime * spin;
            entity.Set(shape);


        }
    }
}
