using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ClusterVR.CreatorKit;
using UnityEditor;
using UnityEngine.UIElements;
using Assets.KaomoLab.CSEmulator.Components;
using JintFunction = Jint.Native.Function.Function;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class ClusterScript
    {
        public GameObject gameObject { get; private set; }
        public readonly ICckComponentFacade cckComponentFacade;
        readonly IItemLifecycler itemLifecycler;
        readonly ISpaceContext spaceContext;
        readonly IRunningContext runningContext;
        public readonly ICallbackExecutor callbackExecutor;
        readonly IStartListenerBinder startListenerBinder;
        readonly IUpdateListenerBinder updateListenerBinder;
        readonly IUpdateListenerBinder fixedUpdateListenerBinder;
        readonly IItemReceiveListenerBinder itemReceiveListenerBinder;
        readonly IMessageSender messageSender;
        readonly ITextInputListenerBinder textInputListenerBinder;
        readonly IItemOwnerHandler itemOwnerHandler;
        readonly IPlayerHandleFactory playerHandleFactory;
        readonly IProgramStatus programStatus;
        //こういうタイプの公開は悪手だと分かっているがもう面倒なので
        public IItemExceptionFactory itemExceptionFactory { get; private set; }
        readonly IExternalCaller externalCaller;
        readonly IMaterialSubstituter materialSubstituter;
        readonly IProductPurchaser productPurchaser;
        readonly IClusterEvent clusterEvent;
        readonly PlayerScriptSetter playerScriptSetter;
        readonly ISendableSanitizer sendableSanitizer;
        readonly IRayDrawer rayDrawer;
        readonly StateProxy stateProxy;
        readonly GroupStateProxy groupStateProxy;
        readonly IGroupStateProxyMapper groupStateProxyMapper;
        readonly ILoggingOptions loggingOptions;
        readonly ILogger logger;

        readonly ClusterVR.CreatorKit.Item.Implements.MovableItem movableItem;
        readonly ClusterVR.CreatorKit.Item.IItem item;
        public Components.CSEmulatorItemHandler csItemHandler { get; private set; }

        readonly bool hasMovableItem;
        readonly bool hasCharacterItem;

        bool isInFixedUpdate = false;

        readonly BurstableThrottle createItemThrottle = new BurstableThrottle(0.09d, 5);
        IChargeThrottle callExternalThrottle = new PassThroughThrottle();
        readonly BurstableThrottle requestOwnerThrottle = new BurstableThrottle(0.09d, 5);

        JintCallback<Collision> OnCollideHandler = new();
        JintCallback<GiftInfo[]> OnGiftSentHandler = new();
        JintCallback<bool, bool, PlayerHandle> OnGrabHandler = new();
        JintCallback<PlayerHandle> OnInteractHandler = new();
        JintCallback<bool, PlayerHandle> OnRideHandler = new();
        JintCallback<bool, PlayerHandle> OnUseHandler = new();
        JintCallback<EmulateVector2, PlayerHandle> OnSteerHandler = new();
        JintCallback<float, PlayerHandle> OnSteerAdditionalAxisHandler = new();

        PlayerHandle grabbingPlayer = null;
        PlayerHandle ridingPlayer = null;

        public ClusterScript(
            GameObject gameObject,
            ICckComponentFacadeFactory cckComponentFacadeFactory,
            IItemLifecycler itemLifecycler,
            ISpaceContext spaceContext,
            IRunningContext runningContext,
            ICallbackExecutor callbackExecutor,
            IStartListenerBinder startListenerBinder,
            IUpdateListenerBinder updateListenerBinder,
            IUpdateListenerBinder fixedUpdateListenerBinder,
            IItemReceiveListenerBinder itemReceiveListenerBinder,
            IMessageSender messageSender,
            ITextInputListenerBinder textInputListenerBinder,
            IItemOwnerHandler itemOwnerHandler,
            IPlayerHandleFactory playerHandleFactory,
            IItemExceptionFactory itemExceptionFactory,
            IExternalCaller externalCaller,
            IMaterialSubstituter materialSubstituer,
            IProductPurchaser productPurchaser,
            IClusterEvent clusterEvent,
            PlayerScriptSetter playerScriptSetter,
            ISendableSanitizer sendableSanitizer,
            IRayDrawer rayDrawer,
            StateProxy stateProxy,
            GroupStateProxy groupStateProxy,
            IGroupStateProxyMapper groupStateProxyHolder,
            ILoggingOptions loggingOptions,
            ILogger logger
        )
        {
            this.gameObject = gameObject;
            this.cckComponentFacade = cckComponentFacadeFactory.Create(gameObject);
            this.itemLifecycler = itemLifecycler;
            this.spaceContext = spaceContext;
            this.runningContext = runningContext;
            this.callbackExecutor = callbackExecutor;
            this.startListenerBinder = startListenerBinder;
            this.updateListenerBinder = updateListenerBinder;
            this.fixedUpdateListenerBinder = fixedUpdateListenerBinder;
            this.itemReceiveListenerBinder = itemReceiveListenerBinder;
            this.messageSender = messageSender;
            this.textInputListenerBinder = textInputListenerBinder;
            this.itemOwnerHandler = itemOwnerHandler;
            this.playerHandleFactory = playerHandleFactory;
            this.itemExceptionFactory = itemExceptionFactory;
            this.externalCaller = externalCaller;
            this.materialSubstituter = materialSubstituer;
            this.productPurchaser = productPurchaser;
            this.clusterEvent = clusterEvent;
            this.playerScriptSetter = playerScriptSetter;
            this.sendableSanitizer = sendableSanitizer;
            this.rayDrawer = rayDrawer;
            this.stateProxy = stateProxy;
            this.groupStateProxy = groupStateProxy;
            this.groupStateProxyMapper = groupStateProxyHolder;
            this.loggingOptions = loggingOptions;
            this.logger = logger;

            item = this.gameObject.GetComponent<ClusterVR.CreatorKit.Item.IItem>();
            csItemHandler = this.gameObject.GetComponent<Components.CSEmulatorItemHandler>();
            csItemHandler.OnCollision += CsItemHandler_OnCollision;
            csItemHandler.OnGroupMember += CsItemHandler_OnGroupMember;
            hasMovableItem = this.gameObject.TryGetComponent(out movableItem);
            hasCharacterItem = this.gameObject.TryGetComponent<ClusterVR.CreatorKit.Item.Implements.CharacterItem>(out var _);

            cckComponentFacade.onGrabbed += CckComponentFacade_onGrabbed;
            cckComponentFacade.onRide += CckComponentFacade_onRide;
            cckComponentFacade.onInteract += CckComponentFacade_onInteract;
            cckComponentFacade.onUse += CckComponentFacade_onUse;
            cckComponentFacade.onSteerMove += CckComponentFacade_onSteerMove;
            cckComponentFacade.onSteerAdditionalAxis += CckComponentFacade_onSteerAdditionalAxis;

            this.externalCaller.OnChangeLimit += ApplyCallExternalLimit;
            ApplyCallExternalLimit();

            clusterEvent.OnGiftSent += ClusterEvent_OnGiftSent;
        }

        ClusterVR.CreatorKit.Item.ItemId itemId
        {
            //itemIdはGameObject生成直後は0なので、都度取得にすることで、0になる状況を緩和。
            get => item.Id;
        }

        public EmulateVector3 angularVelocity
        {
            get {
                if (runningContext.CheckTopLevel("ClusterScript.angularVelocity")) return new EmulateVector3(0, 0, 0);
                if (!hasMovableItem) return new EmulateVector3(0, 0, 0);
                return new EmulateVector3(movableItem.AngularVelocity);
            }
            set
            {
                if (runningContext.CheckTopLevel("ClusterScript.angularVelocity")) return;
                if (cckComponentFacade.isGrab) return;
                if (!hasMovableItem) throw csItemHandler.itemExceptionFactory.CreateGeneral("MovableItemが必要です。");
                movableItem.Rigidbody.angularVelocity = value._ToUnityEngine();
            }
        }

        public string id
        {
            get => csItemHandler.id;
        }

        public ItemHandle itemHandle
        {
            //cacheしてもいいかもしれないけど、
            //都度newするという想定から外れるとロクなことが起きないのでnewしている。
            get => new ItemHandle(
                csItemHandler, this.csItemHandler, spaceContext, runningContext, sendableSanitizer, messageSender
            );
        }

        public ItemTemplateId itemTemplateId
        {
            get
            {
                var prefabItem = gameObject.GetComponent<CSEmulatorPrefabItem>();

                if (prefabItem == null) return null;

                return new ItemTemplateId(prefabItem.id);
            }
        }

        public StateProxy state
        {
            get => stateProxy;
        }

        public GroupStateProxy groupState
        {
            //ドキュメント上はStateProxyを返すことになっているけども、実用上問題ないのでこのまま
            get => groupStateProxy;
        }

        public bool useGravity
        {
            get
            {
                if (runningContext.CheckTopLevel("ClusterScript.useGravity")) return false;
                if (!hasMovableItem) return false;
                if (movableItem.Rigidbody.isKinematic) return false;
                return movableItem.Rigidbody.useGravity;
            }
            set
            {
                if (runningContext.CheckTopLevel("ClusterScript.useGravity")) return;
                if (!hasMovableItem) throw csItemHandler.itemExceptionFactory.CreateGeneral("MovableItemが必要です。");
                if (movableItem.Rigidbody.isKinematic) throw csItemHandler.itemExceptionFactory.CreateGeneral("非Kinematicにしてください。");
                movableItem.Rigidbody.useGravity = value;
            }
        }

        public EmulateVector3 velocity
        {
            get
            {
                if (runningContext.CheckTopLevel("ClusterScript.velocity")) return new EmulateVector3(0, 0, 0);
                if (!hasMovableItem) return new EmulateVector3(0, 0, 0);
                return new EmulateVector3(movableItem.Velocity);
            }
            set
            {
                if (runningContext.CheckTopLevel("ClusterScript.velocity")) return;
                if (cckComponentFacade.isGrab) return;
                if (!hasMovableItem) throw csItemHandler.itemExceptionFactory.CreateGeneral("MovableItemが必要です。");
                movableItem.Rigidbody.velocity = value._ToUnityEngine();
            }
        }

        public void addForce(EmulateVector3 force)
        {
            if (runningContext.CheckTopLevel("ClusterScript.addForc()")) return;
            if (!isInFixedUpdate)
                throw csItemHandler.itemExceptionFactory.CreateExecutionNotAllowed("onPhysicsUpdate内でのみ実行可能です。");
            movableItem.AddForce(force._ToUnityEngine(), ForceMode.Force);
        }

        public void addForceAt(EmulateVector3 force, EmulateVector3 position)
        {
            if (runningContext.CheckTopLevel("ClusterScript.addForceAt()")) return;
            if (!isInFixedUpdate)
                throw csItemHandler.itemExceptionFactory.CreateExecutionNotAllowed("onPhysicsUpdate内でのみ実行可能です。");
            movableItem.AddForceAtPosition(
                force._ToUnityEngine(), position._ToUnityEngine(), ForceMode.Force
            );
        }

        public void addImpulsiveForce(EmulateVector3 impulsiveForce)
        {
            if (runningContext.CheckTopLevel("ClusterScript.addImpulsiveForce()")) return;
            movableItem.AddForce(impulsiveForce._ToUnityEngine(), ForceMode.Impulse);
        }

        public void addImpulsiveForceAt(EmulateVector3 impulsiveForce, EmulateVector3 position)
        {
            if (runningContext.CheckTopLevel("ClusterScript.addImpulsiveForceAt()")) return;
            movableItem.AddForceAtPosition(
                impulsiveForce._ToUnityEngine(),
                position._ToUnityEngine(),
                ForceMode.Impulse
            );
        }

        public void addImpulsiveTorque(EmulateVector3 impulsiveTorque)
        {
            if (runningContext.CheckTopLevel("ClusterScript.addImpulsiveTorque()")) return;
            movableItem.AddTorque(
                impulsiveTorque._ToUnityEngine(), ForceMode.Impulse
            );
        }

        public void addTorque(EmulateVector3 torque)
        {
            if (runningContext.CheckTopLevel("ClusterScript.addTorque()")) return;
            if (!isInFixedUpdate)
                throw csItemHandler.itemExceptionFactory.CreateExecutionNotAllowed("onPhysicsUpdate内でのみ実行可能です。");
            movableItem.AddTorque(
                torque._ToUnityEngine(), ForceMode.Force
            );

        }

        public ApiAudio audio(string itemAudioSetId)
        {
            var itemAudioSetList = gameObject.GetComponent<ClusterVR.CreatorKit.Item.IItemAudioSetList>();
            //無い場合は、各値のデフォルト値が入った構造体が渡される。
            var itemAudioSet = itemAudioSetList.ItemAudioSets.FirstOrDefault(set => set.Id == itemAudioSetId);
            var apiAudio = new ApiAudio(itemAudioSet, runningContext, gameObject);

            return apiAudio;
        }

        public void callExternal(
            string request,
            string meta
        )
        {
            if (runningContext.CheckTopLevel("ClusterScript.callExternal()")) return;
            CheckCallExternalSizeLimit(request, meta);
            CheckCallExternalOperationLimit();

            externalCaller.CallExternal(request, meta);
        }
        void ApplyCallExternalLimit()
        {
            callExternalThrottle = this.externalCaller.rateLimit switch
            {
                CallExternalRateLimit.unlimited => new PassThroughThrottle(),
                CallExternalRateLimit.limit5 => new BurstableThrottle(12.0d, 5),
                CallExternalRateLimit.limit100 => new BurstableThrottle(60d / 100d, 5),
                _ => throw new NotImplementedException()
            };
        }
        void CheckCallExternalSizeLimit(string request, string meta)
        {
            if (Encoding.UTF8.GetByteCount(request) > 1000)
            {
                throw itemExceptionFactory.CreateRequestSizeLimitExceeded(
                    String.Format("[{0}][request]", gameObject.name)
                );
            }
            if (Encoding.UTF8.GetByteCount(meta) > 100)
            {
                throw itemExceptionFactory.CreateRequestSizeLimitExceeded(
                    String.Format("[{0}][meta]", gameObject.name)
                );
            }
        }
        void CheckCallExternalOperationLimit()
        {
            var result = callExternalThrottle.TryCharge();
            if (result) return;

            throw itemExceptionFactory.CreateRateLimitExceeded(
                String.Format("[{0}]", gameObject.name)
            );
        }

        public void clearVisiblePlayers()
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null) return;

            renderer.enabled = true;
            SetVisibleLayer(true);
        }

        public int computeSendableSize(object obj)
        {
            var size = StateProxy.CalcSendableSize(obj, 0);
            return size;
        }

        public ItemHandle createItem(
            ItemTemplateId itemTemplateId,
            EmulateVector3 position,
            EmulateQuaternion rotation
        )
        {
            if (runningContext.CheckTopLevel("ClusterScript.createItem()")) return null;
            CheckCreateItemOperationLimit();

            var create = itemLifecycler.CreateItem(itemTemplateId, position, rotation);
            if (create == null) return null;

            var csItemHandler = create.gameObject.GetComponent<Components.CSEmulatorItemHandler>();
            var ret = new ItemHandle(csItemHandler, this.csItemHandler, spaceContext, runningContext, sendableSanitizer, messageSender);

            return ret;
        }
        public ItemHandle createItem(
            ItemTemplateId itemTemplateId,
            EmulateVector3 position,
            EmulateQuaternion rotation,
            object option
        )
        {
            //ExpandoObjectで来る
            var opt = (IDictionary<string, object>)option;
            var asMember = opt.ContainsKey("asMember") ? (bool)opt["asMember"] : false;

            if (asMember && !groupState.isHost)
                throw itemExceptionFactory.CreateGeneral("Memberをcreateする場合はHostの必要があります。");

            var itemHandle = createItem(itemTemplateId, position, rotation);
            if (asMember) itemHandle.csItemHandler.SetItemGroupMember(id);

            return itemHandle;
        }
        public ItemHandle createItem(
            WorldItemTemplateId worldItemTemplateId,
            EmulateVector3 position,
            EmulateQuaternion rotation
        )
        {
            if (runningContext.CheckTopLevel("ClusterScript.createItem()")) return null;
            CheckCreateItemOperationLimit();

            var templateList = gameObject.GetComponent<ClusterVR.CreatorKit.Item.IWorldItemTemplateList>();
            if(templateList == null)
            {
                throw itemExceptionFactory.CreateGeneral(
                    "WorldItemTemplateListが指定されていません。"
                );
            }
            var entry = templateList.WorldItemTemplates.FirstOrDefault(set => set.Id == worldItemTemplateId._id);
            if (entry == null || entry.WorldItemTemplate == null)
            {
                throw itemExceptionFactory.CreateGeneral(
                    String.Format("WorldItemTemplateId:{0}が無効です。", worldItemTemplateId._id)
                );
            }

            var create = itemLifecycler.CreateItem(entry, position, rotation);
            if (create == null) return null;

            var csItemHandler = create.gameObject.GetComponent<Components.CSEmulatorItemHandler>();
            var ret = new ItemHandle(csItemHandler, this.csItemHandler, spaceContext, runningContext, sendableSanitizer, messageSender);

            return ret;
        }
        public ItemHandle createItem(
            WorldItemTemplateId worldItemTemplateId,
            EmulateVector3 position,
            EmulateQuaternion rotation,
            object option
        )
        {
            //ExpandoObjectで来る
            var opt = (IDictionary<string, object>)option;
            var asMember = opt.ContainsKey("asMember") ? (bool)opt["asMember"] : false;

            if (asMember && !groupState.isHost)
                throw itemExceptionFactory.CreateGeneral("Memberをcreateする場合はHostの必要があります。");

            var itemHandle = createItem(worldItemTemplateId, position, rotation);
            if (asMember) itemHandle.csItemHandler.SetItemGroupMember(id);

            return itemHandle;
        }

        private void CsItemHandler_OnGroupMember(string hostItemId)
        {
            if(!groupStateProxy.isHost)
            {
                groupStateProxyMapper.ApplyState(groupStateProxy, hostItemId);
            }
        }
        void CheckCreateItemOperationLimit()
        {
            var result = createItemThrottle.TryCharge();
            if (result) return;

            throw itemExceptionFactory.CreateRateLimitExceeded(
                String.Format("[{0}]", gameObject.name)
            );
        }

        public void destroy()
        {
            if (runningContext.CheckTopLevel("ClusterScript.destroy()")) return;
            if (!arrowDestroy() && !csItemHandler.isCreatedItem)
                throw csItemHandler.itemExceptionFactory.CreateExecutionNotAllowed("動的アイテムのみ実行可能です。クラフトアイテムの場合は[CS Emulator Prefab Item]コンポーネントを付けてください。");
            groupState.DisableState();
            itemLifecycler.DestroyItem(item);
        }
        bool arrowDestroy()
        {
            var prefabItem = gameObject.GetComponent<Components.CSEmulatorPrefabItem>();
            if (prefabItem == null) return false;
            var allow = prefabItem.allowDestroy;
            return allow;
        }

        public PlayerHandle getGrabbingPlayer()
        {
            return grabbingPlayer;
        }

        public ItemHandle[] getItemsNear(EmulateVector3 position, float radius)
        {
            if (runningContext.CheckTopLevel("ClusterScript.getItemsNear()")) return new ItemHandle[0];
            var handles = Physics.OverlapSphere(
                position._ToUnityEngine(), radius,
                CSEmulator.Commons.BuildLayerMask(0, 11, 14, 18), //Default, RidingItem, InteractableItem, GrabbingItem
                QueryTriggerInteraction.Collide
            )
                .Select(c => new {
                    i = c.gameObject.GetComponentInParent<ClusterVR.CreatorKit.Item.IItem>(),
                    c = c
                })
                .Where(t => t.i != null)
                .Where(t => !t.i.Id.Equals(item.Id))
                .Where(t =>
                {
                    if (null != t.c.gameObject.GetComponent<ClusterVR.CreatorKit.Item.IPhysicalShape>())
                        return true;
                    if (null != t.c.gameObject.GetComponent<ClusterVR.CreatorKit.Item.IOverlapSourceShape>())
                        return true;
                    if (!t.c.isTrigger)
                        return true;
                    return false;
                })
                .Select(t => t.i.gameObject.GetComponent<Components.CSEmulatorItemHandler>())
                .Select(h => new ItemHandle(h, this.csItemHandler, spaceContext, runningContext, sendableSanitizer, messageSender))
                .ToArray();
            return handles;
        }

        public Overlap[] getOverlaps()
        {
            if (runningContext.CheckTopLevel("ClusterScript.getOverlaps()")) return new Overlap[0];
            var overlaps = csItemHandler.GetOverlaps()
                .Select(o =>
                {
                    var hitObject = HitObject.Create(
                        o.Item2, this.csItemHandler, o.Item3,
                        playerHandleFactory,
                        spaceContext,
                        runningContext,
                        sendableSanitizer,
                        messageSender
                    );
                    object selfNode = o.Item1 == "" ? this : subNode(o.Item1);
                    var ret = new Overlap(hitObject, selfNode);
                    return ret;
                }).ToArray();
            return overlaps;
        }

        public void getOwnProducts(string productId, PlayerHandle players, string meta)
        {
            if (players == null)
            {
                throw itemExceptionFactory.CreateGeneral("playerがnullです");
            }

            CheckGetOwnProductsLimit();

            productPurchaser.GetOwnProducts(csItemHandler.item.Id.Value, productId, new PlayerHandle[] { players }, meta);

        }
        public void getOwnProducts(string productId, PlayerHandle[] players, string meta)
        {
            CheckGetOwnProductsLimit();

            productPurchaser.GetOwnProducts(csItemHandler.item.Id.Value, productId, players, meta);
        }
        void CheckGetOwnProductsLimit()
        {
            if (productPurchaser.IsGetOwnProductsLimit())
            {
                throw itemExceptionFactory.CreateRateLimitExceeded(
                    String.Format("[{0}]", gameObject.name)
                );
            }
        }

        public PlayerHandle getOwner()
        {
            var owner = playerHandleFactory.CreateByIdfc(
                itemOwnerHandler.GetOwnerIdfc(),
                csItemHandler
            );
            return owner;
        }

        public PlayerHandle[] getPlayersNear(EmulateVector3 position, float radius)
        {
            if (runningContext.CheckTopLevel("ClusterScript.getPlayersNear()")) return new PlayerHandle[0];
            var handles = Physics.OverlapSphere(
                position._ToUnityEngine(), radius,
                -1,
                QueryTriggerInteraction.Collide
            )
                .Select(c => c.gameObject.GetComponentInChildren<Components.CSEmulatorPlayerHandler>())
                .Where(h => h != null)
                .Select(h => playerHandleFactory.CreateByIdfc(h.idfc, csItemHandler))
                //いつの間にか重複破棄していた？v2.7.0.4確認
                .GroupBy(h => h.id)
                .Select(g => g.First())
                .ToArray();

            return handles;
        }

        public EmulateVector3 getPosition()
        {
            if (runningContext.CheckTopLevel("ClusterScript.getPosition()")) { } //メッセージのみ
            return new EmulateVector3(gameObject.transform.position);
        }

        public PlayerHandle getRidingPlayer()
        {
            return ridingPlayer;
        }

        public EmulateQuaternion getRotation()
        {
            if (runningContext.CheckTopLevel("ClusterScript.getRotation()")) { } //メッセージのみ
            return new EmulateQuaternion(gameObject.transform.rotation);
        }


        public object getStateCompat(string target, string key, string parameterType)
        {
            if (runningContext.CheckTopLevel("ClusterScript.getStateCompat()")) return ToDefalutValue(parameterType);
            var sendable = cckComponentFacade.GetState(target, key, parameterType);
            return sendable;
        }
        object ToDefalutValue(string parameterType)
        {
            switch (parameterType)
            {
                case "signal": return default(DateTime);
                case "boolean": return default(bool);
                case "float": return default(float);
                case "double": return default(double);
                case "integer": return default(int);
                case "vector2": return new EmulateVector2();
                case "vector3": return new EmulateVector3();
                default: throw new ArgumentException(parameterType);
            }
        }

        public UnityComponent getUnityComponent(string type)
        {
            var ret = UnityComponent.GetScriptableItemUnityComponent(
                gameObject, type, itemExceptionFactory
            );
            return ret;
        }

        public HumanoidAnimation humanoidAnimation(string humanoidAnimationId)
        {
            var list = gameObject.GetComponent<ClusterVR.CreatorKit.Item.IHumanoidAnimationList>();
            var entry = (ClusterVR.CreatorKit.Item.Implements.HumanoidAnimationListEntry)list.HumanoidAnimations.FirstOrDefault(entry => entry.Id == humanoidAnimationId);
            var ha = ClusterVR.CreatorKit.Editor.Builder.HumanoidAnimationBuilder.Build(entry.Animation);
            entry.SetHumanoidAnimation(ha);
            var humanoidAnimation = new HumanoidAnimation(entry, runningContext);

            return humanoidAnimation;
        }

        public bool isEvent()
        {
            return clusterEvent.isEvent;
        }

        public void log(object v)
        {
            if (v == null)
            {
                logger.Info("");
            }
            else if (v is System.Object[] oa)
            {
                logger.Info(CSEmulator.Commons.ObjectArrayToString(oa));
            }
            else if (v is System.Dynamic.ExpandoObject eo)
            {
                logger.Info(CSEmulator.Commons.ExpandoObjectToString(eo, openb: "{", closeb: "}", indent: "", separator: ","));
            }
            else if (v is Jint.Native.JsError je)
            {
                logger.Exception(je);
            }
            else
            {
                logger.Info(v.ToString());
            }
        }

        public MaterialHandle material(string materialId)
        {
            var itemMaterialSetList = gameObject.GetComponent<ClusterVR.CreatorKit.Item.IItemMaterialSetList>();
            if (itemMaterialSetList == null)
            {
                logger.Warning("ItemMaterialSetListが指定されていません。");
                return new MaterialHandle(null, runningContext, itemExceptionFactory);
            }
            var set = itemMaterialSetList.ItemMaterialSets.FirstOrDefault(set => set.Id == materialId);
            if (set.Material == null)
            {
                logger.Warning(String.Format("materialId:{0}がありません。", materialId));
                return new MaterialHandle(null, runningContext, itemExceptionFactory);
            }

            //アイテム毎にMaterialを複製して使用するような動きの模様
            var renderers = gameObject.GetComponentsInChildren<Renderer>();
            var prepared = materialSubstituter.Prepare(set.Material);
            foreach (var renderer in renderers)
            {
                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                {
                    if (materials[i].GetInstanceID() != set.Material.GetInstanceID()) continue;
                    materials[i] = prepared;
                }
                renderer.sharedMaterials = materials;
            }

            var ret = new MaterialHandle(prepared, runningContext, itemExceptionFactory);
            return ret;
        }

        public void onCollide(JintFunction Callback)
        {
            OnCollideHandler = new JintCallback<Collision>(callbackExecutor, Callback);
        }
        private void CsItemHandler_OnCollision(UnityEngine.Collision data)
        {
            //親でも子でもなくItem本体に付いているか(2.95検証)
            var rigid = gameObject.GetComponent<Rigidbody>();
            if (rigid == null) return;

            //kinematicはNG（2.95検証）
            if (rigid.isKinematic) return;

            var points = data.contacts.Select(c =>
            {
                var point = new CollidePoint(
                    new Hit(
                        new EmulateVector3(c.normal),
                        new EmulateVector3(c.point)
                    ),
                    gameObject.GetInstanceID() == c.thisCollider.gameObject.GetInstanceID()
                        ? this
                        : subNode(c.thisCollider.gameObject.name)
                ); ;
                return point;
            });
            //よくわからないけどRigidbodyが入っている場合はそちらが優先される仕様の模様？(2.95)
            var hitObject = GameObjectToHitObject(
                data.rigidbody?.gameObject ?? data.collider.gameObject
            );
            var collision = new Collision(
                points,
                new EmulateVector3(data.impulse),
                hitObject,
                new EmulateVector3(data.relativeVelocity)
            );
            OnCollideHandler.Execute(collision);
        }

        public void onExternalCallEnd(JintFunction Callback)
        {
            externalCaller.SetCallEndCallback(new JintCallback<string, string, string>(callbackExecutor, Callback));
        }

        public void onGetOwnProducts(JintFunction Callback)
        {
            var jintCallback = new JintCallback<OwnProduct[], string, string>(callbackExecutor, Callback);
            productPurchaser.SetGetOwnProductsCallback(csItemHandler.item.Id.Value, jintCallback);
        }

        public void onGiftSent(JintFunction Callback)
        {
            OnGiftSentHandler = new JintCallback<GiftInfo[]>(callbackExecutor, Callback);
        }
        private void ClusterEvent_OnGiftSent(string giftType, int price)
        {
            try
            {
                var player = playerHandleFactory.CreateByIdfc(
                    itemOwnerHandler.GetOwnerIdfc(),
                    csItemHandler
                );

                var position = player.playerController.GetFirstCameraTransform().position;
                var rotation = GetGiftRotation(player);

                var giftInfo = new GiftInfo(
                    giftType,
                    new EmulateVector3(position),
                    new EmulateQuaternion(rotation),
                    price,
                    player
                );

                OnGiftSentHandler.Execute(new GiftInfo[] { giftInfo });
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
            }
        }
        public Quaternion GetGiftRotation(PlayerHandle player)
        {
            if (player.playerController.isFirstPersonView)
            {
                var first = player.playerController.GetFirstCameraTransform().rotation;
                return first * Quaternion.Euler(-30, 0, 0);
            }
            else
            {
                var r = player.getRotation()._ToUnityEngine().eulerAngles;
                return Quaternion.Euler(-30, r.y, 0);
            }
        }

        public void onGrab(JintFunction Callback)
        {
            if (!cckComponentFacade.hasGrabbableItem)
            {
                logger.Warning(String.Format("[{0}]onGrab() need [Grabbable Item] component.", this.gameObject.name));
            }
            OnGrabHandler = new JintCallback<bool, bool, PlayerHandle>(callbackExecutor, Callback);
        }
        private void CckComponentFacade_onGrabbed(bool isLeftHand, bool isGrab)
        {
            try
            {
                //一旦右手＆オーナーの検出機能実装まで固定
                var owner = playerHandleFactory.CreateByIdfc(
                    itemOwnerHandler.GetOwnerIdfc(),
                    csItemHandler
                );
                grabbingPlayer = isGrab ? owner : null;
                owner.playerController.ChangeGrabbing(isGrab);
                OnGrabHandler.Execute(isGrab, false, owner);
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }
        }

        public void onInteract(JintFunction Callback)
        {
            if (!cckComponentFacade.hasCollider)
            {
                logger.Warning(String.Format("[{0}]onInteract() need [Collider] component.", this.gameObject.name));
                return;
            }

            //コライダーがある場合にのみInteractItemTriggerが付く仕様らしい
            cckComponentFacade.AddInteractItemTrigger();
            OnInteractHandler = new JintCallback<PlayerHandle>(callbackExecutor, Callback);
        }
        private void CckComponentFacade_onInteract()
        {
            try
            {
                var owner = playerHandleFactory.CreateByIdfc(
                    itemOwnerHandler.GetOwnerIdfc(),
                    csItemHandler
                );
                OnInteractHandler.Execute(owner);
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
            }
        }

        class JintCallbackPhysicsUpdate : IJintCallback<double>
        {
            readonly Action<double> Callback;
            public JintCallbackPhysicsUpdate(Action<double> Callback)
            {
                this.Callback = Callback;
            }
            public void Execute(double arg)
            {
                Callback(arg);
            }
        }
        public void onPhysicsUpdate(JintFunction Callback)
        {
            var jintCallback = new JintCallback<double>(callbackExecutor, Callback);
            var Wrapped = new JintCallbackPhysicsUpdate(v =>
            {
                isInFixedUpdate = true;
                jintCallback.Execute(v);
                isInFixedUpdate = false;
            });
            fixedUpdateListenerBinder.SetUpdateCallback(gameObject.name, gameObject, Wrapped);
        }

        public void onPurchaseUpdated(JintFunction Callback)
        {
            var jintCallback = new JintCallback<PlayerHandle, string>(callbackExecutor, Callback);
            productPurchaser.SetPurchaseUpdateCallback(csItemHandler.item.Id.Value, jintCallback);
        }

        class JintCallbackReceive : IJintCallback<string, object, object>
        {
            readonly Action<string, object, object> Callback;
            public JintCallbackReceive(Action<string, object, object> Callback)
            {
                this.Callback = Callback;
            }
            public void Execute(string arg1, object arg2, object arg3)
            {
                Callback(arg1, arg2, arg3);
            }
        }
        public void onReceive(JintFunction Callback)
        {
            dynamic option = new System.Dynamic.ExpandoObject();
            option.item = true;
            option.player = false;
            onReceive(Callback, option);
        }
        public void onReceive(JintFunction Callback, object option)
        {
            //ExpandoObjectで来る
            var opt = (IDictionary<string, object>)option;
            var receiveItem = opt.ContainsKey("item") ? (bool)opt["item"] : true;
            var receivePlayer = opt.ContainsKey("player") ? (bool)opt["player"] : false;

            var jintCallback = new JintCallback<string, object, object>(callbackExecutor, Callback);
            var CheckedCallback = new JintCallbackReceive((id, arg, sender) =>
            {
                if (sender is ItemHandle && receiveItem)
                    jintCallback.Execute(id, arg, sender);
                else if (sender is PlayerHandle && receivePlayer)
                    jintCallback.Execute(id, arg, sender);
                else if(loggingOptions.loggingOnReceiveFail)
                    UnityEngine.Debug.LogWarning($"メッセージ[{sender.GetType().Name}>{id}]は受け取れません。");
            });

            itemReceiveListenerBinder.SetItemReceiveCallback(
                csItemHandler, runningContext, sendableSanitizer, CheckedCallback
            );
        }

        public void onRequestPurchaseStatus(JintFunction Callback)
        {
            var jintCallback = new JintCallback<string, PurchaseRequestStatus, string, PlayerHandle>(callbackExecutor, Callback);
            productPurchaser.SetRequestPurchaseStatusCallback(csItemHandler.item.Id.Value, jintCallback);
        }

        public void onRide(JintFunction Callback)
        {
            if (!cckComponentFacade.hasRidableItem)
            {
                logger.Warning(String.Format("[{0}]onRide() need [Ridable Item] component.", this.gameObject.name));
            }
            OnRideHandler = new JintCallback<bool, PlayerHandle>(callbackExecutor, Callback);
        }
        private void CckComponentFacade_onRide(bool isOn)
        {
            try
            {
                var owner = playerHandleFactory.CreateByIdfc(
                    itemOwnerHandler.GetOwnerIdfc(),
                    csItemHandler
                );
                ridingPlayer = isOn ? owner : null;
                OnRideHandler.Execute(isOn, owner);
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }
        }

        public void onStart(JintFunction Callback)
        {
            var jintCallback = new JintCallback(callbackExecutor, Callback);
            startListenerBinder.SetUpdateCallback(jintCallback);
        }

        public void onSteer(JintFunction Callback)
        {
            if (cckComponentFacade.hasSteerItemTrigger)
            {
                logger.Warning(String.Format("[{0}]onSteer()はSteerItemTriggerと同時に使用できません。", this.gameObject.name));
                return;
            }
            OnSteerHandler = new JintCallback<EmulateVector2, PlayerHandle>(callbackExecutor, Callback);
        }
        private void CckComponentFacade_onSteerMove(Vector2 data)
        {
            try
            {
                var owner = playerHandleFactory.CreateByIdfc(
                    itemOwnerHandler.GetOwnerIdfc(),
                    csItemHandler
                );
                OnSteerHandler.Execute(new EmulateVector2(data), owner);
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }
        }

        public void onSteerAdditionalAxis(JintFunction Callback)
        {
            if (cckComponentFacade.hasSteerItemTrigger)
            {
                logger.Warning(String.Format("[{0}]onSteerAdditionalAxis()はSteerItemTriggerと同時に使用できません。", this.gameObject.name));
                return;
            }
            OnSteerAdditionalAxisHandler = new JintCallback<float, PlayerHandle>(callbackExecutor, Callback);
        }
        private void CckComponentFacade_onSteerAdditionalAxis(float data)
        {
            try
            {
                var owner = playerHandleFactory.CreateByIdfc(
                    itemOwnerHandler.GetOwnerIdfc(),
                    csItemHandler
                );
                OnSteerAdditionalAxisHandler.Execute(data, owner);
            }
            catch (Exception e)
            {
                logger.Exception(e);
            }
        }

        public void onTextInput(JintFunction Callback)
        {
            var jintCallback = new JintCallback<string, string, TextInputStatus>(callbackExecutor, Callback);
            textInputListenerBinder.SetReceiveCallback(this.csItemHandler, jintCallback);
        }

        public void onUpdate(JintFunction Callback)
        {
            var jintCallback = new JintCallback<double>(callbackExecutor, Callback);
            updateListenerBinder.SetUpdateCallback(gameObject.name, gameObject, jintCallback);
        }

        public void onUse(JintFunction Callback)
        {
            OnUseHandler = new JintCallback<bool, PlayerHandle>(callbackExecutor, Callback);
        }
        private void CckComponentFacade_onUse(bool isDown)
        {
            try
            {
                var owner = playerHandleFactory.CreateByIdfc(
                    itemOwnerHandler.GetOwnerIdfc(),
                    csItemHandler
                );
                OnUseHandler.Execute(isDown, owner);
            }
            catch (Exception ex)
            {
                logger.Exception(ex);
            }
        }

        public RaycastResult raycast(
            EmulateVector3 origin, EmulateVector3 direction, float maxDistance
        )
        {
            var ret = raycastAllConsiderShape(
                "ClusterScript.raycast()",
                origin, direction, maxDistance
            );
            if (ret.Length == 0) return null;
            return ret[0];
        }

        public RaycastResult[] raycastAll(
            EmulateVector3 origin, EmulateVector3 direction, float maxDistance
        )
        {
            var ret = raycastAllConsiderShape(
                "ClusterScript.raycastAll()",
                origin, direction, maxDistance
            );
            return ret;
        }

        RaycastResult[] raycastAllConsiderShape(
            string topLevelWarningMethod,
            EmulateVector3 origin, EmulateVector3 direction, float maxDistance
        )
        {
            if (runningContext.CheckTopLevel(topLevelWarningMethod)) return new RaycastResult[0];
            var raycastHits = Physics.RaycastAll(
                origin._ToUnityEngine(),
                direction._ToUnityEngine(),
                maxDistance,
                -1,
                QueryTriggerInteraction.Collide
            );

            var ret = raycastHits
                .OrderBy(raycastHit =>
                {
                    var distance = (raycastHit.point - origin._ToUnityEngine()).magnitude;
                    return distance;
                })
                .Where(raycastHit =>
                {
                    var target = raycastHit.transform.gameObject;
                    if (target.TryGetComponent<ClusterVR.CreatorKit.Item.IPhysicalShape>(out _))
                    {
                        return true;
                    }
                    if (target.TryGetComponent<ClusterVR.CreatorKit.Item.IOverlapSourceShape>(out _))
                    {
                        return true;
                    }
                    if (raycastHit.collider.isTrigger)
                    {
                        //Shape無しのisTriggerはNG
                        return false;
                    }
                    return true;
                })
                .Select(raycastHit =>
                {
                    var hit = new Hit(
                        new EmulateVector3(raycastHit.normal),
                        new EmulateVector3(raycastHit.point)
                    );
                    var hitObject = GameObjectToHitObject(raycastHit.transform.gameObject);
                    var raycastResult = new RaycastResult(hit, hitObject);
                    return raycastResult;
                }).ToArray();

            {
                var o = origin._ToUnityEngine();
                var d = direction._ToUnityEngine().normalized * maxDistance;
                rayDrawer.DrawRay(o, o + d, ret.Length == 0 ? Color.green : Color.magenta);
            }

            return ret;
        }

        HitObject GameObjectToHitObject(GameObject gameObject)
        {
            //SubNodeにあたることを考えてInParent。Mainの方にあたっても反応する。
            var csItemHandler = gameObject.GetComponentInParent<Components.CSEmulatorItemHandler>();
            //DesktopPlayerControllerにhitするのでchild
            var csPlayerHandler = gameObject.GetComponentInChildren<Components.CSEmulatorPlayerHandler>();
            var hitObject = HitObject.Create(
                csItemHandler, this.csItemHandler, csPlayerHandler,
                playerHandleFactory, spaceContext, runningContext, sendableSanitizer, messageSender
            );

            return hitObject;
        }

        public void requestOwner(PlayerHandle playerHandle)
        {
            //おそらくこのチェックが先に入っている＞連打しても制限にかからないのはこのため？
            if (playerHandle.idfc == itemOwnerHandler.GetOwnerIdfc()) return;

            CheckRequestOwnerOperationLimit();

            //CSETODO マルチ対応したら対応する＞ぶっちゃけもはや対応できる気がしない
        }
        void CheckRequestOwnerOperationLimit()
        {
            var result = requestOwnerThrottle.TryCharge();
            if (result) return;

            throw itemExceptionFactory.CreateRateLimitExceeded(
                String.Format("[{0}]", gameObject.name)
            );
        }

        public void sendSignalCompat(string target, string key)
        {
            if (runningContext.CheckTopLevel("ClusterScript.sendSignalCompat()")) return;
            cckComponentFacade.SendSignal(target, key);
        }

        public void setPlayerScript(PlayerHandle playerHandle)
        {
            var c = gameObject.GetComponent<ClusterVR.CreatorKit.Item.IPlayerScript>();
            if (c == null)
            {
                //2.18時点でただのErrorを投げている
                /* 以下の方法で確認できる
                    $.log(e);
                    $.log(e.name);
                    $.log(e.message);
                 */
                throw itemExceptionFactory.CreateJsError("PlayerScriptコンポーネントがありません");
            }
            var code = c.GetSourceCode(true);
            playerScriptSetter.Set(playerHandle, this, code);
        }

        public void setPosition(EmulateVector3 v)
        {
            if (runningContext.CheckTopLevel("ClusterScript.setPosition()")) return;
            if (!hasMovableItem && !hasCharacterItem)
            {
                logger.Warning(String.Format("[{0}]setPosition() need [Movable Item] or [Character Item] component.", this.gameObject.name));
                return;
            }
            //movableItem.SetPositionAndRotation(
            //    v._ToUnityEngine(), gameObject.transform.rotation, false
            //);
            gameObject.transform.position = v._ToUnityEngine();
            ResetVelocity();
        }

        public void setRotation(EmulateQuaternion v)
        {
            if (runningContext.CheckTopLevel("ClusterScript.setRotation()")) return;
            if (!hasMovableItem && !hasCharacterItem)
            {
                logger.Warning(String.Format("[{0}]setPosition() need [Movable Item] or [Character Item] component.", this.gameObject.name));
                return;
            }
            //movableItem.SetPositionAndRotation(
            //    gameObject.transform.position, v._ToUnityEngine(), false
            //);
            gameObject.transform.rotation = v._ToUnityEngine();
            ResetVelocity();
        }

        void ResetVelocity()
        {
            movableItem.Rigidbody.velocity = Vector3.zero;
            movableItem.Rigidbody.angularVelocity = Vector3.zero;
        }

        public void setStateCompat(string target, string key, object value)
        {
            if (runningContext.CheckTopLevel("ClusterScript.setStateCompat()")) return;
            cckComponentFacade.SetState(target, key, value);
        }

        public void setVisiblePlayers(PlayerHandle[] players)
        {
            if (players == null) throw itemExceptionFactory.CreateJsError("setVisiblePlayers:playersがnullです");

            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null) return;

            //CSETODO この辺以下、multi対応したらなんとかする
            var player = playerHandleFactory.CreateByIdfc(
                itemOwnerHandler.GetOwnerIdfc(),
                csItemHandler
            );
            if (players.Any(p => p.id == player.id))
            {
                renderer.enabled = true;
                SetVisibleLayer(true);
            }
            else
            {
                renderer.enabled = false;
                SetVisibleLayer(false);
            }
        }
        void SetVisibleLayer(bool visible)
        {
            //本当はIContactableItemのIsContactableを上書きしたかった
            if (visible)
            {
                if (gameObject.layer == 3) //誰も使わなさそうな3番
                {
                    gameObject.layer = 14; //interactの場合は3番とtoggleにする
                }
            }
            else
            {
                if (gameObject.layer == 14)
                {
                    gameObject.layer = 3;
                }
            }

        }

        public SubNode subNode(string subNodeName)
        {
            //2.23.0のindex.t.dsによるとPlayerLocalUI以下への参照はサポートされていないとあるが、
            //今のところnullを返すというわけではないのでこのまま
            var child = FindChild(gameObject.transform, subNodeName);
            if (child == null)
            {
                logger.Warning(String.Format("subNode:[{0}] is null.", subNodeName));
                return null;
            }
            var textView = child.gameObject.GetComponent<ClusterVR.CreatorKit.World.ITextView>();
            var ret = new SubNode(
                child, item, textView, runningContext, updateListenerBinder, itemExceptionFactory
            );
            return ret;
        }

        public void subscribePurchase(string productId)
        {
            if (runningContext.CheckTopLevel("ClusterScript.subscribePurchase()")) return;
            productPurchaser.SubscribePurchase(csItemHandler.item.Id.Value, productId);
        }

        public void unsubscribePurchase(string productId)
        {
            if (runningContext.CheckTopLevel("ClusterScript.unsubscribePurchase()")) return;
            productPurchaser.UnsubscribePurchase(csItemHandler.item.Id.Value, productId);
        }

        public ItemHandle worldItemReference(string worldItemReferenceId)
        {
            var itemList = gameObject.GetComponent<ClusterVR.CreatorKit.Item.IWorldItemReferenceList>();
            if (itemList == null)
            {
                logger.Warning("WorldItemReferenceListが指定されていません。");
                return new ItemHandle();
            }
            var set = itemList.WorldItemReferences.FirstOrDefault(set => set.Id == worldItemReferenceId);
            if (set == null || set.Item == null)
            {
                logger.Warning(String.Format("{1}:{0}が無効です。", worldItemReferenceId, nameof(worldItemReferenceId)));
                return new ItemHandle();
            }

            var h = set.Item.gameObject.GetComponent<Components.CSEmulatorItemHandler>();
            var ret = new ItemHandle(h, this.csItemHandler, spaceContext, runningContext, sendableSanitizer, messageSender);
            return ret;
        }

        public static Transform FindChild(Transform parent, string name)
        {
            if (parent == null) return null;

            var result = parent.Find(name);
            if (result != null)
                return result;

            foreach (Transform child in parent)
            {
                result = FindChild(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        public void DischargeOperateLimit(double time)
        {
            createItemThrottle.Discharge(time);
            callExternalThrottle.Discharge(time);
        }

        public void Shutdown()
        {
            startListenerBinder.DeleteStartCallback();
            updateListenerBinder.DeleteUpdateCallback(gameObject.name);
            fixedUpdateListenerBinder.DeleteUpdateCallback(gameObject.name);
            itemReceiveListenerBinder.DeleteItemReceiveCallback(this.csItemHandler);
            textInputListenerBinder.DeleteReceiveCallback(this.csItemHandler);
            productPurchaser.DeleteCallbacks(csItemHandler.item.Id.Value);
            //プロファイラを見てるとPlayModeを抜ける時に破棄されているようだけど念のため
            materialSubstituter.Destroy();
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            o.angularVelocity = angularVelocity.clone();
            o.state = new object();
            o.useGravity = useGravity;
            o.velocity = velocity.clone();
            o.id = id;
            o.itemHandle = itemHandle;
            o.itemTemplateId = itemTemplateId;
            return o;
        }
        public override string ToString()
        {
            return String.Format("[ClusterScript][{0}]", gameObject == null ? null : gameObject.name);
        }

    }
}
