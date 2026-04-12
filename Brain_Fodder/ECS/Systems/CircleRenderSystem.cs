using Brain_Fodder.Rendering;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Systems
{
    public class CircleRenderSystem : SystemBase
    {
        public CircleRenderSystem()
            : base(new BitMask(typeof(CircleComponent), typeof(PositionComponent), typeof(ColourComponent)))
        {
        }


        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            Vector2 position = entity.Get<PositionComponent>().value;
            float radius = entity.Get<CircleComponent>().radius;
            Vector3 color = entity.Get<ColourComponent>().colour;

            CircleRenderCommand command = new CircleRenderCommand();
            command.position = position;
            command.radius = radius;
            command.color = color;
            MasterRenderer.circles.Add(command);


        }
    }
}
