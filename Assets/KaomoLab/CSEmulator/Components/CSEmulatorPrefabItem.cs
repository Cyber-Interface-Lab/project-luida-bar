using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Components
{
    [DisallowMultipleComponent, RequireComponent(typeof(ClusterVR.CreatorKit.Item.IItem))]
    public class CSEmulatorPrefabItem
         : MonoBehaviour
    {
        [SerializeField] public string _id;

        public string id { get => _id.ToLowerInvariant(); }
        //idが入力されているならワークラアイテム(＝クラフト配置＝動的生成挙動を想定している)だろうという考え
        public bool allowDestroy { get => KaomoLab.CSEmulator.Commons.IsUUID(_id); }
    }
}
