using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator
{
    public class PassThroughThrottle
        : IChargeThrottle
    {
        public void Discharge(double amount)
        {
        }

        public bool TryCharge()
        {
            return true;
        }
    }
}
