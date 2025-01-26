using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class ItemHandle
        : ISendableSize, IHasUnofficialMembers
    {

        public string id
        {
            get => invalid ? "0" : csItemHandler.id;
        }
        public readonly string type = "item";

        public Components.CSEmulatorItemHandler csItemHandler { get; private set; }
        //ownerの切り替えにはnewを強制しておきたいためprivate
        readonly Components.CSEmulatorItemHandler csOwnerItemHandler;
        readonly ISpaceContext spaceContext;
        readonly IRunningContext runningContext;
        readonly ISendableSanitizer sendableSanitizer;
        readonly IMessageSender messageSender;
        readonly ClusterVR.CreatorKit.Item.IMovableItem movableItem;

        readonly bool invalid = false;

        //無効なハンドル
        public ItemHandle()
        {
            invalid = true;
        }

        //csOwnerItemHandlerとはこのハンドルがいるスクリプト空間($)のこと。
        public ItemHandle(
            Components.CSEmulatorItemHandler csItemHandler,
            Components.CSEmulatorItemHandler csOwnerItemHandler,
            ISpaceContext spaceContext,
            IRunningContext runningContext,
            ISendableSanitizer sendableSanitizer,
            IMessageSender messageSender
        )
        {
            this.csItemHandler = csItemHandler;
            //CSETODO 将来的にスクリプト実行オーナーの概念をCSEmulatorItemHandlerから分離させたときに
            //canMove()といった関数でチェックさせるようにする。
            //この修正はそれまでの仮対応。
            if (csItemHandler.Exists())
                movableItem = csItemHandler.gameObject.GetComponent<ClusterVR.CreatorKit.Item.IMovableItem>();
            else
                movableItem = null;

            this.csOwnerItemHandler = csOwnerItemHandler;

            this.spaceContext = spaceContext;
            this.runningContext = runningContext;
            this.sendableSanitizer = sendableSanitizer;
            this.messageSender = messageSender;

            //おそらくメモリーリークの原因になるのでNG
            //csItemHandler.OnFixedUpdate += CsItemHandler_OnFixedUpdate;
        }

        public void addImpulsiveForce(EmulateVector3 force)
        {
            if (invalid) return;
            if (runningContext.CheckTopLevel("ItemHandle.addImpulsiveForce()")) return;
            if (!csItemHandler.Exists() || !csOwnerItemHandler.Exists()) return;
            CheckOwnerOperationLimit();

            //AddInstantForceItemGimmickを参考にしている。
            if (movableItem == null)
            {
                UnityEngine.Debug.LogWarning("Need MovableItem");
                return;
            }
            csItemHandler.AddFixedUpdateAction(() =>
            {
                movableItem.AddForce(force._ToUnityEngine(), UnityEngine.ForceMode.Impulse);
            });
        }

        public void addImpulsiveForceAt(EmulateVector3 impluse, EmulateVector3 position)
        {
            if (invalid) return;
            if (runningContext.CheckTopLevel("ItemHandle.addImpulsiveForceAt()")) return;
            if (!csItemHandler.Exists() || !csOwnerItemHandler.Exists()) return;
            CheckOwnerOperationLimit();

            csItemHandler.AddFixedUpdateAction(() =>
            {
                movableItem.AddForceAtPosition(
                    impluse._ToUnityEngine(),
                    position._ToUnityEngine(),
                    UnityEngine.ForceMode.Impulse
                );
            });
        }

        public void addImpulsiveTorque(EmulateVector3 torque)
        {
            if (invalid) return;
            if (runningContext.CheckTopLevel("ItemHandle.addImpulsiveTorque()")) return;
            //CSETODO 単にreturnではなく親切にメッセージを出したい。
            if (!csItemHandler.Exists() || !csOwnerItemHandler.Exists()) return;
            if (!exists()) return;
            CheckOwnerOperationLimit();

            csItemHandler.AddFixedUpdateAction(() =>
            {
                movableItem.AddForce(torque._ToUnityEngine(), UnityEngine.ForceMode.Impulse);
            });
        }

        public bool exists()
        {
            if (invalid) return false;
            if (runningContext.CheckTopLevel("ItemHandle.exists()")) return false;
            return csItemHandler.Exists();
        }

        public void send(string requestName, Jint.Native.JsValue arg)
        {
            if (invalid) return;
            if (runningContext.CheckTopLevel("ItemHandle.send()")) return;
            if (arg is Jint.Native.JsUndefined)
            {
                UnityEngine.Debug.LogWarning("undefinedはsendできません。");
                return;
            }

            var obj = arg.ToObject();
            send(requestName, obj);
        }
        public void send(string requestName, object arg)
        {
            if (invalid) return;
            if (runningContext.CheckTopLevel("ItemHandle.send()")) return;
            //CSETODO 単にreturnではなく親切にメッセージを出したい。
            if (!csItemHandler.Exists() || !csOwnerItemHandler.Exists()) return;
            //CSETODO ワーニング対応がエラー対応になるまでの仮
            try
            {
                //CSETODO 旧制限を一応残しておく。確定したら関連以下消す。
                CheckRequestSizeLimit(requestName, arg);
                //CSETODO 新制限相当でかいので不要そうな気がする。
                //CheckSendOperationLimit(requestName, StateProxy.CalcSendableSize(arg, 0));
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning(ex.Message);
            }

            var sanitized = sendableSanitizer.Sanitize(arg);
            messageSender.SendToItem(
                csItemHandler.item.Id.Value.ToString(), requestName, sanitized, null, csOwnerItemHandler
            );
        }

        void CheckRequestSizeLimit(string requestName, object arg)
        {
            var rlen = Encoding.UTF8.GetByteCount(requestName);
            var alen = StateProxy.CalcSendableSize(arg, 0);
            if (rlen <= 100 && alen <= 1000) return;

            throw csOwnerItemHandler.itemExceptionFactory.CreateRequestSizeLimitExceeded(
                String.Format("[{0}][messageType:{1}][arg:{2}]", csItemHandler, rlen, alen)
            );
        }
        void CheckOwnerDistanceLimit()
        {
            var p1 = csItemHandler.gameObject.transform.position;
            var p2 = csOwnerItemHandler.gameObject.transform.position;
            var d = UnityEngine.Vector3.Distance(p1, p2);
            //30メートル以内はOK
            if (d <= 30f) return;

            throw csOwnerItemHandler.itemExceptionFactory.CreateDistanceLimitExceeded(
                String.Format("[{0}]>>>[{1}]", csOwnerItemHandler, csItemHandler)
            );
        }
        void CheckOwnerOperationLimit()
        {
            var result = csOwnerItemHandler.TryItemOperate();
            if (result) return;

            throw csOwnerItemHandler.itemExceptionFactory.CreateRateLimitExceeded(
                String.Format("[{0}]>>>[{1}]", csOwnerItemHandler, csItemHandler)
            );
        }
        //void CheckSendOperationLimit(string messageType, int size)
        //{
        //    var spaceLimit = spaceContext.TrySendOperate(size);

        //    if (spaceLimit && playerLimit) return;

        //    throw csOwnerItemHandler.itemExceptionFactory.CreateRateLimitExceeded(
        //        String.Format("Send制限:{3}:スペース({0}):プレイヤー({1}):[{2}]", spaceLimit ? "OK" : "NG", playerLimit ? "OK" : "NG", this, messageType)
        //    );
        //}

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            o.id = id;
            return o;
        }
        public override string ToString()
        {
            if (invalid) return string.Format("[ItemHandle][無効]");
            return string.Format("[ItemHandle][{0}][{1}]", csItemHandler == null ? "null" : csItemHandler.gameObjectName, id);
        }

        public int GetSize()
        {
            //おそらく固定。アイテム名やGameObject名では差がない。
            return 13;
        }

        string[] IHasUnofficialMembers.GetPropertyNames()
        {
            return new string[]
            {
                nameof(csItemHandler),
            };
        }
    }
}
