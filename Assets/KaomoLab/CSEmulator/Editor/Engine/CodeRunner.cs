using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class CodeRunner
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

        public class RayDrawerBridge : EmulateClasses.IRayDrawer
        {
            readonly Components.CSEmulatorItemHandler itemHandler;
            readonly IRunnerOptions options;

            public RayDrawerBridge(
                Components.CSEmulatorItemHandler itemHandler,
                IRunnerOptions options
            )
            {
                this.itemHandler = itemHandler;
                this.options = options;
            }

            public void DrawRay(Vector3 start, Vector3 end, Color color)
            {
                if (!options.isRayDraw) return;
                itemHandler.DrawRay(start, end, color);
            }
        }

        readonly UnityEngine.GameObject gameObject;
        readonly string code;
        public readonly Components.CSEmulatorItemHandler csItemHandler;
        readonly Components.CSEmulatorStateWatcher csStateWatcher;
        readonly PrefabItemStore prefabItemStore;
        readonly ItemCollector itemCollector;
        readonly ItemMessageRouter itemMessageRouter;
        readonly TextInputRouter textInputRouter;
        readonly ProductPurchaser productPurchaser;
        readonly PlayerHandleFactoryBuilder playerHandleFactoryBuilder;
        readonly GroupStateProxyMapper groupStateProxyMapper;
        readonly EmulateClasses.IPlayerScriptRunner playerScriptRunner;
        readonly EmulateClasses.IClusterEvent clusterEvent;
        readonly ISerializedPlayerStorage serializedPlayerStorage;
        readonly EmulateClasses.ISpaceContext spaceContext;
        readonly IRunnerOptions options;
        readonly EmulateClasses.ILoggingOptions loggingOptions;
        readonly ILoggerFactory loggerFactory;

        readonly OnStartInvoker onStartInvoker;
        readonly OnUpdateBridge onUpdateBridge;
        readonly OnUpdateBridge onFixedUpdateBridge;
        readonly CckComponentFacadeFactory cckComponentFacadeFactory;
        readonly ItemLifecycler itemLifecycler;
        readonly RayDrawerBridge rayDrawerBridge;

        List<Action> shutdownActions = new List<Action>();
        bool isRunning = false;

        public CodeRunner(
            ClusterVR.CreatorKit.Item.IScriptableItem scriptableItem,
            Components.CSEmulatorItemHandler csItemHandler,
            Components.CSEmulatorStateWatcher csStateWatcher,
            PrefabItemStore prefabItemStore,
            ItemCollector itemCollector,
            ItemMessageRouter itemMessageRouter,
            TextInputRouter textInputRouter,
            ProductPurchaser productPurchaser,
            EmulateClasses.IClusterEvent clusterEvent,
            PlayerHandleFactoryBuilder playerHandleFactoryBuilder,
            EmulateClasses.IPlayerScriptRunner playerScriptRunner,
            ISerializedPlayerStorage serializedPlayerStorage,
            GroupStateProxyMapper groupStateProxyMapper,
            EmulateClasses.ISpaceContext spaceContext,
            IRunnerOptions options,
            ILoggingOptions loggingOptions,
            ILoggerFactory loggerFactory
        )
        {
            this.gameObject = scriptableItem.Item.gameObject;
            this.csItemHandler = csItemHandler;
            this.csStateWatcher = csStateWatcher;
            this.prefabItemStore = prefabItemStore;
            this.itemCollector = itemCollector;
            this.itemMessageRouter = itemMessageRouter;
            this.textInputRouter = textInputRouter;
            this.productPurchaser = productPurchaser;
            this.clusterEvent = clusterEvent;
            this.playerHandleFactoryBuilder = playerHandleFactoryBuilder;
            this.playerScriptRunner = playerScriptRunner;
            this.serializedPlayerStorage = serializedPlayerStorage;
            this.groupStateProxyMapper = groupStateProxyMapper;
            this.spaceContext = spaceContext;
            this.options = options;
            this.loggingOptions = loggingOptions;
            this.loggerFactory = loggerFactory;

            code = scriptableItem.GetSourceCode(true);

            onStartInvoker = new OnStartInvoker();

            //キーはGameObjectのnameで行っている(v1を使いまわしている)ので、Item間での使いまわしは不可。
            //そのためここで各Item用にインスタンスを作っている。
            onUpdateBridge = new OnUpdateBridge(new DummyLogger());
            onFixedUpdateBridge = new OnUpdateBridge(new DummyLogger());

            cckComponentFacadeFactory = new CckComponentFacadeFactory(
                ClusterVR.CreatorKit.Editor.Preview.Bootstrap.RoomStateRepository,
                ClusterVR.CreatorKit.Editor.Preview.Bootstrap.SignalGenerator,
                ClusterVR.CreatorKit.Editor.Preview.Bootstrap.GimmickManager
            );

            itemLifecycler = new ItemLifecycler(
                prefabItemStore,
                ClusterVR.CreatorKit.Editor.Preview.Bootstrap.ItemCreator,
                ClusterVR.CreatorKit.Editor.Preview.Bootstrap.ItemDestroyer
            );

            rayDrawerBridge = new RayDrawerBridge(
                csItemHandler, options
            );
        }

        public void Start()
        {
            csItemHandler.OnFixedUpdate += CsItemHandler_OnFixedUpdate;
            shutdownActions.Add(() => csItemHandler.OnFixedUpdate -= CsItemHandler_OnFixedUpdate);

            var engineOptions = new Jint.Options();
            if(options.isDebug)
                engineOptions.Debugger.Enabled = true;
            var engine = new Jint.Engine(engineOptions);
            shutdownActions.Add(() => engine.Dispose());

            var callbackExecutor = new CallbackExecutor(engine);

            var logger = loggerFactory.Create(new JintProgramStatus(engine));
            var exceptionFactory = new ByEngineExceptionFactory(engine);
            csItemHandler.itemExceptionFactory = exceptionFactory;
            onUpdateBridge.ChangeLogger(logger);
            onFixedUpdateBridge.ChangeLogger(logger);

            var externalHttpCaller = new ExternalHttpCaller(
                options.externalCallerOptions,
                logger
            );

            var materialSubstituer = new MaterialSubstituter(
            );

            var runningContext = new RunningContextBridge();

            var sendableSanitizer = new SendableSanitizer(
                engine
            );

            var playerSendableSanitizer = new PlayerSendableSanitizer(
                engine
            );

            var jsValueConverter = new JsValueConverter(
                engine
            );

            var stateProxy = new EmulateClasses.StateProxy(
                options.pauseFrameKey,
                spaceContext,
                runningContext,
                sendableSanitizer,
                jsValueConverter
            );

            var groupState = new EmulateClasses.StateProxy(
                options.pauseFrameKey,
                spaceContext,
                runningContext,
                sendableSanitizer,
                jsValueConverter
            );
            var isItemGroupHost = gameObject.TryGetComponent<ClusterVR.CreatorKit.Item.IItemGroupHost>(out _);
            var itemGroupMember = gameObject.GetComponent<ClusterVR.CreatorKit.Item.IItemGroupMember>();
            var groupStateProxy = new EmulateClasses.GroupStateProxy(
                isItemGroupHost,
                groupState,
                csItemHandler,
                exceptionFactory,
                itemMessageRouter
            );
            if(isItemGroupHost)
            {
                groupStateProxyMapper.ApplyState(groupStateProxy, csItemHandler.id);
            }
            else if(itemGroupMember?.Host != null)
            {
                groupStateProxyMapper.ApplyState(groupStateProxy, itemGroupMember.Host.Item.Id.ToString());
            }
            else
            {
                groupStateProxy.DisableState();
            }

            var playerStoragerFactory = new PlayerStorageSerDesFactory(
                itemCollector,
                playerHandleFactoryBuilder.BuildFactory(csItemHandler, engine),
                serializedPlayerStorage,
                exceptionFactory,
                engine
            );

            csStateWatcher.Construct(stateProxy);

            var clusterScript = new EmulateClasses.ClusterScript(
                gameObject,
                cckComponentFacadeFactory,
                itemLifecycler,
                spaceContext,
                runningContext,
                callbackExecutor,
                onStartInvoker,
                onUpdateBridge,
                onFixedUpdateBridge,
                itemMessageRouter,
                itemMessageRouter,
                textInputRouter,
                playerHandleFactoryBuilder,
                playerHandleFactoryBuilder.BuildFactory(
                    csItemHandler,
                    engine
                ),
                exceptionFactory,
                externalHttpCaller,
                materialSubstituer,
                productPurchaser,
                clusterEvent,
                new EmulateClasses.PlayerScriptSetter(
                    playerStoragerFactory,
                    itemMessageRouter,
                    itemMessageRouter,
                    playerSendableSanitizer,
                    exceptionFactory,
                    playerScriptRunner,
                    rayDrawerBridge,
                    new HeaderedLogger("[PlayerScript]", logger)
                ),
                sendableSanitizer,
                rayDrawerBridge,
                stateProxy,
                groupStateProxy,
                groupStateProxyMapper,
                loggingOptions,
                logger
            );
            shutdownActions.Add(() => clusterScript.Shutdown());

            engine.SetValue("$", clusterScript);
            SetClass<EmulateClasses.EmulateVector2>(engine, "Vector2");
            SetClass<EmulateClasses.EmulateVector3>(engine, "Vector3");
            SetClass<EmulateClasses.EmulateVector4>(engine, "Vector4");
            SetClass<EmulateClasses.EmulateQuaternion>(engine, "Quaternion");
            SetClass<EmulateClasses.EmulateColor>(engine, "Color");
            SetClass<EmulateClasses.GiftInfo>(engine, "GiftInfo");
            SetClass<EmulateClasses.HumanoidBone>(engine, "HumanoidBone");
            SetClass<EmulateClasses.HumanoidPose>(engine, "HumanoidPose");
            SetClass<EmulateClasses.Muscles>(engine, "Muscles");
            SetClass<EmulateClasses.ItemTemplateId>(engine, "ItemTemplateId");
            SetClass<EmulateClasses.WorldItemTemplateId>(engine, "WorldItemTemplateId");
            SetClass<EmulateClasses.TextAlignment>(engine, "TextAlignment");
            SetClass<EmulateClasses.TextAnchor>(engine, "TextAnchor");
            SetClass<EmulateClasses.TextInputStatus>(engine, "TextInputStatus");
            SetClass<EmulateClasses.PostProcessEffects>(engine, "PostProcessEffects");
            SetClass<EmulateClasses.PurchaseRequestStatus>(engine, "PurchaseRequestStatus");
            SetClass<EmulateClasses.EventRole>(engine, "EventRole");
            SetClass<EmulateClasses.ItemHandle>(engine, "ItemHandle"); //instanceof用。Constructできてしまうけどもまあよし
            SetClass<EmulateClasses.PlayerHandle>(engine, "PlayerHandle");
            engine.SetValue("ClusterScriptError", exceptionFactory.clusterScriptErrorConstructor);

            try
            {
                runningContext.isTopLevel = true;
                engine.Execute(code);
                runningContext.isTopLevel = false;
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }

            onUpdateBridge.SetLateUpdateCallback(
                csItemHandler.gameObject.name + "_throttle",
                csItemHandler.gameObject,
                (dt) =>
                {
                    clusterScript.DischargeOperateLimit(dt);
                    csItemHandler.DischargeOperateLimit(dt);
                }
            );
            shutdownActions.Add(() => onUpdateBridge.DeleteLateUpdateCallback(
                csItemHandler.gameObject.name + "_throttle"
            ));


            isRunning = true;
            shutdownActions.Add(() => isRunning = false);

            runningContext.Reset();
            shutdownActions.Add(() => runningContext.Reset());
        }
        private void CsItemHandler_OnFixedUpdate()
        {
            onFixedUpdateBridge.InvokeUpdate();
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
            //Start>Updateの順で実行される模様 2.7.0.2調査
            onStartInvoker.InvokeStart();
            onUpdateBridge.InvokeUpdate();
        }


        public void Restart()
        {
            Shutdown();
            Start();
        }

        public void Shutdown()
        {
            foreach(var Action in shutdownActions)
            {
                Action();
            }
        }
    }
}
