using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Implements
{
    public class UnityKeyInput
        : IRawInput
    {
        readonly Func<bool> CanCrouch;

        //CSETODO この辺の設計とか立ち位置よくなかった。V3出す時になんとかする
        public UnityKeyInput(
            Func<bool> CanCrouch
        )
        {
            this.CanCrouch = CanCrouch;
        }

        public Func<bool> IsForwardKey => () => Input.GetKey(KeyCode.W);
        public Func<bool> IsBackKey => () => Input.GetKey(KeyCode.S);
        public Func<bool> IsRightKey => () => Input.GetKey(KeyCode.D);
        public Func<bool> IsLeftKey => () => Input.GetKey(KeyCode.A);

        public Func<bool> IsRightHandKeyDown => () => Input.GetKeyDown(KeyCode.C);
        public Func<bool> IsRightHandKeyUp => () => Input.GetKeyUp(KeyCode.C);
        public Func<bool> IsLeftHandKeyDown => () => Input.GetKeyDown(KeyCode.Z);
        public Func<bool> IsLeftHandKeyUp => () => Input.GetKeyUp(KeyCode.Z);

        public Func<bool> IsWalkKey => () => Input.GetKey(KeyCode.LeftAlt);
        public Func<bool> IsDashKey => () => Input.GetKey(KeyCode.LeftShift);
        public Func<bool> IsCrouchKey => () => CanCrouch() && Input.GetKey(KeyCode.LeftControl);

        public Func<bool> IsJumpKey => () => Input.GetKey(KeyCode.Space);

    }
}
