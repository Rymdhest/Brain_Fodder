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
    public class PlatformLevel : Level
    {


        public PlatformLevel() : base("Platforms")
        {
        }

        public override string GetDescription()
        {
            throw new NotImplementedException();
        }

        public override string[] GetHashTags()
        {
            throw new NotImplementedException();
        }

        public override string[] GetTags()
        {
            throw new NotImplementedException();
        }

        public override string GetTitle()
        {
            throw new NotImplementedException();
        }

        public override void LoadLevel(ECSWorld world)
        {
            spawnBorder(left: true, right: true, top: true, bot: false);
            SpawnBall();
            spawnGoal();

            int n = 20;
            for (int i = 0; i < n; i++)
            {
                Vector2 pos = new Vector2(0, 200);
                float t = ((i + 1) / (float)(n + 1));
                pos.X = t*size.X;
                float rot = 0f;

                t -= 0.5f;
                t*= 10f;
                pos.Y += t*t*35f;

                float spinSpeed = t*0.5f;
                spawnPlatform(pos, spinSpeed, rot);
            }
        }


        private void spawnPlatform(Vector2 pos, float spin, float rot)
        {


            Entity circle2 = CreateEntity(
                new PositionComponent(pos),
                new RectangleComponent(new Vector2(150, 12), rot),
                new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                new ColourComponent(color),
                new collidableTag(),
                new SpinComponent(spin),
                new PhysicsComponent(0.0f, 1.2f),
                new GravityComponent(0f),
                new GravityOnVictoryTag()
            //new OscillatorComponent(pos, pos + osc)
            );
        }

        private void spawnGoal()
        {
            Entity goal = CreateEntity(
                new PositionComponent(new Vector2(center.X, -50)),
                new RectangleComponent(new Vector2(2000, 50), 0f),
                new collidableTag(),
                new VelocityComponent(new Vector2(0f, 0f)),
                new GoalTag(),
                new KillerTag()
            );
        }

        private void SpawnBall()
        {
            Vector2 velocity = MyMath.rng2DMinusPlus().Normalized() * 0;
            Vector3 ballColor = new Vector3(1f) - color;


            if (ballColor.Length < 1.0) ballColor.Normalize();
            Entity player = CreateEntity(
                new PositionComponent(new Vector2(center.X, size.Y*0.8f)),
                new CircleComponent(16),
                //new RectangleComponent(new Vector2(22, 22), 0),
                new VelocityComponent(velocity),
                new ColourComponent(ballColor),
                new collidableTag(),
                new PhysicsComponent(1.0f, 1.01f),
                new GravityComponent(400),
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
                    }
                }
            );
        }
    }
}
