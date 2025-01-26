using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Assets.KaomoLab.CSEmulator.Components;
using ClusterVR.CreatorKit.Preview.PlayerController;
using UnityEngine;


namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    //Reflection以上のことをさせてはいけない。
    public class DesktopPlayerControllerReflector
        : Components.IVelocityYHolder,
        Components.IBaseMoveSpeedHolder,
        Components.IPlayerRotateHandler,
        Components.IRidingHolder,
        Components.IMouseEventEmitter
    {
        readonly DesktopPlayerController controller;

        readonly FieldInfo velocityY;
        readonly FieldInfo baseMoveSpeed;
        readonly FieldInfo rootTransform;
        readonly MethodInfo rotateDelta;
        readonly PropertyInfo isRiding;
        readonly FieldInfo ridingItem;
        readonly FieldInfo desktopPointerEventListener;

        float IVelocityYHolder.value
        {
            get => (float)velocityY.GetValue(controller);
            set => velocityY.SetValue(controller, value);
        }
        float IBaseMoveSpeedHolder.value {
            get => (float)baseMoveSpeed.GetValue(controller);
            set => baseMoveSpeed.SetValue(controller, value);
        }

        Transform IPlayerRotateHandler.rootTransform => (Transform)rootTransform.GetValue(controller);
        void IPlayerRotateHandler.RotateDelta(Vector2 delta)
        {
            rotateDelta.Invoke(controller, new object[] { delta });
        }

        bool IRidingHolder.isRiding => (bool)isRiding.GetValue(controller);
        ClusterVR.CreatorKit.Item.Implements.RidableItem IRidingHolder.ridableItem => (ClusterVR.CreatorKit.Item.Implements.RidableItem)ridingItem.GetValue(controller);

        event Action<Vector2> IMouseEventEmitter.OnMoved
        {
            add => ((DesktopPointerEventListener) desktopPointerEventListener.GetValue(controller)).OnMoved += value;
            remove => ((DesktopPointerEventListener)desktopPointerEventListener.GetValue(controller)).OnMoved -= value;
        }
        event Action<Vector2> IMouseEventEmitter.OnClicked
        {
            add => ((DesktopPointerEventListener)desktopPointerEventListener.GetValue(controller)).OnClicked += value;
            remove => ((DesktopPointerEventListener)desktopPointerEventListener.GetValue(controller)).OnClicked -= value;
        }

        public DesktopPlayerControllerReflector(
            DesktopPlayerController controller
        )
        {
            this.controller = controller;
            velocityY = typeof(DesktopPlayerController)
                .GetField("velocityY", BindingFlags.NonPublic | BindingFlags.Instance);
            baseMoveSpeed = typeof(DesktopPlayerController)
                .GetField("baseMoveSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            rootTransform = typeof(DesktopPlayerController)
                .GetField("rootTransform", BindingFlags.NonPublic | BindingFlags.Instance);
            rotateDelta = typeof(DesktopPlayerController)
                .GetMethod("Rotate", BindingFlags.NonPublic | BindingFlags.Instance);
            isRiding = typeof(DesktopPlayerController)
                .GetProperty("IsRiding", BindingFlags.NonPublic | BindingFlags.Instance);
            ridingItem = typeof(DesktopPlayerController)
                .GetField("ridingItem", BindingFlags.NonPublic | BindingFlags.Instance);
            desktopPointerEventListener = typeof(DesktopPlayerController)
                .GetField("desktopPointerEventListener", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
