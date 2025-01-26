using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class SetHumanoidPoseOption
    {
        public double timeoutSeconds = double.PositiveInfinity;
        public double timeoutTransitionSeconds = 0;
        public double transitionSeconds = 0;

        public SetHumanoidPoseOption Nomalized()
        {
            var ret = new SetHumanoidPoseOption();
            ret.timeoutSeconds = timeoutSeconds;
            ret.timeoutTransitionSeconds = timeoutTransitionSeconds;
            ret.transitionSeconds = transitionSeconds;

            if (ret.timeoutSeconds == double.NaN)
            {
                timeoutSeconds = double.PositiveInfinity;
            }
            if (ret.timeoutTransitionSeconds < 0 || ret.timeoutTransitionSeconds == double.NaN)
            {
                ret.timeoutTransitionSeconds = 0;
            }
            if (ret.transitionSeconds < 0 || ret.transitionSeconds == double.NaN)
            {
                ret.transitionSeconds = 0;
            }

            if(ret.timeoutSeconds < ret.transitionSeconds)
            {
                ret.timeoutSeconds = ret.transitionSeconds;
            }

            return ret;
        }

        public override string ToString()
        {
            return String.Format("timeoutSeconds:{0} timeoutTransitionSeconds:{1} transitionSeconds:{2}", timeoutSeconds, timeoutTransitionSeconds, transitionSeconds);
        }
    }
}
