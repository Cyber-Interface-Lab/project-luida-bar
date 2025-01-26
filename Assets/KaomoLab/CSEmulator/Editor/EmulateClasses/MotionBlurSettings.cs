using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class MotionBlurSettings
    {
        public bool active { get; set; } = false;
        public PostProcessBoolProperty enabled = new PostProcessBoolProperty(v => v).Initialize(true);
        public PostProcessIntProperty sampleCount = new PostProcessIntProperty(
            v => Math.Clamp(v, 4, 32)
        );
        public PostProcessFloatProperty shutterAngle = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0f, 360f)
        );
    }
}
