using Brain_Fodder;
using Dino_Engine.ECS.Components;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using SpaceEngine.Util;

namespace Dino_Engine.ECS.ECS_Architecture
{
    public class ECSWorld
    {
        private IDAllocator<uint> IDManager = new IDAllocator<uint>();
        public readonly CommandBuffer deferredCommands = new();

        private Dictionary<BitMask, Archetype> archetypes = new();
        private Dictionary<uint, (Archetype archetype, int index)> entityLocations = new();
        private Dictionary<Type, Entity> SingletonToEntity = new();
        public Entity Camera;

        public int Count => entityLocations.Count;
        public ECSWorld()
        {
            SpawnLevel();

        }

        public void clearLevel()
        {
            ClearAllEntitiesExcept();
            SingletonToEntity.Clear();
            ApplyDeferredCommands();
        }

        public void SpawnLevel()
        {
            RegisterSingleton<CollisionBufferComponent>(CreateEntity(new CollisionBufferComponent()));
            RegisterSingleton<ConfigComponent>(CreateEntity(new ConfigComponent()));
            RegisterSingleton<GameStateComponent>(CreateEntity(new GameStateComponent()));
            ApplyDeferredCommands();



            //spawnObsticleLevel();
            spawnCircleLevel();

        }

        private void spawnBorder(bool left = true, bool right = true, bool top = true, bool bot = true)
        {
            Vector2 center = Engine.Instance.outerResolution / 2;
            Vector2 size = Engine.Instance.outerResolution;
            float width = 50f;

            Vector3 color = MyMath.rng3D();
            if (color.Length < 1.0) color.Normalize();


            if (left)
            {
                Entity l = CreateEntity(
                    new PositionComponent(new Vector2(0, center.Y)),
                    new RectangleComponent(new Vector2(width, size.Y), 0f),
                    new ColourComponent(color),
                    new collidableTag(),
                    new VelocityComponent(new Vector2(0f, 0f)),
                    new PhysicsComponent(0.0f, 1.0f)
                );
            }
            if (right)
            {
                Entity r = CreateEntity(
                    new PositionComponent(new Vector2(size.X, center.Y)),
                    new RectangleComponent(new Vector2(width, size.Y), 0f),
                    new ColourComponent(color),
                    new collidableTag(),
                    new VelocityComponent(new Vector2(0f, 0f)),
                    new PhysicsComponent(0.0f, 1.0f)
                );
            }
            if (top)
            {
                Entity t = CreateEntity(
                    new PositionComponent(new Vector2(center.X, size.Y)),
                    new RectangleComponent(new Vector2(size.X, width), 0f),
                    new ColourComponent(color),
                    new collidableTag(),
                    new VelocityComponent(new Vector2(0f, 0f)),
                    new PhysicsComponent(0.0f, 1.0f)
                );
            }
            if (bot)
            {
                Entity b = CreateEntity(
                    new PositionComponent(new Vector2(center.X, 0)),
                    new RectangleComponent(new Vector2(size.X, width), 0f),
                    new ColourComponent(color),
                    new collidableTag(),
                    new VelocityComponent(new Vector2(0f, 0f)),
                    new PhysicsComponent(0.0f, 1.0f)
                );
            }
        }

