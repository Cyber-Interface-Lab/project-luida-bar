using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator
{
    public class KeyWalkManager
    {
        readonly IRawInput rawInput;

        bool isForwardKeyPressed = false;
        bool isBackKeyPressed = false;
        bool isRightKeyPressed = false;
        bool isLeftKeyPressed = false;

        int forwardForce = 0;
        int forwardDirection = 0;
        int rightForce = 0;
        int rightDirection = 0;

        bool needRefresh = true;
        bool isFaceConstraintForward = false;
        bool needForceForward = false;

        public KeyWalkManager(
            IRawInput rawInput
        )
        {
            this.rawInput = rawInput;
        }

        public void Update(
            Action<int, int> OnWalking,
            Action<int> OnForwardDirectionChanged,
            Action<int> OnRightDirectionChanged
        )
        {
            var changed = false;
            ApplyWalkForce(rawInput.IsForwardKey, 1, ref isForwardKeyPressed, ref forwardForce, ref changed);
            ApplyWalkForce(rawInput.IsBackKey,  -1, ref isBackKeyPressed, ref forwardForce, ref changed);
            ApplyWalkForce(rawInput.IsRightKey, 1, ref isRightKeyPressed, ref rightForce, ref changed);
            ApplyWalkForce(rawInput.IsLeftKey, -1, ref isLeftKeyPressed, ref rightForce, ref changed);
            if (changed || needRefresh)
            {
                needRefresh = false;

                var toForwardDirection = CalcDirection(forwardDirection, forwardForce, rightForce);
                var toRightDirection = CalcDirection(rightDirection, rightForce, forwardForce);
                if (forwardDirection == 0 && rightDirection == 0)
                {
                    //デフォルト前向き
                    toForwardDirection = 1;
                }
                if (isFaceConstraintForward && forwardForce == 0 && rightForce == 0)
                {
                    toForwardDirection = 1;
                    toRightDirection = 0;
                }
                if (needForceForward)
                {
                    toForwardDirection = 1;
                    toRightDirection = 0;
                    needForceForward = false;
                }
                if (forwardDirection != toForwardDirection)
                {
                    forwardDirection = toForwardDirection;
                    OnForwardDirectionChanged(forwardDirection);
                }

                if (rightDirection != toRightDirection)
                {
                    rightDirection = toRightDirection;
                    OnRightDirectionChanged(rightDirection);
                }
            }
            if (rawInput.IsForwardKey() || rawInput.IsBackKey() || rawInput.IsRightKey() || rawInput.IsLeftKey())
            {
                OnWalking(forwardDirection, rightDirection);
            }
        }

        void ApplyWalkForce(
            Func<bool> IsKey, int direction, ref bool isKeyPressed, ref int force, ref bool changed)
        {
            if (IsKey() && !isKeyPressed)
            {
                force += direction;
                isKeyPressed = true;
                changed = true;
            }
            if (!IsKey() && isKeyPressed)
            {
                if (isKeyPressed) force -= direction;
                isKeyPressed = false;
                changed = true;
            }
        }

        int CalcDirection(int targetDirection, int targetForce, int otherForce)
        {
            if (targetForce != 0) return targetForce;
            if (otherForce == 0) return targetDirection;
            return 0;
        }

        public void ConstraintFaceForward(bool isFaceConstraintForward)
        {
            this.isFaceConstraintForward = isFaceConstraintForward;
            needRefresh = true;
        }

        public void ForceForward()
        {
            needForceForward = true;
            needRefresh = true;
        }

        public int GetDirectionAngle()
        {
            if (forwardDirection ==  1 && rightDirection ==  0) return 45 * 0;
            if (forwardDirection ==  1 && rightDirection ==  1) return 45 * 1;
            if (forwardDirection ==  0 && rightDirection ==  1) return 45 * 2;
            if (forwardDirection == -1 && rightDirection ==  1) return 45 * 3;
            if (forwardDirection == -1 && rightDirection ==  0) return 45 * 4;
            if (forwardDirection == -1 && rightDirection == -1) return 45 * 5;
            if (forwardDirection ==  0 && rightDirection == -1) return 45 * 6;
            if (forwardDirection ==  1 && rightDirection == -1) return 45 * 7;

            //デフォルトの向きが定まったら不要
            if (forwardDirection == 0 && rightDirection == 0) return 45 * 0;
            throw new Exception("設計ミス。開発者に連絡してください。");
        }
    }
}
