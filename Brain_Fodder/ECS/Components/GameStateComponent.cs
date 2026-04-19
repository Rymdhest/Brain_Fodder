using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct GameStateComponent : IComponent
    {
        public float LevelTime = 0f;
        public Boolean shouldReset = false;
        public Boolean shouldClose = false;

        public GameStateComponent(Vector2 position)
        {

        }

    }
}
