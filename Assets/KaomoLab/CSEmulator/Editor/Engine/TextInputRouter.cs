using Assets.KaomoLab.CSEmulator.Editor.EmulateClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.Engine
{
    public class TextInputRouter
        : EmulateClasses.ITextInputListenerBinder,
        EmulateClasses.ITextInputSender
    {
        class TextInput
        {
            public readonly long tick;
            public readonly ulong targetId;
            public readonly string text;
            public readonly string meta;
            public readonly TextInputStatus status;
            public TextInput(
                long tick,
                ulong targetId,
                string text,
                string meta,
                TextInputStatus status
            )
            {
                this.tick = tick;
                this.targetId = targetId;
                this.text = text;
                this.meta = meta;
                this.status = status;
            }
        }

        readonly Dictionary<ulong, IJintCallback<string, string, TextInputStatus>> receivers = new ();
        readonly List<TextInput> queue = new List<TextInput>();

        public ITicker ticker = new Implements.DateTimeTicks();

        public TextInputRouter()
        {
        }

        public void Send(ulong id, string text, string meta, TextInputStatus status)
        {
            //ディレイがある方がよさそう？0.1秒後に通知。
            var tick = ticker.Ticks() + 1_000_000;
            var message = new TextInput(tick, id, text, meta, status);
            queue.Add(message);
        }

        public void Routing()
        {
            var now = ticker.Ticks();
            //途中で削除するのでToArray
            foreach(var textInput in queue.ToArray())
            {
                if (textInput.tick > now) continue;
                queue.Remove(textInput);

                if (!receivers.ContainsKey(textInput.targetId)) continue;

                var receiver = receivers[textInput.targetId];
                receiver.Execute(textInput.text, textInput.meta, textInput.status);
            }
        }

        public void SetReceiveCallback(Components.CSEmulatorItemHandler owner, IJintCallback<string, string, TextInputStatus> Callback)
        {
            receivers[owner.item.Id.Value] = Callback;
        }

        public void DeleteReceiveCallback(Components.CSEmulatorItemHandler owner)
        {
            if (!receivers.ContainsKey(owner.item.Id.Value)) return;
            receivers.Remove(owner.item.Id.Value);
        }
    }
}
