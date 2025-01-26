using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator.Editor.EmulateClasses
{
    public enum PurchaseRequestStatus
    {
        Unknown = 0,
        Purchased = 1,
        Busy = 2,
        UserCanceled = 3,
        NotAvailable = 4,
        Failed = 5,
    }
}
