using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using OverlapManager = Assets.KaomoLab.CSEmulator.OverlapManager<Assets.KaomoLab.CSEmulator.Components.CSEmulatorItemHandler, Assets.KaomoLab.CSEmulator.Components.CSEmulatorPlayerHandler>;

namespace Assets.KaomoLab.CSEmulator.Components
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider))]
    public class CSEmulatorSubNodeHandler
        : MonoBehaviour
    {
        public CSEmulatorItemHandler parent { get; private set; }

        public void Construct(
            CSEmulatorItemHandler parent
        )
        {
            this.parent = parent;
        }

        //ここに機能を追加する場合の注意。
        //このコンポーネントは、OverlapDetectorShapeが付いているSubNodeにしか追加されていない。

        private void OnTriggerEnter(Collider other)
        {
            parent.SetSubNodeOverlap(gameObject, other);
        }

        private void OnTriggerStay(Collider other)
        {
            //必ずEnterが入るならいらない？
        }

        private void OnTriggerExit(Collider other)
        {
            parent.RemoveSubNodeOverlap(gameObject, other);
        }

        //ここに機能を追加する場合の注意。
        //このコンポーネントは、OverlapDetectorShapeが付いているSubNodeにしか追加されていない。
    }
}
