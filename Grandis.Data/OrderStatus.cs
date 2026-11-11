using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Grandis.Data
{
    public enum OrderStatus
    {
        [Description("Új")]
        New = 1,
        [Description("Feldolgozás alatt")]
        Processing = 2,
        [Description("Szállítás alatt")]
        Shiping = 3,
        [Description("Teljesítve")]
        Delivered = 4,
        [Description("Törölve")]
        Cancelled = 5
    }
}
