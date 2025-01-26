using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class WorldItemTemplateId
    {
        public readonly string _id;

        public WorldItemTemplateId(
            string id
        )
        {
            this._id = id;
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            return o;
        }
        public override string ToString()
        {
            return String.Format("[WorldItemTemplateId][{0}]", _id);
        }

    }
}
