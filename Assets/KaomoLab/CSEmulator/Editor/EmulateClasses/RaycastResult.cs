using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class RaycastResult
    {
        public readonly Hit hit;

        //@を付ければOK。CS側からは.objectで参照できる。propertyでも同じくOK。
        readonly HitObject _object;
        public HitObject @object
        {
            get
            {
                UnityEngine.Debug.LogError("RaycastResult.objectは非推奨(deprecate)になりました。");
                return _object;
            }
        }
        public object handle => _object.GetHandle();

        public RaycastResult(
            Hit hit,
            HitObject hitObject
        )
        {
            this.hit = hit;
            this._object = hitObject;
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            o.hit = hit;
            o.@object = @object; //2.26.0現在残っているのでそのまま
            o.handle = handle;
            return o;
        }
        public override string ToString()
        {
            return String.Format("[RaycastResult][{0}][{1}]", handle, hit);
        }
    }
}
