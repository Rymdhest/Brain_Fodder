using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct MassComponent : IComponent
    {
        public float mass;

        public MassComponent(float mass)
        {
            this.mass = mass;
        }
    }

    
}
