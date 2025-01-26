using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ClusterVR.CreatorKit.Item;
using ClusterVR.CreatorKit.Preview.PlayerController;
using UnityEngine;


namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    //Reflection以上のことをさせてはいけない。
    public class DesktopItemControllerReflector
    {
        readonly DesktopItemController controller;

        readonly FieldInfo field_grabbingItem;
        readonly FieldInfo field_grabPoint;
        readonly FieldInfo field_grabPointToTargetOffsetRotation;
        readonly FieldInfo field_grabPointToTargetOffsetPosition;

        public IGrabbableItem grabbingItem
        {
            get => (IGrabbableItem)field_grabbingItem.GetValue(controller);
        }
        public Transform grabPoint
        {
            get => (Transform)field_grabPoint.GetValue(controller);
            set => field_grabPoint.SetValue(controller, value);
        }
        public Quaternion grabPointToTargetOffsetRotation
        {
            get => (Quaternion)field_grabPointToTargetOffsetRotation.GetValue(controller);
            set => field_grabPointToTargetOffsetRotation.SetValue(controller, value);
        }
        public Vector3 grabPointToTargetOffsetPosition
        {
            get => (Vector3)field_grabPointToTargetOffsetPosition.GetValue(controller);
            set => field_grabPointToTargetOffsetPosition.SetValue(controller, value);
        }

        public DesktopItemControllerReflector(
            DesktopItemController controller
        )
        {
            this.controller = controller;
            field_grabbingItem = typeof(DesktopItemController)
                .GetField("grabbingItem", BindingFlags.NonPublic | BindingFlags.Instance);
            field_grabPoint = typeof(DesktopItemController)
                .GetField("grabPoint", BindingFlags.NonPublic | BindingFlags.Instance);
            field_grabPointToTargetOffsetRotation = typeof(DesktopItemController)
                .GetField("grabPointToTargetOffsetRotation", BindingFlags.NonPublic | BindingFlags.Instance);
            field_grabPointToTargetOffsetPosition = typeof(DesktopItemController)
                .GetField("grabPointToTargetOffsetPosition", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
