using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class ColorGradingSettings
    {
        public bool active { get; set; } = false;
        public PostProcessFloatProperty brightness = new PostProcessFloatProperty(
            v => Math.Clamp(v, -100f, 100f)
        );
        public ChannelMixerProperty channelMixerBlue = new ChannelMixerProperty();
        public ChannelMixerProperty channelMixerGreen = new ChannelMixerProperty();
        public ChannelMixerProperty channelMixerRed = new ChannelMixerProperty();
        public PostProcessColorProperty colorFilter = new PostProcessColorProperty(v => v).SetupHDR();
        public PostProcessFloatProperty contrast = new PostProcessFloatProperty(
            v => Math.Clamp(v, -100f, 100f)
        );
        public PostProcessBoolProperty enabled = new PostProcessBoolProperty(v => v).Initialize(true);
        public PostProcessVector4Property gain = new PostProcessVector4Property(v => v);
        public PostProcessVector4Property gamma = new PostProcessVector4Property(v => v);
        public PostProcessFloatProperty hueShift = new PostProcessFloatProperty(
            v => Math.Clamp(v, -180f, 180f)
        );
        public PostProcessVector4Property lift = new PostProcessVector4Property(v => v);
        public PostProcessFloatProperty saturation = new PostProcessFloatProperty(
            v => Math.Clamp(v, -100f, 100f)
        );
        public PostProcessFloatProperty temperature = new PostProcessFloatProperty(
            v => Math.Clamp(v, -100f, 100f)
        );
        public PostProcessFloatProperty tint = new PostProcessFloatProperty(
            v => Math.Clamp(v, -100f, 100f)
        );

    }
}
