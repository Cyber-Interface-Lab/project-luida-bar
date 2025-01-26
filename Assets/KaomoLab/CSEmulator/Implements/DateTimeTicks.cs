using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Implements
{
    public class DateTimeTicks
        : ITicker
    {
        public long Ticks()
        {
            return DateTime.Now.Ticks;
        }
    }
}
