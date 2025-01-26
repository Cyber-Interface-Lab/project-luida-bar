using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator
{
    /// <summary>
    /// 瞬間的な流入回数を許容する流入量制限機能
    /// </summary>
    public class BurstableThrottle
        : IChargeThrottle
    {
        public int burstLimit { get; private set; }

        readonly double charge;
        readonly double[] charges;
        int head = 0; //この位置に入れる
        int tail = 0; //この位置から出す

        /// <summary>
        /// 瞬間的な流入回数を許容する流入量制限機能
        /// </summary>
        /// <param name="charge">１回のCharge量</param>
        /// <param name="burstLimit">瞬間的にChargeできる回数</param>
        public BurstableThrottle(
            double charge,
            int burstLimit
        )
        {
            this.charge = charge;
            this.burstLimit = burstLimit;
            charges = new double[burstLimit].Select(n => 0d).ToArray();
        }
        /// <summary>
        /// 流入させてみる。
        /// </summary>
        /// <returns>流入に失敗した場合はfalse。成功した場合はtrue。</returns>
        public bool TryCharge()
        {
            if (charges[head] > 0) return false;
            charges[head] = charge;
            head++;
            if (head == burstLimit) head = 0;
            return true;
        }
        /// <summary>
        /// 放出させる。
        /// </summary>
        /// <param name="amount">放出量</param>
        public void Discharge(double amount)
        {
            while (charges[tail] > 0)
            {
                charges[tail] -= amount;
                if (charges[tail] > 0)
                    break;

                amount = -charges[tail];
                charges[tail] = 0;
                tail++;
                if (tail == burstLimit) tail = 0;
            }
        }
    }
}
