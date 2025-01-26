using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class EmulateVector4
        : IHasTypeNameAlias
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public EmulateVector4(
        ) : this(0, 0, 0, 0)
        {
        }


        public EmulateVector4(
            float x, float y, float z, float w
        )
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public EmulateVector4(
            UnityEngine.Vector4 source
        ): this(source.x, source.y, source.z, source.w)
        {
        }

        public EmulateVector4(
            EmulateVector4 source
        ) : this(source.x, source.y, source.z, source.w)
        {
        }

        public UnityEngine.Vector4 _ToUnityEngine()
        {
            return new UnityEngine.Vector4(x, y, z, w);
        }

        void Apply(UnityEngine.Vector4 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            w = v.w;
        }

        public EmulateVector4 clone()
        {
            return new EmulateVector4(this);
        }

        public bool equals(EmulateVector4 v)
        {
            //Unity公式の誤差に従っていると思われる。
            return this._ToUnityEngine() == v._ToUnityEngine();
        }

        public EmulateVector4 lerp(EmulateVector4 v, float a)
        {
            var lerped = UnityEngine.Vector4.Lerp(_ToUnityEngine(), v._ToUnityEngine(), a);
            Apply(lerped);
            return this;
        }

        public EmulateVector4 set(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
            return this;
        }


        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("({0:f4},{1:f4},{2:f4},{3:f4})", x, y, z, w);
        }

        public string GetAliasTypeName()
        {
            return typeof(UnityEngine.Vector4).Name;
        }

    }
}
