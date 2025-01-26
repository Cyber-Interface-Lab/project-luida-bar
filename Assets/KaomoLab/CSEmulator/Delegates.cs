using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator
{
    public delegate void Handler();
    public delegate void Handler<T>(T data);
    public delegate void Handler<T1, T2>(T1 data1, T2 data2);

}
