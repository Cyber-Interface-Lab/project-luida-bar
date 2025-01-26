using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    //クラス名は変更できてもファイル名を変更するとunptypackageのimport後に面倒なことになるのでそのまま
    public class ItemMessageRouter
        : EmulateClasses.IItemReceiveListenerBinder,
        EmulateClasses.IPlayerReceiveListenerBinder,
        EmulateClasses.IMessageSender
    {
        class ItemMessage
        {
            public enum SenderType
            {
                Player, Item
            }

            public readonly long tick;
            public readonly string targetId; //itemのid
            public readonly string messageType;
            public readonly object arg;
            public readonly EmulateClasses.PlayerHandle senderPlayer;
            public readonly Components.CSEmulatorItemHandler senderItem;
            public readonly SenderType senderType;
            public ItemMessage(
                long tick,
                string targetId,
                string messageType,
                object arg,
                EmulateClasses.PlayerHandle senderPlayer,
                Components.CSEmulatorItemHandler senderItem
            )
            {
                this.tick = tick;
                this.targetId = targetId;
                this.messageType = messageType;
                this.arg = arg;
                this.senderPlayer = senderPlayer;
                this.senderItem = senderItem;
                senderType = senderItem == null ? SenderType.Player : SenderType.Item;
            }
        }

        class PlayerMessage
        {
            public readonly long tick;
            public readonly EmulateClasses.PlayerId targetId; //playerのid
            public readonly string messageType;
            public readonly object arg;
            public readonly EmulateClasses.PlayerHandle senderPlayer;
            public readonly Components.CSEmulatorItemHandler senderItem;
            public PlayerMessage(
                long tick,
                EmulateClasses.PlayerId targetId,
                string messageType,
                object arg,
                EmulateClasses.PlayerHandle senderPlayer,
                Components.CSEmulatorItemHandler senderItem
            )
            {
                this.tick = tick;
                this.targetId = targetId;
                this.messageType = messageType;
                this.arg = arg;
                this.senderPlayer = senderPlayer;
                this.senderItem = senderItem;
            }
        }

        readonly Dictionary<
            string,
            (
                Components.CSEmulatorItemHandler owner,
                EmulateClasses.IRunningContext,
                EmulateClasses.ISendableSanitizer,
                EmulateClasses.IJintCallback<string, object, object>
            )
        > itemReceivers = new();
        readonly List<ItemMessage> itemQueue = new List<ItemMessage>();

        readonly Dictionary<
            string,
            (
                EmulateClasses.IPlayerSendableSanitizer,
                EmulateClasses.IJintCallback<string, object, object>
            )
        > playerReceivers = new();
        readonly List<PlayerMessage> playerQueue = new List<PlayerMessage>();

        public ITicker ticker = new Implements.DateTimeTicks();

        readonly EmulateClasses.ISpaceContext spaceContext;

        public ItemMessageRouter(
            EmulateClasses.ISpaceContext spaceContext
        )
        {
            this.spaceContext = spaceContext;
        }

        public void SendToItem(
            string id,
            string messageType,
            object arg,
            EmulateClasses.PlayerHandle senderPlayer,
            Components.CSEmulatorItemHandler senderItem
        )
        {
            //ディレイがある方がよさそう？0.1秒後に通知。
            var tick = ticker.Ticks() + 1_000_000;
            var message = new ItemMessage(tick, id, messageType, arg, senderPlayer, senderItem);
            itemQueue.Add(message);
        }

        public void SendToPlayer(
            EmulateClasses.PlayerId id,
            string messageType,
            object arg,
            EmulateClasses.PlayerHandle senderPlayer,
            Components.CSEmulatorItemHandler senderItem
        )
        {
            //ディレイがある方がよさそう？0.1秒後に通知。
            var tick = ticker.Ticks() + 1_000_000;
            var message = new PlayerMessage(tick, id, messageType, arg, senderPlayer, senderItem);
            playerQueue.Add(message);
        }

        public void Routing()
        {
            var now = ticker.Ticks();
            //途中で削除するのでToArray
            foreach(var itemMessage in itemQueue.ToArray())
            {
                if (itemMessage.tick > now) continue;
                itemQueue.Remove(itemMessage);

                if (!itemReceivers.ContainsKey(itemMessage.targetId)) continue;

                if (itemMessage.senderType == ItemMessage.SenderType.Item)
                    SendItemToItem(itemMessage);

                if (itemMessage.senderType == ItemMessage.SenderType.Player)
                    SendPlayerToItem(itemMessage);
            }

            foreach(var playerMessage in playerQueue.ToArray())
            {
                if (playerMessage.tick > now) continue;
                playerQueue.Remove(playerMessage);

                if (!playerReceivers.ContainsKey(playerMessage.targetId.id)) continue;

                if (playerMessage.senderPlayer != null)
                    SendPlayerToPlayer(playerMessage);

                if (playerMessage.senderItem != null)
                    SendItemToPlayer(playerMessage);
            }
        }
        void SendItemToItem(ItemMessage itemMessage)
        {
            var (owner, ownerContext, sanitizer, receiver) = itemReceivers[itemMessage.targetId];
            //このタイミングでItemHandleがスクリプト空間を超えるので
            //owner(スクリプト空間主＝$)が切り替わる。
            //切り替わるタイミングで、過去ownerがhandleを保持している可能性はあるのでnewで作り直す。
            var sender = new EmulateClasses.ItemHandle(
                itemMessage.senderItem, //senderのItemHandleということ
                owner,
                spaceContext,
                ownerContext,
                sanitizer,
                this
            );
            var arg = sanitizer.Sanitize(
                itemMessage.arg,
                h => new EmulateClasses.ItemHandle(
                    h,
                    owner,
                    spaceContext,
                    ownerContext,
                    sanitizer,
                    this
                ),
                h => new EmulateClasses.PlayerHandle(
                    h,
                    owner
                )
            );
            receiver.Execute(itemMessage.messageType, arg, sender);
        }
        void SendPlayerToItem(ItemMessage itemMessage)
        {
            var (owner, ownerContext, sanitizer, receiver) = itemReceivers[itemMessage.targetId];
            var sender = itemMessage.senderPlayer;
            var arg = sanitizer.Sanitize(
                itemMessage.arg,
                h => new EmulateClasses.ItemHandle(
                    h,
                    owner,
                    spaceContext,
                    ownerContext,
                    sanitizer,
                    this
                ),
                h => new EmulateClasses.PlayerHandle(
                    h,
                    owner
                )
            );
            receiver.Execute(itemMessage.messageType, arg, sender);
        }
        void SendPlayerToPlayer(PlayerMessage playerMessage)
        {
            var (sanitizer, receiver) = playerReceivers[playerMessage.targetId.id];
            var sender = new EmulateClasses.PlayerId(playerMessage.senderPlayer);
            var arg = sanitizer.Sanitize(
                playerMessage.arg
            );
            receiver.Execute(playerMessage.messageType, arg, sender);
        }
        void SendItemToPlayer(PlayerMessage playerMessage)
        {
            var (sanitizer, receiver) = playerReceivers[playerMessage.targetId.id];
            var sender = new EmulateClasses.ItemId(playerMessage.senderItem);
            var arg = sanitizer.Sanitize(
                playerMessage.arg
            );
            receiver.Execute(playerMessage.messageType, arg, sender);
        }

        public void SetItemReceiveCallback(
            Components.CSEmulatorItemHandler owner,
            EmulateClasses.IRunningContext ownerContext,
            EmulateClasses.ISendableSanitizer sanitizer,
            EmulateClasses.IJintCallback<string, object, object> Callback
        )
        {
            itemReceivers[owner.item.Id.Value.ToString()] = (owner, ownerContext, sanitizer, Callback);
        }

        public void DeleteItemReceiveCallback(Components.CSEmulatorItemHandler owner)
        {
            if (!itemReceivers.ContainsKey(owner.item.Id.Value.ToString())) return;
            itemReceivers.Remove(owner.item.Id.Value.ToString());
        }

        public void SetPlayerReceiveCallback(
            string playerId,
            EmulateClasses.IPlayerSendableSanitizer sanitizer,
            EmulateClasses.IJintCallback<string, object, object> Callback
        )
        {
            playerReceivers[playerId] = (sanitizer, Callback);
        }

        public void DeletePlayerReceiveCallback(string playerId)
        {
            if (!playerReceivers.ContainsKey(playerId)) return;
            playerReceivers.Remove(playerId);
        }

    }
}
