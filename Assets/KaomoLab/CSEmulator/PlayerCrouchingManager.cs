using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator
{

    public class PlayerCrouchingManager
    {
        public event Handler OnCrouchCanceled = delegate { };
        public event Handler OnCrouchChanged = delegate { };
        public bool isCrouching { get; private set; } = false;
        public bool canCrouching { get; private set; } = true;
        bool canEndCrouch = true;

        readonly IRawInput rawInput;
        readonly CharacterController characterController;
        readonly Transform rootTransform;

        readonly RaycastHit[] raycastHits = new RaycastHit[10];
        readonly float rayRadius;
        readonly Vector3 rayOffset;
        readonly int layLayer;

        bool jumpKeyPressed = false;

        public PlayerCrouchingManager(
            float rayOffsetY,
            IRawInput rawInput,
            CharacterController characterController
        )
        {
            this.rawInput = rawInput;
            this.characterController = characterController;
            this.rootTransform = characterController.gameObject.transform;

            rayOffset = new Vector3(0, rayOffsetY + characterController.radius, 0);
            rayRadius = characterController.radius;
            layLayer = -1 & ~Commons.BuildLayerMask(15, 16, 23, 24);
        }

        void ChangeCrouch(bool isCrouching)
        {
            bool isChanged = isCrouching != this.isCrouching;
            this.isCrouching = isCrouching;
            if (isChanged) OnCrouchChanged.Invoke();
        }

        public void ToggleCrouch()
        {
            if (!canCrouching) return;
            if (!canEndCrouch && isCrouching) return;
            ChangeCrouch(!isCrouching);
        }

        //ユーザー操作で終了した時
        public void EndCrouch()
        {
            if (!canEndCrouch) return;
            ChangeCrouch(false);
        }

        public void TakeOff()
        {
            canCrouching = false;
            ChangeCrouch(false);
            OnCrouchCanceled.Invoke();
        }

        public void Ground()
        {
            canCrouching = true;
        }

        public void Update()
        {
            CheckChange(rawInput.IsJumpKey, ref jumpKeyPressed,
                () => {
                    EndCrouch();
                    OnCrouchCanceled.Invoke(); //キャンセル扱い
                },
                () => { }
            );

            var hitCount = Physics.SphereCastNonAlloc(
                rayOffset + rootTransform.position, rayRadius, Vector3.up, raycastHits, 2, layLayer, QueryTriggerInteraction.Ignore
            );
            canEndCrouch = true;
            var rayDistance = float.MaxValue;
            for (var i = 0; i < hitCount; i++)
            {
                var hit = raycastHits[i];
                if (hit.distance < 0.3) continue; //実質ありえないので意味がないとする
                if (rayDistance > hit.distance) rayDistance = hit.distance;
            }
            rayDistance += rayRadius * 2;
            if (rayDistance <= 1.201)
            {
                canEndCrouch = false;
            }
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
