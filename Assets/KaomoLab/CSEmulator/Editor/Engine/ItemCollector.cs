using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class ItemCollector
    {
        public event Handler<ClusterVR.CreatorKit.Item.IScriptableItem> OnScriptableItemCreated = delegate { };
        public event Handler<ClusterVR.CreatorKit.Item.IItem> OnItemCreated = delegate { };

        public ItemCollector(
            ClusterVR.CreatorKit.Editor.Preview.Item.ItemCreator itemCreator
        )
        {
            itemCreator.OnCreateCompleted += this.ItemCreator_OnCreateCompleted;
        }

        private void ItemCreator_OnCreateCompleted(ClusterVR.CreatorKit.Item.IItem obj)
        {
            OnItemCreated.Invoke(obj);

            var scriptableItem = obj.gameObject.GetComponent<ClusterVR.CreatorKit.Item.IScriptableItem>();

            if(scriptableItem != null)
                OnScriptableItemCreated.Invoke(scriptableItem);
        }

        public IEnumerable<ClusterVR.CreatorKit.Item.IScriptableItem> GetAllScriptableItem()
        {
            var items = GetAllItems()
                .Select(i => i.gameObject.GetComponent<ClusterVR.CreatorKit.Item.IScriptableItem>())
                .Where(i => i != null);
            return items;
        }

        public IEnumerable<ClusterVR.CreatorKit.Item.IItem> GetAllItems()
        {
            var items = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .SelectMany(
                    //inactiveでも動作させる必要がある
                    o => o.GetComponentsInChildren<ClusterVR.CreatorKit.Item.IItem>(true)
                );

            return items;
        }

        public IEnumerable<UnityEngine.GameObject> GetAllItemPrefabs()
        {
            var ret = UnityEditor.AssetDatabase
                .FindAssets("t:Prefab")
                .Select(guid => UnityEditor.AssetDatabase.GUIDToAssetPath(guid))
                .Select(path =>
                    new { o = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path), path = path}
                )
                .Where(t => t.o != null && null != t.o.GetComponentsInChildren<ClusterVR.CreatorKit.Item.IItem>(true))
                .Select(t => t.o)
            ;

            return ret;

        }
    }
}
