using Brain_Fodder.Recording;
using Brain_Fodder.Rendering;
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
        public Vector3 color;
        public Background background = Background.Random;

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
                Title = GetTitle(),
                Description = GetDescription(GetHashtags(), 10),
                Tags = GetTags(5),
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
        private string GetTitle()
        {
            string filePath = Path.Combine("Text Ideas", "Titles.txt");
            try
            {
                if (!File.Exists(filePath)) return string.Empty;

                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8)
                                     .Where(line => !string.IsNullOrWhiteSpace(line))
                                     .Select(line => line.Trim())
                                     .Select(line => line.Length > 100 ? line.Substring(0, 97) + "..." : line)
                                     .ToArray();

                if (lines.Length == 0) return string.Empty;

                // Pick exactly one random title from the list
                Random rng = new Random();
                int randomIndex = rng.Next(lines.Length);

                return lines[randomIndex];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading title from {filePath}: {ex.Message}");
                return string.Empty;
            }
        }
        private string GetDescription(string[] hashtags, int hashtagCount)
        {
            string filePath = Path.Combine("Text Ideas", "Descriptions.txt");
            try
            {
                if (!File.Exists(filePath)) return string.Empty;

                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8)
                                     .Where(line => !string.IsNullOrWhiteSpace(line))
                                     .Select(line => line.Trim())
                                     .ToArray();

                if (lines.Length == 0) return string.Empty;

                // 1. Pick exactly one random base description from the list
                Random rng = new Random();
                int randomIndex = rng.Next(lines.Length);
                string chosenDescription = lines[randomIndex];

                // 2. Append the random hashtags if available and requested
                if (hashtags != null && hashtags.Length > 0 && hashtagCount > 0)
                {
                    var randomHashtags = hashtags.OrderBy(x => rng.Next())
                                                 .Take(hashtagCount);

                    string hashtagString = string.Join(" ", randomHashtags);

                    chosenDescription = $"{chosenDescription}\n\n{hashtagString}";
                }

                // 3. Safety net: Ensure the final string doesn't exceed YouTube's 5000-character limit
                return chosenDescription.Length > 5000
                    ? chosenDescription.Substring(0, 4997) + "..."
                    : chosenDescription;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading description from {filePath}: {ex.Message}");
                return string.Empty;
            }
        }
        private string[] GetTags(int tagCount)
        {
            string filePath = Path.Combine("Text Ideas", "Tags.txt");
            try
            {
                if (!File.Exists(filePath)) return Array.Empty<string>();

                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8)
                                     .Where(line => !string.IsNullOrWhiteSpace(line))
                                     .Select(line => line.Trim())
                                     .ToArray();

                if (lines.Length == 0 || tagCount <= 0) return Array.Empty<string>();

                // Shuffle the tags randomly and take the specified amount
                Random rng = new Random();
                string[] randomTags = lines.OrderBy(x => rng.Next())
                                           .Take(tagCount)
                                           .ToArray();

                return randomTags;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading tags from {filePath}: {ex.Message}");
                return Array.Empty<string>();
            }
        }
        private string[] GetHashtags()
        {
            string filePath = Path.Combine("Text Ideas", "Hashtags.txt");
            try
            {
                if (!File.Exists(filePath)) return Array.Empty<string>();

                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8)
                                     .Where(line => !string.IsNullOrWhiteSpace(line))
                                     .Select(line => line.Trim())
                                     // Automatically prepend '#' if the line doesn't start with it
                                     .Select(line => line.StartsWith("#") ? line : "#" + line)
                                     .ToArray();

                return lines;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading hashtags from {filePath}: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        protected void setRequiredWinScore(int reqScore)
        {
            var world = Engine.Instance.ecsWorld;
            var gameState = world.GetEntityView(world.GetSingleton<GameStateComponent>()).Get<GameStateComponent>();
            gameState.scoreToWin = reqScore;
            world.GetEntityView(world.GetSingleton<GameStateComponent>()).Set<GameStateComponent>(gameState);
        }
        protected void increaseRequiredWinScore(int amount)
        {
            var world = Engine.Instance.ecsWorld;
            var gameState = world.GetEntityView(world.GetSingleton<GameStateComponent>()).Get<GameStateComponent>();
            gameState.scoreToWin += amount;
            world.GetEntityView(world.GetSingleton<GameStateComponent>()).Set<GameStateComponent>(gameState);
        }

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
