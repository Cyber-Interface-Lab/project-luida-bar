using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class VrmPreparer
    {
        public class HumanoidAnimationCreator : IHumanoidAnimationCreator
        {
            public ClusterVR.CreatorKit.Item.Implements.HumanoidAnimation Create(AnimationClip clip)
            {
                var ret = ClusterVR.CreatorKit.Editor.Builder.HumanoidAnimationBuilder.Build(clip);
                return ret;
            }
        }

        readonly CckPreviewFinder previewFinder;
        readonly DesktopPlayerControllerReflector desktopPlayerControllerReflector;
        readonly GameObject vrm;
        readonly EmulateClasses.IPlayerMeta playerMeta;
        readonly Components.IPerspectiveChangeNotifier perspectiveChangeNotifier;
        readonly Components.IPlayerMeasurementsHolder playerMeasurementsHolder;

        public Components.CSEmulatorPlayerController csPlayerController;

        public VrmPreparer(
            CckPreviewFinder previewFinder,
            DesktopPlayerControllerReflector desktopPlayerControllerReflector,
            GameObject vrm,
            EmulateClasses.IPlayerMeta playerMeta,
            Components.IPerspectiveChangeNotifier perspectiveChangeNotifier,
            Components.IPlayerMeasurementsHolder playerMeasurementsHolder
        )
        {
            this.previewFinder = previewFinder;
            this.desktopPlayerControllerReflector = desktopPlayerControllerReflector;
            this.vrm = vrm;
            this.playerMeta = playerMeta;
            this.perspectiveChangeNotifier = perspectiveChangeNotifier;
            this.playerMeasurementsHolder = playerMeasurementsHolder;
        }

        public GameObject InstantiateVrm()
        {
            //挙動的にRootの下がよさそう＞Controllerの下にした
            //＞三人称視点での体の回転はanimationではなく単純にrotationの模様。
            //＞一人称視点もFaceConstraintで制御するのでRootの回転に乗っかる必要なしと判断し、Rootから離脱した。
            //＞もっと挙動を整理すればCCK系の関数で実現できるかもしれないが今はこれで
            var vrmParent = new GameObject("CSEmulatorVrmYRotation");
            vrmParent.transform.SetParent(previewFinder.controller.transform, false);
            var vrmInstance = GameObject.Instantiate(vrm, vrmParent.transform);

            //VenueLayer系衝突判定用のコライダー
            var avatarCollider = vrmInstance.AddComponent<CapsuleCollider>();
            vrmInstance.layer = 16; //OwnAvatar

            //プレビューのカメラに映ってしまうので
            var renderers = vrmInstance.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach(var renderer in renderers)
            {
                renderer.gameObject.layer = 16; //OwnAvatar
            }

            var csPlayerHandler = vrmInstance.AddComponent<Components.CSEmulatorPlayerHandler>();
            //CSETODO V3でcomponent側にIPlayerMetaを移すのを検討する
            csPlayerHandler.Construct(playerMeta.userIdfc);
            var characterController = previewFinder.previewRoot.GetComponentInChildren<CharacterController>();
            csPlayerController = characterController.gameObject.AddComponent<Components.CSEmulatorPlayerController>();
            var animationController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/KaomoLab/CSEmulator/Components/Animations/Player.controller"
            );
            var playerFaceController = new PlayerFaceController(vrmParent);
            var desktopItemControllerReflector = new DesktopItemControllerReflector(
                vrmInstance.GetComponentInParent<ClusterVR.CreatorKit.Preview.PlayerController.DesktopItemController>()
            );
            var worldRuntimeSettings = ClusterVR.CreatorKit.Editor.Builder.WorldRuntimeSettingGatherer.GatherWorldRuntimeSettings(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene()
            ).FirstOrDefault();
            var grabController = new GrabOverwriter(
                new HudSettings(worldRuntimeSettings),
                previewFinder.previewRoot, previewFinder.grabPoint,
                vrmInstance.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand).gameObject,
                desktopItemControllerReflector
            );
            var crouchSettings = new CrouchSettings(worldRuntimeSettings);
            csPlayerController.Construct(
                new MovingPlatformSettings(worldRuntimeSettings),
                new MantlingSettings(worldRuntimeSettings),
                new HudSettings(worldRuntimeSettings),
                new ClippingPlanesSettings(worldRuntimeSettings),
                characterController,
                animationController,
                desktopPlayerControllerReflector,
                desktopPlayerControllerReflector,
                desktopPlayerControllerReflector,
                playerFaceController,
                desktopPlayerControllerReflector,
                desktopPlayerControllerReflector,
                perspectiveChangeNotifier,
                playerMeasurementsHolder,
                grabController,
                csPlayerHandler,
                new HumanoidAnimationCreator(),
                new Implements.UnityKeyInput(() => crouchSettings.enableCrouchWalk)
            );

            //ポーズをA-Poseにする。アニメーション来たら多分不要。
            ResetAPose(csPlayerController);

            BuildPostProcess(previewFinder.previewRoot);

            AddjustAvatarCollider(avatarCollider, characterController);
            csPlayerController.OnPlayerColliderChanged += () =>
            {
                AddjustAvatarCollider(avatarCollider, characterController);
            };

            return vrmInstance;
        }

        void BuildPostProcess(GameObject root)
        {
            var postProcessObject = new GameObject("CSEmulatorPostProcess");
            postProcessObject.layer = 21;
            postProcessObject.transform.parent = root.transform;
            var postProcessVolume = postProcessObject.AddComponent<UnityEngine.Rendering.PostProcessing.PostProcessVolume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 100;
            //プロファイルは実行時に変更するとロールバックされないのでCreateInstanceで追加
            var postProcessProfile = ScriptableObject.CreateInstance<UnityEngine.Rendering.PostProcessing.PostProcessProfile>();
            //activeだと未設定でも有効になる効果がある(DepthOfFieldとか)のでfalse指定をしている。
            postProcessProfile.AddSettings<UnityEngine.Rendering.PostProcessing.Bloom>().active = false;
            postProcessProfile.AddSettings<UnityEngine.Rendering.PostProcessing.ChromaticAberration>().active = false;
            postProcessProfile.AddSettings<UnityEngine.Rendering.PostProcessing.ColorGrading>().active = false;
            postProcessProfile.AddSettings<UnityEngine.Rendering.PostProcessing.DepthOfField>().active = false;
            //FogはRenderSettings？
            //RenderSettingsは実行時に上書きするとロールバックされるのでそのままいじってよし
            postProcessProfile.AddSettings<UnityEngine.Rendering.PostProcessing.Grain>().active = false;
            postProcessProfile.AddSettings<UnityEngine.Rendering.PostProcessing.LensDistortion>().active = false;
            postProcessProfile.AddSettings<UnityEngine.Rendering.PostProcessing.MotionBlur>().active = false;
            postProcessProfile.AddSettings<UnityEngine.Rendering.PostProcessing.Vignette>().active = false;
            postProcessVolume.profile = postProcessProfile;
        }

        void ResetAPose(Components.CSEmulatorPlayerController csPlayerController)
        {
            var poseHandler = new HumanPoseHandler(
                csPlayerController.animator.avatar,
                csPlayerController.animator.transform
            );
            var humanPose = new HumanPose();
            poseHandler.GetHumanPose(ref humanPose);
            humanPose.muscles = EmulateClasses.Muscles.SPOSE.ToArray();
            poseHandler.SetHumanPose(ref humanPose);
        }

        void AddjustAvatarCollider(CapsuleCollider avatarCollider, CharacterController source)
        {
            avatarCollider.center = source.center;
            //CharacterControllerより大きいとOnCollide誤爆があるため
            avatarCollider.height = source.height - 0.02f;
            avatarCollider.radius = source.radius - 0.02f;
        }
    }
}
