using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class ProductPurchaser
        : EmulateClasses.IProductPurchaser
    {
        class PurchaseUpdate
        {
            public readonly PlayerHandle player;
            public readonly string productId;
            public readonly long tick;
            public PurchaseUpdate(PlayerHandle player, string productId, long tick)
            {
                this.player = player;
                this.productId = productId;
                this.tick = tick;
            }
        }
        readonly List<PurchaseUpdate> purchaseUpdateQueue = new();

        class OwnProductSet
        {
            public readonly ulong itemId;
            public readonly string productId;
            public readonly PlayerHandle[] players;
            public readonly string meta;
            public readonly string error;
            public readonly long tick;
            public OwnProductSet(ulong itemId, string productId, PlayerHandle[] players, string meta, string error, long tick)
            {
                this.itemId = itemId;
                this.productId = productId;
                this.players = players;
                this.meta = meta;
                this.error = error;
                this.tick = tick;
            }
        }
        readonly List<OwnProductSet> ownProductQueue = new();

        class PurchaseResult
        {
            public readonly ulong itemId;
            public readonly string meta;
            public readonly string error;
            public readonly PurchaseRequestStatus status;
            public readonly PlayerHandle player;
            public readonly long tick;
            public PurchaseResult(ulong itemId, string meta, string error, PurchaseRequestStatus status, PlayerHandle player, long tick)
            {
                this.itemId = itemId;
                this.meta = meta;
                this.error = error;
                this.status = status;
                this.player = player;
                this.tick = tick;
            }
        }
        readonly List<PurchaseResult> purchaseResultQueue = new();

        readonly Dictionary<ulong, List<string>> subscribes = new();

        readonly IProductOptions productOptions;
        readonly Dictionary<ulong, IJintCallback<PlayerHandle, string>> purchaseUpdateCallbacks = new();
        readonly Dictionary<ulong, IJintCallback<string, PurchaseRequestStatus, string, PlayerHandle>> requestPurchaseStatusCallbacks = new();
        readonly Dictionary<ulong, IJintCallback<OwnProduct[], string, string>> getOwnProductsCallbacks = new();

        readonly ITicker ticker = new Implements.DateTimeTicks();
        long prevTicks = -1;

        readonly BurstableThrottle getOwnProductsThrottle = new BurstableThrottle(60d / 100d, 5);

        public ProductPurchaser(
            IProductOptions productOptions
        )
        {
            this.productOptions = productOptions;
        }

        public bool IsGetOwnProductsLimit()
        {
            var result = getOwnProductsThrottle.TryCharge();
            if (result) return false;

            return true;
        }

        public bool IsPublicProduct(string productId)
        {
            return productOptions.IsPublicProduct(productId);
        }

        public void GetOwnProducts(ulong itemId, string productId, PlayerHandle[] players, string meta)
        {
            if (players.Length == 0) return; //CSETODO multiに対応するようになったら対応
            if (productOptions.GetProductName(productId) == null) return;
            if (!productOptions.IsPublicProduct(productId)) return; //テストスペースではない場合、非公開では取得できない。

            ownProductQueue.Add(
                new OwnProductSet(
                    itemId,
                    productId,
                    players,
                    meta,
                    null, //滅多に起きなさそうだし、起きたところで使われ方としては問題なさそうなので
                    ticker.Ticks() + 1_000_000
                )
            );
        }

        public string GetProductNameById(string productId)
        {
            return productOptions.GetProductName(productId);
        }

        public void SendPurchaseResult(ulong itemId, string productId, string meta, PlayerHandle player, PurchaseRequestStatus status)
        {
            //onPurchaseUpdateが発火する直前でsubscribeした場合は未調査
            if (subscribes.ContainsKey(itemId) && subscribes[itemId].Contains(productId) && status == PurchaseRequestStatus.Purchased)
            {
                purchaseUpdateQueue.Add(new PurchaseUpdate(
                    player, productId, ticker.Ticks() + 1_000_000
                ));
            }
            if(status == PurchaseRequestStatus.Purchased)
            {
                //CSETODO multiに対応したら考える
                var (plus, minus) = productOptions.GetProductAmount(productId);
                productOptions.SetProductAmount(productId, plus + 1, minus);
            }
            purchaseResultQueue.Add(
                new PurchaseResult(
                    itemId,
                    meta,
                    status switch {
                        PurchaseRequestStatus.Unknown => Enum.GetName(typeof(PurchaseRequestStatus), status),
                        PurchaseRequestStatus.NotAvailable => Enum.GetName(typeof(PurchaseRequestStatus), status),
                        PurchaseRequestStatus.Failed => Enum.GetName(typeof(PurchaseRequestStatus), status),
                        _ => null
                    },
                    status, player, ticker.Ticks() + 1_000_000
                )
            );
        }

        public void SetGetOwnProductsCallback(ulong itemId, IJintCallback<OwnProduct[], string, string> Callback)
        {
            getOwnProductsCallbacks[itemId] = Callback;
        }

        public void SetRequestPurchaseStatusCallback(ulong itemId, IJintCallback<string, PurchaseRequestStatus, string, PlayerHandle> Callback)
        {
            requestPurchaseStatusCallbacks[itemId] = Callback;
        }

        public void SetPurchaseUpdateCallback(ulong itemId, IJintCallback<PlayerHandle, string> Callback)
        {
            purchaseUpdateCallbacks[itemId] = Callback;
        }

        public void DeleteCallbacks(ulong itemId)
        {
            if (purchaseUpdateCallbacks.ContainsKey(itemId)) purchaseUpdateCallbacks.Remove(itemId);
            if (requestPurchaseStatusCallbacks.ContainsKey(itemId)) requestPurchaseStatusCallbacks.Remove(itemId);
            if (getOwnProductsCallbacks.ContainsKey(itemId)) getOwnProductsCallbacks.Remove(itemId);
        }

        public void SubscribePurchase(ulong itemId, string productId)
        {
            if (!subscribes.ContainsKey(itemId))
                subscribes[itemId] = new List<string>();
            if (subscribes[itemId].Contains(productId)) return; //複数購読できない
            subscribes[itemId].Add(productId);
        }

        public void UnsubscribePurchase(ulong itemId, string productId)
        {
            if (!subscribes.ContainsKey(itemId))
                subscribes[itemId] = new List<string>();
            if (!subscribes[itemId].Contains(productId)) return;
            subscribes[itemId].Remove(productId);
        }

        public void Routing()
        {
            var tick = ticker.Ticks();
            {
                var deltaTime = (double)(tick - (prevTicks == -1 ? tick : prevTicks)) / 10_000_000d;
                getOwnProductsThrottle.Discharge(deltaTime);
                prevTicks = tick;
            }

            foreach (var p in purchaseUpdateQueue.ToArray())
            {
                if (p.tick < tick) continue;
                purchaseUpdateQueue.Remove(p);

                foreach (var kv in subscribes)
                {
                    if (!kv.Value.Contains(p.productId)) continue;

                    if (!purchaseUpdateCallbacks.ContainsKey(kv.Key)) continue;
                    purchaseUpdateCallbacks[kv.Key].Execute(p.player, p.productId);
                }
            }

            foreach (var p in ownProductQueue.ToArray())
            {
                if (p.tick < tick) continue;
                ownProductQueue.Remove(p);

                foreach(var kv in getOwnProductsCallbacks)
                {
                    if(p.itemId != kv.Key) continue;
                    var ownProducts = p.players.Select(player =>
                    {
                        //CSETODO multi対応したら対応する
                        var (plus, minus) = productOptions.GetProductAmount(p.productId);
                        var ret = new OwnProduct(minus, player, plus, p.productId);
                        return ret;
                    }).ToArray();
                    kv.Value.Execute(ownProducts, p.meta, p.error);
                }
            }

            foreach (var p in purchaseResultQueue.ToArray())
            {
                if (p.tick < tick) continue;
                purchaseResultQueue.Remove(p);

                foreach(var kv in requestPurchaseStatusCallbacks)
                {
                    if (p.itemId != kv.Key) continue;
                    kv.Value.Execute(p.meta, p.status, p.error, p.player);
                }
            }
        }
    }
}
