using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using ClusterVR.CreatorKit;
using ClusterVR.CreatorKit.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class CckComponentFacade
        : EmulateClasses.ICckComponentFacade
    {
        public event Handler<bool, bool> onGrabbed = delegate { };
        public event Handler<bool> onRide = delegate { };
        public event Handler<bool> onUse = delegate { };
        public event Handler onInteract = delegate { };
        public event Handler<Vector2> onSteerMove = delegate { };
        public event Handler<float> onSteerAdditionalAxis = delegate { };

        public bool isGrab { get; private set; }
        public bool hasCollider { get; private set; }
        public bool hasGrabbableItem { get; private set; }
        public bool hasRidableItem { get; private set; }
        public bool hasSteerItemTrigger { get; private set; }

        Action _AddInteractItemTrigger = () => { };

        readonly GameObject gameObject;
        readonly ClusterVR.CreatorKit.Item.IItem item;
        readonly ClusterVR.CreatorKit.Operation.ILogicStateRepository stateRepository;
        readonly ClusterVR.CreatorKit.Operation.LogicExecutor logicExecutor;
        readonly ClusterVR.CreatorKit.Trigger.ISignalGenerator signalGenerator;
        readonly ClusterVR.CreatorKit.Gimmick.IGimmickUpdater gimmickUpdater;
        readonly ClusterVR.CreatorKit.Trigger.Implements.UseItemTrigger useItemTrigger;

        readonly string moveInputTriggerKey = "ehiutgeralkga2435";
        readonly string additionalAxisInputTriggerKey = "hiuagerhjkt435ra";

        ClusterVR.CreatorKit.Trigger.TriggerParam[] useItemTriggerDownTriggers = null;
        ClusterVR.CreatorKit.Trigger.TriggerParam[] useItemTriggerUpTriggers = null;
        FieldInfo useItemTriggerDownTriggersField;
        FieldInfo useItemTriggerUpTriggersField;

        readonly Components.CSEmulatorItemHandler itemHandler;

        public CckComponentFacade(
            UnityEngine.GameObject gameObject,
            ClusterVR.CreatorKit.Editor.Preview.RoomState.RoomStateRepository roomStateRepository,
            ClusterVR.CreatorKit.Trigger.ISignalGenerator signalGenerator,
            ClusterVR.CreatorKit.Gimmick.IGimmickUpdater gimmickUpdater
        )
        {
            this.gameObject = gameObject;
            this.stateRepository = CreateLogicStateRepositiory(roomStateRepository);
            this.logicExecutor = new ClusterVR.CreatorKit.Operation.LogicExecutor(
                signalGenerator, stateRepository, gimmickUpdater
            );
            this.signalGenerator = signalGenerator;
            this.gimmickUpdater = gimmickUpdater;

            item = this.gameObject.GetComponent<ClusterVR.CreatorKit.Item.IItem>();

            itemHandler = this.gameObject.GetComponent<Components.CSEmulatorItemHandler>();

            hasGrabbableItem = false;
            hasRidableItem = false;
            hasCollider = (this.gameObject.GetComponentInChildren<Collider>() != null);

            //ItemTrigger系と重複した時の挙動とか含めて、たぶんこれであってると思うけど自信ない。
            if (itemHandler.grabbableItem != null)
            {
                hasGrabbableItem = true;

                //onGrabはGrabbableItemが必須で、共存可
                itemHandler.grabbableItem.OnGrabbed += GrabbableItem_OnGrabbed;
                itemHandler.grabbableItem.OnReleased += GrabbableItem_OnReleased;

                if (this.gameObject.TryGetComponent<ClusterVR.CreatorKit.Trigger.Implements.UseItemTrigger>(out useItemTrigger))
                {
                    useItemTriggerDownTriggersField = typeof(ClusterVR.CreatorKit.Trigger.Implements.UseItemTrigger)
                        .GetField("downTriggersCache", BindingFlags.NonPublic | BindingFlags.Instance);
                    useItemTriggerUpTriggersField = typeof(ClusterVR.CreatorKit.Trigger.Implements.UseItemTrigger)
                        .GetField("upTriggersCache", BindingFlags.NonPublic | BindingFlags.Instance);
                    useItemTriggerDownTriggers = null;
                    useItemTriggerUpTriggers = null;
                    //nullだと処理に影響がでるのでcache化させる
                    var ds = ((ClusterVR.CreatorKit.Trigger.Implements.ConstantTriggerParam[])typeof(ClusterVR.CreatorKit.Trigger.Implements.UseItemTrigger)
                        .GetField("downTriggers", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(useItemTrigger))
                        .Select(t => t.Convert())
                        .ToArray();
                    useItemTriggerDownTriggersField.SetValue(useItemTrigger, ds);
                    var us = ((ClusterVR.CreatorKit.Trigger.Implements.ConstantTriggerParam[])typeof(ClusterVR.CreatorKit.Trigger.Implements.UseItemTrigger)
                        .GetField("upTriggers", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(useItemTrigger))
                        .Select(t => t.Convert())
                        .ToArray();
                    useItemTriggerUpTriggersField.SetValue(useItemTrigger, us);

                }
                else
                {
                    //onUseは既存のがあった場合は不発なので、こちらで追加して発火させる
                    useItemTrigger = this.gameObject.AddComponent<ClusterVR.CreatorKit.Trigger.Implements.UseItemTrigger>();
                    useItemTrigger.TriggerEvent += UseItemTrigger_TriggerEvent;

                    useItemTriggerDownTriggersField = typeof(ClusterVR.CreatorKit.Trigger.Implements.UseItemTrigger)
                        .GetField("downTriggersCache", BindingFlags.NonPublic | BindingFlags.Instance);
                    useItemTriggerUpTriggersField = typeof(ClusterVR.CreatorKit.Trigger.Implements.UseItemTrigger)
                        .GetField("upTriggersCache", BindingFlags.NonPublic | BindingFlags.Instance);
                    useItemTriggerDownTriggers = null;
                    useItemTriggerUpTriggers = null;

                    //downTriggersだけにダミーを入れておいて、downかupかを判定する材料にする。
                    useItemTriggerDownTriggersField
                        .SetValue(
                            useItemTrigger,
                            new ClusterVR.CreatorKit.Trigger.TriggerParam[]
                            {
                                new ClusterVR.CreatorKit.Trigger.TriggerParam(
                                    ClusterVR.CreatorKit.Trigger.TriggerTarget.Item,
                                    new ClusterVR.CreatorKit.Item.Implements.Item(),
                                    "ahupa40t4ohpiu", //重複しないように適当
                                    ClusterVR.CreatorKit.ParameterType.Bool,
                                    new ClusterVR.CreatorKit.Trigger.TriggerValue(true)
                                )
                            }
                        );
                    useItemTriggerUpTriggersField
                        .SetValue(
                            useItemTrigger,
                            new ClusterVR.CreatorKit.Trigger.TriggerParam[0]
                        );
                }
            }
            else if (this.gameObject.TryGetComponent<ClusterVR.CreatorKit.Item.Implements.RidableItem>(out var ridableItem))
            {
                hasRidableItem = true;
                //onRideはRidableItemが必須で、共存可
                ridableItem.OnGetOn += RidableItem_OnGetOn;
                ridableItem.OnGetOff += RidableItem_OnGetOff;
                //CSETODO HUDv2では、XではなくQで降りる＆QとEでの腕上げが無効化される。

                //onSteer系はSteerItemTriggerとは共存できない
                if (this.gameObject.GetComponent<ClusterVR.CreatorKit.Trigger.Implements.SteerItemTrigger>())
                {
                    hasSteerItemTrigger = true;
                }
                else
                {
                    hasSteerItemTrigger = false;
                    var steerItemTrigger = this.gameObject.AddComponent<ClusterVR.CreatorKit.Trigger.Implements.SteerItemTrigger>();
                    steerItemTrigger.TriggerEvent += SteerItemTrigger_TriggerEvent;

                    var moveInputTriggersField = typeof(ClusterVR.CreatorKit.Trigger.Implements.SteerItemTrigger)
                        .GetField("moveInputTriggers", BindingFlags.NonPublic | BindingFlags.Instance);
                    var additionalAxisInputTriggersField = typeof(ClusterVR.CreatorKit.Trigger.Implements.SteerItemTrigger)
                        .GetField("additionalAxisInputTriggers", BindingFlags.NonPublic | BindingFlags.Instance);

                    var variableTriggerParam_valueTypeField = typeof(ClusterVR.CreatorKit.Trigger.Implements.VariableTriggerParam)
                        .GetField("valueType", BindingFlags.NonPublic | BindingFlags.Instance);
                    var variableTriggerParam_keyField = typeof(ClusterVR.CreatorKit.Trigger.Implements.VariableTriggerParam)
                        .GetField("key", BindingFlags.NonPublic | BindingFlags.Instance);
                    var variableTriggerParam_targetField = typeof(ClusterVR.CreatorKit.Trigger.Implements.VariableTriggerParam)
                        .GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);

                    var moveInputTrigger = new ClusterVR.CreatorKit.Trigger.Implements.VariableTriggerParam();
                    variableTriggerParam_valueTypeField.SetValue(
                        moveInputTrigger, ClusterVR.CreatorKit.Trigger.Implements.VariableTriggerParam.ValueType.Input
                    );
                    variableTriggerParam_keyField.SetValue(
                        moveInputTrigger, moveInputTriggerKey
                    );
                    variableTriggerParam_targetField.SetValue(
                        moveInputTrigger, ClusterVR.CreatorKit.Trigger.TriggerTarget.Item
                    );
                    var moveInputTriggers = (ClusterVR.CreatorKit.Trigger.Implements.VariableTriggerParam[])moveInputTriggersField.GetValue(steerItemTrigger);
                    if (moveInputTriggers == null)
                    {
                        moveInputTriggers = new ClusterVR.CreatorKit.Trigger.Implements.VariableTriggerParam[] { moveInputTrigger };
                    }
                    else
                    {
                        moveInputTriggers = moveInputTriggers.Append(moveInputTrigger).ToArray();
                    }
                    moveInputTriggersField.SetValue(steerItemTrigger, moveInputTriggers);

                    var additionalAxisInputTrigger = new ClusterVR.CreatorKit.Trigger.Implements.VariableTriggerParam();
                    variableTriggerParam_valueTypeField.SetValue(
                        additionalAxisInputTrigger, ClusterVR.CreatorKit.Trigger.Implements.VariableTriggerParam.ValueType.Input
                    );
                    variableTriggerParam_keyField.SetValue(
                        additionalAxisInputTrigger, additionalAxisInputTriggerKey
                    );
                    variableTriggerParam_targetField.SetValue(
                        additionalAxisInputTrigger, ClusterVR.CreatorKit.Trigger.TriggerTarget.Item
                    );
                    var additionalAxisInputTriggers = (ClusterVR.CreatorKit.Trigger.Implements.VariableTriggerParam[])additionalAxisInputTriggersField.GetValue(steerItemTrigger);
                    if (additionalAxisInputTriggers == null)
                    {
                        additionalAxisInputTriggers = new ClusterVR.CreatorKit.Trigger.Implements.VariableTriggerParam[] { additionalAxisInputTrigger };
                    }
                    else
                    {
                        additionalAxisInputTriggers = additionalAxisInputTriggers.Append(additionalAxisInputTrigger).ToArray();
                    }
                    additionalAxisInputTriggersField.SetValue(steerItemTrigger, additionalAxisInputTriggers);
                }
            }
            //ContactableItemは共存できないのでelse if
            else if (!this.gameObject.TryGetComponent<ClusterVR.CreatorKit.Trigger.Implements.InteractItemTrigger>(out var _))
            {
                //ここで実行すると、レイヤーが14:InteractableItem layerになってしまうため、onInteract登録時に実行する。
                _AddInteractItemTrigger = () =>
                {
                    //onInteractは既存のがあった場合は不発で、こちらで追加して発火
                    var interactItemTrigger = this.gameObject.AddComponent<ClusterVR.CreatorKit.Trigger.Implements.InteractItemTrigger>();
                    interactItemTrigger.TriggerEvent += InteractItemTrigger_TriggerEvent;

                    typeof(ClusterVR.CreatorKit.Trigger.Implements.InteractItemTrigger)
                        .GetField("triggers", BindingFlags.NonPublic | BindingFlags.Instance)
                        .SetValue(
                            interactItemTrigger,
                            new ClusterVR.CreatorKit.Trigger.Implements.ConstantTriggerParam[0]
                        );

                    _AddInteractItemTrigger = () => { };
                };
            }

        }

        void GrabbableItem_OnGrabbed(bool isLeftHand)
        {
            isGrab = true;
            onGrabbed.Invoke(isLeftHand, true);

        }
        void GrabbableItem_OnReleased(bool isLeftHand)
        {
            isGrab = false;
            onGrabbed.Invoke(isLeftHand, false);
        }

        void RidableItem_OnGetOff()
        {
            onRide.Invoke(false);
        }
        void RidableItem_OnGetOn()
        {
            onRide.Invoke(true);
        }

        private void SteerItemTrigger_TriggerEvent(ClusterVR.CreatorKit.Trigger.IItemTrigger sender, ClusterVR.CreatorKit.Trigger.TriggerEventArgs e)
        {
            foreach(var p in e.TriggerParams)
            {
                if(p.RawKey == moveInputTriggerKey)
                {
                    //WASD同時押しの場合にmagnitudeが1を超えてしまっているので(現行ではnormalizeされている)
                    var v = p.Value.Vector2Value.normalized;
                    onSteerMove.Invoke(v);
                }
                if (p.RawKey == additionalAxisInputTriggerKey)
                {
                    var f = p.Value.FloatValue;
                    onSteerAdditionalAxis.Invoke(f);
                }
            }
        }

        private void UseItemTrigger_TriggerEvent(ClusterVR.CreatorKit.Trigger.IItemTrigger sender, ClusterVR.CreatorKit.Trigger.TriggerEventArgs e)
        {
            //ダミーが入ってるならdown
            if (e.TriggerParams.Length > 0)
            {
                onUse.Invoke(true);
            }
            else
            {
                onUse.Invoke(false);

            }
        }

        void InteractItemTrigger_TriggerEvent(ClusterVR.CreatorKit.Trigger.IItemTrigger sender, ClusterVR.CreatorKit.Trigger.TriggerEventArgs e)
        {
            onInteract.Invoke();
        }

        public void AddInteractItemTrigger()
        {
            _AddInteractItemTrigger();
        }

        public object GetState(string target, string key, string parameterType)
        {
            //LogicStateRepository経由で操作すると、itemとかplayerとかVector3とかの面倒なあれこれをやってくれるので便利
            var stateValueSet = stateRepository.GetRoomStateValueSet(
                new ClusterVR.CreatorKit.Operation.SourceState(
                    StringToGimmickTarget(target),
                    key,
                    StringToParameterType(parameterType)
                ),
                item.Id
            );
            var sendable = StateValueSetToSendable(stateValueSet);
            return sendable;
        }

        public void SendSignal(string target, string key)
        {
            //Signalを送るLogicを作って実行させている
            var sendSignalLogic = new ClusterVR.CreatorKit.Operation.Logic(
                new ClusterVR.CreatorKit.Operation.Statement[]
                {
                    new ClusterVR.CreatorKit.Operation.Statement(
                        new ClusterVR.CreatorKit.Operation.SingleStatement(
                            new ClusterVR.CreatorKit.Operation.TargetState(
                                StringToTargetStateTarget(target),
                                key,
                                ClusterVR.CreatorKit.ParameterType.Signal
                            ),
                            new ClusterVR.CreatorKit.Operation.Expression(
                                new ClusterVR.CreatorKit.Operation.Value(
                                    new ClusterVR.CreatorKit.Operation.ConstantValue(true)
                                )
                            )
                        )
                    )
                }
            );
            logicExecutor.Execute(sendSignalLogic, item.Id);
        }

        public void SetState(string target, string key, object value)
        {
            //Logicを作って値を送る方式をとっていた。
            //しかし、ConstantValueでdouble値が送れないという問題が発覚した。
            //そのため、LogicExecuterの処理を参考に記述することにした。
            //将来LogicExecuterの処理がどう変更されるか分からないので要注意。
            var updatedKeys = new Queue<string>();
            var stateValueSet = ObjectToStateValueSet(value);
            var parameterType = stateValueSet.ParameterType;
            var targetState = new ClusterVR.CreatorKit.Operation.TargetState(
                StringToTargetStateTarget(target),
                key,
                stateValueSet.ParameterType
            );
            stateRepository.UpdateState(
                targetState,
                item.Id,
                stateValueSet.CastTo(parameterType),
                updatedKeys
            );
            gimmickUpdater.OnStateUpdated(updatedKeys);


            ////値を設定するLogicを作って実行させている
            //var constantValue = ObjectToConstantValue(value);
            //var setStateLogic = new ClusterVR.CreatorKit.Operation.Logic(
            //    new ClusterVR.CreatorKit.Operation.Statement[]
            //    {
            //        new ClusterVR.CreatorKit.Operation.Statement(
            //            new ClusterVR.CreatorKit.Operation.SingleStatement(
            //                new ClusterVR.CreatorKit.Operation.TargetState(
            //                    StringToTargetStateTarget(target),
            //                    key,
            //                    constantValue.Type
            //                ),
            //                new ClusterVR.CreatorKit.Operation.Expression(
            //                    new ClusterVR.CreatorKit.Operation.Value(
            //                        constantValue
            //                    )
            //                )
            //            )
            //        )
            //    }
            //);
            //logicExecutor.Execute(setStateLogic, itemId);
        }



        ClusterVR.CreatorKit.Operation.ILogicStateRepository CreateLogicStateRepositiory(
            ClusterVR.CreatorKit.Editor.Preview.RoomState.RoomStateRepository roomStateRepository
        )
        {
            //RoomStateRepositoryへの値の出し入れはLogicStateRepositoryが行っていそうなので
            var asm = typeof(ClusterVR.CreatorKit.Editor.Preview.Operation.LogicManager).Assembly;
            var type = asm.GetType(typeof(ClusterVR.CreatorKit.Editor.Preview.Operation.LogicManager).FullName + "+LogicStateRepository");
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).First();
            var ret = (ClusterVR.CreatorKit.Operation.ILogicStateRepository)ctor.Invoke(new object[] { roomStateRepository });
            return ret;
        }

        object StateValueSetToSendable(
            ClusterVR.CreatorKit.IStateValueSet stateValueSet
        )
        {
            switch (stateValueSet.ParameterType)
            {
                case ClusterVR.CreatorKit.ParameterType.Bool:
                    return stateValueSet.ToGimmickValue().BoolValue;
                case ClusterVR.CreatorKit.ParameterType.Double:
                    return stateValueSet.GetStateValue(new ClusterVR.CreatorKit.FieldName()).ToDouble();
                case ClusterVR.CreatorKit.ParameterType.Float:
                    return stateValueSet.GetStateValue(new ClusterVR.CreatorKit.FieldName()).ToFloat();
                case ClusterVR.CreatorKit.ParameterType.Integer:
                    return stateValueSet.GetStateValue(new ClusterVR.CreatorKit.FieldName()).ToInt();
                case ClusterVR.CreatorKit.ParameterType.Signal:
                    return stateValueSet.GetStateValue(new ClusterVR.CreatorKit.FieldName()).ToDateTime();
                case ClusterVR.CreatorKit.ParameterType.Vector2:
                    return new EmulateVector2(
                        stateValueSet.ToGimmickValue().Vector2Value
                    );
                case ClusterVR.CreatorKit.ParameterType.Vector3:
                    return new EmulateVector3(
                        stateValueSet.ToGimmickValue().Vector3Value
                    );
                default: throw new NotImplementedException(stateValueSet.ParameterType.ToString());
            }
        }

        ClusterVR.CreatorKit.Operation.TargetStateTarget StringToTargetStateTarget(
            string target
        )
        {
            switch (target)
            {
                case "this": return ClusterVR.CreatorKit.Operation.TargetStateTarget.Item;
                case "owner": return ClusterVR.CreatorKit.Operation.TargetStateTarget.Player;
                default: throw new ArgumentException(target);
            }
        }

        ClusterVR.CreatorKit.Gimmick.GimmickTarget StringToGimmickTarget(
            string target
        )
        {
            switch (target)
            {
                case "this": return ClusterVR.CreatorKit.Gimmick.GimmickTarget.Item;
                case "owner": return ClusterVR.CreatorKit.Gimmick.GimmickTarget.Player;
                case "global": return ClusterVR.CreatorKit.Gimmick.GimmickTarget.Global;
                default: throw new ArgumentException(target);
            }
        }

        ClusterVR.CreatorKit.ParameterType StringToParameterType(
            string parameterType
        )
        {
            switch (parameterType)
            {
                case "signal": return ClusterVR.CreatorKit.ParameterType.Signal;
                case "boolean": return ClusterVR.CreatorKit.ParameterType.Bool;
                case "float": return ClusterVR.CreatorKit.ParameterType.Float;
                case "double": return ClusterVR.CreatorKit.ParameterType.Double;
                case "integer": return ClusterVR.CreatorKit.ParameterType.Integer;
                case "vector2": return ClusterVR.CreatorKit.ParameterType.Vector2;
                case "vector3": return ClusterVR.CreatorKit.ParameterType.Vector3;
                default: throw new ArgumentException(parameterType);
            }
        }

        ClusterVR.CreatorKit.Operation.ConstantValue ObjectToConstantValue(
            object value
        )
        {
            switch (value)
            {
                case bool boolValue:
                    return new ClusterVR.CreatorKit.Operation.ConstantValue(boolValue);
                case double doubleValue:
                    //numberはdoubleになるけど、ConstantValueはfloatまで
                    return new ClusterVR.CreatorKit.Operation.ConstantValue((float)doubleValue);
                case EmulateVector2 vector2Value:
                    return new ClusterVR.CreatorKit.Operation.ConstantValue(
                        vector2Value._ToUnityEngine()
                    );
                case EmulateVector3 vector3Value:
                    return new ClusterVR.CreatorKit.Operation.ConstantValue(
                        vector3Value._ToUnityEngine()
                    );
                default: throw new NotImplementedException();
            }
        }

        ClusterVR.CreatorKit.IStateValueSet ObjectToStateValueSet(
            object value
        )
        {
            switch (value)
            {
                case bool boolValue:
                    return new ClusterVR.CreatorKit.BoolStateValueSet(boolValue);
                case double doubleValue:
                    return new ClusterVR.CreatorKit.DoubleStateValueSet(doubleValue);
                case EmulateVector2 vector2Value:
                    return new ClusterVR.CreatorKit.Vector2StateValueSet(
                        vector2Value._ToUnityEngine()
                    );
                case EmulateVector3 vector3Value:
                    return new ClusterVR.CreatorKit.Vector3StateValueSet(
                        vector3Value._ToUnityEngine()
                    );
                default: throw new NotImplementedException();
            }
        }

        public void InvalidUseItemTrigger()
        {
            useItemTriggerDownTriggers = (ClusterVR.CreatorKit.Trigger.TriggerParam[])useItemTriggerDownTriggersField.GetValue(useItemTrigger);
            useItemTriggerUpTriggers = (ClusterVR.CreatorKit.Trigger.TriggerParam[])useItemTriggerUpTriggersField.GetValue(useItemTrigger);

            useItemTriggerDownTriggersField.SetValue(useItemTrigger, new ClusterVR.CreatorKit.Trigger.TriggerParam[0]);
            useItemTriggerUpTriggersField.SetValue(useItemTrigger, new ClusterVR.CreatorKit.Trigger.TriggerParam[0]);
        }

        public void ResumeUseItemTrigger()
        {
            //別アイテムの処理をすることはない＞triggersは不変＞一度でもInvalidしてくれればOKという判断
            if (useItemTriggerDownTriggers == null) return;
            useItemTriggerDownTriggersField.SetValue(useItemTrigger, useItemTriggerDownTriggers);
            useItemTriggerUpTriggersField.SetValue(useItemTrigger, useItemTriggerUpTriggers);
        }
    }
}
