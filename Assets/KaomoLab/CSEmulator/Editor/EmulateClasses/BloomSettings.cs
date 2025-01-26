using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class BloomSettings
    {
        public bool active { get; set; } = false;
        public PostProcessFloatProperty anamorphicRatio = new PostProcessFloatProperty(
            v => Math.Clamp(v, -1f, 1f)
        );
        public PostProcessFloatProperty clamp = new PostProcessFloatProperty(
            v => Math.Max(v, 0f)
        );
        public PostProcessColorProperty color = new PostProcessColorProperty(v => v).SetupHDR();
        public PostProcessBoolProperty enabled = new PostProcessBoolProperty(v => v).Initialize(true);
        public PostProcessFloatProperty intensity = new PostProcessFloatProperty(
            v => Math.Max(v, 0f)
        );
        public PostProcessFloatProperty softKnee = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0f, 1f)
        );
        public PostProcessFloatProperty threshold = new PostProcessFloatProperty(
            v => Math.Max(v, 0f)
        );
    }
}
