using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class EmulateColor
        : IHasTypeNameAlias
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public EmulateColor(
        ) : this(0, 0, 0, 0)
        {
        }


        public EmulateColor(
            float r, float g, float b, float a
        )
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public EmulateColor(
            UnityEngine.Color source
        ): this(source.r, source.g, source.b, source.a)
        {
        }

        public EmulateColor(
            EmulateColor source
        ) : this(source.r, source.g, source.b, source.a)
        {
        }

        public UnityEngine.Color _ToUnityEngine()
        {
            return new UnityEngine.Color(r, g, b, a);
        }

        void Apply(UnityEngine.Color v)
        {
            r = v.r;
            g = v.g;
            b = v.b;
            a = v.a;
        }


        public EmulateColor clone()
        {
            return new EmulateColor(this);
        }

        public bool equals(EmulateColor v)
        {
            //Unity公式の誤差に従っていると思われる。
            return this._ToUnityEngine() == v._ToUnityEngine();
        }

        public EmulateColor lerp(EmulateColor v, float a)
        {
            var lerped = UnityEngine.Color.Lerp(_ToUnityEngine(), v._ToUnityEngine(), a);
            Apply(lerped);
            return this;
        }

        public EmulateColor set(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
            return this;
        }

        public EmulateColor setFromHsv(float h, float s, float v)
        {
            var c = UnityEngine.Color.HSVToRGB(h, s, v);
            this.r = c.r;
            this.g = c.g;
            this.b = c.b;
            return this;
        }


        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("({0:f4},{1:f4},{2:f4},{3:f4})", r, g, b, a);
        }

        public string GetAliasTypeName()
        {
            return typeof(UnityEngine.Color).Name;
        }

    }
}
