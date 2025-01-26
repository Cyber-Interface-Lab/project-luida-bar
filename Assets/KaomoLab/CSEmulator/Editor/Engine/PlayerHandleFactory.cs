using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class PlayerHandleFactoryBuilder
        : IItemOwnerHandler, IEngineApplyBuilder
    {
        public class PlayerHandleFactory
            : IPlayerHandleFactory
        {
            readonly PlayerHandleFactoryBuilder builder;
            readonly Jint.Engine engine;

            //Engine層で渡したい時がある時はここ
            public PlayerHandleFactory(
                PlayerHandleFactoryBuilder bridge,
                Jint.Engine engine
            )
            {
                this.builder = bridge;
                this.engine = engine;
            }

            public PlayerHandle CreateByIdfc(string idfc, Components.CSEmulatorItemHandler csOwnerItemHandler)
            {
                var (csPlayerHandler, playerMeta) = builder.players[idfc];

                if (csPlayerHandler == null) return null;
                var desktopPlayerController = csPlayerHandler.GetComponentInParent<ClusterVR.CreatorKit.Preview.PlayerController.DesktopPlayerController>();
                if (desktopPlayerController == null) return null;

                var csPlayerController = desktopPlayerController.gameObject.GetComponent<Components.CSEmulatorPlayerController>();
                var playerController = new CCKPlayerController(
                    csPlayerHandler, csPlayerController, desktopPlayerController, builder.playerOptions, builder.spawnPointManager
                );

                //VrmPrepare側での設定をここで意識する必要があるのはどうにもよくないと思うけど
                //いいアイデアが出るか、困った事になるまではこれで行く
                var postProcessApplier = new PostProcessApplier(
                    csPlayerHandler.gameObject.transform.parent.parent.parent.gameObject,
                    builder.fogSettingsBridge
                );

                var handle = new PlayerHandle(
                    playerMeta,
                    builder.spaceContext,
                    playerController,
                    builder.userInterfaceHandler,
                    builder.textInputSender,
                    builder.productPurchaser,
                    builder.clusterEvent,
                    postProcessApplier,
                    builder.messageSender,
                    csOwnerItemHandler
                );
                return handle;
            }
        }

        readonly Dictionary<string, (Components.CSEmulatorPlayerHandler, IPlayerMeta)> players = new ();

        readonly ISpaceContext spaceContext;
        readonly IUserInputInterfaceHandler userInterfaceHandler;
        readonly ITextInputSender textInputSender;
        readonly IProductPurchaser productPurchaser;
        readonly IClusterEvent clusterEvent;
        readonly IMessageSender messageSender;
        readonly IPlayerOptions playerOptions;
        readonly FogSettingsBridge fogSettingsBridge;
        readonly ISerializedPlayerStorage serializedPlayerStorage;
        readonly ClusterVR.CreatorKit.Editor.Preview.World.SpawnPointManager spawnPointManager;

        public PlayerHandleFactoryBuilder(
            ISpaceContext spaceContext,
            IUserInputInterfaceHandler userInterfaceHandler,
            ITextInputSender textInputSender,
            IProductPurchaser productPurchaser,
            IClusterEvent clusterEvent,
            IMessageSender messageSender,
            IPlayerOptions playerOptions,
            FogSettingsBridge fogSettingsBridge,
            ISerializedPlayerStorage serializedPlayerStorage,
            ClusterVR.CreatorKit.Editor.Preview.World.SpawnPointManager spawnPointManager
        )
        {
            this.spaceContext = spaceContext;
            this.userInterfaceHandler = userInterfaceHandler;
            this.textInputSender = textInputSender;
            this.productPurchaser = productPurchaser;
            this.clusterEvent = clusterEvent;
            this.messageSender = messageSender;
            this.playerOptions = playerOptions;
            this.fogSettingsBridge = fogSettingsBridge;
            this.serializedPlayerStorage = serializedPlayerStorage;
            this.spawnPointManager = spawnPointManager;
        }

        public void AddPlayer(UnityEngine.GameObject vrm, Components.CSEmulatorPlayerHandler playerHandler, IPlayerMeta playerMeta)
        {
            players.Add(playerHandler.idfc, (playerHandler, playerMeta));
        }

        public IPlayerHandleFactory BuildFactory(
            Components.CSEmulatorItemHandler csItemHandler,
            Jint.Engine engine
        )
        {
            var ret = new PlayerHandleFactory(
                this, engine
            );
            return ret;
        }

        //CSETODO 仮実装
        public string GetOwnerIdfc()
        {
            return players.First().Value.Item1.idfc;
        }

    }
}
