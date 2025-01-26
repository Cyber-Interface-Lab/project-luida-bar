using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using CCKBootstrap = ClusterVR.CreatorKit.Editor.Preview.Bootstrap;


namespace Assets.KaomoLab.CSEmulator.Editor.Preview
{
    [InitializeOnLoad]
    public static class Bootstrap
    {
        const string TickerPrefab = "Assets/KaomoLab/CSEmulator/Editor/Preview/CSEmulatorTicker.prefab";

        public static EngineFacade engine = null;
        public static EmulatorOptions options = new EmulatorOptions();
        public static OptionBridge optionBridge = new OptionBridge(options);
        public static GameObject ticker = null;

        static bool isInPlayMode = false;

        //コンパイル後に１回。PlayModeに入る前に１回。
        static Bootstrap()
        {
            EditorApplication.playModeStateChanged += playMode =>
            {
                OnChangePlayMode(playMode);
            };
            options.OnChangedFps += Options_OnChangedFps;
        }

        static void OnChangePlayMode(PlayModeStateChange playMode)
        {
            if (playMode == PlayModeStateChange.EnteredPlayMode) EnteredPlayMode();
            if (playMode == PlayModeStateChange.ExitingPlayMode) ExitingPlayMode();
        }



        static bool IsInitializedCCK()
        {
            return CCKBootstrap.RoomStateRepository != null;
        }

        [InitializeOnEnterPlayMode]
        static void OnInitializeOnEnterPlayMode()
        {
            if(optionBridge.ignorePackageListUpdate)
            {
                var type_PackageListRepository = typeof(ClusterVR.CreatorKit.Editor.Preview.EditorUI.PackageListRepository);
                var fieldInfo_jsonLastUpdateTimeKey = type_PackageListRepository.GetField(
                    "jsonLastUpdateTimeKey", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
                );
                var fieldInfo_status = type_PackageListRepository.GetField(
                    "status", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
                );
                var key = (string)fieldInfo_jsonLastUpdateTimeKey.GetValue(type_PackageListRepository);
                fieldInfo_status.SetValue(type_PackageListRepository, (object)UnityEditor.PackageManager.StatusCode.Success);
                PlayerPrefs.SetFloat(
                    key, (float)EditorApplication.timeSinceStartup
                );
                UnityEngine.Debug.LogWarning("パッケージ一覧の更新を抑制しているので、上部メニュー「Cluster/Preview/PackageInstaller」の画面は正常に動作しません。");
            }
        }

        static void EnteredPlayMode()
        {
            isInPlayMode = true;
            if (IsInitializedCCK())
            {
                StartCSEmulator();
                return;
            }
            CCKBootstrap.OnInitializedEvent += CCKBootstrap_OnInitializedEvent;
        }

        static void CCKBootstrap_OnInitializedEvent()
        {
            CCKBootstrap.OnInitializedEvent -= CCKBootstrap_OnInitializedEvent;
            if (!isInPlayMode)
            {
                //asyncで動いているパッケージチェックが重く、
                //プレビューが開始しないからといって中止すると
                //EditModeに戻ってからパッケージチェックが終わりOnIntializedが発火する。
                //これを防ぐための実装。
                Debug.LogWarning("Check unnecessary [PreviewOnly] object.");
                return;
            }
            StartCSEmulator();
        }

        static void ExitingPlayMode()
        {
            isInPlayMode = false;
            ShutdownCSEmulator();
        }


        static void StartCSEmulator()
        {
            ShutdownCSEmulator();
            ApplyFpsLimit(); //念のため
            SetAdditionalLayerCollisions();
            engine = new EngineFacadeFactory(optionBridge).CreateDefault();
            engine.Start();
            ticker = GameObject.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(TickerPrefab));
            ticker.GetComponent<CSEmulator.Components.CSEmulatorTicker>().OnUpdate += OnUpdate;
        }

        static void ShutdownCSEmulator()
        {
            if (ticker != null)
            {
                ticker.GetComponent<CSEmulator.Components.CSEmulatorTicker>().OnUpdate -= OnUpdate;
                GameObject.DestroyImmediate(ticker);
            }
            ticker = null;
            engine?.Shutdown();
            engine = null;
            options.Shutdown();
        }

        static void OnUpdate()
        {
            UpdateCSEmulator();
        }

        static void UpdateCSEmulator()
        {
            engine.Update();
        }

        private static void Options_OnChangedFps()
        {
            ApplyFpsLimit();
        }

        static void ApplyFpsLimit()
        {
            //VSYNC変えてもUpdateに変化がない？FixedUpdateはclusterとUnityで同じ。
            UnityEngine.Application.targetFrameRate = options.fps switch
            {
                EmulatorOptions.FpsLimit.unlimited => -1,
                EmulatorOptions.FpsLimit.limit90 => 90,
                EmulatorOptions.FpsLimit.limit30 => 30,
                _ => throw new InvalidOperationException("Invalid FpsLimit"),
            };
        }

        static void SetAdditionalLayerCollisions()
        {
            //CCK2.11.0現在
            Physics.IgnoreLayerCollision(16, 0, false); //OwnAvatar - Default
            Physics.IgnoreLayerCollision(16, 16, true); //OwnAvatar - OwnAvatar
            Physics.IgnoreLayerCollision(16, 19, false); //OwnAvatar - VenueLayer0
            Physics.IgnoreLayerCollision(16, 20, false); //OwnAvatar - VenueLayer1
            Physics.IgnoreLayerCollision(16, 29, true); //OwnAvatar - VenueLayer2
        }
    }
}
