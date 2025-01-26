using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator
{
    //CCKのIMoveInputControllerみたいにしたかったけど若干時すでに遅し
    public interface IRawInput
    {
        Func<bool> IsForwardKey { get; }
        Func<bool> IsBackKey { get; }
        Func<bool> IsRightKey { get; }
        Func<bool> IsLeftKey { get; }

        Func<bool> IsRightHandKeyDown { get; }
        Func<bool> IsRightHandKeyUp { get; }
        Func<bool> IsLeftHandKeyDown { get; }
        Func<bool> IsLeftHandKeyUp { get; }

        Func<bool> IsWalkKey { get; }
        Func<bool> IsDashKey { get; }
        Func<bool> IsCrouchKey { get; }

        Func<bool> IsJumpKey { get; }
    }
    public interface IProgramStatus
    {
        string GetLineInfo();
    }
    public interface IItemExceptionFactory
    {
        Exception CreateDistanceLimitExceeded(string message);
        Exception CreateRequestSizeLimitExceeded(string message);
        Exception CreateRateLimitExceeded(string message);
        Exception CreateExecutionNotAllowed(string message);
        Exception CreateGeneral(string message);
        Exception CreateJsError(string message);
    }
    public interface ITicker
    {
        long Ticks();
    }
    public interface IChargeThrottle
    {
        bool TryCharge();
        void Discharge(double amount);
    }
    public interface IHumanoidAnimationCreator
    {
        //悪手過ぎるけども、そもそもPoseManagerがここのレイヤーにいるのが間違い説あるので、そのうちなんとかする
        ClusterVR.CreatorKit.Item.Implements.HumanoidAnimation Create(AnimationClip clip);
    }
}
