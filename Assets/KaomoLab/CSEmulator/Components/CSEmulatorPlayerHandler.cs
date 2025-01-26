using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using ClusterVR.CreatorKit.Preview.PlayerController;

namespace Assets.KaomoLab.CSEmulator.Components
{
    [DisallowMultipleComponent, RequireComponent(typeof(VRM.VRMMeta))]
    public class CSEmulatorPlayerHandler
        : MonoBehaviour, IVrmIKNotifier
    {
        public event Action<int> OnIK = delegate { };


        public string id
        {
            get
            {
                //一旦UUIDにする。
                if (_id == null)
                    _id = Guid.NewGuid().ToString();
                return _id;
            }
        }
        string _id = null;

        public string idfc { get; private set; }

        public GameObject vrm => gameObject;

        ////CSETODO ★要調査
        //readonly MassTimeThrottle sendThrottle = new MassTimeThrottle(
        //    3000, 1 * TimeSpan.TicksPerSecond, new MassTimeThrottle.ByDateTimeTicks()
        //);

        public void Construct(string idfc)
        {
            this.idfc = idfc;
        }

        //public bool TrySendOperate()
        //{
        //    return sendThrottle.TryCharge(1);
        //}

        public void Update()
        {
            //sendThrottle.Discharge();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            OnIK.Invoke(layerIndex);
        }
    }
}
