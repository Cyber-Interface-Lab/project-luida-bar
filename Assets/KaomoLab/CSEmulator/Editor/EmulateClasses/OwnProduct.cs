using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public class OwnProduct
    {
        public float minusAmount { get; private set; }
        public PlayerHandle player { get; private set; }
        public float plusAmount { get; private set; }
        public string productId { get; private set; }

        public OwnProduct(
            float minusAmount, PlayerHandle player, float plusAmount, string productId)
        {
            this.minusAmount = minusAmount;
            this.player = player;
            this.plusAmount = plusAmount;
            this.productId = productId;
        }

        public object toJSON(string key)
        {
            return this;
        }
        public override string ToString()
        {
            return String.Format("[OwnProduct][{0}:{1}:{2}:{3}]", productId, plusAmount, minusAmount, player);
        }

    }
}
