using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct SpinComponent : IComponent
    {
        public float spin;

        public SpinComponent(float spin)
        {
            this.spin = spin;
        }

    }
}
