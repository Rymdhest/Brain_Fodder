using Brain_Fodder.Rendering;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Systems
{
    public class TriangleRenderSystem : SystemBase
    {
        public TriangleRenderSystem()
            : base(new BitMask(typeof(TriangleComponent), typeof(PositionComponent), typeof(ColourComponent)))
        {
        }


        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            Vector2 position = entity.Get<PositionComponent>().value;
            Vector2 size = entity.Get<TriangleComponent>().size;
            float rotation = entity.Get<TriangleComponent>().rotation;
            Vector3 color = entity.Get<ColourComponent>().colour;


            TriangleRenderCommand command = new TriangleRenderCommand();
            command.position = position;
            command.size = size;
            command.color = color;
            command.rotation = rotation;
            MasterRenderer.triangles.Add(command);


        }
    }
}
