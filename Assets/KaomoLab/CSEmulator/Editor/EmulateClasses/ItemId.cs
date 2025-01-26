using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class ItemId
        : ISendableSize
    {
        public string id => invalid ? "0" : csItemHandler.id;
        public string type => "item";

        public Components.CSEmulatorItemHandler csItemHandler { get; private set; }

        readonly bool invalid = false;

        public ItemId()
        {
            invalid = true;
        }

        public ItemId(
            Components.CSEmulatorItemHandler csItemHandler
        )
        {
            this.csItemHandler = csItemHandler;
        }

        public int GetSize()
        {
            //軽く調べたところ、ItemHandleと同サイズの模様
            return 13;
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            o.id = id;
            return o;
        }
        public override string ToString()
        {
            if (invalid) return String.Format("[ItemId][無効]");
            return String.Format("[ItemId][{0}]", id);
        }
    }
}
