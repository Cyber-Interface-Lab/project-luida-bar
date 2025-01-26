using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class AxisAngle
    {
        public readonly float angle;
        public readonly EmulateVector3 axis;

        public AxisAngle(
            float angle,
            EmulateVector3 axis
        )
        {
            this.angle = angle;
            this.axis = axis;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[AxisAngle][{0}][{1}]", angle, axis);
        }
    }
}
