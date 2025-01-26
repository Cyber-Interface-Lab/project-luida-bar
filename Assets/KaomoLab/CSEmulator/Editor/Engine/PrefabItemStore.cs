using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class PrefabItemStore
        : EmulateClasses.IPrefabItemHolder
    {
        readonly Dictionary<string, GameObject> prefabs;

        public PrefabItemStore(
            IEnumerable<GameObject> prefabs
        )
        {
            var source = prefabs.ToArray()
                //生成するItemがクラフトアイテム系なら直下にItemが付与されているはず。
                .Select(o => o.GetComponent<Components.CSEmulatorPrefabItem>())
                .Where(o => o != null)
                .Where(o => o.id != "");

            var duplicates = source
                .OrderBy(o => o.id).GroupBy(o => o.id)
                .Select(g => new {
                    key = g.Key,
                    count = g.Count(),
                    names = g.Select(o => o.gameObject.name).ToArray()
                })
                .Where(g => g.count > 1)
                .ToArray();

            if (duplicates.Count() > 0)
            {
                foreach(var d in duplicates)
                {
                    Debug.LogError(String.Format(
                        "[CS Emulator Prefab Item][{1}] duplicate UUID.[{0}]",
                        d.key,
                        String.Join("][", d.names)
                    ));
                }
            }

            var duplicateIds = duplicates.Select(s => s.key).ToArray();

            this.prefabs = source
                .Where(o => !duplicateIds.Contains(o.id))
                .ToDictionary(o => o.id, o => o.gameObject);
        }

        public GameObject GetPrefab(string uuid)
        {
            if (prefabs.ContainsKey(uuid))
                return prefabs[uuid];

            UnityEngine.Debug.LogError(String.Format("prefab not found.[{0}]", uuid));
            return null;
        }
    }
}
