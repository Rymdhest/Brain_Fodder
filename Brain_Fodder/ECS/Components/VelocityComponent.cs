using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct VelocityComponent : IComponent
    {
        public Vector2 value;

        public VelocityComponent(Vector2 velocity)
        {
            value = velocity;
        }
    }

    
}
