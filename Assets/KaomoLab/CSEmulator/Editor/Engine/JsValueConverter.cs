using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class JsValueConverter
        : IJsValueConverter
    {
        readonly Jint.Engine engine;

        public JsValueConverter(
            Jint.Engine engine
        )
        {
            this.engine = engine;
        }

        public JsValue FromObject(object value)
        {
            var ret = Jint.Native.JsValue.FromObject(engine, value);
            return ret;
        }
    }
}
