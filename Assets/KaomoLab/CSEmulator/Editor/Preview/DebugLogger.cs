using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class DebugLogger
        : ILogger
    {
        readonly UnityEngine.GameObject gameObject;
        readonly IProgramStatus programStatus;
        readonly EmulatorOptions options;
        readonly string path;

        public DebugLogger(
            UnityEngine.GameObject gameObject,
            IProgramStatus programStatus,
            EmulatorOptions options
        )
        {
            this.gameObject = gameObject;
            this.programStatus = programStatus;
            this.options = options;
            this.path = gameObject.GetFullPath();
        }

        string BuildMessage(string message)
        {
            if (!options.debug) return message;
            var ret = String.Format(
                "[{0}][{1}]{2}",
                path,
                programStatus.GetLineInfo(),
                message
            );
            return ret;
        }

        public void Error(string message)
        {
            UnityEngine.Debug.LogError(BuildMessage(message));
        }

        public void Info(string message)
        {
            UnityEngine.Debug.Log(BuildMessage(message));
        }

        public void Warning(string message)
        {
            UnityEngine.Debug.LogWarning(BuildMessage(message));
        }

        public void Exception(Jint.Native.JsError e)
        {
            var ps = e.GetOwnProperties()
                .ToDictionary(kv => kv.Key.ToString(), kv => kv.Value.Value.ToString());
            var message = BuildMessage(ps["message"] + "\n" + ps["stack"]);
            UnityEngine.Debug.LogError(message);
        }

        public void Exception(Exception e)
        {
            Exception(e, gameObject);
        }
        public void Exception(Exception e, UnityEngine.GameObject source)
        {
            switch (e)
            {
                case Jint.Runtime.JavaScriptException jse:
                    UnityEngine.Debug.LogError(String.Format(
                        "[{0}][{1}]{2}\n{3}",
                        source.GetFullPath(),
                        programStatus.GetLineInfo(),
                        jse.Message,
                        jse.JavaScriptStackTrace
                    ));
                    break;
                default:
                    UnityEngine.Debug.LogException(e);
                    UnityEngine.Debug.LogError(
                       String.Format("Exception in [{0}]{1}",
                           source.GetFullPath(),
                           programStatus.GetLineInfo()
                       )
                   );
                    break;
            }
        }
    }
}
