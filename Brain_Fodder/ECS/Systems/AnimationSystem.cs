
using Brain_Fodder;
using Dino_Engine.ECS.Components;
using Dino_Engine.ECS.ECS_Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino_Engine.ECS.Systems
{
    public class AnimationSystem : SystemBase
    {
        public AnimationSystem()
            : base(new BitMask(typeof(PositionComponent), typeof(AnimationComponent)))
        {
        }
        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            var position = entity.Get<PositionComponent>();
            var animation = entity.Get<AnimationComponent>();

            if (animation.t >= 1.0f)
            {
                return;

            }

            // 1. Increment timer
            animation.t += deltaTime / animation.duration;

            if (animation.t > 1.0f)
            {
                animation.t = 1.0f;
            }

            // 2. Apply the chosen Easing function
            float easedT = OscillatorSystem.Interpolate(EasingType.Cosine, animation.t);

            // 3. Final LERP between points
            // Formula: A + (B - A) * t
            position.value = animation.start + (animation.goal - animation.start) * easedT;

            entity.Set(position);
            entity.Set(animation);
        }
    }
}