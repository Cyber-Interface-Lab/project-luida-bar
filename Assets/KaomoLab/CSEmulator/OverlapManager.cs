using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator
{
    public class OverlapManager<ITEM_HANDLER, PLAYER_HANDLER>
        where ITEM_HANDLER : class
        where PLAYER_HANDLER : class
    {
        readonly GameObject gameObject;

        readonly Dictionary<string, GameObject> subNodeGameObjects = new Dictionary<string, GameObject>();
        readonly Dictionary<string, Dictionary<int, (ITEM_HANDLER, Collider)>> subNodeItemOverlaps = new Dictionary<string, Dictionary<int, (ITEM_HANDLER, Collider)>>();
        readonly Dictionary<string, Dictionary<int, (PLAYER_HANDLER, Collider)>> subNodePlayerOverlaps = new Dictionary<string, Dictionary<int, (PLAYER_HANDLER, Collider)>>();
        readonly Dictionary<string, Dictionary<int, Collider>> subNodeColliderOverlaps = new Dictionary<string, Dictionary<int, Collider>>();
        readonly Dictionary<int, (ITEM_HANDLER, Collider)> itemOverlaps = new Dictionary<int, (ITEM_HANDLER, Collider)>();
        readonly Dictionary<int, (PLAYER_HANDLER, Collider)> playerOverlaps = new Dictionary<int, (PLAYER_HANDLER, Collider)>();
        readonly Dictionary<int, Collider> colliderOverlaps = new Dictionary<int, Collider>();

        public OverlapManager(
            GameObject gameObject
        )
        {
            this.gameObject = gameObject;
        }

        public void SetOverlap(
            Collider collider
        )
        {
            if (!IsOverlapTarget(gameObject, collider)) return;
            var (itemHandler, playerHandler) = GetHandler(collider);

            if (itemHandler != null)
                itemOverlaps[collider.GetInstanceID()] = (itemHandler, collider);
            if (playerHandler != null)
                playerOverlaps[collider.GetInstanceID()] = (playerHandler, collider);
            if (IsOverlapCollider(itemHandler, playerHandler))
                colliderOverlaps[collider.GetInstanceID()] = collider;
        }

        public void RemoveOverlap(
            Collider collider
        )
        {
            if (!IsOverlapTarget(gameObject, collider)) return;
            var (itemHandler, playerHandler) = GetHandler(collider);

            if (itemHandler != null)
                itemOverlaps.Remove(collider.GetInstanceID());
            if (playerHandler != null)
            {
                playerOverlaps.Remove(collider.GetInstanceID());
                //playerがワープすると１つ残るので同じplayerの場合に削除をするようにしている（SubNode側も）
                //こういう解決でいいか分からない
                //シングルなので上手くいっているけど、マルチにしたときは想定してない
                var otherIDs = new List<int>();
                foreach(var kv in playerOverlaps)
                {
                    var p = kv.Value.Item1;
                    if (p == playerHandler) otherIDs.Add(kv.Key);
                }
                foreach(var otherID in otherIDs)
                {
                    playerOverlaps.Remove(otherID);
                }
            }
            if (IsOverlapCollider(itemHandler, playerHandler))
                colliderOverlaps.Remove(collider.GetInstanceID());

        }

        public void SetSubNodeOverlap(
            GameObject gameObject,
            Collider collider
        )
        {
            if (!IsOverlapTarget(gameObject, collider)) return;
            var (itemHandler, playerHandler) = GetHandler(collider);
            var name = gameObject.name;

            if (!subNodeGameObjects.ContainsKey(name))
                subNodeGameObjects[name] = gameObject;
            if (!subNodeItemOverlaps.ContainsKey(name))
                subNodeItemOverlaps[name] = new Dictionary<int, (ITEM_HANDLER, Collider)>();
            if (!subNodePlayerOverlaps.ContainsKey(name))
                subNodePlayerOverlaps[name] = new Dictionary<int, (PLAYER_HANDLER, Collider)>();
            if (!subNodeColliderOverlaps.ContainsKey(name))
                subNodeColliderOverlaps[name] = new Dictionary<int, Collider>();

            if (itemHandler != null)
                subNodeItemOverlaps[name][collider.GetInstanceID()] = (itemHandler, collider);

            if (playerHandler != null)
                subNodePlayerOverlaps[name][collider.GetInstanceID()] = (playerHandler, collider);

            if (IsOverlapCollider(itemHandler, playerHandler))
                subNodeColliderOverlaps[name][collider.GetInstanceID()] = collider;
        }

        public void RemoveSubNodeOverlap(
            GameObject gameObject,
            Collider collider
        )
        {
            if (!IsOverlapTarget(gameObject, collider)) return;
            var (itemHandler, playerHandler) = GetHandler(collider);
            var name = gameObject.name;

            if (!subNodeGameObjects.ContainsKey(name))
                subNodeGameObjects[name] = gameObject;
            if (!subNodeItemOverlaps.ContainsKey(name))
                subNodeItemOverlaps[name] = new Dictionary<int, (ITEM_HANDLER, Collider)>();
            if (!subNodePlayerOverlaps.ContainsKey(name))
                subNodePlayerOverlaps[name] = new Dictionary<int, (PLAYER_HANDLER, Collider)>();
            if (!subNodeColliderOverlaps.ContainsKey(name))
                subNodeColliderOverlaps[name] = new Dictionary<int, Collider>();

            if (itemHandler != null)
                subNodeItemOverlaps[name].Remove(collider.GetInstanceID());

            if (playerHandler != null)
            {
                subNodePlayerOverlaps[name].Remove(collider.GetInstanceID());
                var otherIDs = new List<int>();
                foreach (var kv in subNodePlayerOverlaps[name])
                {
                    var p = kv.Value.Item1;
                    if (p == playerHandler) otherIDs.Add(kv.Key);
                }
                foreach (var otherID in otherIDs)
                {
                    subNodePlayerOverlaps[name].Remove(otherID);
                }
            }

            if (collider != null)
                subNodeColliderOverlaps[name].Remove(collider.GetInstanceID());
        }

        public void CheckOverlapsActive()
        {
            //SetActiveでfalseされた場合、OnTriggerExitが動かないので、
            //リスト中のActiveを確認し必要に応じて削除する。という挙動がある模様。
            foreach (var key in itemOverlaps.Keys.ToArray())
            {
                var c = itemOverlaps[key].Item2;
                if (c != null && c.gameObject.activeInHierarchy) continue;
                itemOverlaps.Remove(key);
            }
            foreach (var key in playerOverlaps.Keys.ToArray())
            {
                //playerもこの判定でいいかはわからないけど、プレビュー上ならどうにでもできる
                var c = playerOverlaps[key].Item2;
                if (c != null && c.gameObject.activeInHierarchy) continue;
                playerOverlaps.Remove(key);
            }
            foreach (var key in colliderOverlaps.Keys.ToArray())
            {
                var c = colliderOverlaps[key];
                if (c != null && c.gameObject.activeInHierarchy) continue;
                colliderOverlaps.Remove(key);
            }

            foreach (var name in subNodeItemOverlaps.Keys)
                foreach (var key in subNodeItemOverlaps[name].Keys.ToArray())
                {
                    var s = subNodeGameObjects[name];
                    var c = subNodeItemOverlaps[name][key].Item2;
                    if (s.activeInHierarchy && c != null && c.gameObject.activeInHierarchy) continue;
                    subNodeItemOverlaps[name].Remove(key);
                }
            foreach (var name in subNodePlayerOverlaps.Keys)
                foreach (var key in subNodePlayerOverlaps[name].Keys.ToArray())
                {
                    //playerもこの判定でいいかはわからないけど、プレビュー上ならどうにでもできる
                    var s = subNodeGameObjects[name];
                    var c = subNodePlayerOverlaps[name][key].Item2;
                    if (s.activeInHierarchy && c != null && c.gameObject.activeInHierarchy) continue;
                    subNodePlayerOverlaps[name].Remove(key);
                }
            foreach (var name in subNodeColliderOverlaps.Keys)
                foreach (var key in subNodeColliderOverlaps[name].Keys.ToArray())
                {
                    var s = subNodeGameObjects[name];
                    var c = subNodeColliderOverlaps[name][key];
                    if (s.activeInHierarchy && c != null && c.gameObject.activeInHierarchy) continue;
                    subNodeColliderOverlaps[name].Remove(key);
                }
        }

        public (string, ITEM_HANDLER, PLAYER_HANDLER)[] GetOverlaps()
        {
            var ret = new List<(string, ITEM_HANDLER, PLAYER_HANDLER)>();

            foreach (var key in itemOverlaps.Keys)
                ret.Add(("", itemOverlaps[key].Item1, null));
            foreach (var key in playerOverlaps.Keys)
                ret.Add(("", null, playerOverlaps[key].Item1));
            foreach (var _ in colliderOverlaps.Keys)
                ret.Add(("", null, null));

            foreach (var name in subNodeItemOverlaps.Keys)
                foreach (var key in subNodeItemOverlaps[name].Keys)
                    ret.Add((name, subNodeItemOverlaps[name][key].Item1, null));
            foreach (var name in subNodePlayerOverlaps.Keys)
                foreach (var key in subNodePlayerOverlaps[name].Keys)
                    ret.Add((name, null, subNodePlayerOverlaps[name][key].Item1));
            foreach (var name in subNodeColliderOverlaps.Keys)
                foreach (var _ in subNodeColliderOverlaps[name].Keys)
                    ret.Add((name, null, null));

            return ret.ToArray();
        }

        public static bool IsOverlapTarget(GameObject self, Collider collider)
        {
            //まずDetectorは自身に必須
            if (self.GetComponent<ClusterVR.CreatorKit.Item.IOverlapDetectorShape>() == null)
                return false;

            //MovableItemならRigidbodyがある。
            if (
                //CharacterControllerが親にいても反応しない。
                //Rigidbodyが親にいるのには反応する。
                null == collider.gameObject.GetComponent<CharacterController>()
                && null == collider.gameObject.GetComponentInParent<Rigidbody>()
                && null == collider.gameObject.GetComponentInChildren<PLAYER_HANDLER>()
                && null == self.GetComponent<CharacterController>()
                && null == self.GetComponentInParent<Rigidbody>()
                && null == self.GetComponentInChildren<PLAYER_HANDLER>()
            ) return false;

            if (null != collider.gameObject.GetComponent<ClusterVR.CreatorKit.Item.IPhysicalShape>())
                return true;
            if (null != collider.gameObject.GetComponent<ClusterVR.CreatorKit.Item.IOverlapSourceShape>())
                return true;
            if (!collider.isTrigger) return true;

            return false;
        }

        public static (ITEM_HANDLER, PLAYER_HANDLER) GetHandler(Collider collider)
        {
            var itemHandler = collider.GetComponentInParent<ITEM_HANDLER>();
            var playerHandler = collider.GetComponentInChildren<PLAYER_HANDLER>();

            return (
                itemHandler, playerHandler
            );
        }
        public static bool IsOverlapCollider(ITEM_HANDLER item, PLAYER_HANDLER handler)
        {
            return item == null && handler == null;
        }

    }
}
