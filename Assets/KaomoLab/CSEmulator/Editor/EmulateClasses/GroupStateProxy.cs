using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class GroupStateProxy
        : Components.IVariablesStore
    {
        public class GroupStateProxySet
        {
            public readonly StateProxy.StateDictionary stateDictionary;
            public readonly DisableNotifier disableNotifier;
            public GroupStateProxySet(
                StateProxy.StateDictionary stateDirectory, DisableNotifier disableNotifier
            )
            {
                this.stateDictionary = stateDirectory;
                this.disableNotifier = disableNotifier;
            }
        }

        public class DisableNotifier
        {
            public event Handler OnDisabled = delegate { };
            public bool isDisabled { get; private set; } = false;

            public void NotifyDisable()
            {
                isDisabled = true;
                OnDisabled.Invoke();
            }
        }

        public bool isHost { get; private set; }
        readonly StateProxy state;
        DisableNotifier disableNotifier;
        readonly IItemExceptionFactory itemExceptionFactory;

        public GroupStateProxy(
            bool isHost,
            StateProxy state,
            Components.CSEmulatorItemHandler csItemHandler,
            IItemExceptionFactory itemExceptionFactory,
            IMessageSender messageSender
        )
        {
            this.isHost = isHost;
            this.state = state;
            this.disableNotifier = new DisableNotifier();
            this.itemExceptionFactory = itemExceptionFactory;
            //ItemHandleがスクリプト空間を超える可能性があるので
            //owner(スクリプト空間主＝$)を切り替える。
            //これによりsenderが正しくなる。
            var handleReplacer = new StateProxy.HandleReplacer(
                state, csItemHandler, messageSender
            );
            state.handleReplacer = handleReplacer;

            disableNotifier.OnDisabled += DisableNotifier_OnDisable;
        }

        private void DisableNotifier_OnDisable()
        {
            this.disableNotifier.OnDisabled -= DisableNotifier_OnDisable;
            state.stateDictionary = null;
        }

        public void OverwriteState(
            GroupStateProxySet state
        )
        {
            if(state == null)
            {
                this.state.stateDictionary = null;
                return;
            }
            this.state.stateDictionary = state.stateDictionary;

            this.disableNotifier.OnDisabled -= DisableNotifier_OnDisable;
            this.disableNotifier = state.disableNotifier;
            this.disableNotifier.OnDisabled += DisableNotifier_OnDisable;
            if(this.disableNotifier.isDisabled) this.state.stateDictionary = null;
        }
        public GroupStateProxySet GetState()
        {
            return new GroupStateProxySet(state.stateDictionary, disableNotifier);
        }

        public void DisableState()
        {
            if(isHost) disableNotifier.NotifyDisable();
            this.disableNotifier.OnDisabled -= DisableNotifier_OnDisable;
            state.stateDictionary = null;
        }

        public Jint.Native.JsValue this[string index]
        {
            get
            {
                if(state.stateDictionary == null)
                {
                    throw itemExceptionFactory.CreateGeneral("ItemGroupのHostもしくはMemberではありません。");
                }

                var obj = state[index];

                var ret = obj;
                return ret;
            }
            set
            {
                if (state.stateDictionary == null)
                {
                    throw itemExceptionFactory.CreateGeneral("ItemGroupのHostもしくはMemberではありません。");
                }

                state[index] = value;
            }
        }

        event Action Components.IVariablesStore.OnVariablesUpdated
        {
            add => ((Components.IVariablesStore)state).OnVariablesUpdated += value;
            remove => ((Components.IVariablesStore)state).OnVariablesUpdated -= value;
        }
        IEnumerable<Components.IVariable> Components.IVariablesStore.GetVariables()
        {
            return ((Components.IVariablesStore)state).GetVariables();
        }
    }
}
