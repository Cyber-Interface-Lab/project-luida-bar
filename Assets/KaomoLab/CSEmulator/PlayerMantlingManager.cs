using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator
{

    public class PlayerMantlingManager
    {
        public class GroundedChecker
        {
            readonly RaycastHit[] raycastHits = new RaycastHit[5];
            readonly Vector3 rayOffset;
            readonly float rayDistance;

            public bool isGrounded { get; private set; } = false;
            public Vector3 lastGroundedPoint {  get; private set; } = new Vector3();

            public GroundedChecker(
                float rayOffsetY,
                float capsuleRadius,
                float capsuleSkinWidth
            )
            {
                rayOffset = new Vector3(0, rayOffsetY + capsuleRadius, 0);
                rayDistance = capsuleRadius * 1.415f + rayOffsetY + capsuleSkinWidth; //45度(√2)まで接地判定
            }

            public void CheckGrounded(Vector3 origin)
            {
                //ここの処理はMovingPlatformや以前の0.2秒空中ジャンプあたりで実装するのが正かもしれない
                //しかしエディタ拡張のファイル管理の関係で面倒そうな予感しかしない
                //そのためここで実装して使い切り

                var hitCount = Physics.RaycastNonAlloc(
                    origin + rayOffset, Vector3.down, raycastHits, rayDistance, -1, QueryTriggerInteraction.Ignore
                );

                if (hitCount == 0)
                {
                    isGrounded = false;
                    return;
                }

                var hit = raycastHits
                    .Take(hitCount)
                    .OrderBy(h => h.distance)
                    .First();

                isGrounded = true;
                lastGroundedPoint = hit.point;
            }
        }

        public class MantlingPointer
        {
            readonly float maxLimit;
            readonly float minLimit;
            readonly float jumpHeight;
            readonly float jumpMargin;
            readonly float rayDistance;

            readonly RaycastHit[] raycastHits = new RaycastHit[5];
            readonly GameObject rayOrigin;
            readonly GameObject mantlingRoot;

            public MantlingPointer(
                float maxLimit,
                float minLimit,
                float jumpHeight,
                float jumpMargin,
                float capsuleRadius,
                float capsuleSkinWidth,
                Transform root
            )
            {
                this.maxLimit = maxLimit;
                this.minLimit = minLimit;
                this.jumpHeight = jumpHeight;
                this.jumpMargin = jumpMargin;
                this.rayDistance = this.maxLimit + jumpHeight + jumpMargin - this.minLimit;

                mantlingRoot = new GameObject("CSEmulatorMantling");
                mantlingRoot.transform.parent = root;
                mantlingRoot.transform.localPosition = Vector3.zero;

                rayOrigin = new GameObject("CSEmulatorMantlingRayOrigin");
                rayOrigin.transform.parent = mantlingRoot.transform;
                rayOrigin.transform.localPosition = new Vector3(
                    0,
                    rayDistance + this.minLimit,
                    capsuleRadius * 1.415f + capsuleSkinWidth //45度(√2)まで登れる
                );
            }

            public Vector3? GetMantlingPoint(Collider[] colliders, float groundedY, bool isJumping)
            {
                var hitCount = Physics.RaycastNonAlloc(
                    rayOrigin.transform.position, Vector3.down, raycastHits, rayDistance, -1, QueryTriggerInteraction.Ignore
                );

                if (hitCount == 0) return null;

                var validHits = raycastHits
                    .Take(hitCount)
                    .Where(h =>
                    {
                        return colliders.Contains(h.collider);
                    })
                    .Where(h =>
                    {
                        if(isJumping)
                        {
                            //ジャンプしても登れない高さ
                            if((h.point.y - groundedY) >= maxLimit + jumpHeight)
                                return false;

                            //ジャンプの頂点でcastできないのでマージンを入れて判定を甘くしている
                            return h.distance /*- jumpMargin*/ - jumpHeight >= 0;
                        }
                        else
                        {
                            //接地時はマージンを抜いてきっちり判定
                            return h.distance - jumpMargin - jumpHeight > 0;
                        }
                    })
                    .OrderBy(h => h.distance)
                    .ToArray();

                if (validHits.Length == 0) return null;

                var targetHit = validHits[0];
                var degree = Vector3.Angle(targetHit.normal, Vector3.down);
                if (degree <= 135) return null; //45度以上は登れない

                return targetHit.point;
            }
        }

        public class SpaceChecker
        {
            readonly float heightLimit;

            public SpaceChecker(
                float heightLimit
            )
            {
                this.heightLimit = heightLimit;
            }

            public bool CheckHeight(Vector3 position)
            {
                var hit = Physics.Raycast(position, Vector3.up, heightLimit);
                return !hit;
            }
        }

        readonly GroundedChecker groundedChecker;
        readonly MantlingPointer mantlingPointer;
        readonly SpaceChecker spaceChecker;

        readonly IRawInput rawInput;
        readonly CharacterController characterController;

        readonly FrameOnlyList<Collider> colliders = new FrameOnlyList<Collider>();

        //ワープで登る実装のため、連続して登ってしまうと違和感がある。対策として0.5秒のインターバルを設ける
        readonly static long mantlingInterval = TimeSpan.TicksPerSecond / 2;
        long lastMantlingTicks = -1;

        public PlayerMantlingManager(
            IRawInput rawInput,
            CharacterController characterController,
            Transform rotationRoot
        )
        {
            this.rawInput = rawInput;
            this.characterController = characterController;

            groundedChecker = new GroundedChecker(
                0.0001f,
                characterController.radius,
                characterController.skinWidth
            );
            mantlingPointer = new MantlingPointer(
                1.6f, 0.38f, 0.8f, 0.05f,
                characterController.radius,
                characterController.skinWidth,
                rotationRoot
            );
            spaceChecker = new SpaceChecker(
                1.0f
            );
        }

        public void ApplyStayCollision(Collision collision)
        {
            if (colliders.Contains(collision.collider)) return;
            colliders.Add(collision.collider);
        }

        public bool CanMantling()
        {
            var nowTicks = DateTime.Now.Ticks;
            if ((nowTicks - lastMantlingTicks) < mantlingInterval) return false;
            return true;
        }

        public void CheckMantling(Action<Vector3> Callback)
        {
            if (!CanMantling()) return;

            groundedChecker.CheckGrounded(characterController.transform.position);

            var target = GetMantlingPoint();
            if (target == null) return;

            lastMantlingTicks = DateTime.Now.Ticks;
            Callback(target.Value);
        }
        Vector3? GetMantlingPoint()
        {
            var colliders = this.colliders.GetItems().ToArray();

            if (colliders.Length == 0)
                return null;

            if (!IsMoveKeysPushed())
                return null;

            var targetPoint = mantlingPointer.GetMantlingPoint(
                colliders, groundedChecker.lastGroundedPoint.y, !groundedChecker.isGrounded
            );
            if (targetPoint == null)
                return null;

            if (!spaceChecker.CheckHeight(targetPoint.Value))
                return null;

            return targetPoint;
        }
        bool IsMoveKeysPushed()
        {
            return rawInput.IsForwardKey() || rawInput.IsBackKey() || rawInput.IsLeftKey() || rawInput.IsRightKey();
        }
    }
}
