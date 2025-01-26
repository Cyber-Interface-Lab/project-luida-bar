using ClusterVR.CreatorKit.Preview.PlayerController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    public class EmulatorOptions
    {
        public event Handler OnChangedFps = delegate { };
        public event Handler OnChangedExternalCallLimit = delegate { };
        public event Handler OnChangedPerspective = delegate { };
        public event Handler<string, int> OnGiftSent = delegate { };

        const string PrefsKeyEnable = "KaomoCSEmulator_enable";
        public bool enable {
            get => PlayerPrefs.GetInt(PrefsKeyEnable, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyEnable, value ? 1 : 0);
        }


        const string PrefsKeyFps = "KaomoCSEmulator_fps";
        public enum FpsLimit : int
        {
            unlimited,
            limit90,
            limit30
        };
        public FpsLimit fps
        {
            get => (FpsLimit)PlayerPrefs.GetInt(PrefsKeyFps, (int)FpsLimit.limit90);
            set {
                PlayerPrefs.SetInt(PrefsKeyFps, (int)value);
                OnChangedFps.Invoke();
            }
        }

        //一人称視点開始だと三人称視点に変更してもItemHighligherが正常に稼働する（原因調査しても沼りそうなので未調査）
        const string PrefsKeyFirstPersonPerspective = "KaomoCSEmulator_firstPersonPerspective";
        public bool perspective
        {
            get => PlayerPrefs.GetInt(PrefsKeyFirstPersonPerspective, 1) == 1;
            set {
                PlayerPrefs.SetInt(PrefsKeyFirstPersonPerspective, value ? 1 : 0);
                OnChangedPerspective.Invoke();
            }
        }
        //実行時のみの想定なのでprefを使用しない
        public bool canChangePerspective = true;

        const string PrefsKeyVrm = "KaomoCSEmulator_vrm";
        public const string DefaultVrmPath = "Assets/KaomoLab/CSEmulator/VRM/CSEmulatorDummyHumanoid.prefab";
        public GameObject vrm
        {
            get
            {
                var path = PlayerPrefs.GetString(PrefsKeyVrm, DefaultVrmPath);
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                return prefab;
            }
            set
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(value);
                if (path == "") path = DefaultVrmPath;
                PlayerPrefs.SetString(PrefsKeyVrm, path);
            }
        }

        const string PrefsKeyUserIdfc = "KaomoCSEmulator_userIdfc";
        public readonly string DefaultUserIdfc = new String(Enumerable.Repeat(0, 4).SelectMany(_ => new System.Random().Next().ToString("X")).ToArray()).ToLower();
        public readonly Regex userIdfcPattern = new Regex("^[0-9a-z]{32}$");
        public string userIdfc
        {
            get => PlayerPrefs.GetString(PrefsKeyUserIdfc, DefaultUserIdfc);
            set => PlayerPrefs.SetString(PrefsKeyUserIdfc, value);
        }
        const string PrefsKeyUserId = "KaomoCSEmulator_userId";
        public readonly string DefaultUserId = new String(Enumerable.Repeat('a', 16).Select(c => (char)(c + (new System.Random().Next() % 26))).ToArray());
        public readonly Regex userIdPattern = new Regex(".*");
        public string userId
        {
            get => PlayerPrefs.GetString(PrefsKeyUserId, DefaultUserId);
            set => PlayerPrefs.SetString(PrefsKeyUserId, value);
        }
        const string PrefsKeyUserName = "KaomoCSEmulator_userName";
        public readonly string DefaultUserName = "テストユーザー";
        public string userName
        {
            get => PlayerPrefs.GetString(PrefsKeyUserName, DefaultUserName);
            set => PlayerPrefs.SetString(PrefsKeyUserName, value);
        }
        const string PrefsKeyExists = "KaomoCSEmulator_exists";
        public bool exists
        {
            get => PlayerPrefs.GetInt(PrefsKeyExists, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyExists, value ? 1 : 0);
        }
        const string PrefsKeyPlayerEventRole = "KaomoCSEmulator_playerEventRole";
        public EmulateClasses.EventRole playerEventRole
        {
            get => (EmulateClasses.EventRole)PlayerPrefs.GetInt(PrefsKeyPlayerEventRole, (int)EmulateClasses.EventRole.Audience);
            set => PlayerPrefs.SetInt(PrefsKeyPlayerEventRole, (int)value);
        }

        const string PrefsKeyIsVr = "KaomoCSEmulator_isVr"; //非使用
        const string PrefsKeyPlayerDevice = "KaomoCSEmulator_playerDevice";
        public enum PlayerDevice : int
        {
            Desktop, VR, Mobile
        }
        public PlayerDevice playerDevice
        {
            get => (PlayerDevice)PlayerPrefs.GetInt(PrefsKeyPlayerDevice, (int)PlayerDevice.Desktop);
            set => PlayerPrefs.SetInt(PrefsKeyPlayerDevice, (int)value);
        }
        const string PrefsKeyPlayerOperatingSystem = "KaomoCSEmulator_playerOperatingSystem";
        public enum PlayerOperatingSystem : int
        {
            Windows, macOS, iOS, Android
        }
        public PlayerOperatingSystem playerOperatingSystem
        {
            get => (PlayerOperatingSystem)PlayerPrefs.GetInt(PrefsKeyPlayerOperatingSystem, (int)PlayerOperatingSystem.Windows);
            set => PlayerPrefs.SetInt(PrefsKeyPlayerOperatingSystem, (int)value);
        }
        const string PrefsKeyPlayerColliderRadius = "KaomoCSEmulator_playerColliderRadius";
        public readonly float DefaultPlayerColliderRadius = 0.2f; //PreviewOnlyの値
        public float playerColliderRadius
        {
            get => PlayerPrefs.GetFloat(PrefsKeyPlayerColliderRadius, DefaultPlayerColliderRadius);
            set => PlayerPrefs.SetFloat(PrefsKeyPlayerColliderRadius, value);
        }

        const string PrefsKeyCallExternalUrl = "KaomoCSEmulator_callExternalUrl";
        public string callExternalUrl
        {
            get => PlayerPrefs.GetString(PrefsKeyCallExternalUrl, "");
            set => PlayerPrefs.SetString(PrefsKeyCallExternalUrl, value);
        }
        const string PrefsKeyLimitExternalCall = "KaomoCSEmulator_limitExternalCall";
        public EmulateClasses.CallExternalRateLimit limitExternalCall
        {
            get => (EmulateClasses.CallExternalRateLimit)PlayerPrefs.GetInt(
                PrefsKeyLimitExternalCall, (int)EmulateClasses.CallExternalRateLimit.unlimited
            );
            set {
                PlayerPrefs.SetInt(PrefsKeyLimitExternalCall, (int)value);
                OnChangedExternalCallLimit.Invoke();
            }
        }

        const string PrefsKeyIsEvent = "KaomoCSEmulator_isEvent";
        public bool isEvent
        {
            get => PlayerPrefs.GetInt(PrefsKeyIsEvent, 0) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyIsEvent, value ? 1 : 0);
        }

        //const string PrefsKeyOverwriteMouseSensitivity = "KaomoCSEmulator_overwriteMouseSensitivity";
        //独自の値を保持してもあまり意味がなさそうなので直操作
        public float overwriteMouseSensitivity
        {
            get => CameraControlSettings.Sensitivity;
            set => CameraControlSettings.Sensitivity = value;
        }

        const string PrefsKeyDebug = "KaomoCSEmulator_debug";
        public bool debug
        {
            get => PlayerPrefs.GetInt(PrefsKeyDebug, 0) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyDebug, value ? 1 : 0);
        }

        const string PrefsKeyIgnoreCckPackageListUpdate = "KaomoCSEmulator_ignoreCckPackageListUpdate";
        public bool ignorePackageListUpdate
        {
            get => PlayerPrefs.GetInt(PrefsKeyIgnoreCckPackageListUpdate, 0) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyIgnoreCckPackageListUpdate, value ? 1 : 0);
        }

        const string PrefsKeyPauseFrameKey = "KaomoCSEmulator_pauseFrameKey";
        public string pauseFrameKey
        {
            get => PlayerPrefs.GetString(PrefsKeyPauseFrameKey, "_pauseFrame");
            set => PlayerPrefs.SetString(PrefsKeyPauseFrameKey, value);
        }

        const string PrefsKeyIsRayDraw = "KaomoCSEmulator_isRayDraw";
        public bool isRayDraw
        {
            get => PlayerPrefs.GetInt(PrefsKeyIsRayDraw, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyIsRayDraw, value ? 1 : 0);
        }

        public class ProductInfo
        {
            public string productName = "";
            public string productId = "";
            public int plus;
            public int minus;
            public bool isPublic = true;
            public static ProductInfo[] Deserialize(string serialized)
            {
                var ret = serialized.Split('\r')
                    .Where(info => info != "")
                    .Select(info => {
                        var p = info.Split('\n');
                        return new ProductInfo {
                            productId = p[0], productName = p[1],
                            plus = int.Parse(p[2]),
                            minus = int.Parse(p[3]),
                            isPublic = int.Parse(p[4]) == 1,
                        };
                    }).ToArray();
                return ret;
            }
            public static string Serialize(ProductInfo[] productInfos)
            {
                var sb = new StringBuilder();
                foreach(var p in productInfos)
                {
                    sb.AppendFormat("{0}\n{1}\n{2}\n{3}\n{4}\r", p.productId.Trim(), p.productName.Trim(), p.plus, p.minus, p.isPublic ? 1 : 0);
                }
                return sb.ToString();
            }
        }
        const string PrefsKeyProductInfos = "KaomoCSEmulator_productInfos";
        public readonly Regex productIdPattern = new Regex("^[0-9a-z]{8}-[0-9a-z]{4}-[0-9a-z]{4}-[0-9a-z]{4}-[0-9a-z]{12}$");
        public ProductInfo[] productInfos
        {
            get => ProductInfo.Deserialize(PlayerPrefs.GetString(PrefsKeyProductInfos, ""));
            set => PlayerPrefs.SetString(PrefsKeyProductInfos, ProductInfo.Serialize(value));
        }

        const string PrefsKeyPlayerStorage = "KaomoCSEmulator_playerStorage";
        public string playerStorage
        {
            get => PlayerPrefs.GetString(PrefsKeyPlayerStorage, "");
            set => PlayerPrefs.SetString(PrefsKeyPlayerStorage, value);
        }

        const string PrefsKeyGiftType = "KaomoCSEmulator_giftType";
        public string giftType
        {
            get => PlayerPrefs.GetString(PrefsKeyGiftType, "");
            set => PlayerPrefs.SetString(PrefsKeyGiftType, value);
        }
        const string PrefsKeyGiftPrice = "KaomoCSEmulator_giftPrice";
        public int giftPrice
        {
            get => PlayerPrefs.GetInt(PrefsKeyGiftPrice, 0);
            set => PlayerPrefs.SetInt(PrefsKeyGiftPrice, value);
        }

        const string PrefsKeyLoggingOnReceiveFail = "KaomoCSEmulator_loggingOnReceiveFail";
        public bool loggingOnReceiveFail
        {
            get => PlayerPrefs.GetInt(PrefsKeyLoggingOnReceiveFail, 1) == 1;
            set => PlayerPrefs.SetInt(PrefsKeyLoggingOnReceiveFail, value ? 1 : 0);
        }

        public EmulatorOptions()
        {
        }

        public void SendGift()
        {
            OnGiftSent.Invoke(giftType, giftPrice);
        }

        public bool IsVrmPrefab(GameObject gameObject)
        {
            if (gameObject == null) return false;
            //UniVRMを入れていないと型名をコンパイルできずにエラーになるため
            //UnityではGetTypeする時はアセンブリ名(DLL名)も併せて必要
            var vrmMetaType = Type.GetType("VRM.VRMMeta, VRM");
            if (vrmMetaType == null) return false;
            if (null == gameObject.GetComponent(vrmMetaType)) return false;

            return true;
        }

        public void Shutdown()
        {
            canChangePerspective = true;
        }
    }
}
