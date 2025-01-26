using Jint.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class PlayerCodeRunner
        : EmulateClasses.IPlayerScriptRunner
    {
        public class DummyLogger
            : ILogger
        {
            public void Error(string message) => UnityEngine.Debug.LogError(message);
            public void Exception(JsError e) => UnityEngine.Debug.LogError(e.ToString());
            public void Exception(Exception e) => UnityEngine.Debug.LogException(e);
            public void Exception(Exception e, UnityEngine.GameObject source) => UnityEngine.Debug.LogException(e);
            public void Info(string message) => UnityEngine.Debug.Log(message);
            public void Warning(string message) => UnityEngine.Debug.LogWarning(message);
        }

        readonly EmulateClasses.IButtonInterfaceHandler buttonInterfaceHandler;
        readonly ItemMessageRouter itemMessageRouter;
        readonly IRunnerOptions options;
        readonly ILoggerFactory loggerFactory;

        readonly OnUpdateBridge onUpdateBridge;

        List<Action> shutdownActions = new List<Action>();
        bool isRunning = false;

        EmulateClasses.PlayerScript runningPlayerScript = null;

        public PlayerCodeRunner(
            EmulateClasses.IButtonInterfaceHandler buttonInterfaceHandler,
            ItemMessageRouter itemMessageRouter,
            IRunnerOptions options,
            ILoggerFactory loggerFactory
        )
        {
            this.buttonInterfaceHandler = buttonInterfaceHandler;
            this.itemMessageRouter = itemMessageRouter;
            this.options = options;
            this.loggerFactory = loggerFactory;

            //キーはGameObjectのnameで行っている(v1を使いまわしている)ので、Item間での使いまわしは不可。
            //そのためここで各Item用にインスタンスを作っている。
            onUpdateBridge = new OnUpdateBridge(new DummyLogger());

            ClusterVR.CreatorKit.Editor.Preview.Bootstrap.ItemDestroyer.OnDestroy += item =>
            {
                if(runningPlayerScript != null && runningPlayerScript.gameObject.GetHashCode() == item.gameObject.GetHashCode())
                {
                    runningPlayerScript._Shutdown();
                    runningPlayerScript = null;
                }
            };
        }

        public void Run(EmulateClasses.PlayerScript playerScript)
        {
            if (runningPlayerScript != null)
                runningPlayerScript._Shutdown();
            runningPlayerScript = playerScript;




            var engineOptions = new Jint.Options();
            if(options.isDebug)
                engineOptions.Debugger.Enabled = true;
            var engine = new Jint.Engine(engineOptions);
            shutdownActions.Add(() => engine.Dispose());

            var logger = loggerFactory.Create(new JintProgramStatus(engine));
            var exceptionFactory = new ByEngineExceptionFactory(engine);
            onUpdateBridge.ChangeLogger(logger);

            var runningContext = new RunningContextBridge();

            var sendableSanitizer = new PlayerSendableSanitizer(
                engine
            );

            playerScript._InjectEventHandler(
                onUpdateBridge,
                buttonInterfaceHandler
            );

            engine.SetValue("_", playerScript);
            SetClass<EmulateClasses.EmulateVector2>(engine, "Vector2");
            SetClass<EmulateClasses.EmulateVector3>(engine, "Vector3");
            SetClass<EmulateClasses.EmulateVector4>(engine, "Vector4");
            SetClass<EmulateClasses.EmulateQuaternion>(engine, "Quaternion");
            SetClass<EmulateClasses.EmulateColor>(engine, "Color");
            SetClass<EmulateClasses.HumanoidBone>(engine, "HumanoidBone");
            SetClass<EmulateClasses.HumanoidPose>(engine, "HumanoidPose");
            SetClass<EmulateClasses.Muscles>(engine, "Muscles");
            SetClass<EmulateClasses.ItemTemplateId>(engine, "ItemTemplateId");
            SetClass<EmulateClasses.TextAlignment>(engine, "TextAlignment");
            SetClass<EmulateClasses.TextAnchor>(engine, "TextAnchor");
            SetClass<EmulateClasses.TextInputStatus>(engine, "TextInputStatus");
            SetClass<EmulateClasses.PostProcessEffects>(engine, "PostProcessEffects");
            SetClass<EmulateClasses.ItemId>(engine, "ItemId"); //instanceof用。Constructできてしまうけどもまあよし
            SetClass<EmulateClasses.PlayerId>(engine, "PlayerId");
            //WorldItemTemplateIdは定義されていない(2.20.0確認済み)
            engine.SetValue("ClusterScriptError", exceptionFactory.clusterScriptErrorConstructor);

            try
            {
                runningContext.isTopLevel = true;
                engine.Execute(playerScript._code);
                runningContext.isTopLevel = false;
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }


            isRunning = true;
            shutdownActions.Add(() => isRunning = false);

            runningContext.Reset();
            shutdownActions.Add(() => runningContext.Reset());
        }

        void SetClass<T>(Jint.Engine engine, string name)
        {
            engine.SetValue(
                name, GetTypeReference<T>(engine)
            );
        }
        Jint.Runtime.Interop.TypeReference GetTypeReference<T>(
            Jint.Engine engine
        )
        {
            return Jint.Runtime.Interop.TypeReference.CreateTypeReference(
                engine,
                typeof(T)
            );
        }

        public void Update()
        {
            if (!isRunning) return;
            onUpdateBridge.InvokeUpdate();
        }

        public void Shutdown()
        {
            foreach(var Action in shutdownActions)
            {
                Action();
            }
            if(runningPlayerScript != null)
                runningPlayerScript._Shutdown();
            runningPlayerScript = null;
        }
    }
}
