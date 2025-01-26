using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class HitObject
    {
        public readonly ItemHandle itemHandle;
        public readonly PlayerHandle playerHandle;

        public HitObject(
            ItemHandle itemHandle,
            PlayerHandle playerHandle
        )
        {
            this.itemHandle = itemHandle;
            this.playerHandle = playerHandle;
        }

        //雑対応だけど一旦これで
        public object GetHandle()
        {
            if (itemHandle != null) return itemHandle;
            if (playerHandle != null) return playerHandle;
            return null;
        }

        public static HitObject Create(
            CSEmulator.Components.CSEmulatorItemHandler csItemHandler,
            CSEmulator.Components.CSEmulatorItemHandler csItemOwnerHandler,
            CSEmulator.Components.CSEmulatorPlayerHandler csPlayerHandler,
            IPlayerHandleFactory playerHandleFactory,
            ISpaceContext spaceContext,
            IRunningContext runningContext,
            ISendableSanitizer sendableSanitizer,
            IMessageSender messageSender
        )
        {
            var itemHandler = csItemHandler == null ? null : new ItemHandle(
                csItemHandler, csItemOwnerHandler, spaceContext, runningContext, sendableSanitizer, messageSender
            );
            var playerHandler = csPlayerHandler == null ? null : playerHandleFactory.CreateByIdfc(
                csPlayerHandler.idfc,
                csItemOwnerHandler
            );
            var hitObject = new HitObject(itemHandler, playerHandler);
            return hitObject;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[HitObject][{0}][{1}]", itemHandle, playerHandle);
        }
    }
}
