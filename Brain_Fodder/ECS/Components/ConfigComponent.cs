using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct ConfigComponent : IComponent
    {
        public float MinGameTime = 15;
        public float MaxGameTime = 30;
        public float CelebrationDuration = 3.2f;

        public ConfigComponent()
        {

        }

    }
}
