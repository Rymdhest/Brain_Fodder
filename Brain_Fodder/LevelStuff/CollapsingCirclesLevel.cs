using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Compute.OpenCL;
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
    public class CollapsingCirclesLevel : Level
    {

        private int i = 0;

        public CollapsingCirclesLevel() : base("Collapsing Circles")
        {
        }


        public override  void LoadLevel(ECSWorld world)
        {
            
            setRequiredWinScore(0);

            spawnOuterShape(100);

            int numBalls = 1;
            for (int i = 0; i<numBalls; i++)
            {
                Vector2 pos = center;
                Entity circle3 = CreateEntity(
                    new PositionComponent(pos),
                    new ShapeComponent(new Vector2(40, 40) , 0, 0, 4, true),
                    new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                    new ColourComponent(new Vector3(0.2f, 1f, 0.1f)),
                    new collidableTag(),
                    new PhysicsComponent(1.0f, 1.008f),
                    new KillerTag(),
                    new GravityComponent(200f),
                    new ScorerTag(),
                    new SizeChangerComponent(5),
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
            for (int i = 0; i<0; i++)
            {
                Vector2 pos = center;
                Vector3 col = MyMath.Mix(new Vector3(1f, 0.3f, 0.2f), new Vector3(0.1f, 0.2f, 1f), MathF.Sin( (i / 40f)*MathF.Tau)*0.5f + 0.5f);
                Entity circle3 = CreateEntity(
                    new PositionComponent(pos),
                    new ShapeComponent(new Vector2(200, 200) + new Vector2(i, i) * 20, 8, i * 0.1f, 4, false),
                    new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                    new ColourComponent(col),
                    new collidableTag(),
                    new PhysicsComponent(0.0f,1.2f),
                    new GravityComponent(0f),
                    new SizeChangerComponent(-100),
                    new KillableTag(),
                    new SpinComponent(0f),
                    new ScorerTag()
                );
            }
            Entity spawner = CreateEntity();
            var spawnerComp = new IntervalActionComponent
            {
                Action = (w) => {

                    // Safely fetch and update this specific spawner
                    var view = w.GetEntityView(spawner);
                    var e = view.Get<IntervalActionComponent>();

                    spawnOuterShape(100f + (0.5f - e.SpawnInterval)*500f);

                    e.SpawnInterval *= 0.988f;

                    view.Set(e);
                },
                SpawnInterval = 0.5f
            };
            world.AddComponentToEntity(spawner, spawnerComp);
            /*
            Entity goal = CreateEntity(
                new PositionComponent(center),
                new ShapeComponent(size*1.15f , 4, 0, 30, false),
                new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                new collidableTag(),
                new KillableTag(),
                new GoalTag()
            );
            */
        }
        private void spawnOuterShape(float shrinkspeed)
        {
            increaseRequiredWinScore(1);
            Vector2 pos = center;
            Vector3 col = MyMath.Mix(new Vector3(1f, 0.3f, 0.2f), new Vector3(0.1f, 0.2f, 1f), MathF.Sin((i / 30f) * MathF.Tau) * 0.5f + 0.5f);
            Entity circle3 = CreateEntity(
                new PositionComponent(pos),
                new ShapeComponent(new Vector2(500, 500), 4, i * 0.087f, 10, false),
                new VelocityComponent(MyMath.rng2DMinusPlus() * 0.0f),
                new ColourComponent(col),
                new collidableTag(),
                new PhysicsComponent(0.0f, 1.2f),
                new GravityComponent(0f),
                new SizeChangerComponent(-shrinkspeed),
                new KillableTag(),
                new SpinComponent(0f),
                new GoalTag()
            );
            i++;
        }
    }
}
