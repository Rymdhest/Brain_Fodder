using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct ColourComponent : IComponent
    {
        public Vector3 colour;

        public ColourComponent(Vector3 colour)
        {
            this.colour = colour;
        }
    }
}
