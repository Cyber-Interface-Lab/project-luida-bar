using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using Jint.Native.Function;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class CallbackExecutor
        : EmulateClasses.ICallbackExecutor
    {
        readonly Jint.Engine engine;

        public CallbackExecutor(
            Jint.Engine engine
        )
        {
            this.engine = engine;
        }

        public void Execute(Function callback, params object[] args)
        {
            engine.Invoke(callback, args);
        }
    }
}
