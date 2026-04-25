using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct SizeChangerComponent : IComponent
    {
        public float change;

        public SizeChangerComponent(float change)
        {
            this.change = change;
        }
    }
}
