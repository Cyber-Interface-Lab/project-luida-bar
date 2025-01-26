using Jint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class MaterialHandle
    {
        readonly Material material;
        readonly IRunningContext runningContext;
        readonly IItemExceptionFactory itemExceptionFactory;

        public MaterialHandle(
            Material material,
            IRunningContext runningContext,
            IItemExceptionFactory itemExceptionFactory
        )
        {
            this.material = material;
            this.runningContext = runningContext;
            this.itemExceptionFactory = itemExceptionFactory;
        }

        bool IsInvalid(params float[] values)
        {
            var ret = values.All(v => !float.IsNaN(v) && !float.IsFinite(v) && !float.IsNegativeInfinity(v));
            return ret;
        }
        void CheckPropertyName(string propertyName)
        {
            if (propertyName.Length > 64)
            {
                throw itemExceptionFactory.CreateGeneral(
                    String.Format("プロパティ名が64文字を超えています。{0}", propertyName)
                );
            }
        }
        void CheckArrayLength(float[] array)
        {
            if (array.Length != 16)
            {
                throw itemExceptionFactory.CreateGeneral(
                    String.Format("16要素が必要です。{0}", array.Length)
                );
            }
        }

        public void setBaseColor(float r, float g, float b, float a)
        {
            if (runningContext.CheckTopLevel("MaterialHandle.setBaseColor()")) return;
            if (IsInvalid(r, g, b, a)) return;
            if (material == null) return;

            material.color = new Color(
                Math.Clamp(r, 0f, 1f),
                Math.Clamp(g, 0f, 1f),
                Math.Clamp(b, 0f, 1f),
                Math.Clamp(a, 0f, 1f)
            );
        }
        public void setBaseColor(EmulateColor color)
        {
            setBaseColor(color.r, color.g, color.b, color.a);
        }

        public void setColor(string propertyName, float r, float g, float b, float a)
        {
            if (runningContext.CheckTopLevel("MaterialHandle.setColor()")) return;
            CheckPropertyName(propertyName);
            if (IsInvalid(r, g, b, a)) return;
            if (material == null) return;

            material.SetColor(propertyName, new Color(
                Math.Max(r, 0f),
                Math.Max(g, 0f),
                Math.Max(b, 0f),
                Math.Clamp(a, 0f, 1f)
            ));
        }
        public void setColor(string propertyName, EmulateColor color)
        {
            setColor(propertyName, color.r, color.g, color.b, color.a);
        }


        public void setEmissionColor(float r, float g, float b, float a)
        {
            if (runningContext.CheckTopLevel("MaterialHandle.setEmissionColor()")) return;
            if (IsInvalid(r, g, b, a)) return;
            if (material == null) return;

            setColor("_EmissionColor", r, g, b, a);
            material.EnableKeyword("_EMISSION");
        }
        public void setEmissionColor(EmulateColor color)
        {
            setEmissionColor(color.r, color.g, color.b, color.a);
        }

        public void setFloat(string propertyName, float value)
        {
            if (runningContext.CheckTopLevel("MaterialHandle.setFloat()")) return;
            CheckPropertyName(propertyName);
            if (IsInvalid(value)) return;
            if (material == null) return;

            material.SetFloat(propertyName, value);
        }

        public void setFloat2(string propertyName, float x, float y)
        {
            if (runningContext.CheckTopLevel("MaterialHandle.setFloat2()")) return;
            CheckPropertyName(propertyName);
            if (IsInvalid(x, y)) return;
            if (material == null) return;

            material.SetVector(propertyName, new Vector2(x, y));
        }

        public void setFloat3(string propertyName, float x, float y, float z)
        {
            if (runningContext.CheckTopLevel("MaterialHandle.setFloat3()")) return;
            CheckPropertyName(propertyName);
            if (IsInvalid(x, y, z)) return;
            if (material == null) return;

            material.SetVector(propertyName, new Vector3(x, y, z));
        }
        public void setFloat3(string propertyName, EmulateVector3 v)
        {
            setFloat3(propertyName, v.x, v.y, v.z);
        }

        public void setFloat4(string propertyName, float x, float y, float z, float w)
        {
            if (runningContext.CheckTopLevel("MaterialHandle.setFloat4()")) return;
            CheckPropertyName(propertyName);
            if (IsInvalid(x, y, z, w)) return;
            if (material == null) return;

            material.SetVector(propertyName, new Vector4(x, y, z, w));
        }
        public void setFloat4(string propertyName, EmulateVector4 v)
        {
            setFloat4(propertyName, v.x, v.y, v.z, v.w);
        }

        public void setMatrix(string propertyName, Jint.Native.JsTypedArray _matrix)
        {
            if (runningContext.CheckTopLevel("MaterialHandle.setMatrix()")) return;
            var matrix = PackFloatArray(_matrix);
            CheckPropertyName(propertyName);
            CheckArrayLength(matrix);
            if (IsInvalid(matrix)) return;
            if (material == null) return;

            //material.SetMatrix(propertyName, new Matrix4x4(
            //    new Vector4(matrix[0], matrix[0+4], matrix[0+8], matrix[0+12]),
            //    new Vector4(matrix[1], matrix[1+4], matrix[1+8], matrix[1+12]),
            //    new Vector4(matrix[2], matrix[2+4], matrix[2+8], matrix[2+12]),
            //    new Vector4(matrix[3], matrix[3+4], matrix[3+8], matrix[3+12])
            //));
            //こっちで正解
            material.SetMatrix(propertyName, new Matrix4x4(
                new Vector4(matrix[0], matrix[1], matrix[2], matrix[3]),
                new Vector4(matrix[0 + 4], matrix[1 + 4], matrix[2 + 4], matrix[3 + 4]),
                new Vector4(matrix[0 + 8], matrix[1 + 8], matrix[2 + 8], matrix[3 + 8]),
                new Vector4(matrix[0 + 12], matrix[1 + 12], matrix[2 + 12], matrix[3 + 12])
            ));
        }
        float[] PackFloatArray(Jint.Native.JsTypedArray typedArray)
        {
            var ret = new float[typedArray.Length];
            for(var i = 0; i < typedArray.Length; i++)
            {
                ret[i] = (float)typedArray[i].AsNumber();
            }
            return ret;
        }


    }
}
