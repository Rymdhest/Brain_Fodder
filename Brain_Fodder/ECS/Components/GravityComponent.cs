using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct GravityComponent : IComponent
    {

        public float gravity = 280f;

        public GravityComponent(float gravity)
        {
            this.gravity = gravity;
        }

        public GravityComponent()
        {
            this.gravity = 280f;
        }
    }

    
}
