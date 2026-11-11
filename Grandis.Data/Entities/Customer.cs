using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Entities
{
    public class Customer
    {
        public long Id { get; set; }
        public DateTime CreditedAt { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public long AddresseeId { get; set; }

        public Addressee? Addressee { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
