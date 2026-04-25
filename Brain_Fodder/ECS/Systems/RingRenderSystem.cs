using Brain_Fodder.Rendering;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Systems
{
    public class RingRenderSystem : SystemBase
    {
        public RingRenderSystem()
            : base(new BitMask(typeof(RingComponent), typeof(PositionComponent), typeof(ColourComponent)))
        {
        }


        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            Vector2 position = entity.Get<PositionComponent>().value;
            float radius = entity.Get<RingComponent>().radius;
            float width = entity.Get<RingComponent>().width;
            Vector3 color = entity.Get<ColourComponent>().colour;

            RingRenderCommand command = new RingRenderCommand();
            command.position = position;
            command.radius = radius;
            command.width = width;
            command.color = color;
            MasterRenderer.rings.Add(command);


        }
    }
}
