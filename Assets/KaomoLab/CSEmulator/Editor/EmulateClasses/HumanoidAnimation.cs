using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class HumanoidAnimation
    {
        readonly ClusterVR.CreatorKit.Item.IHumanoidAnimationListEntry entry;
        readonly IRunningContext runningContext;

        public HumanoidAnimation(
            ClusterVR.CreatorKit.Item.IHumanoidAnimationListEntry entry,
            IRunningContext runningContext
        )
        {
            this.entry = entry;
            this.runningContext = runningContext;
        }

        public float getLength()
        {
            if (entry == null) return 0;
            return this.entry.HumanoidAnimation.Length;
        }

        public HumanoidPose getSample(float time)
        {
            if (entry == null) return new HumanoidPose(null, null, null);
            var partial = entry.HumanoidAnimation.Sample(time);
            var pose = new HumanoidPose(partial);
            return pose;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[HumanoidAnimation][{0}]", entry.Id);
        }
    }
}
