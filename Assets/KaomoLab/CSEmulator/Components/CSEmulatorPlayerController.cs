#if UNITY_EDITOR
using ClusterVR.CreatorKit.Preview.PlayerController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Components
{
    [DisallowMultipleComponent, RequireComponent(typeof(IPlayerController)), RequireComponent(typeof(CharacterController))]
    public class CSEmulatorPlayerController
        : MonoBehaviour
    {
        public event Handler OnPlayerColliderChanged = delegate { };

        const string RIGHT_HAND_UP = "IsRightHandUp";
        const string LEFT_HAND_UP = "IsLeftHandUp";
        const string WALKING = "IsWalking";
        const string DIRECTION_FORWARD = "DirectionForward";
        const string DIRECTION_RIGHT = "DirectionRight";
        const string MOVE_SPEED_RATIO = "SpeedRatio";

        CharacterController characterController;
        public Vector3 velocity { get; private set; } = Vector3.zero;
        public float gravity { get; set; } = Commons.STANDARD_GRAVITY;
        bool isVelocityAdded = false;
        Vector3 movingPlatformHorizontalInertia = Vector3.zero;
        Vector3 movingPlatformVerticalInertia = Vector3.zero;

        Vector3 prevPosition = new Vector3(0, 0, 0);
        float baseSpeed;
        MovingAveragerFloat speedAverage = new MovingAveragerFloat(10);

        bool isRightHandUp = false;

        ClusterVR.CreatorKit.Item.Implements.RidableItem prevRidableItem = null;

        KeyWalkManager walkManager;
        BaseMoveSpeedManager speedManager;
        public FaceConstraintManager faceConstraintManager { get; private set; } = null;
        PlayerGroundingManager playerGroundingManager;
        PlayerMantlingManager playerMantlingManager;
        PlayerCrouchingManager playerCrouchingManager;
        public PointOfViewManager pointOfViewManager { get; private set; } = null;
        bool needPointOfViewCommandBufferRead = false;
        public HumanPoseManager poseManager { get; private set; } = null;
        HumanPoseManager.RidingPoseManager ridingPoseManager;

        public Animator animator
        {
            get
            {
                if (_animator == null)
                    _animator = gameObject
                        .GetComponentInChildren<VRM.VRMMeta>()
                        .GetComponentInChildren<Animator>();
                return _animator;
            }
        }
        Animator _animator = null;

        MovingPlatformSettings movingPlatformSettings;
        MantlingSettings mantlingSettings;
        HudSettings hudSettings;
        ClippingPlanesSettings clippingPlanesSettings;
        IVelocityYHolder cckPlayerVelocityY;
        IBaseMoveSpeedHolder cckPlayerBaseMoveSpeed;
        IPlayerRotateHandler playerRotateHandler;
        IPlayerFaceController playerFaceController;
        IRidingHolder ridingHolder;
        IMouseEventEmitter mouseEventEmitter;
        IPerspectiveChangeNotifier perspectiveChangeNotifier;
        IPlayerMeasurementsHolder playerMeasurementsHolder;
        IGrabController grabController;
        IVrmIKNotifier vrmIKNotifier;
        IHumanoidAnimationCreator humanoidAnimationCreator;
        IRawInput rawInput;

        public void Construct(
            MovingPlatformSettings movingPlatformSettings,
            MantlingSettings mantlingSettings,
            HudSettings hudSettings,
            ClippingPlanesSettings clippingPlanesSettings,
            CharacterController characterController,
            RuntimeAnimatorController animatorController,
            IVelocityYHolder cckPlayerVelocityY,
            IBaseMoveSpeedHolder cckPlayerBaseMoveSpeed,
            IPlayerRotateHandler playerRotateHandler,
            IPlayerFaceController playerFaceSetter,
            IRidingHolder ridingHolder,
            IMouseEventEmitter mouseEventEmitter,
            IPerspectiveChangeNotifier perspectiveChangeNotifier,
            IPlayerMeasurementsHolder playerMeasurementsHolder,
            IGrabController grabController,
            IVrmIKNotifier vrmIKNotifier,
            IHumanoidAnimationCreator humanoidAnimationCreator,
            IRawInput rawInput
        )
        {
            this.movingPlatformSettings = movingPlatformSettings;
            this.mantlingSettings = mantlingSettings;
            this.hudSettings = hudSettings;
            this.clippingPlanesSettings = clippingPlanesSettings;
            this.characterController = characterController;
            this.cckPlayerVelocityY = cckPlayerVelocityY;
            this.cckPlayerBaseMoveSpeed = cckPlayerBaseMoveSpeed;
            this.playerRotateHandler = playerRotateHandler;
            this.playerFaceController = playerFaceSetter;
            this.ridingHolder = ridingHolder;
            this.mouseEventEmitter = mouseEventEmitter;
            this.humanoidAnimationCreator = humanoidAnimationCreator;
            this.animator.runtimeAnimatorController = animatorController;
            this.walkManager = new KeyWalkManager(
                rawInput
            );
            playerCrouchingManager = new PlayerCrouchingManager(
                0.001f,
                rawInput,
                characterController
            );
            this.baseSpeed = cckPlayerBaseMoveSpeed.value;
            this.speedManager = new BaseMoveSpeedManager(
                baseSpeed,
                //2.96実測値+2.139計測値(crouch)
                1.8f, 1.0f, 0.5f, 0.5f, playerCrouchingManager, rawInput
            );
            animator.SetFloat(MOVE_SPEED_RATIO, 1.0f);
            this.poseManager = new HumanPoseManager(
                animator
            );
            var defaultRideAnimationClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(
                "Assets/KaomoLab/CSEmulator/Components/Animations/Ride.anim"
            );
            this.ridingPoseManager = new HumanPoseManager.RidingPoseManager(
                humanoidAnimationCreator,
                animator,
                defaultRideAnimationClip
            );
            this.faceConstraintManager = new FaceConstraintManager(
                hudSettings,
                isFaceConstraintForward =>
                {
                    walkManager.ConstraintFaceForward(isFaceConstraintForward);
                }
            );
            this.perspectiveChangeNotifier = perspectiveChangeNotifier;
            perspectiveChangeNotifier.OnChanged += PerspectiveChangeNotifier_OnValueChanged;
            this.playerMeasurementsHolder = playerMeasurementsHolder;
            this.rawInput = rawInput;

            ApplyCharacterController();

            //slopeLimitとstepOffsetの挙動を見ると
            //CharacterControllerとRigidbody(+CapsuleCollider)の併用は
            //ほぼ間違いないように思える
            AddCapsuleCollider();

            playerGroundingManager = new PlayerGroundingManager(
                0.0001f,
                characterController,
                playerRotateHandler.rootTransform
            );
            playerMantlingManager = new PlayerMantlingManager(
                rawInput,
                characterController,
                playerFaceSetter.vrmRotateRoot
            );

            AddThridPersonCamera();
            mouseEventEmitter.OnMoved += MouseEventEmitter_OnMoved;

            this.grabController = grabController;
            this.vrmIKNotifier = vrmIKNotifier;
            this.vrmIKNotifier.OnIK += VrmIKNotifier_OnIK;

            if (movingPlatformSettings.UseMovingPlatform)
                UnityEngine.Debug.Log("移動する床に追従する挙動はCSEmulatorにより少しは再現できているかもしれません。");
            if (mantlingSettings.UseMantling)
                UnityEngine.Debug.Log("よじ登る挙動はCSEmulatorにより少しは再現できているかもしれません。");

            //遅れて反映させることで、ItemHighlighterの謎挙動を一旦回避
            StartCoroutine("LazyApplyPerspective");
        }

        private void ApplyCharacterController()
        {
            characterController.height = playerMeasurementsHolder.height;
            characterController.radius = playerMeasurementsHolder.radius;
            characterController.center = new Vector3(
                0, characterController.height / 2 + characterController.skinWidth, 0
            );
        }
        private void AddCapsuleCollider()
        {
            //Colliderが2つになる…
            //＞getOverlapsは、今(cluster2.95)のところ2つ返すのでOK。
            //＞getPlayersNearは、1つしか返さないので要調整。
            var col = gameObject.AddComponent<CapsuleCollider>();
            col.center = characterController.center;
            //cc側に衝突処理を持っていかれることがあるので+0.01
            //skinWidthより小さい値にしたのが良かったのかもしれない(未検証)
            col.height = characterController.height + 0.01f;
            col.radius = characterController.radius + 0.01f;
            var rb = gameObject.AddComponent<Rigidbody>();
            //この辺の設定をしておくとccと併用できそう
            rb.useGravity = false;
            rb.mass = 0;
            rb.drag = 0;
            rb.angularDrag = 0;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            var ccHeight = characterController.height;
            var ccCenter = characterController.center;
            var colHeight = col.height;
            var colCenter = col.center;
            playerCrouchingManager.OnCrouchChanged += () =>
            {
                var heightOffset = 0.2f * (playerCrouchingManager.isCrouching ? 1 : 0);
                var centerOffset = Vector3.up * 0.1f * (playerCrouchingManager.isCrouching ? 1 : 0);
                characterController.height = ccHeight - heightOffset;
                col.height = colHeight - heightOffset;
                characterController.center = ccCenter - centerOffset;
                col.center = colCenter - centerOffset;
                OnPlayerColliderChanged.Invoke();
            };
        }
        private void AddThridPersonCamera()
        {
            var cameraObject = gameObject.transform.Find("Root/MainCamera").gameObject;
            var contactableItemRaycaster = gameObject.GetComponent<ClusterVR.CreatorKit.Preview.Item.ContactableItemRaycaster>();
            var contactableItemRaycaster_targetCamera = typeof(ClusterVR.CreatorKit.Preview.Item.ContactableItemRaycaster).GetField("targetCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var itemHighlighter = gameObject.GetComponent<ClusterVR.CreatorKit.Preview.Item.ItemHighlighter>();
            var itemHighlighter_targetCamera = typeof(ClusterVR.CreatorKit.Preview.Item.ItemHighlighter).GetField("targetCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var vector = gameObject.transform.Find("Root/MainCamera/Vector").gameObject;
            pointOfViewManager = new PointOfViewManager(
                3, //実測値
                clippingPlanesSettings.useCustomClippingPlanes ? clippingPlanesSettings.nearPlane : null,
                clippingPlanesSettings.useCustomClippingPlanes ? clippingPlanesSettings.farPlane : null,
                cameraObject,
                gameObject.GetComponentsInChildren<Collider>(),
                firstCamera =>
                {
                    vector.SetActive(true);
                    contactableItemRaycaster_targetCamera.SetValue(contactableItemRaycaster, firstCamera);
                    itemHighlighter_targetCamera.SetValue(itemHighlighter, firstCamera);
                },
                thridCamera =>
                {
                    vector.SetActive(false);
                    contactableItemRaycaster_targetCamera.SetValue(contactableItemRaycaster, thridCamera);
                    itemHighlighter_targetCamera.SetValue(itemHighlighter, thridCamera);
                }
            );
            needPointOfViewCommandBufferRead = true;
        }
        private System.Collections.IEnumerator LazyApplyPerspective()
        {
            yield return new WaitForSeconds( 1.0f );
            perspectiveChangeNotifier.RequestNotify();
        }

        private void PerspectiveChangeNotifier_OnValueChanged(bool data)
        {
            faceConstraintManager.ChangePerspective(data);
            pointOfViewManager?.ChangeView(data);
        }

        private void VrmIKNotifier_OnIK(int layerIndex)
        {
            //CSETODO マウスで手を動かすやつそのうち実装
            if (grabController.isGrab && !isRightHandUp)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, grabController.grabPoint);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            }
        }

        public void AddVelocity(Vector3 velocity)
        {
            this.velocity += velocity;
            isVelocityAdded = true;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.collider.gameObject.IsChild(characterController.gameObject)) return;

            playerGroundingManager.ApplyStayCollision(collision);
            playerMantlingManager.ApplyStayCollision(collision);
        }

        private void MouseEventEmitter_OnMoved(Vector2 delta)
        {
            if (pointOfViewManager.NeedRotationCancel())
            {
                playerRotateHandler.RotateDelta(-delta);
            }
        }

        private void Update()
        {
            //ItemHighligherでの登録がStartで行われているので
            if (needPointOfViewCommandBufferRead)
            {
                pointOfViewManager.SetupCommandBuffer();
                needPointOfViewCommandBufferRead = false;
            }

            var p = gameObject.transform.position;
            var speedRate = (prevPosition - p).magnitude / Time.deltaTime / baseSpeed;
            //平均にしないとめっちゃ震えるので
            speedAverage.Push(speedRate);
            if(speedAverage.hasAverage)
                animator.SetFloat(MOVE_SPEED_RATIO, speedAverage.average);
            prevPosition = p;

            pointOfViewManager.UpdateThirdPersonCameraPosition();
        }

        private void LateUpdate()
        {
            //OnUpdateといった形でeventを外に出してはいけないとする。
            //複数インスタンスがOnUpdateを購読した場合
            //そのインスタンス数分だけ処理が重複してしまう可能性がある。
            //その危険性を排除したい。

            ApplyGroundingAndMovingPlatform(); //velocity弄るので最初
            ApplyAdditionalGravity();
            ApplyAdditionalVelocity();
            ApplyBaseMoveSpeed();
            //https://docs.unity3d.com/ja/2019.4/ScriptReference/CharacterController.SimpleMove.html
            //1フレームで複数回呼ぶのを推奨していないけどもしかたない
            characterController.Move(velocity * Time.deltaTime);
            ApplyMantling(); //登り後の位置に上書きするのでMoveの後
            ApplyCrouching(); //とりあえずここにしてみた

            //歩きモーションの後にポーズを上書きするという挙動
            //つまり歩きモーションがポーズのリセット兼ねている
            ApplyAnimation();
            poseManager.Apply();
            //ここの処理は苦肉の策でRideをイベントとして認識するためには、
            //全RidableItemのGetOnをListenする必要があった。
            //別で管理しても良かったかもしれないが、今のところ使用するのがここだけなので、
            //他に使うところが出てきたら考える。
            if (prevRidableItem != ridingHolder.ridableItem)
            {
                if (ridingHolder.isRiding)
                {
                    var clip = ((ClusterVR.CreatorKit.Item.IRidableItem)ridingHolder.ridableItem).AvatarOverrideAnimation;
                    ridingPoseManager.GetOn(clip);
                    faceConstraintManager.ChangeRiding(true);
                    prevRidableItem = ridingHolder.ridableItem;
                }
                else
                {
                    ridingPoseManager.GetOff();
                    faceConstraintManager.ChangeRiding(false);
                    prevRidableItem = null;
                }
            }
            var seat = ridingHolder.ridableItem?.Seat;
            if (seat != null)
            {
                var p = seat.position;
                var r = seat.rotation;
                ridingPoseManager.SetHip(p, r);
            }
            //座りながら手を動かすことができないけども、座りモーションを再現できる方が大事という判断。
            //もし手の動きを何とかするならridingPoseManagerを統合する。
            ridingPoseManager.Apply(); 
            grabController.ApplyUpdate(); //手の位置を確定してから持ち物の位置を確定する
        }
        void ApplyGroundingAndMovingPlatform()
        {
            if (movingPlatformSettings.UseMovingPlatform)
            {
                playerGroundingManager.UpdateGrounded((isGrounded, delta) =>
                {
                    if (!isGrounded) return;
                    gameObject.transform.position += delta.position;
                    prevPosition += delta.position;
                    playerRotateHandler.rootTransform.rotation = Quaternion.Euler(
                        0, (delta.rotation * playerRotateHandler.rootTransform.rotation).eulerAngles.y, 0
                    );
                });
                if (playerGroundingManager.IsTakingOff())
                {
                    movingPlatformHorizontalInertia = playerGroundingManager.GetHorizontalInertia();
                    movingPlatformVerticalInertia = playerGroundingManager.GetVerticalInertia();
                    if (!movingPlatformSettings.MovingPlatformHorizontalInertia)
                        movingPlatformHorizontalInertia = Vector3.zero;
                    if (!movingPlatformSettings.MovingPlatformVerticalInertia)
                        movingPlatformVerticalInertia = Vector3.zero;
                    velocity += movingPlatformHorizontalInertia;
                    velocity += movingPlatformVerticalInertia;
                    if (movingPlatformHorizontalInertia != Vector3.zero || movingPlatformVerticalInertia != Vector3.zero)
                        isVelocityAdded = true;
                }
                if (playerGroundingManager.IsGrounding())
                {
                    //CCK2.11.0着地時に移動床からの慣性分はキャンセルされるっぽい
                    if (movingPlatformHorizontalInertia == Vector3.zero && movingPlatformVerticalInertia == Vector3.zero)
                        return;
                    velocity -= movingPlatformHorizontalInertia;
                    velocity -= movingPlatformVerticalInertia;
                    movingPlatformHorizontalInertia = Vector3.zero;
                    movingPlatformVerticalInertia = Vector3.zero;
                }
            }
            else
            {
                //crouchで使う
                playerGroundingManager.UpdateGrounded((isGrounded, delta) => { });
            }
        }
        void ApplyAdditionalGravity()
        {
            if (gravity != CSEmulator.Commons.STANDARD_GRAVITY)
            {
                //DesktopPlayerControllerの重力加速度が決め打ちなので
                //ここで追計算する必要がある。
                var delta = Time.deltaTime * (gravity - CSEmulator.Commons.STANDARD_GRAVITY);
                cckPlayerVelocityY.value += delta;
            }
            if (playerCrouchingManager.isCrouching)
            {
                cckPlayerVelocityY.value = 0; //crouch時はジャンプしない
            }
        }
        void ApplyAdditionalVelocity()
        {
            //抵抗の係数。接地してたら摩擦の方で大きくなるという発想。
            //現地調査の結果これが一番近い。
            //調査方法：垂直飛び、接地からの横、上空からの横の３パターンで到達点を比較。
            var k = characterController.isGrounded ? 0.013f : 0.00f;
            velocity -= velocity * k;

            if (!isVelocityAdded && characterController.isGrounded)
            {
                //着地した時に上方向の速度が残っていると跳ねる。
                velocity = new Vector3(velocity.x, 0, velocity.z);
            }
            isVelocityAdded = false;

        }
        void ApplyBaseMoveSpeed()
        {
            speedManager.Update(
                speed =>
                {
                    cckPlayerBaseMoveSpeed.value = speed;
                }
            );
        }
        void ApplyMantling()
        {
            if (!mantlingSettings.UseMantling) return;
            if (playerCrouchingManager.isCrouching) return;

            playerMantlingManager.CheckMantling((pos) =>
            {
                characterController.enabled = false;
                characterController.transform.position = pos;
                characterController.enabled = true;
            });

        }
        void ApplyCrouching()
        {
            playerCrouchingManager.Update();
            if (playerGroundingManager.IsTakingOff())
            {
                playerCrouchingManager.TakeOff();
            }
            if (playerGroundingManager.IsGrounding())
            {
                playerCrouchingManager.Ground();
            }
        }
        void ApplyAnimation()
        {
            if (Input.GetKeyDown(KeyCode.C) && !grabController.isGrab)
            {
                isRightHandUp = true;
                animator.SetBool(RIGHT_HAND_UP, true);
            }
            if (Input.GetKeyUp(KeyCode.C))
            {
                isRightHandUp = false;
                animator.SetBool(RIGHT_HAND_UP, false);
            }

            if (Input.GetKeyDown(KeyCode.Z))
                animator.SetBool(LEFT_HAND_UP, true);
            if (Input.GetKeyUp(KeyCode.Z))
                animator.SetBool(LEFT_HAND_UP, false);

            animator.SetLayerWeight(animator.GetLayerIndex("IdleAndWalk"), playerCrouchingManager.isCrouching ? 0f : 1f);
            animator.SetLayerWeight(animator.GetLayerIndex("Crouch"), playerCrouchingManager.isCrouching ? 1f : 0f);

            //CSETODO walkManager周りの処理を整理すれば以下の処理も簡潔になるはず。
            if (faceConstraintManager.isConstraintForward)
            {
                walkManager.Update(
                    (_, _) => { }, _ => { }, _ => { }
                );
                playerFaceController.SetBaseRotate(playerRotateHandler.rootTransform.rotation.eulerAngles.y);
                playerFaceController.SetFaceForward(1);
                playerFaceController.SetFaceRight(0);
            }
            else
            {
                playerFaceController.SetBaseRotate(0);
                walkManager.Update(
                    (forward, right) => {
                        playerFaceController.SetBaseRotate(playerRotateHandler.rootTransform.rotation.eulerAngles.y);
                        playerFaceController.SetFaceForward(forward);
                        playerFaceController.SetFaceRight(right);
                    },
                    forwardDirection => {
                        playerFaceController.SetFaceForward(forwardDirection);
                    },
                    rightDirection =>
                    {
                        playerFaceController.SetFaceRight(rightDirection);
                    }
                );
            }
        }

        public void RunCoroutine(Func<System.Collections.IEnumerator> Coroutine)
        {
            StartCoroutine(Coroutine());
        }

        public void ChangeGrabbing(bool isGrab)
        {
            faceConstraintManager.ChangeGrabbing(isGrab);
        }
        public void ChangePerspective(bool isFirstPerson)
        {
            faceConstraintManager.ChangePerspective(isFirstPerson);
        }

        public void ForceForward()
        {
            walkManager.ForceForward();
        }

        public Quaternion GetPlayerRotation()
        {
            var d = playerFaceController.GetNowRotate();
            var ret = Quaternion.Euler(0, d, 0);
            return ret;
        }

        public int GetMovementFlags()
        {
            //よじ登りがワープになっている関係で、よじ登り後のインターバル中をよじ登り中としている
            var ret =
                (playerGroundingManager.IsGrounded() ? 0x0001 : 0x000)
                | (!playerMantlingManager.CanMantling() ? 0x0002 : 0x000)
                | (ridingHolder.isRiding ? 0x0004 : 0x000)
            ;
            return ret;
        }

        private void OnDestroy()
        {
            perspectiveChangeNotifier.OnChanged -= PerspectiveChangeNotifier_OnValueChanged;
        }
    }
}
#endif