using Brain_Fodder;
using Dino_Engine.ECS.Components;
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

            for (int i = 0; i<1; i++)
            {
                Vector3 color = MyMath.rng3D();
                if (color.Length<1.0) color.Normalize();
                color =new Vector3(0.5f, 1.0f, 0.5f);

                Entity circle2 = CreateEntity(
                    new PositionComponent(new Vector2(100, 800)+MyMath.rng2D()* new Vector2(400, 100)),
                    new CircleComponent(20),
                    new VelocityComponent(MyMath.rng2DMinusPlus()*0),
                    new ColourComponent(color),
                    new collidableTag(),
                    new PhysicsComponent(1, 0.95f),
                    new GravityTag(),
                    new CollisionSound()
                );
            }
            for (int i = 0; i < 10  ; i++)
            {
                Vector3 color = MyMath.rng3D();
                if (color.Length < 1.0) color.Normalize();
                color = new Vector3(0.5f, 0.4f, 0.7f);
                Entity circle2 = CreateEntity(
                    new PositionComponent(new Vector2(100, 100) + MyMath.rng2D() * new Vector2(400, 800)),
                    new RectangleComponent(new Vector2(100+MyMath.rng()*300, 20), MathF.PI/5f),
                    new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                    new ColourComponent(color),
                    new collidableTag(),
                    new SpinComponent(MyMath.rngMinusPlus()),
                    new PhysicsComponent(0.0f, 1)
                );
            }

            for (int i = 0; i < 0; i++)
            {
                Vector3 color = MyMath.rng3D();
                if (color.Length < 1.0) color.Normalize();
                color = new Vector3(1.0f, 0.5f, 0.5f);
                Entity circle2 = CreateEntity(
                    new PositionComponent(new Vector2(50, 0)* i),
                    new CircleComponent(45),
                    new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                    new ColourComponent(color),
                    new collidableTag(),
                    new PhysicsComponent(0, 1)
                );
            }
            for (int i = 0; i < 12; i++)
            {
                Vector3 color = MyMath.rng3D();
                if (color.Length < 1.0) color.Normalize();
                color = new Vector3(1.0f, 0.5f, 0.5f);
                Entity circle2 = CreateEntity(
                    new PositionComponent(new Vector2(0, 950)+new Vector2(50, 0) * i),
                    new CircleComponent(45),
                    new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                    new ColourComponent(color),
                    new collidableTag(),
                    new PhysicsComponent(0, 1)
                );
            }

            for (int i = 0; i < 20; i++)
            {
                Vector3 color = MyMath.rng3D();
                if (color.Length < 1.0) color.Normalize();
                color = new Vector3(1.0f, 0.5f, 0.5f);
                Entity circle2 = CreateEntity(
                    new PositionComponent(new Vector2(0, 0) + new Vector2(0, 50) * i),
                    new CircleComponent(45),
                    new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                    new ColourComponent(color),
                    new collidableTag(),
                    new PhysicsComponent(0, 1)
                );
            }
            for (int i = 0; i < 20; i++)
            {
                Vector3 color = MyMath.rng3D();
                if (color.Length < 1.0) color.Normalize();
                color = new Vector3(1.0f, 0.5f, 0.5f);
                Entity circle2 = CreateEntity(
                    new PositionComponent(new Vector2(550, 0) + new Vector2(0, 50) * i),
                    new CircleComponent(45),
                    new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                    new ColourComponent(color),
                    new collidableTag(),
                    new PhysicsComponent(0, 1)
                );
            }
        }

        public void Update(float deltaTime)
        {
            SystemRegistry.UpdateAll(this, deltaTime);

            ApplyDeferredCommands();
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
                throw new InvalidOperationException("trying to destroy an entity that is not in enityLocations");
                //continue;
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