        private void spawnObsticleLevel()
        {
            spawnBorder(bot:false);

            Vector2 center = Engine.Instance.outerResolution / 2;
            Vector2 size = Engine.Instance.outerResolution;

            Entity goal = CreateEntity(
                new PositionComponent(new Vector2(center.X, -50)),
                new RectangleComponent(new Vector2(2000, 50), 0f),
                //new ColourComponent(new Vector3(1.0f, 0.0f, 0.5f)),
                new collidableTag(),
                new VelocityComponent(new Vector2(0f, 0f)),
                new GoalTag(),
                new KillerTag()
            );

            Vector3 color = MyMath.rng3D();
            if (color.Length < 1.0) color.Normalize();

            for (int i = 0; i < 1; i++)
            {
                Vector3 color3 = MyMath.rng3D();
                if (color3.Length < 1.0) color3.Normalize();
                color3 = new Vector3(0.5f, 1.0f, 0.5f);

                Entity circle2 = CreateEntity(
                    new PositionComponent(new Vector2(center.X, size.Y-50)),
                    //new RingComponent(20, 5),
                    new CircleComponent(25),
                    new VelocityComponent(new Vector2(0, -200f)),
                    new ColourComponent(color3),
                    new collidableTag(),
                    //new SizeChangerComponent(3f),
                    new PhysicsComponent(1, 0.99f),
                    new GravityComponent(700),
                    new CollisionSound(),
                    new KillableTag(),
                    new ScorerTag()
                );
            }

            Vector3 color2 = MyMath.rng3D();
            color2 = MyMath.rng3D();
            if (color2.Length < 1.0) color2.Normalize();
            for (int i = 0; i < 16; i++)
            {
                float spin = 0;
                Vector2 osc = new Vector2(0f, 0f);
                switch (MyMath.rand.Next(1))
                {
                    case 0:
                        spin = 0.5f + MyMath.rng() * 2f;
                        spin = 0.0f + MyMath.rngMinusPlus() * 2f;
                        break;
                    case 1:
                        osc.X = 100 + MyMath.rngMinusPlus() * 200f;
                        break;
                    case 2:
                        osc.Y = 100 + MyMath.rngMinusPlus() * 300f;
                        break;
                    default:
                        spin = -0.5f + -MyMath.rng() * 2f;
                        break;
                }

                //color2 = new Vector3(0.5f, 0.4f, 0.7f);
                Vector2 pos = new Vector2(0, 0) + MyMath.rng2D() * size;
                pos.Y = pos.Y*(1.0f-(200/size.Y));
                pos.Y = pos.Y * (1.0f - (200 / size.Y)) + 200;

                Entity circle2 = CreateEntity(
                    new PositionComponent(pos),
                    new RectangleComponent(new Vector2(100 + MyMath.rng() * 150, 10), MyMath.rng()*MathF.Tau),
                    new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                    new ColourComponent(color2),
                    new collidableTag(),
                    new SpinComponent(spin),
                    new PhysicsComponent(0.0f, 1),
                    new GravityComponent(0f),
                    new GravityOnVictoryTag()
                    //new OscillatorComponent(pos, pos + osc)
                );
            }
        }


        private void spawnCircleLevel()
        {
            Vector2 center = Engine.Instance.outerResolution / 2;


            Entity spawner = CreateEntity(
                new SpawnerComponent(0.3f),
                new PositionComponent(center)
                );

            Entity goal = CreateEntity(
                new PositionComponent(center),
                new RingComponent(1080 / 2f, 10),
                //new ColourComponent(new Vector3(1.0f, 0.0f, 0.5f)),
                new collidableTag(),
                new VelocityComponent(new Vector2(0f, 0f)),
                new GoalTag()
            );

            Vector3 color = MyMath.rng3D();
            if (color.Length < 1.0) color.Normalize();

            for (int i = 0; i < 2; i++)
            {
                Vector3 color3 = MyMath.rng3D();
                if (color3.Length < 1.0) color3.Normalize();
                color3 = new Vector3(0.5f, 1.0f, 0.5f);

                Entity circle2 = CreateEntity(
                    new PositionComponent(center),
                    //new RingComponent(20, 5),
                    new CircleComponent(10),
                    new VelocityComponent(new Vector2(0, -200f)),
                    new ColourComponent(color3),
                    new collidableTag(),
                    new SizeChangerComponent(2f),
                    new PhysicsComponent(1, 1.01f),
                    new GravityComponent(700),
                    new CollisionSound(),
                    new KillerTag(),
                    new ScorerTag()
                );
            }
        }

