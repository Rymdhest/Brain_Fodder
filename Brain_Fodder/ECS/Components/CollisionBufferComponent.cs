using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct PotentialPair
    {
        public Entity EntityA;
        public Entity EntityB;

        public PotentialPair(Entity entityA, Entity entityB)
        {
            EntityA = entityA;
            EntityB = entityB;
        }
    }

    public struct CollisionManifold
    {
        public Entity EntityA;
        public Entity EntityB;
        public Vector2 Normal;      // Direction to push EntityA
        public float Penetration;   // Distance of overlap
    }

    public struct CollisionBufferComponent : IComponent
    {
        public List<PotentialPair> Pairs = new List<PotentialPair>(1024);

        public List<CollisionManifold> Manifolds = new List<CollisionManifold>(1024);

        public CollisionBufferComponent()
        {
        }

        public void Clear()
        {
            Pairs.Clear();
            Manifolds.Clear();
        }

    }
}
