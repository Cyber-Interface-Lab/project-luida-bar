using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Components
{
    [DisallowMultipleComponent, RequireComponent(typeof(ClusterVR.CreatorKit.Item.IItem))]
    public class CSEmulatorStateWatcher
        : MonoBehaviour, IVariablesStore
    {
        IVariablesStore variablesStore;

        public event Action OnVariablesUpdated = delegate { };

        public Dictionary<string, bool> foldouts = new();

        public void Construct(
            IVariablesStore variablesStore
        )
        {
            if (this.variablesStore != null)
            {
                variablesStore.OnVariablesUpdated -= InvokeOnVariablesUpdated;
            }
            this.variablesStore = variablesStore;
            variablesStore.OnVariablesUpdated += InvokeOnVariablesUpdated;
        }
        void InvokeOnVariablesUpdated()
        {
            OnVariablesUpdated.Invoke();
        }

        public IEnumerable<IVariable> GetVariables()
        {
            if(variablesStore == null) return Enumerable.Empty<IVariable>();
            return variablesStore.GetVariables();
        }

    }
}
