using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Entities
{
    public class Addressee
    {
        public long Id { get; set; }
        public string AddresseeCountry { get; set; } = string.Empty;
        public string AddresseeZip { get; set; } = string.Empty;
        public string AddresseeAddress { get; set; } = string.Empty;
        public string AddresseeCity { get; set; } = string.Empty;
    }
}
