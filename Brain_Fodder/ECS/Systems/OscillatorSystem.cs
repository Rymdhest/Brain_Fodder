
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
    public class OscillatorSystem : SystemBase
    {
        public OscillatorSystem()
            : base(new BitMask(typeof(PositionComponent), typeof(OscillatorComponent)))
        {
        }
        protected override void UpdateEntity(EntityView entity, ECSWorld world, float deltaTime)
        {
            var position = entity.Get<PositionComponent>();
            var osc = entity.Get<OscillatorComponent>();

            // 1. Increment timer
            osc.Timer += deltaTime * osc.Speed;

            // Handle ping-pong logic (0 -> 1 -> 0)
            float t = MathF.Sin(osc.Timer) * 0.5f + 0.5f;

            // 2. Apply the chosen Easing function
            float easedT = Interpolate(osc.Easing, t);

            // 3. Final LERP between points
            // Formula: A + (B - A) * t
            position.value = osc.positionA + (osc.positionB - osc.positionA) * easedT;


            entity.Set(position);
            entity.Set(osc);
        }


        public static float Interpolate(EasingType type, float t)
        {
            // Ensure t is clamped between 0 and 1
            t = Math.Clamp(t, 0.0f, 1.0f);

            return type switch
            {
                EasingType.Linear => t,
                EasingType.Cosine => (1.0f - MathF.Cos(t * MathF.PI)) / 2.0f,
                EasingType.SmoothStep => t * t * (3.0f - 2.0f * t),
                EasingType.QuadraticIn => t * t,
                EasingType.QuadraticOut => t * (2.0f - t),
                _ => t
            };
        }
    }
}
