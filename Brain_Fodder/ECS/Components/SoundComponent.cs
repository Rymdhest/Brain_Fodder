using Dino_Engine.ECS.ECS_Architecture;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace Dino_Engine.ECS.Components
{
    public struct SoundComponent : IComponent
    {
        public int note = 0;

        public SoundComponent(int note)
        {
            this.note = note;
        }

    }
}
