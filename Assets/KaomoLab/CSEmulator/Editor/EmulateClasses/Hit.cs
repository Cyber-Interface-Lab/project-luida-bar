using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class Hit
    {
        public EmulateVector3 normal { get; private set; }
        public EmulateVector3 point { get; private set; }

        public Hit(
            EmulateVector3 normal,
            EmulateVector3 point
        )
        {
            this.normal = normal;
            this.point = point;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[Hit][{0}][{1}]", normal, point);
        }
    }
}
