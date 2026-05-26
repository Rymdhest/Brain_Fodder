using Brain_Fodder;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using System;

namespace Dino_Engine.ECS.Systems
{
    public class CollisionEventStateSystem : SystemBase
    {
        public CollisionEventStateSystem() : base(new BitMask())
        {
            Priority = -3;
        }

        public override void Update(ECSWorld world, float deltaTime)
        {
            Entity singletonEntity = world.GetSingleton<CollisionBufferComponent>();
            CollisionBufferComponent buffer = world.GetComponent<CollisionBufferComponent>(singletonEntity);

            // ----------------------------------------------------------------
            // STEP 1: EVALUATE CURRENT FRAME MANIFOLDS (Enters & Stays)
            // ----------------------------------------------------------------
            for (int i = 0; i < buffer.Manifolds.Count; i++)
            {
                var manifold = buffer.Manifolds[i];
                var key = CollisionBufferComponent.GetPairKey(manifold.EntityA, manifold.EntityB);

                // HashSet.Add returns true if the key was actually added.
                // It returns false if the key was already in the set.
                // This perfectly deduplicates multiple manifolds for the same pair!
                if (buffer.CurrentCollisions.Add(key))
                {
                    if (!buffer.PreviousCollisions.Contains(key))
                    {
                        buffer.EnterEvents.Add(new CollisionPair
                        {
                            EntityA = manifold.EntityA,
                            EntityB = manifold.EntityB
                        });
                    }
                    else
                    {
                        buffer.StayEvents.Add(new CollisionPair
                        {
                            EntityA = manifold.EntityA,
                            EntityB = manifold.EntityB
                        });
                    }
                }
            }

            // ----------------------------------------------------------------
            // STEP 2: EVALUATE WHAT SEPARATED THIS FRAME (Exits)
            // ----------------------------------------------------------------
            foreach (var oldPair in buffer.PreviousCollisions)
            {
                if (!buffer.CurrentCollisions.Contains(oldPair))
                {
                    buffer.ExitEvents.Add(new CollisionPair
                    {
                        EntityA = oldPair.Item1, // Assuming key is a Tuple
                        EntityB = oldPair.Item2
                    });
                }
            }

            // ----------------------------------------------------------------
            // STEP 3: FLIP THE HISTORY VALUES
            // ----------------------------------------------------------------
            buffer.PreviousCollisions.Clear();
            foreach (var pair in buffer.CurrentCollisions)
            {
                buffer.PreviousCollisions.Add(pair);
            }

            // Write the completely mutated data back to the world
            world.GetEntityView(singletonEntity).Set<CollisionBufferComponent>(buffer);
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            // Unused in Narrowphase as we use the Singleton Buffer
        }
    }
}