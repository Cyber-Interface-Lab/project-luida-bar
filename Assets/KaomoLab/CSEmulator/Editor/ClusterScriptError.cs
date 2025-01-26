using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor
{
    public class ClusterScriptError : Jint.Runtime.JavaScriptException
    {
        public bool distanceLimitExceeded;
        public bool executionNotAllowed;
        public bool rateLimitExceeded;
        public bool requestSizeLimitExceeded;
        public string message => this.Message;
        public string name => GetType().Name;
        public string stack => base.JavaScriptStackTrace;

        public ClusterScriptError(
            Jint.Native.Error.ErrorConstructor errorConstructor,
            string message
        )
            : base(errorConstructor, message)
        {
        }

        public ClusterScriptError(
            Jint.Native.Object.ObjectInstance objectInstance
        )
            : base(objectInstance)
        {
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            o.name = name;
            o.distanceLimitExceeded = distanceLimitExceeded;
            o.executionNotAllowed = executionNotAllowed;
            o.rateLimitExceeded = rateLimitExceeded;
            o.requestSizeLimitExceeded = requestSizeLimitExceeded;
            return o;
        }
        public override string ToString()
        {
            return String.Format("[ClusterScriptError][{0}]", message);
        }
    }
}
