using Assets.KaomoLab.CSEmulator.Components;
using Assets.KaomoLab.CSEmulator.Editor.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class OptionBridge
        : Engine.IRunnerOptions,
        Engine.IPlayerOptions,
        Engine.IProductOptions,
        Components.IPerspectiveChangeNotifier,
        EmulateClasses.IPlayerMeta,
        Engine.ISerializedPlayerStorage,
        EmulateClasses.IClusterEvent,
        EmulateClasses.ILoggingOptions
    {
        public event Handler<string, int> OnGiftSent = delegate{};

        public bool isDebug => raw.debug;
        public IExternalCallerOptions externalCallerOptions { get; private set; }
        public string pauseFrameKey => raw.pauseFrameKey;

        public EmulatorOptions raw { get; private set; }

        public string userIdfc => raw.userIdfc;
        public string userId => raw.userId;
        public string userDisplayName => raw.userName;
        public EmulateClasses.EventRole eventRole => raw.playerEventRole;

        public bool exists => raw.exists;

        public bool isAndroid => raw.playerOperatingSystem == EmulatorOptions.PlayerOperatingSystem.Android;
        public bool isDesktop => raw.playerDevice == EmulatorOptions.PlayerDevice.Desktop;
        public bool isIos => raw.playerOperatingSystem == EmulatorOptions.PlayerOperatingSystem.iOS;
        public bool isMacOs => raw.playerOperatingSystem == EmulatorOptions.PlayerOperatingSystem.macOS;
        public bool isMobile => raw.playerDevice == EmulatorOptions.PlayerDevice.Mobile;
        public bool isVr => raw.playerDevice == EmulatorOptions.PlayerDevice.VR;
        public bool isWindows => raw.playerOperatingSystem == EmulatorOptions.PlayerOperatingSystem.Windows;

        public bool isFirstPersonView
        {
            get => raw.perspective;
            set => raw.perspective = value;
        }

        public bool isRayDraw => raw.isRayDraw;

        public bool ignorePackageListUpdate => raw.ignorePackageListUpdate;

        public IPlayerMeasurementsHolder playerMeasurementsHolder { get; private set; }

        bool EmulateClasses.IClusterEvent.isEvent => raw.isEvent;

        public bool loggingOnReceiveFail => raw.loggingOnReceiveFail;

        public class ExternalCallerOptions : IExternalCallerOptions
        {
            public event Handler OnChangeLimit = delegate { };
            readonly EmulatorOptions options;
            public ExternalCallerOptions(EmulatorOptions options)
            {
                this.options = options;
                this.options.OnChangedExternalCallLimit += () => {
                    OnChangeLimit.Invoke();
                };
            }
            public string url => options.callExternalUrl;
            public EmulateClasses.CallExternalRateLimit rateLimit => options.limitExternalCall;
        }

        public class PlayerMeasurementsHolder : IPlayerMeasurementsHolder
        {
            public float height => ClusterVR.CreatorKit.Preview.PlayerController.CameraControlSettings.StandingEyeHeight;
            public float radius => options.playerColliderRadius;

            readonly EmulatorOptions options;
            public PlayerMeasurementsHolder(
                EmulatorOptions options
            )
            {
                this.options = options;
            }
        }

        public OptionBridge(
            EmulatorOptions options
        )
        {
            this.raw = options;
            this.externalCallerOptions = new ExternalCallerOptions(options);
            this.playerMeasurementsHolder = new PlayerMeasurementsHolder(options);
            options.OnChangedPerspective += Options_OnChangedPerspective;
            options.OnGiftSent += Options_OnGiftSent;
        }

        private void Options_OnChangedPerspective()
        {
            foreach (var l in perspectiveChangeListeners)
            {
                l.Invoke(raw.perspective);
            }
        }

        private void Options_OnGiftSent(string giftType, int price)
        {
            OnGiftSent.Invoke(giftType, price);
        }

        readonly List<Handler<bool>> perspectiveChangeListeners = new List<Handler<bool>>();
        event Handler<bool> IPerspectiveChangeNotifier.OnChanged
        {
            add => perspectiveChangeListeners.Add(value);
            remove => perspectiveChangeListeners.Remove(value);
        }

        public void SetPersonViewChangeable(bool canChange)
        {
            raw.canChangePerspective = canChange;
        }

        void IPerspectiveChangeNotifier.RequestNotify()
        {
            Options_OnChangedPerspective();
        }

        public bool IsPublicProduct(string productId)
        {
            foreach (var info in raw.productInfos)
            {
                if (info.productId == productId) return info.isPublic;
            }
            return false;
        }
        public string GetProductName(string productId)
        {
            foreach(var info in raw.productInfos)
            {
                if(info.productId == productId) return info.productName;
            }
            return null; //nullで判定する。限界きたらちゃんとする
        }
        public (int, int) GetProductAmount(string productId)
        {
            foreach (var info in raw.productInfos)
            {
                if (info.productId == productId) return (info.plus, info.minus);
            }
            return (0, 0);
        }
        public void SetProductAmount(string productId, int plus, int minus)
        {
            var infos = raw.productInfos.ToArray();
            foreach (var info in infos)
            {
                if (info.productId == productId)
                {
                    info.plus = plus;
                    info.minus = minus;
                }
            }
            raw.productInfos = infos;
        }

        public void SavePlayerStorage(string serialized)
        {
            raw.playerStorage = serialized;
        }
        public string LoadPlayerStorage()
        {
            return raw.playerStorage;
        }
    }
}
