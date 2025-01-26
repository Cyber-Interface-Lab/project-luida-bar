using Jint;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class ByEngineExceptionFactory
        : IItemExceptionFactory
    {
        public readonly Jint.Native.Error.ErrorConstructor clusterScriptErrorConstructor;

        readonly Jint.Engine engine;

        public ByEngineExceptionFactory(
            Jint.Engine engine
        )
        {
            this.engine = engine;
            clusterScriptErrorConstructor = CreateErrorConstructor(
                nameof(ClusterScriptError),
                engine,
                GetPrototypeObject(engine.Intrinsics.Error),
                intrinsics => GetPrototypeObject(clusterScriptErrorConstructor)
            );
        }

        Jint.Native.Object.ObjectInstance GetPrototypeObject(Jint.Native.Error.ErrorConstructor source)
        {
            var property = typeof(Jint.Native.Error.ErrorConstructor)
                .GetProperty("PrototypeObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(source);
            return (Jint.Native.Object.ObjectInstance)property;
        }
        Jint.Runtime.Realm GetRealm()
        {
            var property = typeof(Jint.Engine)
                .GetProperty("Realm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(engine);
            return (Jint.Runtime.Realm)property;

        }
        Jint.Native.Error.ErrorConstructor CreateErrorConstructor(
            string name,
            Jint.Engine engine,
            Jint.Native.Object.ObjectInstance objectPrototype,
            Func<Jint.Runtime.Intrinsics, Jint.Native.Object.ObjectInstance> intrinsicDefaultProto
        )
        {
            var ret = Activator.CreateInstance(
                typeof(Jint.Native.Error.ErrorConstructor),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new object[]
                {
                    engine,
                    GetRealm(),
                    engine.Intrinsics.Error,
                    objectPrototype,
                    new Jint.Native.JsString(name),
                    intrinsicDefaultProto
                },
                null
            );
            return (Jint.Native.Error.ErrorConstructor)ret;
        }
        public Jint.Native.Object.ObjectInstance CreateClusterScriptErrorBase(string name, string message)
        {
            var jsError = clusterScriptErrorConstructor.Construct(String.Format("[{0}]{1}", name, message));
            jsError.Set("distanceLimitExceeded", false);
            jsError.Set("executionNotAllowed", false);
            jsError.Set("rateLimitExceeded", false);
            jsError.Set("requestSizeLimitExceeded", false);
            jsError.Set("name", "ClusterScriptError");
            jsError.Set("message", message);
            jsError.Set("stack", engine.Advanced.StackTrace);

            return jsError;
        }

        public Exception CreateDistanceLimitExceeded(string message)
        {
            var jsError = CreateClusterScriptErrorBase("DistanceLimitExceeded", message);
            jsError.Set("distanceLimitExceeded", true);
            return new ClusterScriptError(jsError);
        }

        public Exception CreateRateLimitExceeded(string message)
        {
            var jsError = CreateClusterScriptErrorBase("RateLimitExceeded", message);
            jsError.Set("rateLimitExceeded", true);
            return new ClusterScriptError(jsError);
        }

        public Exception CreateExecutionNotAllowed(string message)
        {
            var jsError = CreateClusterScriptErrorBase("ExecutionNotAllowed", message);
            jsError.Set("executionNotAllowed", true);
            return new ClusterScriptError(jsError);
        }

        public Exception CreateRequestSizeLimitExceeded(string message)
        {
            var jsError = CreateClusterScriptErrorBase("RequestSizeLimitExceeded", message);
            jsError.Set("requestSizeLimitExceeded", true);
            return new ClusterScriptError(jsError);
        }

        public Exception CreateGeneral(string message)
        {
            var jsError = this.engine.Intrinsics.Error.Construct(message);
            return new ClusterScriptError(jsError);
        }

        public Exception CreateJsError(string message)
        {
            var instance = this.engine.Intrinsics.Error.Construct(message);
            return new Jint.Runtime.JavaScriptException(instance);
        }
    }
}
