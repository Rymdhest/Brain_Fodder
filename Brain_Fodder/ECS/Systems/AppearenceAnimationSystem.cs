
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
    public class AppearenceAnimationSystem : SystemBase
    {
        public AppearenceAnimationSystem()
            : base(new BitMask(typeof(ColourComponent), typeof(AppearenceAnimationComponent)))
        {
        }
        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            var color = entity.Get<ColourComponent>();
            var animation = entity.Get<AppearenceAnimationComponent>();
            var circle = entity.Get<CircleComponent>();

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
            color.colour = animation.startColor + (animation.goalColor - animation.startColor) * easedT;
            circle.radius = animation.startSize + (animation.goalSize - animation.startSize) * easedT;

            entity.Set(color);
            entity.Set(animation);
            entity.Set(circle);
        }
    }
}