using Brain_Fodder.Rendering;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using SpaceEngine.RenderEngine;

namespace Dino_Engine.ECS.Systems
{
    public class ScoreSystem : SystemBase
    {
        public ScoreSystem()
            : base(new BitMask())
        {
        }

        public override void Update(ECSWorld world, float deltaTime)
        {
            var buffer = world.GetComponent<CollisionBufferComponent>(world.GetSingleton<CollisionBufferComponent>());

            foreach (var collisionEvent in buffer.EnterEvents)
            {
                bool isGoal = false;
                if (IsGoal(world.GetEntityView(collisionEvent.EntityA), world.GetEntityView(collisionEvent.EntityB))
                || IsGoal(world.GetEntityView(collisionEvent.EntityB), world.GetEntityView(collisionEvent.EntityA)))
                {
                    isGoal = true;
                }
                if (isGoal)
                {
                    Console.WriteLine("GOAL! " + collisionEvent.EntityA + " " + collisionEvent.EntityB);
                    var gameStateComponent = world.GetComponent<GameStateComponent>( world.GetSingleton<GameStateComponent>());
                    gameStateComponent.score += 1;
                    world.GetEntityView(world.GetSingleton<GameStateComponent>()).Set<GameStateComponent>(gameStateComponent);
                }
            }
        }

        private bool IsGoal(EntityView entityA, EntityView entityB)
        {
            if (!entityA.Has<ScorerTag>()) return false;
            if (!entityB.Has<GoalTag>()) return false;
            return true;
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase as we use the Singleton Buffer
        }
    }
}
