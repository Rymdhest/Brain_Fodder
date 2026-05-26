using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct ColourComponent : IComponent
    {
        public Vector3 colour;
        public Vector3 innerColour;

        public ColourComponent(Vector3 colour)
        {
            this.colour = colour;
            this.innerColour = colour;
        }
        public ColourComponent(Vector3 borderColor,Vector3 fillColor)
        {
            this.colour = borderColor;
            this.innerColour = fillColor;
        }
    }
}
