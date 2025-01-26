using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class LensDistortionSettings
    {
        public bool active { get; set; } = false;
        public PostProcessFloatProperty centerX = new PostProcessFloatProperty(
            v => Math.Clamp(v, -1f, 1f)
        );
        public PostProcessFloatProperty centerY = new PostProcessFloatProperty(
            v => Math.Clamp(v, -1f, 1f)
        );
        public PostProcessBoolProperty enabled = new PostProcessBoolProperty(v => v).Initialize(true);
        public PostProcessFloatProperty intensity = new PostProcessFloatProperty(
            v => Math.Clamp(v, -100f, 100f)
        );
        public PostProcessFloatProperty scale = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0.01f, 5f)
        );
        public PostProcessFloatProperty xMultiplier = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0f, 1f)
        );
        public PostProcessFloatProperty yMultiplier = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0f, 1f)
        );
    }
}
