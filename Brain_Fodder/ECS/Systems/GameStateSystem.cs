
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino_Engine.ECS.Systems
{
    public class GameStateSystem : SystemBase
    {
        public GameStateSystem()
            : base(new BitMask(typeof(GameStateComponent)))
        {
        }
        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            var gameState = world.GetEntityView(world.GetSingleton<GameStateComponent>()).Get<GameStateComponent>();
            var gameConfig = world.GetEntityView( world.GetSingleton<ConfigComponent>()).Get< ConfigComponent>();

            gameState.LevelTime += deltaTime;

            if (gameState.IsVictory)
            {
                gameState.VictoryTime += deltaTime;
            }

            if (gameState.score >= gameState.scoreToWin && !gameState.IsVictory)
            {
                gameState.IsVictory = true;
                gameState.VictoryTime = 0f;
                SoundManager.Play(SoundManager.GenerateCelebrationSound());
            }

            if (gameState.VictoryTime > gameConfig.CelebrationDuration)
            {
                if (gameState.LevelTime > gameConfig.MinGameTime && gameState.LevelTime < gameConfig.MaxGameTime)
                {
                    gameState.shouldSaveVideo = true;
                    gameState.shouldClose = true;
                } else
                {
                    gameState.shouldReset = true;
                }
            }

            if (gameState.score >= gameState.scoreToWin && !gameState.IsVictory)
            {
                gameState.IsVictory = true;
                gameState.VictoryTime = 0f;
            }
            world.GetEntityView(world.GetSingleton<GameStateComponent>()).Set<GameStateComponent>(gameState);

        }
    }
}
