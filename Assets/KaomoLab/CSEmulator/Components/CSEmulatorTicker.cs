using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Components
{
    public class CSEmulatorTicker
        : MonoBehaviour
    {
        public Handler OnUpdate = delegate { };

        private void Update()
        {
            OnUpdate.Invoke();
        }
    }
}
