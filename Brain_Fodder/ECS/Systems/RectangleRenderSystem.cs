using Brain_Fodder.Rendering;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Systems
{
    public class RectangleRenderSystem : SystemBase
    {
        public RectangleRenderSystem()
            : base(new BitMask(typeof(RectangleComponent), typeof(PositionComponent), typeof(ColourComponent)))
        {
        }


        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            Vector2 position = entity.Get<PositionComponent>().value;
            Vector2 size = entity.Get<RectangleComponent>().size;
            float rotation = entity.Get<RectangleComponent>().rotation;
            Vector3 color = entity.Get<ColourComponent>().colour;


            RectangleRenderCommand command = new RectangleRenderCommand();
            command.position = position;
            command.size = size;
            command.color = color;
            command.rotation = rotation;
            MasterRenderer.rectangles.Add(command);


        }
    }
}
