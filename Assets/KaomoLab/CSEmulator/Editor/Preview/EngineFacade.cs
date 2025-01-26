using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using Assets.KaomoLab.CSEmulator.Editor.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class EngineFacade
    {
        //必要なら別ファイルに
        class SpaceContext
            : ISpaceContext
        {
            ////CSETODO ★仮置き　ロジック要調査
            //readonly MassTimeThrottle sendThrottle = new MassTimeThrottle(
            //    80 * 1000, 1 * TimeSpan.TicksPerSecond, new MassTimeThrottle.ByDateTimeTicks()
            //);


            //public bool TrySendOperate(int size)
            //{
            //    return sendThrottle.TryCharge(size);
            //}

            public void Update()
            {
                //sendThrottle.Discharge();
            }
        }

        readonly CckPreviewFinder previewFinder;
        readonly ItemCollector itemCollector;
        readonly DesktopPlayerControllerReflector desktopPlayerControllerReflector;
        readonly VrmPreparer vrmPreparer;

        readonly PrefabItemStore prefabItemStore;
        readonly ItemMessageRouter itemMessageRouter;
        readonly TextInputRouter textInputRouter;
        readonly PlayerHandleFactoryBuilder playerHandleFactoryBuilder;
        readonly UserInterfacePreparer userInterfacePreparer;
        readonly ProductPurchaser productPurchaser;
        readonly FogSettingsBridge fogSettingsBridge;
        readonly PlayerStorageSerDes playerStorageSerDes;
        readonly GroupStateProxyMapper groupStateProxyMapper;
        readonly OptionBridge optionBridge;

        List<CodeRunner> codeRunners = new List<CodeRunner>();
        PlayerCodeRunner playerCodeRunner = null;
        UnityEngine.GameObject vrm = null;
        SpaceContext spaceContext = null;
        //CSETODO マルチ環境になったら本当になんとかする
        Components.CSEmulatorPlayerHandler csPlayerHandler = null;

        bool isRunning = false;

        public EngineFacade(
            OptionBridge optionBridge,
            ClusterVR.CreatorKit.Editor.Preview.Item.ItemCreator itemCreator,
            ClusterVR.CreatorKit.Editor.Preview.Item.ItemDestroyer itemDestroyer,
            ClusterVR.CreatorKit.Editor.Preview.World.SpawnPointManager spawnPointManager
        )
        {
            this.optionBridge = optionBridge;
            spaceContext = new SpaceContext(
            );
            previewFinder = new CckPreviewFinder();
            this.itemCollector = new ItemCollector(
                itemCreator
            );

            this.desktopPlayerControllerReflector = new DesktopPlayerControllerReflector(
                previewFinder.controller.GetComponentInParent<ClusterVR.CreatorKit.Preview.PlayerController.DesktopPlayerController>()
            );

            this.vrmPreparer = new VrmPreparer(
                previewFinder,
                desktopPlayerControllerReflector,
                optionBridge.raw.vrm,
                optionBridge,
                optionBridge,
                optionBridge.playerMeasurementsHolder
            );

            prefabItemStore = new PrefabItemStore(
                itemCollector.GetAllItemPrefabs()
            );
            itemMessageRouter = new ItemMessageRouter(
                spaceContext
            );
            textInputRouter = new TextInputRouter();
            userInterfacePreparer = new UserInterfacePreparer(
                previewFinder
            );
            productPurchaser = new ProductPurchaser(
                optionBridge
            );
            fogSettingsBridge = new FogSettingsBridge();
            playerHandleFactoryBuilder = new PlayerHandleFactoryBuilder(
                spaceContext,
                userInterfacePreparer,
                textInputRouter,
                productPurchaser,
                optionBridge,
                itemMessageRouter,
                optionBridge,
                fogSettingsBridge,
                optionBridge,
                spawnPointManager
            );
            groupStateProxyMapper = new GroupStateProxyMapper();
            this.optionBridge = optionBridge;

            playerCodeRunner = new PlayerCodeRunner(
                userInterfacePreparer, itemMessageRouter, optionBridge,
                new DebugLogFactory(new UnityEngine.GameObject("CSEmulator PlayerScript"), optionBridge.raw)
            );

            itemCollector.OnScriptableItemCreated += i =>
            {
                codeRunners.Add(StartRunner(playerCodeRunner, spaceContext, i));
            };
            itemCollector.OnItemCreated += i =>
            {
                var csItemHandler = CSEmulator.Commons.AddComponent<Components.CSEmulatorItemHandler>(i.gameObject);
                csItemHandler.Construct(optionBridge, true);
                CSEmulator.Commons.AddComponent<Components.CSEmulatorStateWatcher>(i.gameObject);
                csItemHandler.SetOwnerPlayer(csPlayerHandler);
            };
            itemDestroyer.OnDestroy += i =>
            {
                var destoryed = codeRunners
                    .FirstOrDefault(c => c.csItemHandler.item.Id.Value == i.Id.Value);
                if (destoryed == null) return;
                destoryed.Shutdown();
                codeRunners.Remove(destoryed);
            };
        }

        public void Start()
        {
            if (!optionBridge.raw.enable) return;

            foreach(var i in itemCollector.GetAllItems())
            {
                var csItemHandler = CSEmulator.Commons.AddComponent<Components.CSEmulatorItemHandler>(i.gameObject);
                csItemHandler.Construct(optionBridge, false);
                CSEmulator.Commons.AddComponent<Components.CSEmulatorStateWatcher>(i.gameObject);
            }

            //StartRunner前にVRMをInstantinateするのは合ってる。
            //しかし複数プレイヤーを考えるとこれは雑なのでそのうち何とかする。
            vrm = vrmPreparer.InstantiateVrm();
            //CSETODO マルチ環境になったらなんとかする
            csPlayerHandler = vrm.GetComponent<Components.CSEmulatorPlayerHandler>();
            playerHandleFactoryBuilder.AddPlayer(vrm, csPlayerHandler, optionBridge);
            //InstantiateVrmでPointOfViewManagerが作られるので
            var localUIEventBridge = CSEmulator.Commons.AddComponent<Components.CSEmulatorPlayerLocalUIEventBridge>(
                previewFinder.panel
            );
            foreach (var csPlayerLocalUI in Components.CSEmulatorPlayerLocalUI.GetAllPlayerLocalUIs())
            {
                //createItemでPlayerLocalUIを含むものは生成できない(CCKドキュメント)ので、これで十分のはず。
                var c = CSEmulator.Commons.AddComponent<Components.CSEmulatorPlayerLocalUI>(csPlayerLocalUI.RectTransform.gameObject);
                c.Construct(localUIEventBridge, vrmPreparer.csPlayerController.pointOfViewManager);
            }

            foreach (var i in itemCollector.GetAllItems())
            {
                //CSETODO マルチ環境になった時になにか必要かも
                var csItemHandler = i.gameObject.GetComponent<Components.CSEmulatorItemHandler>();
                var ownerPlayer = vrm.GetComponent<Components.CSEmulatorPlayerHandler>();
                csItemHandler.SetOwnerPlayer(ownerPlayer);
            }

            fogSettingsBridge.Start();

            //各種コンポーネントを付けてから実行した方がいい気がする。
            var newRunners = itemCollector
                .GetAllScriptableItem()
                .Select(i => StartRunner(playerCodeRunner, spaceContext, i));
            codeRunners.AddRange(newRunners);
            isRunning = true;
        }

        CodeRunner StartRunner(
            PlayerCodeRunner playerCodeRunner,
            SpaceContext spaceContext,
            ClusterVR.CreatorKit.Item.IScriptableItem scriptableItem
        )
        {
            var itemHandler = scriptableItem.Item.gameObject.GetComponent<Components.CSEmulatorItemHandler>();
            var stateWatcher = scriptableItem.Item.gameObject.GetComponent<Components.CSEmulatorStateWatcher>();
            //見やすさ重視で上に持っていく仕組みだったけど、ウィンドウあるしそこまでしなくていいかと
            //while (UnityEditorInternal.ComponentUtility.MoveComponentUp(stateWatcher)) { }
            var loggerFactory = new DebugLogFactory(itemHandler.gameObject, optionBridge.raw);
            var ret = new CodeRunner(
                scriptableItem,
                itemHandler,
                stateWatcher,
                prefabItemStore,
                itemCollector,
                itemMessageRouter,
                textInputRouter,
                productPurchaser,
                optionBridge,
                playerHandleFactoryBuilder,
                playerCodeRunner,
                optionBridge,
                groupStateProxyMapper,
                spaceContext,
                optionBridge,
                optionBridge,
                loggerFactory
            );
            ret.Start();
            return ret;
        }

        public void Update()
        {
            //Update中にDestroyされて減ることがあるのでToArray
            foreach (var runner in codeRunners.ToArray())
            {
                runner.Update();
            }
            playerCodeRunner.Update();
            itemMessageRouter.Routing();
            textInputRouter.Routing();
            productPurchaser.Routing();
            if(spaceContext != null) spaceContext.Update();
            if (csPlayerHandler != null) csPlayerHandler.Update();
        }

        public void Shutdown()
        {
            if (!isRunning) return;

            foreach (var runner in codeRunners)
            {
                runner.Shutdown();
            }
            codeRunners.Clear();

            isRunning = false;
            vrm = null;
        }
    }
}
