using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class ChromaticAberrationSettings
    {
        //active: boolean
        public bool active { get; set; } = false;
        public PostProcessBoolProperty enabled = new PostProcessBoolProperty(v => v).Initialize(true);
        public PostProcessFloatProperty intensity = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0f, 1f)
        );
    }
}
