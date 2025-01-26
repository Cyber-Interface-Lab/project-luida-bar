using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor
{
    public interface ILogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Exception(Jint.Native.JsError e);
        void Exception(Exception e);
        void Exception(Exception e, UnityEngine.GameObject source);
    }
}
