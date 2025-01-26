using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class StateProxy
        : Components.IVariablesStore
    {
        public class HandleReplacer
        {
            readonly Components.CSEmulatorItemHandler owner;
            readonly ISpaceContext spaceContext;
            readonly IRunningContext ownerContext;
            readonly ISendableSanitizer sanitizer;
            readonly IMessageSender messageSender;

            public HandleReplacer(
                Components.CSEmulatorItemHandler owner,
                ISpaceContext spaceContext,
                IRunningContext ownerContext,
                ISendableSanitizer sanitizer,
                IMessageSender messageSender
            )
            {
                this.owner = owner;
                this.spaceContext = spaceContext;
                this.ownerContext = ownerContext;
                this.sanitizer = sanitizer;
                this.messageSender = messageSender;
            }

            public HandleReplacer(
                StateProxy parent,
                Components.CSEmulatorItemHandler owner,
                IMessageSender messageSender
            ): this(
                owner,
                parent.spaceContext,
                parent.runningContext,
                parent.sendableSanitizer,
                messageSender
            )
            { }

            public ItemHandle ReplaceItemHandle(Components.CSEmulatorItemHandler old)
            {
                var ret = new ItemHandle(
                    old, owner, spaceContext, ownerContext, sanitizer, messageSender
                );
                return ret;
            }

            public PlayerHandle ReplacePlayerHandle(PlayerHandle old)
            {
                var ret = new PlayerHandle(
                    old, owner
                );
                return ret;
            }
        }

        public class StateDictionary : Dictionary<string, object> { };

        readonly string pauseFrameIndex;
        readonly ISendableSanitizer sendableSanitizer;
        readonly ISpaceContext spaceContext;
        readonly IRunningContext runningContext;
        readonly IJsValueConverter jsValueConverter;

        public StateDictionary stateDictionary;

        public HandleReplacer handleReplacer = null;

        public StateProxy(
            string pauseFrameIndex,
            ISpaceContext spaceContext,
            IRunningContext runningContext,
            ISendableSanitizer sendableSanitizer,
            IJsValueConverter jsValueConverter
        )
        {
            this.pauseFrameIndex = pauseFrameIndex;
            this.sendableSanitizer = sendableSanitizer;
            this.spaceContext = spaceContext;
            this.runningContext = runningContext;
            this.jsValueConverter = jsValueConverter;
            stateDictionary = new StateDictionary();
        }

        public Jint.Native.JsValue this[string index]
        {
            get
            {
                if (runningContext.CheckTopLevel("ClusterScript.State")) { }; //メッセージのみ
                if (!stateDictionary.ContainsKey(index))
                    return Jint.Native.JsValue.Undefined;

                var obj = stateDictionary[index];

                if (handleReplacer == null)
                {
                    obj = sendableSanitizer.Sanitize(obj);
                }
                else
                {
                    obj = sendableSanitizer.Sanitize(
                        obj, handleReplacer.ReplaceItemHandle, handleReplacer.ReplacePlayerHandle
                    );
                }

                var ret = jsValueConverter.FromObject(obj);

                return ret;
            }
            set
            {
                if(index == pauseFrameIndex) UnityEngine.Debug.Break();
                if (runningContext.CheckTopLevel("ClusterScript.State")) { }; //メッセージのみ
                if (value is Jint.Native.JsUndefined)
                {
                    UnityEngine.Debug.LogWarning($"undefinedは指定できません。[{index}]");
                    return;
                }
                var jsObject = sendableSanitizer.Sanitize(value);
                stateDictionary[index] = ConvertJsValueToObject(jsObject);
                _OnVariablesUpdated.Invoke();
            }
        }

        public object ConvertJsValueToObject(object jsObject)
        {
            if (jsObject is Jint.Native.Object.ObjectInstance oi)
            {
                var toObject = oi.ToObject();
                if (toObject is EmulateVector2 v2) return v2;
                if (toObject is EmulateVector3 v3) return v3;
                if (toObject is EmulateQuaternion q) return q;
                if (toObject is ItemHandle ih) return ih;
                if (toObject is PlayerHandle ph) return ph;
                if (toObject is ItemId id) return id;
                if (toObject is PlayerId pd) return pd;
                if (oi is Jint.Runtime.Interop.ObjectWrapper ow)
                {
                    //剥がして再送(中身はVector2等)
                    return ConvertJsValueToObject(ow.ToObject());
                }
                if (oi is Jint.Native.Array.ArrayInstance ai)
                {
                    var ret = new List<object>();
                    foreach (var o in ai)
                    {
                        var peeled = ConvertJsValueToObject(o);
                        ret.Add(peeled);
                    }
                    return ret.ToArray();
                }
                {
                    var ret = new System.Dynamic.ExpandoObject() as IDictionary<string, object>;
                    foreach (var key in oi.GetOwnPropertyKeys(Jint.Runtime.Types.String))
                    {
                        var peeled = ConvertJsValueToObject(oi[key]);
                        ret.Add(key.ToString(), peeled);
                    }
                    return ret;
                }
            }
            else if (jsObject is Jint.Native.JsValue jv)
            {
                //intやbool
                var toObject = jv.ToObject();
                return toObject;
            }

            return jsObject;
        }

        public static int CalcSendableSize(
            object value, int arrayAddition, int size = 0
        )
        {
            var add = 0;
            if (value == null)
            {
                add = 0;
            }
            else if (value is Jint.Native.JsValue jsv && jsv == Jint.Native.JsValue.Undefined)
            {
                throw new Exception("undefinedはSendableではありません。nullを指定してください。");
            }
            else if (value is bool boolValue)
            {
                add = 2;
            }
            else if (value is int intValue)
            {
                add = 9; //たぶん
            }
            else if (value is float floatValue)
            {
                add = 9; //たぶん
            }
            else if (value is double doubleValue)
            {
                add = 9; //数字は基本これで入ってくる
            }
            else if (value is string stringValue)
            {
                //"a":1、"あ":3
                var count = Encoding.UTF8.GetByteCount(stringValue);
                add = 2 + count;
            }
            else if (value.GetType().IsArray)
            {
                var objects = (object[])value;
                add = 2 + objects.Select(o => CalcSendableSize(o, 2, size)).Sum();
            }
            else if (value is ISendableSize sendableSize)
            {
                add = sendableSize.GetSize();
            }
            else if (value is System.Dynamic.ExpandoObject eo)
            {
                add = 2;
                foreach (var kv in eo.ToArray())
                {
                    add += Encoding.UTF8.GetByteCount(kv.Key);
                    add += 6 + CalcSendableSize(kv.Value, 0, size);
                }
            }

            //階層が深くなると加算される
            add += arrayAddition;

            //なんだか分からないけど130に入った瞬間に+1される
            if (size < 130 && size + add >= 130) add++;

            return size + add;
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            return o;
        }
        public override string ToString()
        {
            return String.Format("[StateProxy]");
        }


        event Action _OnVariablesUpdated = delegate { };
        event Action Components.IVariablesStore.OnVariablesUpdated
        {
            add => _OnVariablesUpdated += value;
            remove => _OnVariablesUpdated -= value;
        }

        public class ExpandoObjectWrapper
            : Components.IVariable
        {
            public string name { get; private set; }
            public string value { get; private set; }
            public string type { get; private set; }
            public bool hasChild { get; private set; }
            public IEnumerable<Components.IVariable> children { get; private set; }

            public ExpandoObjectWrapper(
                string name,
                object value
            )
            {
                this.name = name;
                this.value = value.ToString();
                this.type = value.GetType().Name;
                if (value is object[] oa)
                {
                    this.type = "Array";
                    this.children = GetArrayChildren(oa);
                    this.hasChild = true;
                }
                else if (value is System.Dynamic.ExpandoObject eo)
                {
                    this.type = "Object";
                    this.children = GetObjectChildren(eo);
                    this.hasChild = true;
                }
                else if (value is double)
                {
                    this.type = "Number";
                    this.hasChild = false;
                }
                else if (value is EmulateVector2)
                {
                    this.type = "Vector2";
                    this.hasChild = false;
                }
                else if (value is EmulateVector2)
                {
                    this.type = "Vector2";
                    this.hasChild = false;
                }
                else if (value is EmulateVector3)
                {
                    this.type = "Vector3";
                    this.hasChild = false;
                }
                else if (value is EmulateQuaternion)
                {
                    this.type = "Quaternion";
                    this.hasChild = false;
                }
                else
                {
                    this.hasChild = false;
                }
            }
            IEnumerable<Components.IVariable> GetArrayChildren(object[] array)
            {
                for(var i = 0; i < array.Length; i++)
                {
                    if (array[i] == null) continue;
                    var ret = new ExpandoObjectWrapper(i.ToString(), array[i]);
                    yield return ret;
                }
                yield break;
            }
            IEnumerable<Components.IVariable> GetObjectChildren(System.Dynamic.ExpandoObject eo)
            {
                foreach(var kv in eo)
                {
                    if (kv.Value == null) continue;
                    var ret = new ExpandoObjectWrapper(kv.Key, kv.Value);
                    yield return ret;
                }
                yield break;
            }
        }

        IEnumerable<Components.IVariable> Components.IVariablesStore.GetVariables()
        {
            foreach(var kv in stateDictionary){
                if (kv.Value == null) continue;
                var ret = new ExpandoObjectWrapper(kv.Key, kv.Value);
                yield return ret;
            }
        }
    }
}
