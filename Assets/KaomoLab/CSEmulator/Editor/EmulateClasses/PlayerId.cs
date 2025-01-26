using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class PlayerId
        : ISendableSize
    {
        public string id => playerHandle.id;
        public string type => "player";
        public string __idfc => playerHandle.idfc;

        public PlayerHandle playerHandle { get; private set; }

        public PlayerId(
            PlayerHandle playerHandle
        )
        {
            this.playerHandle = playerHandle;
        }

        public int GetSize()
        {
            //軽く調べたところ、PlayerHandleと同サイズの模様
            return 40;
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            o.id = id;
            return o;
        }
        public override string ToString()
        {
            return String.Format("[PlayerId][{0}]", id);
        }
    }
}
