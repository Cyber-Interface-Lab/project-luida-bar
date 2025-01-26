using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class ChannelMixerProperty
    {
        public PostProcessFloatProperty blue = new PostProcessFloatProperty(
            v => Math.Clamp(v, -200f, 200f)
        );
        public PostProcessFloatProperty green = new PostProcessFloatProperty(
            v => Math.Clamp(v, -200f, 200f)
        );
        public PostProcessFloatProperty red = new PostProcessFloatProperty(
            v => Math.Clamp(v, -200f, 200f)
        );

        public object toJSON(string key)
        {
            return new System.Dynamic.ExpandoObject();
        }
    }
}
