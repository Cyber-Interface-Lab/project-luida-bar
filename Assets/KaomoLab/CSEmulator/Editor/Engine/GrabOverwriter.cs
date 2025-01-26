using ClusterVR.CreatorKit.World.Implements.WorldRuntimeSetting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class GrabOverwriter
        : Components.IGrabController
    {
        readonly HudSettings hudSettings;
        readonly GameObject oldGrabPoint;
        readonly GameObject vrmGrabPoint;
        readonly DesktopItemControllerReflector desktopItemController;

        readonly GameObject grabPointRoot;
        readonly ParentConstraint grabPointConstraint;

        Transform constraintTo = null;
        bool isGrabbingStart = true;

        public GrabOverwriter(
            HudSettings hudSettings,
            GameObject previewRoot,
            GameObject grabPoint,
            GameObject vrmGrabPoint,
            DesktopItemControllerReflector desktopItemController
        )
        {
            this.hudSettings = hudSettings;
            this.oldGrabPoint = grabPoint;
            this.vrmGrabPoint = vrmGrabPoint;
            this.desktopItemController = desktopItemController;

            grabPointRoot = new GameObject("CSEmulatorGrabPoint");
            grabPointRoot.transform.SetParent(previewRoot.transform, false);

            desktopItemController.grabPoint = grabPointRoot.transform;
        }

        public bool isGrab => desktopItemController.grabbingItem != null;

        public Vector3 grabPoint
        {
            get
            {
                if (hudSettings.useClusterHudV2)
                    return vrmGrabPoint.transform.position;
                else
                    return oldGrabPoint.transform.position;
            }
        }

        public void ApplyUpdate()
        {
            if (desktopItemController.grabbingItem == null)
            {
                if (constraintTo == null) return;
                constraintTo = null;
                isGrabbingStart = true;
                return;
            }

            if (isGrabbingStart)
            {
                if (hudSettings.useClusterHudV2)
                {
                    AddConstraint(vrmGrabPoint);
                    var p = desktopItemController.grabPointToTargetOffsetPosition;
                    var r = desktopItemController.grabPointToTargetOffsetRotation;
                    //手のボーンと思しき回転分と前方向に持つ分をキャンセルする。
                    var offset = Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(90, Vector3.forward);
                    p = offset * p;
                    r = offset * r;
                    desktopItemController.grabPointToTargetOffsetPosition = p;
                    desktopItemController.grabPointToTargetOffsetRotation = r;
                }
                else
                {
                    AddConstraint(oldGrabPoint);
                }
            }
            isGrabbingStart = false;

            if(constraintTo != null)
            {
                grabPointRoot.transform.position = constraintTo.transform.position;
                grabPointRoot.transform.rotation = constraintTo.transform.rotation;
            }

        }
        void AddConstraint(GameObject gameObject)
        {
            constraintTo = gameObject.transform;
        }

    }
}
