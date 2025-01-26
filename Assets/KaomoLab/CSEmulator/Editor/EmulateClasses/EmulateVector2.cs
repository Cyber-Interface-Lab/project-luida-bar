using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class EmulateVector2
        : ISendableSize, IHasTypeNameAlias
    {
        public float x;
        public float y;

        public EmulateVector2(
        ) : this(0, 0)
        {
        }

        public EmulateVector2(
            float x, float y
        )
        {
            this.x = x;
            this.y = y;
        }

        public EmulateVector2(
            UnityEngine.Vector2 source
        ): this(source.x, source.y)
        {
        }

        public EmulateVector2(
            EmulateVector2 source
        ) : this(source.x, source.y)
        {
        }

        public UnityEngine.Vector2 _ToUnityEngine()
        {
            return new UnityEngine.Vector2(x, y);
        }

        public EmulateVector2 add(EmulateVector2 v)
        {
            x += v.x;
            y += v.y;
            return this;
        }

        public EmulateVector2 addScalar(float s)
        {
            x += s;
            y += s;
            return this;
        }

        public EmulateVector2 clone()
        {
            return new EmulateVector2(this);
        }

        public float cross(EmulateVector2 v)
        {
            return x * v.y - y * v.x;
        }

        public EmulateVector2 divide(EmulateVector2 v)
        {
            x = x / v.x;
            y = y / v.y;
            return this;
        }

        public EmulateVector2 divideScalar(float s)
        {
            x = x / s;
            y = y / s;
            return this;
        }

        public float dot(EmulateVector2 v)
        {
            return x * v.x + y * v.y;
        }

        public bool equals(EmulateVector2 v)
        {
            //Unity公式の誤差に従っていると思われる。
            return this._ToUnityEngine() == v._ToUnityEngine();
        }

        public float length()
        {
            return (float)Math.Sqrt(lengthSq());
        }

        public float lengthSq()
        {
            return x * x + y * y;
        }

        public EmulateVector2 lerp(EmulateVector2 v, float a)
        {
            //実ワールドで0.1を2回すると0.2ではなく0.19になるので、この仕様でヨシ！
            var lerped = UnityEngine.Vector2.Lerp(_ToUnityEngine(), v._ToUnityEngine(), a);
            x = lerped.x;
            y = lerped.y;
            return this;
        }

        public EmulateVector2 multiply(EmulateVector2 v)
        {
            x = x * v.x;
            y = y * v.y;
            return this;
        }

        public EmulateVector2 multiplyScalar(float s)
        {
            x = x * s;
            y = y * s;
            return this;
        }

        public EmulateVector2 negate()
        {
            x = -x;
            y = -y;
            return this;
        }

        public EmulateVector2 normalize()
        {
            var normalized = _ToUnityEngine().normalized;
            x = normalized.x;
            y = normalized.y;
            return this;
        }

        public EmulateVector2 set(float x, float y)
        {
            this.x = x;
            this.y = y;
            return this;
        }

        public EmulateVector2 sub(EmulateVector2 v)
        {
            x = x - v.x;
            y = y - v.y;
            return this;
        }

        public EmulateVector2 subScalar(float s)
        {
            x = x - s;
            y = y - s;
            return this;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("({0:f4},{1:f4})", x, y);
        }

        public int GetSize()
        {
            return 20;
        }

        public string GetAliasTypeName()
        {
            return typeof(UnityEngine.Vector2).Name;
        }
    }
}
