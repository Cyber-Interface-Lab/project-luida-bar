using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator
{
    /// <summary>
    /// 単位時間の流量を制限する機能
    /// </summary>
    public class MassTimeThrottle
    {
        public interface ITimeServer
        {
            long GetTime();
        }

        public class ByDateTimeTicks : ITimeServer
        {
            public long GetTime()
            {
                return DateTime.Now.Ticks;
            }
        }

        class Unit
        {
            public double unit;
            public long time;
            public Unit(double unit, long time)
            {
                this.unit = unit;
                this.time = time;
            }
        }

        public double massLimit { get; private set; }
        public long span { get; private set; }

        readonly ITimeServer timeServer;

        readonly LinkedList<Unit> mass = new LinkedList<Unit>();

        double nowMass = 0;

        /// <summary>
        /// 単位時間の流量を制限する機能
        /// </summary>
        /// <param name="massLimit">単位時間で許容される総流量</param>
        /// <param name="span">単位時間</param>
        /// <param name="timeServer">時間を提供する機能</param>
        public MassTimeThrottle(
            double massLimit,
            long span,
            ITimeServer timeServer
        )
        {
            this.massLimit = massLimit;
            this.span = span;
            this.timeServer = timeServer;
        }
        /// <summary>
        /// 流入させてみる。
        /// </summary>
        /// <returns>流入に失敗した場合はfalse。成功した場合はtrue。</returns>
        public bool TryCharge(double unit)
        {
            //毎回Dischargeするのが正確だけど負荷を考えて行わない
            if(nowMass + unit > massLimit) return false;

            var massUnit = new Unit(unit, timeServer.GetTime());
            mass.AddFirst(new LinkedListNode<Unit>(massUnit));
            nowMass += unit;
            return true;
        }
        /// <summary>
        /// 放出させる。
        /// </summary>
        public void Discharge()
        {
            var nowTime = timeServer.GetTime();
            var unit = mass.Last;
            while (unit != null && unit.Value.time + span <= nowTime)
            {
                nowMass -= unit.Value.unit;
                mass.RemoveLast();
                unit = mass.Last;
            }
            if (unit == null) nowMass = 0; //誤差用の予防
        }
    }
}
