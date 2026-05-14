using Dino_Engine.ECS.ECS_Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brain_Fodder.Level
{
    public abstract class Level
    {
        public Level(string name)
        {
            LoadLevel(Engine.Instance.ecsWorld);
        }
        public abstract void LoadLevel(ECSWorld world);
        public abstract string GetTitle();
        public abstract string GetDescription();
        public abstract string[] GetTags();
        public abstract string[] GetHashTags();
    }
}
