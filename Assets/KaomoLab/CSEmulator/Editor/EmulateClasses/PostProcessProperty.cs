using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class PostProcessProperty<T>
    {
        public T value = default(T);
        public bool hasValue { get; private set; } = false; //初期状態は未設定とする

        readonly Func<T, T> Normalize;

        public PostProcessProperty(
            Func<T, T> Normalize
        )
        {
            this.Normalize = Normalize;
        }

        public void clear()
        {
            hasValue = false;
        }

        protected void _setValue(T value)
        {
            hasValue = true;
            this.value = Normalize(value);
        }

        public void Apply(ParameterOverride<T> parameter)
        {
            if (hasValue)
            {
                parameter.overrideState = true;
                parameter.Override(value);
            }
            else
            {
                parameter.overrideState = false;
            }
        }

        public object toJSON(string key)
        {
            return new System.Dynamic.ExpandoObject();
        }
    }

    public class PostProcessBoolProperty : PostProcessProperty<bool>
    {
        public PostProcessBoolProperty(Func<bool, bool> Normalize) : base(Normalize) { }
        public void setValue(bool v) => _setValue(v);
        public PostProcessBoolProperty Initialize(bool v)
        {
            _setValue(v);
            return this;
        }
    }
    public class PostProcessFloatProperty : PostProcessProperty<float>
    {
        public PostProcessFloatProperty(Func<float, float> Normalize) : base(Normalize) { }
        public void setValue(float v) => _setValue(v);
    }
    public class PostProcessIntProperty : PostProcessProperty<int>
    {
        public PostProcessIntProperty(Func<int, int> Normalize) : base(Normalize) { }
        public void setValue(int v) => _setValue(v);
    }
    public class PostProcessStringProperty : PostProcessProperty<string>
    {
        public PostProcessStringProperty(Func<string, string> Normalize) : base(Normalize) { }
        public void setValue(string v) => _setValue(v);
    }
    public class PostProcessColorProperty : PostProcessProperty<UnityEngine.Color>
    {
        //2.8.0.1 LDRとHDRの差は多分こう
        Func<float, float, float, float, Color> BuildColor = (r, g, b, a) => new Color(
            Math.Clamp(r, 0f, 1f), Math.Clamp(g, 0f, 1f), Math.Clamp(b, 0f, 1f), Math.Clamp(a, 0f, 1f)
        );
        public PostProcessColorProperty(Func<Color, Color> Normalize) : base(Normalize) { }
        public void setValue(float r, float g, float b, float a) => _setValue(BuildColor(r, g, b, a));
        public PostProcessColorProperty SetupHDR()
        {
            BuildColor = (r, g, b, a) => new Color(
                (float)Math.Pow(2, r - 1), (float)Math.Pow(2, g - 1), (float)Math.Pow(2, b - 1), a
            );
            return this;
        }
    }
    public class PostProcessVector2Property : PostProcessProperty<UnityEngine.Vector2>
    {
        public PostProcessVector2Property(Func<Vector2, Vector2> Normalize) : base(Normalize) { }
        public void setValue(float x, float y) => _setValue(new Vector2(x, y));
    }
    public class PostProcessVector4Property : PostProcessProperty<UnityEngine.Vector4>
    {
        public PostProcessVector4Property(Func<Vector4, Vector4> Normalize) : base(Normalize) { }
        public void setValue(float x, float y, float z, float w) => _setValue(new Vector4(x, y, z, w));
    }
}
