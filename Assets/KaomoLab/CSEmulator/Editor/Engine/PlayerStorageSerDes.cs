using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class PlayerStorageSerDesFactory
        : EmulateClasses.IPlayerStoragerFactory
    {
        readonly ItemCollector itemCollector;
        readonly EmulateClasses.IPlayerHandleFactory playerHandleFactory;
        readonly ISerializedPlayerStorage playerStorage;
        readonly IItemExceptionFactory itemExceptionFactory;
        readonly Jint.Engine engine;

        public PlayerStorageSerDesFactory(
            ItemCollector itemCollector,
            EmulateClasses.IPlayerHandleFactory playerHandleFactory,
            ISerializedPlayerStorage playerStorage,
            IItemExceptionFactory itemExceptionFactory,
            Jint.Engine engine
        )
        {
            this.itemCollector = itemCollector;
            this.playerHandleFactory = playerHandleFactory;
            this.playerStorage = playerStorage;
            this.itemExceptionFactory = itemExceptionFactory;
            this.engine = engine;
        }

        public EmulateClasses.IPlayerStorager Create(
            Components.CSEmulatorItemHandler csItemHandler
        )
        {
            var ret = new PlayerStorageSerDes(
                csItemHandler,
                itemCollector,
                playerHandleFactory,
                playerStorage,
                itemExceptionFactory,
                engine
            );
            return ret;
        }
    }

    public class PlayerStorageSerDes
        : EmulateClasses.IPlayerStorager
    {
        [Serializable]
        public class StorageNode
        {
            public StorageObject @object;
            public StorageArray array;
            public StorageItem item;
        }
        [Serializable]
        public class StorageKeyedNode
        {
            public string key;
            public StorageNode node;
        }
        [Serializable]
        public class StorageObject
        {
            public bool valid = false;
            public List<StorageKeyedNode> nodes = new();
        }
        [Serializable]
        public class StorageArray
        {
            public bool valid = false;
            public List<StorageNode> nodes = new();
        }
        [Serializable]
        public class StorageItem
        {
            public bool valid = false;
            public ItemTypes type;
            public string serialized;
        }

        public enum ItemTypes
        {
            Null, Undefined, Bool, Number, String,
            Vector2, Vector3, Quaternion,
            PlayerId, ItemId
        }

        readonly Components.CSEmulatorItemHandler csItemHandler;
        readonly ItemCollector itemCollector;
        readonly EmulateClasses.IPlayerHandleFactory playerHandleFactory;
        readonly ISerializedPlayerStorage playerStorage;
        readonly IItemExceptionFactory itemExceptionFactory;
        readonly Jint.Engine engine;

        public PlayerStorageSerDes(
            Components.CSEmulatorItemHandler csItemHandler,
            ItemCollector itemCollector,
            EmulateClasses.IPlayerHandleFactory playerHandleFactory,
            ISerializedPlayerStorage playerStorage,
            IItemExceptionFactory itemExceptionFactory,
            Jint.Engine engine
        )
        {
            this.csItemHandler = csItemHandler;
            this.itemCollector = itemCollector;
            this.playerHandleFactory = playerHandleFactory;
            this.playerStorage = playerStorage;
            this.itemExceptionFactory = itemExceptionFactory;
            this.engine = engine;
        }

        public void Save(object value)
        {
            var n = Serialize(value);
            var s = ToJson(n);
            playerStorage.SavePlayerStorage(s);
        }
        string ToJson(object value)
        {
            return UnityEngine.JsonUtility.ToJson(value, false);
        }
        StorageNode Serialize(object value)
        {
            var ret = new StorageNode();
            if (value == null)
            {
                ret.item = new StorageItem() { valid = true, type = ItemTypes.Null };
                return ret;
            }
            else if (value is Jint.Native.JsNull)
            {
                ret.item = new StorageItem() { valid = true, type = ItemTypes.Null };
                return ret;
            }
            else if (value is Jint.Native.JsUndefined)
            {
                throw itemExceptionFactory.CreateGeneral("PlayerStorageではundefinedは指定できません");
            }
            else if (value is Jint.Native.JsBoolean boolValue)
            {
                ret.item = new StorageItem() { valid = true, type = ItemTypes.Bool, serialized = ((bool)boolValue.ToObject() ? "1" : "0") };
                return ret;
            }
            else if (value is Jint.Native.JsNumber numberValue)
            {
                ret.item = new StorageItem() { valid = true, type = ItemTypes.Number, serialized = numberValue.ToString() };
                return ret;
            }
            else if (value is Jint.Native.JsString stringValue)
            {
                ret.item = new StorageItem() { valid = true, type = ItemTypes.String, serialized = stringValue.ToString() };
                return ret;
            }
            else if (value is EmulateClasses.EmulateVector2 vector2)
            {
                ret.item = new StorageItem() { valid = true, type = ItemTypes.Vector2, serialized = ToJson(vector2._ToUnityEngine()) };
                return ret;
            }
            else if (value is EmulateClasses.EmulateVector3 vector3)
            {
                ret.item = new StorageItem() { valid = true, type = ItemTypes.Vector3, serialized = ToJson(vector3._ToUnityEngine()) };
                return ret;
            }
            else if (value is EmulateClasses.EmulateQuaternion quaternion)
            {
                ret.item = new StorageItem() { valid = true, type = ItemTypes.Quaternion, serialized = ToJson(quaternion._ToUnityEngine()) };
                return ret;
            }
            else if (value is EmulateClasses.ItemId itemId)
            {
                ret.item = new StorageItem() { valid = true, type = ItemTypes.ItemId, serialized = itemId.id };
                return ret;
            }
            else if (value is EmulateClasses.PlayerId playerId)
            {
                ret.item = new StorageItem() { valid = true, type = ItemTypes.PlayerId, serialized = playerId.__idfc };
                return ret;
            }
            else if (value is Jint.Runtime.Interop.ObjectWrapper wrapped)
            {
                //剥がして再走
                var v = Serialize(wrapped.Target);
                return v; //再走先でnodeに入ってるのでretに入れない
            }
            else if (value.GetType().IsArray)
            {
                var objects = (object[])value;
                var nodes = new List<StorageNode>();
                foreach (var o in objects)
                {
                    nodes.Add(Serialize(o));
                }
                ret.array = new StorageArray() { valid = true, nodes = nodes };
                return ret;
            }
            else if (value is Jint.Native.Array.ArrayInstance objects)
            {
                var nodes = new List<StorageNode>();
                foreach (var o in objects)
                {
                    nodes.Add(Serialize(o));
                }
                ret.array = new StorageArray() { valid = true, nodes = nodes };
                return ret;
            }
            else if (value is Jint.Native.Object.ObjectInstance oi)
            {
                var so = new StorageObject() { valid = true };
                ret.@object = so;
                foreach (var key in oi.GetOwnPropertyKeys(Jint.Runtime.Types.String))
                {
                    var o = new StorageKeyedNode();
                    o.key = key.ToString();
                    o.node = Serialize(oi[key]);
                    so.nodes.Add(o);
                }
                return ret;
            }
            else if (value is ExpandoObject eo)
            {
                var so = new StorageObject() { valid = true };
                ret.@object = so;
                foreach (var kv in eo.ToArray())
                {
                    var o = new StorageKeyedNode();
                    o.key = kv.Key;
                    o.node = Serialize(kv.Value);
                    so.nodes.Add(o);
                }
                return ret;

            }

            throw new Exception(String.Format("このエラーが出た場合は開発者に連絡してください。{0}", value));
        }

        public object Load()
        {
            var s = playerStorage.LoadPlayerStorage();
            var n = FromJson<StorageNode>(s);
            var o = Deserialize(n);
            return o;
        }
        T FromJson<T>(string value)
        {
            return UnityEngine.JsonUtility.FromJson<T>(value);
        }
        object Deserialize(StorageNode node)
        {
            if(node == null)
            {
                return Jint.Native.JsValue.Null;
            }

            if (node.item.valid)
            {
                var item = node.item;
                if(item.type == ItemTypes.Null)
                {
                    return Jint.Native.JsValue.Null;
                }
                else if(item.type == ItemTypes.Undefined)
                {
                    throw itemExceptionFactory.CreateGeneral("PlayerStorageではundefinedは指定できません");
                }
                else if(item.type == ItemTypes.Bool)
                {
                    return item.serialized == "1";
                }
                else if(item.type == ItemTypes.Number)
                {
                    return double.Parse(item.serialized);
                }
                else if(item.type == ItemTypes.String)
                {
                    return item.serialized;
                }
                else if (item.type == ItemTypes.Vector2)
                {
                    var v2 = FromJson<UnityEngine.Vector2>(item.serialized);
                    return new EmulateClasses.EmulateVector2(v2);
                }
                else if (item.type == ItemTypes.Vector3)
                {
                    var v3 = FromJson<UnityEngine.Vector3>(item.serialized);
                    return new EmulateClasses.EmulateVector3(v3);
                }
                else if (item.type == ItemTypes.Quaternion)
                {
                    var q = FromJson<UnityEngine.Quaternion>(item.serialized);
                    return new EmulateClasses.EmulateQuaternion(q);
                }
                else if (item.type == ItemTypes.ItemId)
                {
                    var target = itemCollector.GetAllItems()
                        .Where(i => i.Id.ToString() == item.serialized)
                        .FirstOrDefault();
                    if(target == null)
                        return new EmulateClasses.ItemId();
                    var c = target.gameObject.GetComponent<Components.CSEmulatorItemHandler>();
                    var ret = new EmulateClasses.ItemId(c);
                    return ret;
                }
                else if (item.type == ItemTypes.PlayerId)
                {
                    //CSETODO PlayerIdをPlayerHandle無しでつくりたい
                    //ここの処理はなんかまずい気がする
                    var h = playerHandleFactory.CreateByIdfc(item.serialized, csItemHandler);
                    var p = new EmulateClasses.PlayerId(h);
                    return h;
                }
            }
            else if (node.@object.valid)
            {
                var ret = new Jint.Native.JsObject(engine);
                foreach(var n in node.@object.nodes)
                {
                    var o = Deserialize(n.node);
                    ret.Set(n.key, Jint.Native.JsValue.FromObject(engine, o));
                }
                return ret;
            }
            else if (node.array.valid)
            {
                var jvs = new List<Jint.Native.JsValue>();
                foreach (var n in node.array.nodes)
                {
                    var o = Deserialize(n);
                    var jv = Jint.Native.JsValue.FromObject(engine, o);
                    jvs.Add(jv);
                }
                var ret = new Jint.Native.JsArray(engine, jvs.ToArray());
                return ret;
            }

            throw new Exception(String.Format("このエラーが出た場合は開発者に連絡してください。{0}", node));
        }

    }
}
