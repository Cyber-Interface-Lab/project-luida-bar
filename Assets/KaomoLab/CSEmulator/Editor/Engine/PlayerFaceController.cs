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
    public class PlayerFaceController
        : Components.IPlayerFaceController
    {
        int forward = 0;
        int right = 0;
        float degree = 0;

        public PlayerFaceController(
            GameObject vrmRotateRoot
        )
        {
            this.vrmRotateRoot = vrmRotateRoot.transform;
        }

        public Transform vrmRotateRoot { get; private set; }

        public void SetFaceForward(int direction)
        {
            forward = direction;
            ApplyDirection();
        }

        public void SetFaceRight(int direction)
        {
            right = direction;
            ApplyDirection();
        }

        public float GetNowRotate()
        {
            return vrmRotateRoot.transform.localRotation.eulerAngles.y;
        }
        public void SetBaseRotate(float degree)
        {
            this.degree = degree;
        }

        void ApplyDirection()
        {
            var d = CalcDegree();
            vrmRotateRoot.transform.localRotation = Quaternion.Euler(0, d + degree, 0);
        }
        int CalcDegree()
        {
            if (forward == 1 && right == 0) return 45 * 0;
            if (forward == 1 && right == 1) return 45 * 1;
            if (forward == 0 && right == 1) return 45 * 2;
            if (forward == -1 && right == 1) return 45 * 3;
            if (forward == -1 && right == 0) return 45 * 4;
            if (forward == -1 && right == -1) return 45 * 5;
            if (forward == 0 && right == -1) return 45 * 6;
            if (forward == 1 && right == -1) return 45 * 7;

            return 0;
        }
    }
}
