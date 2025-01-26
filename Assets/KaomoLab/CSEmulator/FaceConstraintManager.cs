using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator
{
    public class FaceConstraintManager
    {
        bool isGrab = false;
        bool isRide = false;
        bool isFirstPersonPerspective = false;
        public bool isConstraintForward { get; private set; } = true; //起動直後はカメラ方向を強制することで、SpawnPointのrotationに対応する。
        bool? overwriteConstraintForward = null;

        readonly HudSettings hudSettings;
        readonly Action<bool> FaceConstraintChangedCallback;

        public FaceConstraintManager(
            HudSettings hudSettings,
            Action<bool> FaceConstraintChangedCallback
        )
        {
            this.hudSettings = hudSettings;
            this.FaceConstraintChangedCallback = FaceConstraintChangedCallback;
        }

        public void ChangeRiding(bool isRide)
        {
            this.isRide = isRide;
            CheckFaceConstraint();
        }
        public void ChangeGrabbing(bool isGrab)
        {
            this.isGrab = isGrab;
            CheckFaceConstraint();
        }
        public void ChangePerspective(bool isFirstPerson)
        {
            this.isFirstPersonPerspective = isFirstPerson;
            CheckFaceConstraint();
        }
        void CheckFaceConstraint()
        {
            var nowConstraintForward = (isGrab && !hudSettings.useClusterHudV2) || isRide || isFirstPersonPerspective;
            if(overwriteConstraintForward.HasValue)
            {
                nowConstraintForward = overwriteConstraintForward.Value;
            }

            if (isConstraintForward == nowConstraintForward) return;

            isConstraintForward = nowConstraintForward;
            FaceConstraintChangedCallback(nowConstraintForward);
        }

        public void OverwriteFaceConstraint(bool? forward)
        {
            overwriteConstraintForward = forward;
            CheckFaceConstraint();
        }

    }
}
