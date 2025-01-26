using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator
{
    public class BaseMoveSpeedManager
    {
        enum Speed
        {
            Crouch,
            Walk,
            Run,
            Dash
        }
        Speed speed;
        Speed? reservedSpeed = null;

        readonly float baseSpeed;
        readonly float dashRatio;
        readonly float runRatio;
        readonly float walkRatio;
        readonly float crouchRatio;

        readonly PlayerCrouchingManager playerCrouchingManager;

        readonly IRawInput rawInput;

        bool dashKeyPressed = false;
        bool walkKeyPressed = false;
        bool crouchKeyPressed = false;

        public BaseMoveSpeedManager(
            float baseSpeed,
            float dashRatio,
            float runRatio,
            float walkRatio,
            float crouchRatio,
            PlayerCrouchingManager playerCrouchingManager,
            IRawInput rawInput
        )
        {
            this.rawInput = rawInput;
            this.baseSpeed = baseSpeed;
            this.dashRatio = dashRatio;
            this.runRatio = runRatio;
            this.walkRatio = walkRatio;
            this.crouchRatio = crouchRatio;
            this.playerCrouchingManager = playerCrouchingManager;
            this.speed = Speed.Run;

            playerCrouchingManager.OnCrouchCanceled += () =>
            {
                if (dashKeyPressed) return;
                this.reservedSpeed = Speed.Run;
            };
        }

        public void Update(
            Action<float> OnSpeedChanged
        )
        {
            //後に押された操作が優先される
            var nextSpeed = reservedSpeed ?? speed;
            var isKeyUp = false;
            reservedSpeed = null;

            CheckChange(
                rawInput.IsDashKey, ref dashKeyPressed,
                () => {
                    nextSpeed = Speed.Dash;
                    playerCrouchingManager.EndCrouch();
                },
                () => isKeyUp = true
            );

            CheckChange( //crouch中の歩きはキー入力だけ受け付けられる
                rawInput.IsWalkKey, ref walkKeyPressed,
                () => { if (!playerCrouchingManager.isCrouching) nextSpeed = Speed.Walk; },
                () => { if (!playerCrouchingManager.isCrouching) isKeyUp = true; }
            );

            if (isKeyUp)
            {
                nextSpeed = Speed.Run;
                if (dashKeyPressed) nextSpeed = Speed.Dash;
                if (walkKeyPressed) nextSpeed = Speed.Walk;
            }

            CheckChange(
                rawInput.IsCrouchKey, ref crouchKeyPressed,
                () => {
                    if (!playerCrouchingManager.canCrouching) return;
                    playerCrouchingManager.ToggleCrouch();
                    if (playerCrouchingManager.isCrouching)
                        nextSpeed = Speed.Crouch;
                    if (!playerCrouchingManager.isCrouching)
                        nextSpeed = Speed.Run; //shilt/altの状況に関わらずrunになる
                },
                () => { }
            );

            if (nextSpeed == speed) return;

            speed = nextSpeed;
            var ratio = speed switch
            {
                Speed.Crouch => crouchRatio,
                Speed.Walk => walkRatio,
                Speed.Run => runRatio,
                Speed.Dash => dashRatio,
                _ => throw new Exception("開発者のミス。開発者に連絡してください。")
            };
            OnSpeedChanged(baseSpeed * ratio);
        }

        void CheckChange(Func<bool> IsKey, ref bool pressed, Action OnDown, Action OnUp)
        {
            if (IsKey())
            {
                if (!pressed)
                {
                    pressed = true;
                    OnDown();
                }
            }
            else
            {
                if (pressed)
                {
                    pressed = false;
                    OnUp();
                }
            }
        }

    }
}
