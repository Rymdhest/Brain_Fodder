using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct PositionComponent : IComponent
    {
        public Vector2 value;

        public PositionComponent(Vector2 position)
        {
            value = position;
        }

        public PositionComponent(float x, float y)
        {
            value = new Vector2(x, y);
        }
    }
}
