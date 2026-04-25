using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct SpawnerComponent : IComponent
    {

        public float SpawnInterval = 1.0f;
        public float lastSpawnTime = 99999.0f;

        public SpawnerComponent(float SpawnInterval)
        {
            this.SpawnInterval = SpawnInterval;
        }

    }
}
