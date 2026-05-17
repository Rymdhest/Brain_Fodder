using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;

namespace Dino_Engine.ECS.Components
{
    public struct CollisionPair
    {
        public Entity EntityA;
        public Entity EntityB;

        public CollisionPair(Entity entityA, Entity entityB)
        {
            EntityA = entityA;
            EntityB = entityB;
        }
    }

    public struct CollisionManifold
    {
        public Entity EntityA;
        public Entity EntityB;
        public Vector2 Normal;      
        public float Penetration;
        public float Impulse;
    }

    public struct CollisionBufferComponent : IComponent
    {
        public List<CollisionPair> PotentialPairs = new List<CollisionPair>(1024);

        public List<CollisionManifold> Manifolds = new List<CollisionManifold>(1024);

        // Persistent history state
        public HashSet<(Entity, Entity)> PreviousCollisions = new HashSet<(Entity, Entity)>();
        // NEW: Moved from system class into the component to keep systems stateless
        public HashSet<(Entity, Entity)> CurrentCollisions = new HashSet<(Entity, Entity)>(1024);

        public List<CollisionPair> EnterEvents = new List<CollisionPair>(256);
        public List<CollisionPair> StayEvents = new List<CollisionPair>(256);
        public List<CollisionPair> ExitEvents = new List<CollisionPair>(256);
        public CollisionBufferComponent()
        {
        }
        public static (Entity, Entity) GetPairKey(Entity a, Entity b)
        {
            return a.Id < b.Id ? (a, b) : (b, a);
        }
        public void Clear()
        {
            PotentialPairs.Clear();
            Manifolds.Clear();
            EnterEvents.Clear();
            StayEvents.Clear();
            ExitEvents.Clear();
            CurrentCollisions.Clear();
        }

    }
}
