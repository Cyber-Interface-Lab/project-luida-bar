using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class HumanoidPose
    {
        public EmulateVector3 centerPosition;
        public EmulateQuaternion centerRotation;
        public Muscles muscles;

        //undefinedで初期化してもnullで入ってくるのでobjectで受ける必要なし
        public HumanoidPose(
            EmulateVector3 centerPosition,
            EmulateQuaternion centerRotation,
            Muscles muscles
        )
        {
            this.centerPosition = centerPosition;
            this.centerRotation = centerRotation;
            this.muscles = muscles;
        }

        public HumanoidPose(
            ClusterVR.CreatorKit.Common.PartialHumanPose source
        )
        {
            this.centerPosition = source.CenterPosition.HasValue ? new EmulateVector3(source.CenterPosition.Value) : null;
            this.centerRotation = source.CenterRotation.HasValue ? new EmulateQuaternion(source.CenterRotation.Value) : null;
            this.muscles = new Muscles();
            var len = source.Muscles.Count();
            for (var i = 0; i < len; i++)
            {
                var muscle = source.Muscles[i];
                this.muscles.Set(
                    i, muscle.HasValue ? muscle.Value : Jint.Native.JsValue.Undefined
                );
            }
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[HumanoidPose][{0}][{1}][{2}]", centerPosition, centerRotation, muscles);
        }

    }
}
