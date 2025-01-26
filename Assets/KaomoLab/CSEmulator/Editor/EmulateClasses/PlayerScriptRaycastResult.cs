using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class PlayerScriptRaycastResult
    {
        public readonly Hit hit;

        public PlayerScriptRaycastResult(
            Hit hit
        )
        {
            this.hit = hit;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[PlayerScriptRaycastResult][{0}]", hit);
        }
    }
}
