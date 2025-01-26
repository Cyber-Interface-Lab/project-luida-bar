using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Assets.KaomoLab.CSEmulator.Editor.Preview;

namespace Assets.KaomoLab.CSEmulator.Editor.Window
{
    public class EmulatorOptionsWindow : EditorWindow
    {
        const string UNIVRAM_PACKAGE = "Assets/VRM/package.json";
        bool isValidUniVrm = false;

        private Vector2 scroll = Vector2.zero;


        [System.Serializable]
        public class UniVrmPackage
        {
            public string version;
        }


        [MenuItem("Window/かおもラボ/CSEmulator")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<EmulatorOptionsWindow>(false, "CSEmulator");
        }

        void OnGUI()
        {
            var logo = AssetDatabase.LoadAssetAtPath<Texture>("Assets/KaomoLab/CSEmulator/Editor/Window/logo.png");
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(new GUIContent(logo), GUILayout.Height(30));
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndHorizontal();

            scroll = EditorGUILayout.BeginScrollView(scroll);


            CheckUniVrmVersion();

            var op = Bootstrap.options;
            op.fps = (EmulatorOptions.FpsLimit)EditorGUILayout.EnumPopup("FPSを制限する。", op.fps);
            EditorGUILayout.HelpBox(
                "環境によっては$.onUpdateに対してFPS制限が働きません。", MessageType.Info
            );
            //if (QualitySettings.vSyncCount > 0)
            //{
            //    EditorGUILayout.HelpBox(
            //        "VSYNCが有効なので、$.onUpdateに対してFPS制限が働きません。", MessageType.Warning
            //    );
            //}
            EditorGUI.BeginDisabledGroup(!op.canChangePerspective);
            op.perspective = EditorGUILayout.Toggle("一人称視点", op.perspective);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.HelpBox(
                "三人称視点での各種挙動は参考程度でお願いします。", MessageType.Info
            );

            EditorGUILayout.LabelField("PlayerHandle");
            op.exists = EditorGUILayout.Toggle(new GUIContent("　.exists", "PlayerHandle.existsの値を指定できます。"), op.exists);
            op.userIdfc = EditorGUILayout.TextField(new GUIContent("　.idfc", "PlayerHandle.idfcの値を指定できます。"), op.userIdfc);
            if (!op.userIdfcPattern.IsMatch(op.userIdfc))
            {
                EditorGUILayout.HelpBox(
                    "idfcの形式ではありません。", MessageType.Warning
                );
            }
            if (op.userIdfc == "")
                op.userIdfc = op.DefaultUserIdfc;
            op.userId = EditorGUILayout.TextField(new GUIContent("　.userId", "PlayerHandle.userIdの値を指定できます。"), op.userId);
            if (!op.userIdPattern.IsMatch(op.userId))
            {
                EditorGUILayout.HelpBox(
                    "userIdの形式ではありません。", MessageType.Warning
                );
            }
            if(op.userId == "")
                op.userId = op.DefaultUserId;
            op.userName = EditorGUILayout.TextField(new GUIContent("　.userDisplayName", "PlayerHandle.userDisplayNameの値を指定できます。"), op.userName);
            if (op.userName == "")
                op.userName = op.DefaultUserName;
            op.playerEventRole = (EmulateClasses.EventRole)EditorGUILayout.EnumPopup(new GUIContent("　.getEventRole()", "PlayerHandle.getEventRoleの値を指定できます。"), op.playerEventRole);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("PlayerScript");
            op.playerDevice = (EmulatorOptions.PlayerDevice)EditorGUILayout.EnumPopup(new GUIContent("　デバイス", "PlayerScript.isDesktop/isVr/isMobileの値を指定できます。"), op.playerDevice);
            op.playerOperatingSystem = (EmulatorOptions.PlayerOperatingSystem)EditorGUILayout.EnumPopup(new GUIContent("　OS", "PlayerScript.isWindows/isMacOs/isIos/isAndroidの値を指定できます。"), op.playerOperatingSystem);
            EditorGUILayout.Separator();

            op.isEvent = EditorGUILayout.Toggle(new GUIContent("イベント中とする", "$.isEventの値を指定できます。"), op.isEvent);
            EditorGUILayout.Separator();

            op.callExternalUrl = EditorGUILayout.TextField(new GUIContent("callExternal用URL"), op.callExternalUrl);
            op.limitExternalCall = (EmulateClasses.CallExternalRateLimit)EditorGUILayout.EnumPopup(new GUIContent("　実行回数制限を行う", "連続してテストしたい場合などでOFFにしてください。"), op.limitExternalCall);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("ワールド内課金商品");
            using(var productHeader = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField("名前", GUILayout.Width(100));
                EditorGUILayout.LabelField("商品ID", GUILayout.Width(40));
            }
            op.productInfos = op.productInfos.Select(info =>
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                var name = EditorGUILayout.TextField("", info.productName, GUILayout.Width(100));
                var id = EditorGUILayout.TextField("", info.productId, GUILayout.MinWidth(30));
                if (GUILayout.Button("削除", GUILayout.Width(40)))
                {
                    EditorGUILayout.EndHorizontal();
                    return null;
                }
                EditorGUILayout.EndHorizontal();

                if (info.productId != "" && !op.productIdPattern.IsMatch(info.productId))
                {
                    EditorGUILayout.HelpBox(
                        "商品IDの形式ではありません。", MessageType.Warning
                    );
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.LabelField("公開", GUILayout.Width(25));
                var enable = EditorGUILayout.Toggle("", info.isPublic, GUILayout.Width(25));
                EditorGUILayout.LabelField(String.Format("所持状況：{0}＝", info.plus - info.minus), GUILayout.Width(95));
                var plus = EditorGUILayout.IntField("", info.plus, GUILayout.Width(30));
                EditorGUILayout.LabelField("－", GUILayout.Width(10));
                var minus = EditorGUILayout.IntField("", info.minus, GUILayout.Width(30));
                EditorGUILayout.EndHorizontal();

                return new EmulatorOptions.ProductInfo() {
                    productId = id, productName = name, plus = plus, minus = minus, isPublic = enable
                };
            }).Where(info => info != null).ToArray();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10);
                if (GUILayout.Button("商品を追加する"))
                {
                    var added = op.productInfos.ToList();
                    added.Add(new EmulatorOptions.ProductInfo());
                    op.productInfos = added.ToArray();
                }
            }
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("ギフト");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.LabelField("giftType", GUILayout.Width(60));
            op.giftType = EditorGUILayout.TextField(op.giftType, GUILayout.Width(60));
            GUILayout.Space(20);
            EditorGUILayout.LabelField("price", GUILayout.Width(35));
            op.giftPrice = EditorGUILayout.IntField(op.giftPrice, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10);
                if (GUILayout.Button("ギフトを投げる"))
                {
                    op.SendGift();
                }
            }
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("PlayerStorage");
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10);
                if (GUILayout.Button("保存している内容を削除する"))
                {
                    op.playerStorage = "";
                }
            }
            op.playerStorage = EditorGUILayout.TextField("　内容(コピペ用)", op.playerStorage);
            EditorGUILayout.Separator();

            op.overwriteMouseSensitivity = EditorGUILayout.Slider(new GUIContent("マウス操作感度", "clusterでの感度3(デフォルト値)は2.15ぐらいです"), op.overwriteMouseSensitivity, 0, 4f);

            op.isRayDraw = EditorGUILayout.Toggle("raycastを可視化する。", op.isRayDraw);

            EditorGUILayout.LabelField("$.onReceive失敗時にログ出力する。");
            op.loggingOnReceiveFail = EditorGUILayout.Toggle("　　", op.loggingOnReceiveFail);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("以下はプレビュー開始前に設定してください。");
            op.enable = EditorGUILayout.Toggle("ClusterScriptを実行する。", op.enable);
            op.vrm = (GameObject)EditorGUILayout.ObjectField("動作確認用のVRM", op.vrm, typeof(GameObject), false);
            if (!op.IsVrmPrefab(op.vrm))
            {
                EditorGUILayout.HelpBox(
                    "VRMのPrefabを指定してください。", MessageType.Error
                );
            }
            EditorGUILayout.LabelField(new GUIContent("アバターのコライダーサイズ", "アバターのカプセル状のコライダーが以下の設定に合わせリサイズされます。"));
            op.playerColliderRadius = EditorGUILayout.FloatField(new GUIContent("　半径", "CapsuleColliderの半径です。"), op.playerColliderRadius);
            if (op.playerColliderRadius <= 0)
            {
                op.playerColliderRadius = op.DefaultPlayerColliderRadius;
            }
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(new GUIContent("　高さ", "CapsuleColliderの高さです。\nPreviewSettingsの視点の高さ(立ち)を参照します。"), GUILayout.Width(148));
                EditorGUILayout.TextField(
                    ClusterVR.CreatorKit.Preview.PlayerController.CameraControlSettings.StandingEyeHeight.ToString(),
                    EditorStyles.textField
                );
                if (GUILayout.Button("変更はこちら"))
                {
                    ClusterVR.CreatorKit.Editor.Preview.EditorUI.SettingsWindow.ShowWindow();
                }
            }
            EditorGUILayout.EndHorizontal();
            op.debug = EditorGUILayout.Toggle("デバッグモードで実行する。", op.debug);
            EditorGUILayout.LabelField("　動作が遅くなりますが、ログ出力が詳細になります。");

            op.pauseFrameKey = EditorGUILayout.TextField(new GUIContent("一時停止キー", "$.stateに値を入れた時に、プレビューを一時停止させるキー名"), op.pauseFrameKey);

            op.ignorePackageListUpdate = EditorGUILayout.Toggle("プレビュー起動を早くする。", op.ignorePackageListUpdate);
            EditorGUILayout.LabelField("　以下の処理を抑制します。");
            EditorGUILayout.LabelField(String.Format(
                "　　・「{0}」の処理",
                ClusterVR.CreatorKit.Translation.TranslationTable.cck_package_list_fetch_success
            ));

            EditorGUILayout.EndScrollView();
        }

        void CheckUniVrmVersion()
        {
            if (!isValidUniVrm)
            {
                if (!System.IO.File.Exists(UNIVRAM_PACKAGE))
                {
                    //Debug.Logで出すと出すぎる。出すなら工夫が必要。
                    EditorGUILayout.HelpBox(
                        "UniVRM 0.61.1が必要です。",
                        MessageType.Error
                    );
                }
                else
                {
                    var packageJson = System.IO.File.ReadAllText("Assets/VRM/package.json");
                    var package = JsonUtility.FromJson<UniVrmPackage>(packageJson);

                    if (package.version != "0.61.1")
                    {
                        //Debug.Logで出すと出すぎる。出すなら工夫が必要。
                        EditorGUILayout.HelpBox(
                            "UniVRM 0.61.1が必要です。",
                            MessageType.Error
                        );
                    }
                    else
                    {
                        isValidUniVrm = true;
                    }
                }
            }
        }

    }
}
