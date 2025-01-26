using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class ItemTemplateId
    {
        //おそらく外部から参照する必要がある。小文字に正規化している。
        public readonly string id;

        //2.5.0現在ItemTemplateIdのidがUUIDではないので個別に保持。
        public readonly ClusterVR.CreatorKit.Item.ItemTemplateId cckId;

        //アップロードしたクラフトアイテムのURL末尾がUUID。
        public ItemTemplateId(
            string id
        )
        {
            if (!CSEmulator.Commons.IsUUID(id))
            {
                //Exceptionにすべき？
                UnityEngine.Debug.LogError(String.Format("need UUID format.[{0}]", id));
                return;
            }

            this.id = id.ToLowerInvariant();

            //負荷の問題で不利だと思うが、簡便化のために毎度新規生成する。
            //itemCreatorに毎度追加になるため大量生成で重くなったという問題が浮上したら対応する。
            this.cckId = ClusterVR.CreatorKit.Item.ItemTemplateId.Create();
        }

        public object toJSON(string key)
        {
            dynamic o = new System.Dynamic.ExpandoObject();
            return o;
        }
        public override string ToString()
        {
            return String.Format("[ItemTemplateId][{0}]", id);
        }

    }
}
