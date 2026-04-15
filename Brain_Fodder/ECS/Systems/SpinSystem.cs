using Brain_Fodder.Rendering;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Systems
{
    public class SpinSystem : SystemBase
    {
        public SpinSystem()
            : base(new BitMask(typeof(RectangleComponent), typeof(SpinComponent)))
        {
        }


        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            Vector2 size = entity.Get<RectangleComponent>().size;
            float rotation = entity.Get<RectangleComponent>().rotation;
            float spin = entity.Get<SpinComponent>().spin;

            entity.Set<RectangleComponent>(new RectangleComponent(size,rotation+deltaTime* spin));


        }
    }
}
