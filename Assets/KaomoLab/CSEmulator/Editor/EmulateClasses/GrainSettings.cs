using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class GrainSettings
    {
        public bool active { get; set; } = false;
        public PostProcessBoolProperty colored = new PostProcessBoolProperty(v => v);
        public PostProcessBoolProperty enabled = new PostProcessBoolProperty(v => v).Initialize(true);
        public PostProcessFloatProperty intensity = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0f, 1f)
        );
        public PostProcessFloatProperty luminanceContribution = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0f, 1f)
        );
        public PostProcessFloatProperty size = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0.3f, 3f)
        );
    }
}
