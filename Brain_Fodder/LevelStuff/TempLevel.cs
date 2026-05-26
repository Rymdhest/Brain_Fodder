using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using SpaceEngine.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brain_Fodder.LevelStuff
{
    public class TempLevel : Level
    {

        public TempLevel() : base("Platforms")
        {

        }

        public override void LoadLevel(ECSWorld world)
        {
            Entity b = world.CreateEntity(
            new PositionComponent(new Vector2(center.X, 50)),
            new RectangleComponent(new Vector2(size.X, 30), 0f),
            new ColourComponent(new Vector3(1.0f, 1.0f, 1.0f)),
            new collidableTag(),
            new VelocityComponent(new Vector2(0f, 0f)),
            new PhysicsComponent(0.0f, 1.0f)
        );

            SpawnBall();
        }

        private void SpawnBall()
        {
            Vector2 velocity = MyMath.rng2DMinusPlus().Normalized() * 0;
            Vector3 color = MyMath.rng3D();
            if (color.Length < 1.0) color.Normalize();
            Entity player = CreateEntity(
                new PositionComponent(center),
                new CircleComponent(13),
                //new RectangleComponent(new Vector2(22, 22), 0),
                new VelocityComponent(velocity),
                new ColourComponent(new Vector3(0.5f, 0.4f, 0.9f)),
                new collidableTag(),
                new PhysicsComponent(1.0f, 1.05f),
                new GravityComponent(200),
                new SizeChangerComponent(1.0f),
                new GravityOnVictoryTag(),
                //new KillerTag(),
                new ScorerTag(),
                new OnHitComponent
                {
                    Action = (self, other, w) => {
                        // If the other object is also a ball, only proceed if I have the lower ID
                        if (other.Has<OnHitComponent>() && self.Entity.Id > other.Entity.Id)
                        {
                            return; // Reject execution on this side of the pair
                        }

                        SoundManager.OnBallBounce();
                        SpawnBall();
                    }
                }
            );
        }
    }
}
