using Brain_Fodder.Recording;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using SpaceEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Brain_Fodder.LevelStuff
{
    public abstract class Level
    {

        protected Vector2 center = Engine.Instance.outerResolution / 2;
        protected Vector2 size = Engine.Instance.outerResolution;
        private string name;
        protected Vector3 color;

        public Level(string name)
        {
            this.name = name;
            color = MyMath.rng3D();
            if (color.Length < 1.0) color.Normalize();
            LoadLevel(Engine.Instance.ecsWorld);
        }

        protected Entity CreateEntity(params IComponent[] components)
        {
            return Engine.Instance.ecsWorld.CreateEntity(components);
        }

        public void GenerateVideoMetadata(string videoFilePath)
        {
            // 1. Create the metadata object based on the simulation run
            var metadata = new VideoMetadata
            {
                Title = $"Satisfying Ball Bounce",
                Description = $"Custom physics simulation.",
                Tags = new[] { "shorts", "physics", "simulation", "asmr", "satisfying" },
                HashTags = new[] { "shorts", "physics", "simulation", "asmr", "satisfying" },
                CategoryId = "24",        // Entertainment
                PlaylistName = $"{name}"
            };

            // 2. Determine the path (Change .mp4 to .json)
            string jsonPath = Path.ChangeExtension(videoFilePath, ".json");

            // 3. Serialize to JSON with "Pretty Print" so it's readable
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(metadata, options);

            // 4. Write to disk
            File.WriteAllText(jsonPath, jsonString);

            Console.WriteLine($"Metadata generated: {jsonPath}");
        }

        public abstract void LoadLevel(ECSWorld world);
        public abstract string GetTitle();
        public abstract string GetDescription();
        public abstract string[] GetTags();
        public abstract string[] GetHashTags();

        protected void spawnBorder(bool left = true, bool right = true, bool top = true, bool bot = true)
        {
            float width = 50f;

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
    }
}
