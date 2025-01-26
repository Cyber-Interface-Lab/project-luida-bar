using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class Overlap
    {
        public object selfNode { get; private set; }

        readonly HitObject _object;
        public HitObject @object
        {
            get
            {
                UnityEngine.Debug.LogError("Overlap.objectは非推奨(deprecate)になりました。");
                return _object;
            }
        }
        public object handle => _object.GetHandle();

        public Overlap(
            HitObject hitObject,
            object selfNode
        )
        {
            this._object = hitObject;
            this.selfNode = selfNode;
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            o.selfNode = selfNode;
            o.@object = @object; //2.26.0現在残っているのでそのまま
            o.handle = handle;
            return o;
        }
        public override string ToString()
        {
            return String.Format("[Overlap][{0}][{1}]", handle, selfNode);
        }
    }
}