        public void ClearAllEntitiesExcept(params Entity[] exceptions)
        {
            foreach (var (bitmask, archetype) in archetypes)
            {
                archetype.ClearAllEntitiesExcept(exceptions);
            }
            ApplyDeferredCommands();
        }

        public void Update(float deltaTime)
        {
            SystemRegistry.UpdateAll(this, deltaTime);

            ApplyDeferredCommands();


            //Console.WriteLine(GetEntityView( GetSingleton<GameStateComponent>()).Get<GameStateComponent>().score);
        }

        public void OnResize(ResizeEventArgs args)
        {
            SystemRegistry.OnResize(this, args);

            ApplyDeferredCommands();
        }

        public List<Entity> QueryEntities(BitMask withMask, BitMask withoutMask)
        {
            List<Entity> entities = new List<Entity>();

            foreach(Archetype archetype in QueryArchetypes(withMask, withoutMask))
            {
                entities.AddRange(archetype.entities);
            }

            return entities;
        }


        public Entity GetSingleton<T>()
        {
            if (!SingletonToEntity.TryGetValue(typeof(T), out Entity entity))
                throw new Exception($"Singleton of type {typeof(T).Name} not registered!");
            return entity;
        }

        public void RegisterSingleton<T>(Entity entity)
        {
            SingletonToEntity[typeof(T)] = entity;
        }

        public void ApplyDeferredCommands()
        {
            foreach (var cmd in deferredCommands.createEntityCommands)
                CreateEntityDirect(cmd.Entity, cmd.Components);

            foreach (var cmd in deferredCommands.addComponentCommands)
                AddComponentToEntityDirect(cmd.Entity, cmd.Component);

            foreach (var cmd in deferredCommands.removeComponentCommands)
                RemoveComponentFromEntityDirect(cmd.Entity, cmd.Type);

            foreach (var cmd in deferredCommands.removeEntityCommands)
                DestroyEntityDirect(cmd.Entity);

            deferredCommands.Clear();
        }

        //TODO should cache this in the system
        public IEnumerable<Archetype> QueryArchetypes(BitMask withMask, BitMask withoutMask)
        {
            foreach (var arch in archetypes.Values)
            {
                if (!arch.Mask.ContainsAll(withMask))
                    continue;
                if (arch.Mask.IntersectsAny(withoutMask))
                    continue;
                yield return arch;
            }
        }

        public Entity CreateEntity(params IComponent[] components)
        {
            var newEntity = new Entity(IDManager.Allocate());
            deferredCommands.createEntityCommands.Add(new CreateEntityCommand(newEntity, components));
            return newEntity;
        }

        public Entity CreateEntity(string name, params IComponent[] components)
        {
            var newEntity = new Entity(IDManager.Allocate());
            deferredCommands.createEntityCommands.Add(new CreateEntityCommand(newEntity, components));
            //deferredCommands.addComponentCommands.Add(new AddComponentCommand(newEntity, new NameComponent(name)));
            return newEntity;
        }

        private Entity CreateEntityDirect(Entity newEntity, params IComponent[] components)
        {
            var mask = new BitMask();
            var CompIDtoDataMap = new Dictionary<int, object>();

            foreach (IComponent compoent in components)
            {
                int componentID = ComponentTypeRegistry.GetId(compoent.GetType());
                mask = mask.WithBit(componentID);
                CompIDtoDataMap[componentID] = compoent;
            }

            if (!archetypes.TryGetValue(mask, out Archetype? archetype))
            {
                archetype = new Archetype(mask);
                archetypes[mask] = archetype;
            }

            archetype.AddEntity(newEntity, CompIDtoDataMap);
            entityLocations[newEntity.Id] = (archetype, archetype.Count - 1);
            return newEntity;
        }
        public void DestroyEntity(Entity entity)
        {
            deferredCommands.removeEntityCommands.Add(new RemoveEntityCommand(entity));
        }

