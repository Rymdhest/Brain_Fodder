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
    public class CircleLevel : Level
    {


        public CircleLevel() : base("Platforms")
        {
        }


        public override  void LoadLevel(ECSWorld world)
        {

            int numBalls = 1;
            setRequiredWinScore(numBalls);
            for (int i = 0; i<numBalls; i++)
            {
                SpawnBall();
            }
            spawnGoal();
            float platformLength = 40f + MyMath.rng() * 10f;
            int n = 50+(int)(MyMath.rng()*130);
            for (int i = 0; i < n; i++)
            {
                float t = ((i) / (float)(n));
                Console.WriteLine(t);
                float x = MathF.Sin(t * MathF.Tau);
                float y = MathF.Cos(t * MathF.Tau);
                Vector2 pos = center+new Vector2(x, y)*150f;
                //if (pos.Y < 180) continue;
                float spinSpeed = 1f;

                //if (t > 0.9f) continue;
                spawnPlatform(pos, spinSpeed, t*MathF.Tau, MyMath.Mix(new Vector3(1f, 0.5f, 0.3f), color , MathF.Sin(t*MathF.Tau*3)), platformLength, 250f);
                //spawnPlatform(pos, spinSpeed, t * MathF.Tau, MyMath.Mix(new Vector3(1f, 0.5f, 0.3f), color, MathF.Sin(t * MathF.Tau*3)), platformWidth, 240f);

            }
        }


        private void spawnPlatform(Vector2 pos, float spin, float angle, Vector3 col, float length, float r)
        {


            Entity circle2 = CreateEntity(
                new PositionComponent(pos),
                new RectangleComponent(new Vector2(length, 20), angle),
                new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                new ColourComponent(col),
                new collidableTag(),
                new SpinComponent(spin),
                new PhysicsComponent(0.0f, 1.2f),
                new GravityComponent(0f),
                //new SpinAroundComponent(center, r, 5f, angle),
                new PushOutFromOnCollision(center, 50f),
                new AnimationComponent(),
                new GravityOnVictoryTag()
            );
        }

        private void spawnGoal()
        {
            Entity goal = CreateEntity(
                new PositionComponent(center),
                new RingComponent(550, 20f),
                new collidableTag(),
                new VelocityComponent(new Vector2(0f, 0f)),
                new GoalTag(),
                new KillerTag()
            );
        }

        private void SpawnBall()
        {
            Vector2 velocity = MyMath.rng2DMinusPlus().Normalized() * 350;
            Vector3 ballColor = new Vector3(1f) - color;


            if (ballColor.Length < 1.0) ballColor.Normalize();
            Entity player = CreateEntity(
                new PositionComponent(center),
                new CircleComponent(25),
                //new RectangleComponent(new Vector2(22, 22), 0),
                new VelocityComponent(velocity),
                new ColourComponent(ballColor),
                new collidableTag(),
                new PhysicsComponent(1.0f, 1.0175f),
                new GravityComponent(600),
                new GravityOnVictoryTag(),
                //new KillerTag(),
                new ScorerTag(),
                new KillableTag(),
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
