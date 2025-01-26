using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class VignetteSettings
    {
        public bool active { get; set; } = false;
        public PostProcessVector2Property center = new PostProcessVector2Property(v => v);
        public PostProcessColorProperty color = new PostProcessColorProperty(v => v);
        public PostProcessBoolProperty enabled = new PostProcessBoolProperty(v => v).Initialize(true);
        public PostProcessFloatProperty intensity = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0f, 1f)
        );
        public PostProcessBoolProperty rounded = new PostProcessBoolProperty(v => v);
        public PostProcessFloatProperty roundness = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0f, 1f)
        );
        public PostProcessFloatProperty smoothness = new PostProcessFloatProperty(
            v => Math.Clamp(v, 0.01f, 1f)
        );
    }
}
