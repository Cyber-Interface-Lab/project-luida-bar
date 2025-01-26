using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class EmulateVector3
        : ISendableSize, IHasTypeNameAlias
    {
        public float x;
        public float y;
        public float z;

        public EmulateVector3(
        ) : this(0, 0, 0)
        {
        }


        public EmulateVector3(
            float x, float y, float z
        )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public EmulateVector3(
            UnityEngine.Vector3 source
        ): this(source.x, source.y, source.z)
        {
        }

        public EmulateVector3(
            EmulateVector3 source
        ) : this(source.x, source.y, source.z)
        {
        }

        public UnityEngine.Vector3 _ToUnityEngine()
        {
            return new UnityEngine.Vector3(x, y, z);
        }

        void Apply(UnityEngine.Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public EmulateVector3 add(EmulateVector3 v)
        {
            x += v.x;
            y += v.y;
            z += v.z;
            return this;
        }

        public EmulateVector3 addScalar(float s)
        {
            x += s;
            y += s;
            z += s;
            return this;
        }

        public EmulateVector3 applyQuaternion(EmulateQuaternion q)
        {
            var applied = q._ToUnityEngine() * _ToUnityEngine();
            Apply(applied);
            return this;
        }

        public EmulateVector3 clone()
        {
            return new EmulateVector3(this);
        }

        public EmulateVector3 cross(EmulateVector3 v)
        {
            var crossed = UnityEngine.Vector3.Cross(_ToUnityEngine(), v._ToUnityEngine());
            Apply(crossed);
            return this;
        }

        public EmulateVector3 divide(EmulateVector3 v)
        {
            x = x / v.x;
            y = y / v.y;
            z = z / v.z;
            return this;
        }

        public EmulateVector3 divideScalar(float s)
        {
            x = x / s;
            y = y / s;
            z = z / s;
            return this;
        }

        public float dot(EmulateVector3 v)
        {
            return UnityEngine.Vector3.Dot(this._ToUnityEngine(), v._ToUnityEngine());
        }

        public bool equals(EmulateVector3 v)
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
            return x * x + y * y + z * z;
        }

        public EmulateVector3 lerp(EmulateVector3 v, float a)
        {
            var lerped = UnityEngine.Vector3.Lerp(_ToUnityEngine(), v._ToUnityEngine(), a);
            Apply(lerped);
            return this;
        }

        public EmulateVector3 multiply(EmulateVector3 v)
        {
            x = x * v.x;
            y = y * v.y;
            z = z * v.z;
            return this;
        }

        public EmulateVector3 multiplyScalar(float s)
        {
            x = x * s;
            y = y * s;
            z = z * s;
            return this;
        }

        public EmulateVector3 negate()
        {
            x = -x;
            y = -y;
            z = -z;
            return this;
        }

        public EmulateVector3 normalize()
        {
            var normalized = _ToUnityEngine().normalized;
            Apply(normalized);
            return this;
        }

        public EmulateVector3 set(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            return this;
        }

        public EmulateVector3 sub(EmulateVector3 v)
        {
            x = x - v.x;
            y = y - v.y;
            z = z - v.z;
            return this;
        }

        public EmulateVector3 subScalar(float s)
        {
            x = x - s;
            y = y - s;
            z = z - s;
            return this;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("({0:f4},{1:f4},{2:f4})", x, y, z);
        }

        public int GetSize()
        {
            return 28;
        }

        public string GetAliasTypeName()
        {
            return typeof(UnityEngine.Vector3).Name;
        }

    }
}
