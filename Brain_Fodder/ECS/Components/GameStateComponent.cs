using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct GameStateComponent : IComponent
    {
        public int score = 0;
        public int scoreToWin = 1;
        public float LevelTime = 0f;
        public Boolean IsVictory = false;
        public float VictoryTime = 0f;
        public Boolean shouldReset = false;
        public Boolean shouldClose = false;
        public Boolean shouldSaveVideo = false;

        public GameStateComponent()
        {

        }

    }
}
