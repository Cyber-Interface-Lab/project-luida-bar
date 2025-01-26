using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.KaomoLab.CSEmulator;

namespace Assets.KaomoLab.CSEmulator.Editor
{
    public static class Commons
    {
        public static void ExceptionLogger(Exception e, UnityEngine.GameObject source)
        {
            switch (e)
            {
                case Jint.Runtime.JavaScriptException jse:
                    UnityEngine.Debug.LogError(String.Format(
                        "[{0}]{1}\n{2}",
                        source.GetFullPath(),
                        jse.Message,
                        jse.JavaScriptStackTrace
                    ));
                    break;
                default:
                    UnityEngine.Debug.LogException(e);
                    break;
            }
        }
    }
}
