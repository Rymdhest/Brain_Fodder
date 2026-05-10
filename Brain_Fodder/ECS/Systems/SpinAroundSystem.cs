
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dino_Engine.ECS.Systems
{
    public class SpinAroundSystem : SystemBase
    {
        public SpinAroundSystem()
            : base(new BitMask(typeof(PositionComponent), typeof(SpinAroundComponent)))
        {
        }
        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            var position = entity.Get<PositionComponent>();
            var spin = entity.Get<SpinAroundComponent>();
            var sound = entity.Get<SoundComponent>();

            var aniamtion = entity.Get<AppearenceAnimationComponent>();
            // 1. Increment timer
            spin.t += deltaTime;

            if (spin.t > spin.Duration) {
                spin.t = spin.t%spin.Duration;


                SoundManager.Play( SoundManager.GenerateSound(sound.note));
                aniamtion.t = 0f;
            }

            // Handle ping-pong logic (0 -> 1 -> 0)
            position.value.X = spin.center.X+ MathF.Sin((spin.t/spin.Duration)*MathF.Tau)*spin.radius;
            position.value.Y = spin.center.Y + MathF.Cos((spin.t / spin.Duration) * MathF.Tau)*spin.radius;



            entity.Set(position);
            entity.Set(aniamtion);
            entity.Set(spin);
        }

    }
}
