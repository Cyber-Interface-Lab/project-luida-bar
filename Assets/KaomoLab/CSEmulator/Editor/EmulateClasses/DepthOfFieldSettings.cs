using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class DepthOfFieldSettings
    {
        public bool active { get; set; } = false;
        public PostProcessFloatProperty aperture = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0.05f, 32f)
        );
        public PostProcessBoolProperty enabled = new PostProcessBoolProperty(v => v).Initialize(true);
        public PostProcessFloatProperty focalLength = new PostProcessFloatProperty(
            v => Math.Clamp(v, 1f, 300f)
        );
        public PostProcessFloatProperty focusDistance = new PostProcessFloatProperty(
            v => Math.Max(v, 0.1f)
        );


    }
}
