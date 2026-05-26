using Brain_Fodder.Rendering;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Systems
{
    public class ShapeRenderSystem : SystemBase
    {
        public ShapeRenderSystem()
            : base(new BitMask(typeof(ShapeComponent), typeof(PositionComponent), typeof(ColourComponent)))
        {
        }


        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            Vector2 position = entity.Get<PositionComponent>().value;
            Vector2 size = entity.Get<ShapeComponent>().size;
            float rotation = entity.Get<ShapeComponent>().rotation;
            Vector3 fillcolor = entity.Get<ColourComponent>().innerColour;
            Vector3 borderColor = entity.Get<ColourComponent>().colour;

            float alpha = 0.0f;
            if (entity.Get<ShapeComponent>().filled) alpha = 1.0f;

            ShapeRenderCommand command = new ShapeRenderCommand();
            command.position = position;
            command.size = size;
            command.fillColor = new Vector4(fillcolor.X, fillcolor.Y, fillcolor.Z, alpha);
            command.borderColor = new Vector4(borderColor.X, borderColor.Y, borderColor.Z, 1.0f);
            command.rotation = rotation;
            command.sides = entity.Get<ShapeComponent>().sides;
            command.borderSize = entity.Get<ShapeComponent>().thickness;
            MasterRenderer.shapes.Add(command);


        }
    }
}
