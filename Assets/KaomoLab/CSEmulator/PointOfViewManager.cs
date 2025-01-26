using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator
{
    public class PointOfViewManager
    {
        public enum CameraConstraint
        {
            ThridPerson, Floating
        }

        public class ThirdPersonCameraConstrainter
        {
            readonly GameObject parentThridPersonCamera;
            public readonly GameObject parentFloatingCamera;

            readonly GameObject thirdPersonCameraRoot;

            CameraConstraint positionConstraint = CameraConstraint.ThridPerson;
            public CameraConstraint rotateConstraint { get; private set; } = CameraConstraint.ThridPerson;

            public ThirdPersonCameraConstrainter(
                GameObject thridPersonCameraParent,
                GameObject thirdPersonCameraRoot

            )
            {
                parentThridPersonCamera = new GameObject("CSEmulatorParentThirdPersonCamera");
                parentThridPersonCamera.transform.SetParent(thridPersonCameraParent.transform, false);
                parentFloatingCamera = new GameObject("CSEmulatorParentFloatingCamera"); //これはsceneroot
                this.thirdPersonCameraRoot = thirdPersonCameraRoot;
            }

            public void SetPositionConstraint(CameraConstraint constraintTo)
            {
                positionConstraint = constraintTo;
            }
            public void SetRotationConstraint(CameraConstraint constraintTo)
            {
                rotateConstraint = constraintTo;
            }

            public void UpdateConstraint()
            {
                thirdPersonCameraRoot.transform.position = positionConstraint switch
                {
                    CameraConstraint.ThridPerson => parentThridPersonCamera.transform.position,
                    CameraConstraint.Floating => parentFloatingCamera.transform.position,
                    _ => throw new Exception("このエラーが出た場合は開発者に連絡してください")
                };
                thirdPersonCameraRoot.transform.rotation = rotateConstraint switch
                {
                    CameraConstraint.ThridPerson => parentThridPersonCamera.transform.rotation,
                    CameraConstraint.Floating => parentFloatingCamera.transform.rotation,
                    _ => throw new Exception("このエラーが出た場合は開発者に連絡してください")
                };
            }
        }

        public class ThirdPersonCameraDistanceManager
        {
            public float defaultDistance { get; private set; }
            public float distance {
                get
                {
                    if(isForceZero) return 0;
                    var ret = _distance * collideRatio;
                    return ret;
                }
            }

            float _distance;
            float collideRatio = 1;
            bool isForceZero = false;

            public ThirdPersonCameraDistanceManager(
                float defaultDistance
            )
            {
                this.defaultDistance = defaultDistance;
                this._distance = defaultDistance;
            }

            public void SetTemporaryDistance(float distance)
            {
                this._distance = distance;
            }

            public void SetCollideDistance(float rayDistance, float rayMagnitude)
            {
                collideRatio = rayDistance / rayMagnitude;
            }
            public void ResetCollideDistance()
            {
                collideRatio = 1;
            }

            public void SetForceZeroDistance(bool isForceZero)
            {
                this.isForceZero = isForceZero;
            }
        }

        public Camera targetCamera => isFirstPerson ? firstPersonCamera : thirdPersonCamera;

        readonly GameObject firstPersonCameraObject;
        Camera firstPersonCamera;
        UnityEngine.Rendering.PostProcessing.PostProcessLayer firstPersonPpl;

        readonly GameObject thirdPersonCameraObject;
        Camera thirdPersonCamera;
        UnityEngine.Rendering.PostProcessing.PostProcessLayer thirdPersonPpl;

        readonly GameObject thirdPersonCameraDistance;
        readonly GameObject thirdPersonCameraScreenPosition;
        readonly GameObject thirdPersonCameraDistanceRayTarget;
        readonly GameObject thirdPersonCameraScreenPositionRayTarget;
        readonly GameObject thirdPersonCameraRayTarget;

        Vector2 thirdPersonCameraScreenPositionTarget = new Vector2(0.5f, 0.5f); //左下が0右上が1

        ThirdPersonCameraConstrainter thirdPersonCameraConstrainter;
        ThirdPersonCameraDistanceManager thirdPersonCameraDistanceManager;

        //readonly float defaultCameraDistance;
        readonly float nearPlane;
        readonly float farPlane;
        //float nowCameraDistance = 0;
        readonly Collider[] ignoreColliders;
        readonly Action<Camera> ToFirstPersonCallback;
        readonly Action<Camera> ToThirdPersonCallback;

        readonly RaycastHit[] raycastHits = new RaycastHit[5];

        bool isFirstPerson = true;
        float defaultFieldOfView = 0;

        public PointOfViewManager(
            float cameraDistance,
            float? nearPlane,
            float? farPlane,
            GameObject firstPersonCameraObject,
            Collider[] ignoreColliders,
            Action<Camera> ToFirstPersonCallback,
            Action<Camera> ToThirdPersonCallback
        )
        {
            this.firstPersonCameraObject = firstPersonCameraObject;
            this.firstPersonCamera = firstPersonCameraObject.GetComponent<Camera>();
            this.firstPersonPpl = firstPersonCameraObject.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>();
            this.ignoreColliders = ignoreColliders;
            this.ToFirstPersonCallback = ToFirstPersonCallback;
            this.ToThirdPersonCallback = ToThirdPersonCallback;

            this.firstPersonCamera.cullingMask |= (1 << 7); //CameraOnlyを追加

            var root = new GameObject("CSEmulatorThirdPersonCameraRoot");

            thirdPersonCameraDistance = new GameObject("Distance");
            thirdPersonCameraDistance.transform.SetParent(root.transform, false);
            thirdPersonCameraDistance.transform.localPosition = Vector3.zero;

            thirdPersonCameraScreenPosition = new GameObject("ScreenPosition");
            thirdPersonCameraScreenPosition.transform.SetParent(thirdPersonCameraDistance.transform, false);
            thirdPersonCameraScreenPosition.transform.localPosition = Vector3.zero;

            thirdPersonCameraObject = new GameObject("CSEmulatorThirdPersonCamera");
            thirdPersonCamera = thirdPersonCameraObject.AddComponent<Camera>();
            thirdPersonCameraObject.transform.SetParent(thirdPersonCameraScreenPosition.transform, false);
            thirdPersonCamera.enabled = false;

            thirdPersonCameraDistanceRayTarget = new GameObject("DistanceRayTarget");
            thirdPersonCameraDistanceRayTarget.transform.SetParent(root.transform, false);
            thirdPersonCameraDistanceRayTarget.transform.localPosition = Vector3.zero;

            thirdPersonCameraScreenPositionRayTarget = new GameObject("ScreenPosition");
            thirdPersonCameraScreenPositionRayTarget.transform.SetParent(thirdPersonCameraDistanceRayTarget.transform, false);
            thirdPersonCameraScreenPositionRayTarget.transform.localPosition = Vector3.zero;

            thirdPersonCameraRayTarget = new GameObject("RayTarget");
            thirdPersonCameraRayTarget.transform.SetParent(thirdPersonCameraScreenPositionRayTarget.transform, false);
            thirdPersonCameraRayTarget.transform.localPosition = new Vector3(0, 0, -1);

            thirdPersonCameraConstrainter = new ThirdPersonCameraConstrainter(
                firstPersonCameraObject, root
            );
            //CopyFromでカメラの向きもCopyされるのでその前にConstraintしておく必要がある
            thirdPersonCameraConstrainter.UpdateConstraint();

            thirdPersonCameraDistanceManager = new ThirdPersonCameraDistanceManager(
                cameraDistance
            );

            thirdPersonCamera.CopyFrom(firstPersonCamera);
            SetThirdPersonCameraDistance(thirdPersonCameraDistanceManager.distance);
            SetThirdPersonCameraRayTargetDistance(thirdPersonCameraDistanceManager.distance);
            thirdPersonCamera.cullingMask |= (1 << 16); //OwnAvater表示

            thirdPersonPpl = thirdPersonCameraObject.AddComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>();
            InitPostProcessLayer(thirdPersonPpl);
            thirdPersonPpl.volumeLayer = (1 << 21); //PostProcessing
            thirdPersonPpl.volumeTrigger = thirdPersonCameraObject.transform;

            defaultFieldOfView = firstPersonCamera.fieldOfView;

            if (nearPlane.HasValue)
            {
                firstPersonCamera.nearClipPlane = nearPlane.Value;
                thirdPersonCamera.nearClipPlane = nearPlane.Value;
            }
            if (farPlane.HasValue)
            {
                firstPersonCamera.farClipPlane = farPlane.Value;
                thirdPersonCamera.farClipPlane = farPlane.Value;
            }
        }
        void InitPostProcessLayer(UnityEngine.Rendering.PostProcessing.PostProcessLayer postProcessLayer)
        {
#if UNITY_EDITOR            
            var resources = UnityEditor.AssetDatabase.FindAssets("t:PostProcessResources");
            string resourcesPath = UnityEditor.AssetDatabase.GUIDToAssetPath(resources[0]);
            postProcessLayer.Init(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.PostProcessing.PostProcessResources>(resourcesPath));
#endif
        }
        void SetThirdPersonCameraDistance(float distance)
        {
            thirdPersonCameraDistance.transform.localScale = new Vector3(distance, distance, distance);
            thirdPersonCameraObject.transform.localPosition = new Vector3(0, 0, -1);
        }
        void SetThirdPersonCameraRayTargetDistance(float distance)
        {
            thirdPersonCameraDistanceRayTarget.transform.localScale = new Vector3(distance, distance, distance);
        }
        public void SetupCommandBuffer()
        {
            foreach (var c in firstPersonCamera.GetCommandBuffers(UnityEngine.Rendering.CameraEvent.BeforeImageEffects))
            {
                if (c.name != "Unnamed command buffer") continue;
                thirdPersonCamera.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.BeforeImageEffects, c);
            }
        }

        public Vector3 GetCameraPosition()
        {
            if (isFirstPerson)
            {
                return firstPersonCameraObject.transform.position;
            }
            else
            {
                return thirdPersonCameraObject.transform.position;
            }
        }
        public Quaternion GetCameraRotation()
        {
            if (isFirstPerson)
            {
                return firstPersonCameraObject.transform.rotation;
            }
            else
            {
                return thirdPersonCameraObject.transform.rotation;
            }
        }
        public Transform GetFirstPersonCameraTransform()
        {
            return firstPersonCameraObject.transform;
        }
        public Transform GetThridPersonCameraTransform()
        {
            return thirdPersonCameraObject.transform;
        }
        public void SetCameraPosition(Vector3 position)
        {
            thirdPersonCameraConstrainter.SetPositionConstraint(CameraConstraint.Floating);
            thirdPersonCameraConstrainter.parentFloatingCamera.transform.position = position;
            thirdPersonCameraDistanceManager.SetForceZeroDistance(true);
        }
        public void SetCameraRotation(Quaternion rotation)
        {
            //setRotation指定中に視点を切り替える事があるので両方に設定
            firstPersonCameraObject.transform.rotation = rotation;
            thirdPersonCameraConstrainter.SetRotationConstraint(CameraConstraint.Floating);
            thirdPersonCameraConstrainter.parentFloatingCamera.transform.rotation = rotation;
        }
        public void ClearCameraPosition()
        {
            thirdPersonCameraConstrainter.SetPositionConstraint(CameraConstraint.ThridPerson);
            thirdPersonCameraDistanceManager.SetForceZeroDistance(false);
        }
        public void ClearCameraRotation()
        {
            thirdPersonCameraConstrainter.SetRotationConstraint(CameraConstraint.ThridPerson);
            //CSETODO 2.24.0.1カメラの向きをThirdPersonに反映させる
        }
        public bool NeedRotationCancel()
        {
            return thirdPersonCameraConstrainter.rotateConstraint == CameraConstraint.Floating;
        }

        public void SetCameraFieldOfViewTemporary(float value)
        {
            firstPersonCamera.fieldOfView = value;
            thirdPersonCamera.fieldOfView = value;
        }
        public void SetCameraFieldOfView(float value)
        {
            firstPersonCamera.fieldOfView = value;
            thirdPersonCamera.fieldOfView = value;
            defaultFieldOfView = value;
        }
        public float GetCameraFieldOfViewNow()
        {
            return firstPersonCamera.fieldOfView;
        }
        public float GetCameraFieldOfView()
        {
            return defaultFieldOfView;
        }
        //CameraHandle.setThirdPersonDistanceは一時的な変更という考え方
        public void SetThirdPersonCameraDistanceTemporary(float value)
        {
            thirdPersonCameraDistanceManager.SetTemporaryDistance(value);
            SetThirdPersonCameraDistance(thirdPersonCameraDistanceManager.distance);
            SetThirdPersonCameraRayTargetDistance(thirdPersonCameraDistanceManager.distance);
        }
        public float GetThirdPersonCameraDistanceNow()
        {
            return thirdPersonCameraDistance.transform.localScale.x;
        }
        public float GetThirdPersonCameraDistanceDefault()
        {
            return thirdPersonCameraDistanceManager.defaultDistance;
        }
        public void SetThirdPersonCameraScreenPosition(Vector2 pos)
        {
            thirdPersonCameraScreenPositionTarget = pos;
        }
        public Vector2 GetThirdPersonCameraScreenPositionNow()
        {
            return thirdPersonCameraScreenPositionTarget.Clone();
        }

        public void UpdateThirdPersonCameraPosition()
        {
            thirdPersonCameraConstrainter.UpdateConstraint();
            var ray = thirdPersonCameraRayTarget.transform.position - firstPersonCameraObject.transform.position;
            var hitCount = Physics.RaycastNonAlloc(
                firstPersonCameraObject.transform.position,
                ray.normalized,
                raycastHits,
                ray.magnitude,
                ~(1 << 7),  //CameraOnly以外
                QueryTriggerInteraction.Ignore
            );

            var validHits = raycastHits
                .Take(hitCount)
                .Where(h => !ignoreColliders.Contains(h.collider))
                .OrderBy(h => h.distance)
                .ToArray();

            if (validHits.Length == 0)
            {
                thirdPersonCameraDistanceManager.ResetCollideDistance();
            }
            else
            {
                thirdPersonCameraDistanceManager.SetCollideDistance(validHits[0].distance, ray.magnitude);
            }

            SetThirdPersonCameraDistance(thirdPersonCameraDistanceManager.distance);
            UpdateThirdPersonCameraScreenPosition(thirdPersonCameraDistanceManager.distance);
        }
        void UpdateThirdPersonCameraScreenPosition(float distance)
        {
            var posCenter = thirdPersonCamera.ScreenToWorldPoint(new Vector3(
                thirdPersonCamera.pixelWidth / 2,
                thirdPersonCamera.pixelHeight / 2,
                distance
            ));
            var posTarget = thirdPersonCamera.ScreenToWorldPoint(new Vector3(
                thirdPersonCamera.pixelWidth * thirdPersonCameraScreenPositionTarget.x,
                thirdPersonCamera.pixelHeight * thirdPersonCameraScreenPositionTarget.y,
                distance
            ));
            var localCenter = thirdPersonCameraDistance.transform.InverseTransformPoint(posCenter);
            var localPos = thirdPersonCameraDistance.transform.InverseTransformPoint(posTarget);
            //視界上の位置じゃなくてカメラの位置なので反転
            thirdPersonCameraScreenPosition.transform.localPosition = -(localPos - localCenter);
            thirdPersonCameraScreenPositionRayTarget.transform.localPosition = -(localPos - localCenter);
        }

        public void ChangeView(bool isFirstPerson)
        {
            if (isFirstPerson)
            {
                this.isFirstPerson = true;
                thirdPersonCamera.enabled = false;
                thirdPersonPpl.enabled = false; //よくわからないタイミングでPPLがエラーになる問題に、これで対応できているか分からない。
                firstPersonCamera.enabled = true;
                firstPersonPpl.enabled = true;
                ToFirstPersonCallback(firstPersonCamera);
            }
            else
            {
                this.isFirstPerson = false;
                thirdPersonCamera.enabled = true;
                thirdPersonPpl.enabled = true;
                firstPersonCamera.enabled = false;
                firstPersonPpl.enabled = false;
                ToThirdPersonCallback(thirdPersonCamera);
            }
        }
    }
}
