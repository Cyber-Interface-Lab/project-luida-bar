using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class PlayerScriptSetter
    {
        readonly IPlayerStoragerFactory playerStoragerFactory;
        readonly IMessageSender messageSender;
        readonly IPlayerReceiveListenerBinder playerReceiveListenerBinder;
        readonly IPlayerSendableSanitizer playerSendableSanitizer;
        readonly IItemExceptionFactory itemExceptionFactory;
        readonly IPlayerScriptRunner playerScriptRunner;
        readonly PlayerScript.ClusterWorldRuntimeSettings clusterWorldRuntimeSettings;
        readonly IRayDrawer rayDrawer;
        readonly ILogger logger;

        public PlayerScriptSetter(
            IPlayerStoragerFactory playerStoragerFactory,
            IMessageSender messageSender,
            IPlayerReceiveListenerBinder playerReceiveListenerBinder,
            IPlayerSendableSanitizer playerSendableSanitizer,
            IItemExceptionFactory itemExceptionFactory,
            IPlayerScriptRunner playerScriptRunner,
            IRayDrawer rayDrawer,
            ILogger logger
        )
        {
            this.playerStoragerFactory = playerStoragerFactory;
            this.messageSender = messageSender;
            this.playerReceiveListenerBinder = playerReceiveListenerBinder;
            this.playerSendableSanitizer = playerSendableSanitizer;
            this.itemExceptionFactory = itemExceptionFactory;
            this.playerScriptRunner = playerScriptRunner;
            this.rayDrawer = rayDrawer;
            this.logger = logger;

            var worldRuntimeSettings = ClusterVR.CreatorKit.Editor.Builder.WorldRuntimeSettingGatherer.GatherWorldRuntimeSettings(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            ).FirstOrDefault();
            clusterWorldRuntimeSettings = new PlayerScript.ClusterWorldRuntimeSettings(
                worldRuntimeSettings
            );
        }


        public void Set(
            PlayerHandle playerHandle, ClusterScript clusterScript, string code
        )
        {
            var storager = playerStoragerFactory.Create(clusterScript.csItemHandler);
            var ps = new PlayerScript(
                clusterWorldRuntimeSettings,
                messageSender,
                playerReceiveListenerBinder,
                playerSendableSanitizer,
                storager,
                itemExceptionFactory,
                rayDrawer,
                logger,
                playerHandle,
                clusterScript,
                code
            );
            playerScriptRunner.Run(ps);
        }
    }
}
