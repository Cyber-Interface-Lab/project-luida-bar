using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class ItemLifecycler
        : EmulateClasses.IItemLifecycler
    {
        public class CreateItemTemplate
            : ClusterVR.CreatorKit.Gimmick.ICreateItemGimmick
        {
            public ClusterVR.CreatorKit.Item.IItem ItemTemplate { get; private set; }

            public ClusterVR.CreatorKit.Item.ItemTemplateId ItemTemplateId { get; private set; }

            public event ClusterVR.CreatorKit.Gimmick.CreateItemEventHandler OnCreateItem = delegate { };

            public CreateItemTemplate(
                UnityEngine.GameObject prefab,
                EmulateClasses.ItemTemplateId itemTemplateId
            )
            {
                var item = prefab.GetComponent<ClusterVR.CreatorKit.Item.IItem>();
                this.ItemTemplate = item;
                this.ItemTemplateId = itemTemplateId.cckId;
            }
        }

        public class CreateWorldItemTemplate
            : ClusterVR.CreatorKit.Gimmick.ICreateItemGimmick
        {
            public ClusterVR.CreatorKit.Item.IItem ItemTemplate { get; private set; }

            public ClusterVR.CreatorKit.Item.ItemTemplateId ItemTemplateId { get; private set; }

            public event ClusterVR.CreatorKit.Gimmick.CreateItemEventHandler OnCreateItem = delegate { };

            public CreateWorldItemTemplate(
                UnityEngine.GameObject prefab,
                ClusterVR.CreatorKit.Item.ItemTemplateId itemTemplateId
            )
            {
                var item = prefab.GetComponent<ClusterVR.CreatorKit.Item.IItem>();
                this.ItemTemplate = item;
                this.ItemTemplateId = itemTemplateId;
            }
        }

        readonly PrefabItemStore prefabItemStore;
        readonly ClusterVR.CreatorKit.Editor.Preview.Item.ItemCreator itemCreator;
        readonly ClusterVR.CreatorKit.Editor.Preview.Item.ItemDestroyer itemDestroyer;

        public ItemLifecycler(
            PrefabItemStore prefabItemStore,
            ClusterVR.CreatorKit.Editor.Preview.Item.ItemCreator itemCreator,
            ClusterVR.CreatorKit.Editor.Preview.Item.ItemDestroyer itemDestroyer
        )
        {
            this.prefabItemStore = prefabItemStore;
            this.itemCreator = itemCreator;
            this.itemDestroyer = itemDestroyer;
        }

        public ClusterVR.CreatorKit.Item.IItem CreateItem(
            EmulateClasses.ItemTemplateId itemTemplateId,
            EmulateClasses.EmulateVector3 position,
            EmulateClasses.EmulateQuaternion rotation
        )
        {
            //実行ごとにテンプレートを登録
            var prefab = prefabItemStore.GetPrefab(itemTemplateId.id);
            var template = new CreateItemTemplate(prefab, itemTemplateId);
            var AddItemTemplate = itemCreator.GetType().GetMethod("AddItemTemplate", BindingFlags.NonPublic | BindingFlags.Instance);
            AddItemTemplate.Invoke(itemCreator, new object[1] { template });

            ClusterVR.CreatorKit.Item.IItem create = null;
            Action<ClusterVR.CreatorKit.Item.IItem> OnCreateCompleted = item =>
            {
                //イベント引数をキャプチャで取得する。
                create = item;
            };
            itemCreator.OnCreateCompleted += OnCreateCompleted;
            itemCreator.Create(
                itemTemplateId.cckId,
                position._ToUnityEngine(),
                rotation._ToUnityEngine()
            );
            itemCreator.OnCreateCompleted -= OnCreateCompleted;
            if (create == null) return null;

            return create;
        }

        public ClusterVR.CreatorKit.Item.IItem CreateItem(
            ClusterVR.CreatorKit.Item.IWorldItemTemplateListEntry worldItemTemplateListEntry,
            EmulateClasses.EmulateVector3 position,
            EmulateClasses.EmulateQuaternion rotation
        )
        {
            //実行ごとにテンプレートを登録
            var prefab = worldItemTemplateListEntry.WorldItemTemplate.gameObject;
            var template = new CreateWorldItemTemplate(prefab, worldItemTemplateListEntry.ItemTemplateId);
            var AddItemTemplate = itemCreator.GetType().GetMethod("AddItemTemplate", BindingFlags.NonPublic | BindingFlags.Instance);
            AddItemTemplate.Invoke(itemCreator, new object[1] { template });

            ClusterVR.CreatorKit.Item.IItem create = null;
            Action<ClusterVR.CreatorKit.Item.IItem> OnCreateCompleted = item =>
            {
                //イベント引数をキャプチャで取得する。
                create = item;
            };
            itemCreator.OnCreateCompleted += OnCreateCompleted;
            itemCreator.Create(
                worldItemTemplateListEntry.ItemTemplateId,
                position._ToUnityEngine(),
                rotation._ToUnityEngine()
            );
            itemCreator.OnCreateCompleted -= OnCreateCompleted;
            if (create == null) return null;

            return create;
        }

        public void DestroyItem(ClusterVR.CreatorKit.Item.IItem item)
        {
            itemDestroyer.Destroy(item);
        }
    }
}
