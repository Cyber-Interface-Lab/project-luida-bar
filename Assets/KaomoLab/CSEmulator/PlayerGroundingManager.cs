using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator
{
    public class PlayerGroundingManager
    {
        public class ReferenceComponent : MonoBehaviour
        {
        }

        public class Posture
        {
            public Vector3 position;
            public Quaternion rotation;
            public Posture(Vector3 position, Quaternion rotation)
            {
                this.position = position;
                this.rotation = rotation;
            }
            public Posture(Transform transform)
            {
                this.position = transform.position;
                this.rotation = transform.rotation;
            }
            public Posture()
            {
                this.position = Vector3.zero;
                this.rotation = Quaternion.identity;
            }
            public Posture Sub(Posture target)
            {
                var p = this.position - target.position;
                var r = this.rotation * Quaternion.Inverse(target.rotation);
                return new Posture(p, r);
            }
            public override string ToString()
            {
                return String.Format("{0}:{1}", position, rotation);
            }
        }

        readonly CharacterController characterController;
        readonly Transform characterRotationRoot;

        readonly RaycastHit[] raycastHits = new RaycastHit[5];
        readonly float rayRadius;
        readonly Vector3 rayOffset;
        readonly float rayDistance;

        readonly FrameOnlyList<Collider> colliders = new FrameOnlyList<Collider>();
        Collider groundedCollider = null;
        Collider groundedColliderPrev = null;

        long ticksPrev = 0;
        float frameSecDelta = 0;

        Posture referencePosturePrev = null;
        Posture movingDelta = new Posture();
        GameObject referenceObject = null;

        public PlayerGroundingManager(
            float rayOffsetY,
            CharacterController characterController,
            Transform rotationRoot
        )
        {
            this.characterController = characterController;
            this.characterRotationRoot = rotationRoot;
            rayOffset = new Vector3(0, rayOffsetY + characterController.radius, 0);
            rayDistance = characterController.radius * 1.415f + rayOffsetY + characterController.skinWidth; //45度(√2)まで乗る判定
            rayRadius = characterController.radius;
        }

        public void ApplyStayCollision(Collision collision)
        {
            if (colliders.Contains(collision.collider)) return;
            colliders.Add(collision.collider);
        }

        public void UpdateGrounded(Action<bool, Posture> Callback)
        {
            {
                foreach (var added in colliders.GetAddedItems())
                {
                    var o = added.gameObject;

                    if (o.GetComponentInChildren<ReferenceComponent>()) continue;

                    var r = new GameObject("CSEmulatorMovingPlatformReference");
                    r.AddComponent<ReferenceComponent>();
                    r.transform.parent = o.transform;
                }
                foreach (var removed in colliders.GetRemovedItems())
                {
                    var o = removed.gameObject;
                    var r = o.GetComponentInChildren<ReferenceComponent>();
                    if (r == null) continue;
                    GameObject.Destroy(r.gameObject);
                }
            }
            {
                var position = characterController.gameObject.transform.position;
                var rotation = characterRotationRoot.rotation;
                UpdateGroundedCollider(position);
                InitializeReferenceObject(position, rotation);
                CalcFrameSecDelta();
                CalcMovingDelta();
            }
            Callback(IsGrounded(), movingDelta);
            {
                //delta加算後に位置の更新をしないと、回転系で外側にずれる(1フレーム前の位置で更新してしまうため)
                var position = characterController.gameObject.transform.position;
                var rotation = characterRotationRoot.rotation;
                UpdateReference(position, rotation);
            }
        }
        void UpdateGroundedCollider(Vector3 origin)
        {
            groundedColliderPrev = groundedCollider;
            var hitCount = Physics.SphereCastNonAlloc(
                rayOffset + origin, rayRadius, Vector3.down, raycastHits, rayDistance, -1, QueryTriggerInteraction.Ignore
            );

            if (hitCount == 0)
            {
                groundedCollider = null;
                referenceObject = null;
                return;
            }

            groundedCollider = GetGroundedCollider(hitCount);
        }
        Collider GetGroundedCollider(int hitCount)
        {
            for (var i = 0; i < hitCount; i++)
            {
                var raycastHit = raycastHits[i];
                foreach (var collider in colliders.GetItems())
                {
                    if (collider.GetInstanceID() == raycastHit.collider.GetInstanceID())
                    {
                        return collider;
                    }
                }
            }
            return null;
        }
        void InitializeReferenceObject(Vector3 position, Quaternion rotation)
        {
            var now = GetGroundedObject();

            if (referenceObject != null && now != null && referenceObject.GetInstanceID() == now.GetInstanceID())
                return;

            referenceObject = now;
            if(referenceObject != null)
            {
                referenceObject.transform.position = position;
                referenceObject.transform.rotation = rotation;
            }
        }
        GameObject GetGroundedObject()
        {
            if (groundedCollider == null) return null;
            var component = groundedCollider.GetComponentInChildren<ReferenceComponent>();
            if (component == null) return null;
            return component.gameObject;
        }
        void CalcFrameSecDelta()
        {
            var now = DateTime.Now.Ticks;

            if(ticksPrev == 0)
            {
                ticksPrev = now;
                frameSecDelta = 0;
                return;
            }

            frameSecDelta = (float)(now - ticksPrev) / (float)TimeSpan.TicksPerSecond;
            ticksPrev = now;
        }
        void CalcMovingDelta()
        {
            if(groundedCollider == null)
            {
                //離陸時はDeltaを使用するのでそのまま。
                referencePosturePrev = null;
                return;
            }

            if(groundedCollider == null || groundedColliderPrev == null)
            {
                movingDelta = new Posture();
                referencePosturePrev = null;
                return;
            }

            if (groundedColliderPrev != null && groundedCollider.GetInstanceID() != groundedColliderPrev.GetInstanceID())
            {
                movingDelta = new Posture();
                referencePosturePrev = null;
                return;
            }

            if(referencePosturePrev != null)
                movingDelta = new Posture(referenceObject.transform).Sub(referencePosturePrev);
        }
        void UpdateReference(Vector3 position, Quaternion rotation)
        {
            if (referenceObject == null) return;
            referenceObject.transform.position = position;
            referenceObject.transform.rotation = rotation;
            referencePosturePrev = new Posture(position, rotation);
        }

        public bool IsGrounded()
        {
            return groundedCollider != null
                && groundedColliderPrev != null
                && groundedCollider.GetInstanceID() == groundedColliderPrev.GetInstanceID();
        }

        public bool IsTakingOff()
        {
            return groundedCollider == null && groundedColliderPrev != null;
        }

        public bool IsGrounding()
        {
            return groundedCollider != null && groundedColliderPrev == null;
        }

        public Vector3 GetHorizontalInertia()
        {
            if(frameSecDelta == 0) return Vector3.zero;
            var ret = new Vector3(
                movingDelta.position.x / frameSecDelta,
                0,
                movingDelta.position.z / frameSecDelta
            );
            return ret;
        }

        public Vector3 GetVerticalInertia()
        {
            if (frameSecDelta == 0) return Vector3.zero;
            var ret = new Vector3(
                0,
                movingDelta.position.y / frameSecDelta,
                0
            );
            return ret;

        }
    }
}
