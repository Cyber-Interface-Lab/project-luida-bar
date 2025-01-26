using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class PlayerSendableSanitizer
        : IPlayerSendableSanitizer
    {
        readonly Jint.Engine engine;

        public PlayerSendableSanitizer(
            Jint.Engine engine
        )
        {
            this.engine = engine;
        }

        public object Sanitize(
            object value
        )
        {
            //★SendableSanitizerの方も忘れずに
            if (value is Jint.Native.JsUndefined)
            {
                //ここでnullにするとobjectとarrayでの処理差に対応できない
                UnityEngine.Debug.LogWarning("undefinedは利用できません。");
                return Jint.Native.JsValue.Undefined;
            }
            else if (value is Jint.Native.JsNull || value == null)
            {
                return Jint.Native.JsValue.Null;
            }
            else if (value is bool boolValue)
            {
                return boolValue;
            }
            else if (value is int intValue)
            {
                return intValue;
            }
            else if (value is float floatValue)
            {
                return floatValue;
            }
            else if (value is double doubleValue)
            {
                return doubleValue;
            }
            else if (value is string stringValue)
            {
                return stringValue;
            }
            else if (value.GetType().IsArray)
            {
                var objects = (object[])value;
                var jvs = new List<Jint.Native.JsValue>();
                foreach (var o in (object[])value)
                {
                    var sanitized = Sanitize(o);
                    //arrayはnullにして保持する。
                    if (sanitized is Jint.Native.JsUndefined) sanitized = null;
                    var jv = Jint.Native.JsValue.FromObject(engine, sanitized);
                    jvs.Add(jv);
                }
                var ret = new Jint.Native.JsArray(engine, jvs.ToArray());
                return ret;
            }
            else if (value is Jint.Native.Array.ArrayInstance objects)
            {
                var jvs = new List<Jint.Native.JsValue>();
                foreach (var o in objects)
                {
                    var sanitized = Sanitize(o);
                    //arrayはnullにして保持する。
                    if (sanitized is Jint.Native.JsUndefined) sanitized = null;
                    var jv = Jint.Native.JsValue.FromObject(engine, sanitized);
                    jvs.Add(jv);
                }
                var ret = new Jint.Native.JsArray(engine, jvs.ToArray());
                return ret;
            }
            else if (value is Delegate)
            {
                return new Jint.Native.JsObject(engine);
            }
            else if (value is DateTime)
            {
                return new Jint.Native.JsObject(engine);
            }
            else if (value is EmulateVector2 vector2)
            {
                return vector2.clone();
            }
            else if (value is EmulateVector3 vector3)
            {
                return vector3.clone();
            }
            else if (value is EmulateQuaternion quaternion)
            {
                return quaternion.clone();
            }
            else if (value is ItemHandle itemHandle)
            {
                return new ItemId(itemHandle.csItemHandler);
            }
            else if (value is PlayerHandle playerHandle)
            {
                return new PlayerId(playerHandle);
            }
            else if (value is ItemId itemId)
            {
                return new ItemId(itemId.csItemHandler);
            }
            else if (value is PlayerId playerId)
            {
                return new PlayerId(playerId.playerHandle);
            }
            else if (value is Jint.Runtime.Interop.ObjectWrapper wrapped)
            {
                //JsValue.FromObjectでwrapされたら剥がして再送
                var v = Sanitize(wrapped.Target);
                return v;
            }
            else if (value is Jint.Native.Object.ObjectInstance oi)
            {
                var ret = new Jint.Native.JsObject(engine);
                foreach (var key in oi.GetOwnPropertyKeys(Jint.Runtime.Types.String))
                {
                    var sanitized = Sanitize(oi[key]);
                    //objectのundefinedは無視する。
                    if (sanitized is Jint.Native.JsUndefined) continue;
                    var jv = Jint.Native.JsValue.FromObject(engine, sanitized);
                    ret.Set(key, jv);
                }
                return ret;
            }
            else if (value is System.Dynamic.ExpandoObject eo)
            {
                var ret = new Jint.Native.JsObject(engine);
                foreach (var kv in eo.ToArray())
                {
                    var sanitized = Sanitize(kv.Value);
                    //objectのundefinedは無視する。
                    if (sanitized is Jint.Native.JsUndefined) continue;
                    var jv = Jint.Native.JsValue.FromObject(engine, sanitized);
                    ret.Set(kv.Key, jv);
                }
                return ret;
            }
            else if (value is Jint.Native.JsValue jsv)
            {
                return jsv;
            }

            return new Jint.Native.JsObject(engine);
        }
    }
}
