using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class FogSettings
    {
        public bool active { get; set; } = false; //2.8.0.1確認
        public PostProcessColorProperty color = new PostProcessColorProperty(v => v);
        public PostProcessFloatProperty density = new PostProcessFloatProperty(v => v);
        public PostProcessBoolProperty enabled = new PostProcessBoolProperty(v => v);
        public PostProcessFloatProperty end = new PostProcessFloatProperty(v => v);
        public PostProcessStringProperty mode = new PostProcessStringProperty(v => v);
        public PostProcessFloatProperty start = new PostProcessFloatProperty(v => v);

        public FogSettings()
        {
            //デフォルト値はLightingウィンドウで確認した値
            //color.setValue(0.5f, 0.5f, 0.5f, 1.0f);
            //density.setValue(0.01f);
            enabled.setValue(true);
            //end.setValue(300f);
            //mode.setValue("ExponentialSquared");
            //start.setValue(0f);
        }

    }
}