        private void DestroyEntityDirect(Entity entity)
        {
            if (!entityLocations.TryGetValue(entity.Id, out var location))
            {
                //Console.WriteLine("trying to destroy an entity that is not in enityLocations");
                return;
            }

            var (archetype, index) = location;
            int last = archetype.Count - 1;
            var lastEntity = archetype.entities[last];

            archetype.RemoveEntityAt(index);

            if (index != last)
                entityLocations[lastEntity.Id] = (archetype, index);

            entityLocations.Remove(entity.Id);
            IDManager.Release(entity.Id);
        }
        public EntityView GetEntityView(Entity entity)
        {
            var location = entityLocations[entity.Id];
            return new EntityView(location.archetype, location.index);
        }

        public T GetComponent<T>(Entity entity) where T : struct, IComponent
        {
            var (arch, index) = entityLocations[entity.Id];
            return arch.GetComponent<T>(index);
        }
        public void AddComponentToEntity(Entity entity, IComponent newComponent)
        {
            deferredCommands.addComponentCommands.Add(new AddComponentCommand(entity, newComponent));
        }

        private void AddComponentToEntityDirect(Entity entity, IComponent newComponent)
        {
            if (!entityLocations.TryGetValue(entity.Id, out var currentLocation))
                throw new Exception("Entity does not exist.");

            Archetype oldArchetype = currentLocation.archetype;
            int oldIndex = currentLocation.index;

            // Compute the new archetype's bitmask
            int newComponentId = ComponentTypeRegistry.GetId(newComponent.GetType());
            BitMask newMask = oldArchetype.Mask.WithBit(newComponentId);

            // Get or create the target archetype
            if (!archetypes.TryGetValue(newMask, out var newArchetype))
            {
                newArchetype = new Archetype(newMask);
                archetypes[newMask] = newArchetype;
            }

            // Prepare component values to transfer
            var components = new Dictionary<int, object>();

            foreach (int compId in oldArchetype.Mask.GetSetBits())
            {
                var array = oldArchetype.ComponentArrays[compId];
                var componentValue = array.GetType().GetMethod("Get")!.Invoke(array, new object[] { oldIndex });
                components[compId] = componentValue;
            }

            // Add the new component
            components[newComponentId] = newComponent;

            // Remove from old archetype (swap-remove)
            oldArchetype.RemoveEntityAt(oldIndex);

            // Add to new archetype
            newArchetype.AddEntity(entity, components);

            // Update entity location
            entityLocations[entity.Id] = (newArchetype, newArchetype.Count - 1);
        }
        public void RemoveComponentFromEntity(Entity entity, Type type)
        {
            deferredCommands.removeComponentCommands.Add(new RemoveComponentCommand(entity, type));
        }
        private void RemoveComponentFromEntityDirect(Entity entity, Type type) 
        {
            if (!entityLocations.TryGetValue(entity.Id, out var currentLocation))
                throw new Exception("Entity does not exist.");

            int compID = ComponentTypeRegistry.GetId(type);
            if (!currentLocation.archetype.Mask.Has(compID))
            {
                throw new Exception("Entitys archetypes mask does not have component.");
                return; // Component not present
            }


            var newMask = currentLocation.archetype.Mask.WithoutBit(compID);

            if (!archetypes.TryGetValue(newMask, out Archetype newArchetype))
            {
                newArchetype = new Archetype(newMask);
                archetypes[newMask] = newArchetype;
            }

            var componentMap = new Dictionary<int, object>();
            for (int i = 0; i < ComponentTypeRegistry.Count; i++)
            {
                if (i == compID || !currentLocation.archetype.Mask.Has(i)) continue;
                var array = currentLocation.archetype.ComponentArrays[i];
                var method = array.GetType().GetMethod("Get");
                componentMap[i] = method.Invoke(array, new object[] { currentLocation.index });
            }

            currentLocation.archetype.RemoveEntityAt(currentLocation.index);
            newArchetype.AddEntity(entity, componentMap);
            entityLocations[entity.Id] = (newArchetype, newArchetype.Count - 1);
        }
    }
}
