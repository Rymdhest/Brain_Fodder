
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino_Engine.ECS.Systems
{
    public class BroadphaseCollisionSystem : SystemBase
    {
        public BroadphaseCollisionSystem()
            : base(new BitMask(typeof(PositionComponent), typeof(collidableTag)))
        {
        }


        public override void Update(ECSWorld world, float deltaTime)
        {
            // 1. Grab your Singleton Buffer
            CollisionBufferComponent buffer = world.GetComponent<CollisionBufferComponent>(world.GetSingleton<CollisionBufferComponent>());
            // 2. Clear previous frame's pairs
            buffer.Clear();

            // 3. Get all entities that can collide
            var archetypes = world.QueryArchetypes(WithMask, WithoutMask);
            var allCollidables = new List<EntityView>();

            foreach (var arch in archetypes)
            {
                foreach (var entity in new ComponentAccessor(arch))
                {
                    allCollidables.Add(entity);
                }
            }

            // 4. Brute-force double loop
            for (int i = 0; i < allCollidables.Count; i++)
            {
                for (int j = i + 1; j < allCollidables.Count; j++)
                {
                    var entityA = allCollidables[i];
                    var entityB = allCollidables[j];

                    // OPTIMIZATION: Skip if both are static (walls hitting walls)
                    bool isAStatic = entityA.Get<PhysicsComponent>().InvMass == 0;
                    bool isBStatic = entityB.Get<PhysicsComponent>().InvMass == 0;
                    if (isAStatic && isBStatic) continue;

                    // Forward the pair to the Narrowphase
                    buffer.Pairs.Add(new PotentialPair(entityA.Entity, entityB.Entity));
                }
            }
        }

        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}
