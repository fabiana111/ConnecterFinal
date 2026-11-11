using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Entities
{
    public class Supplier
    {
        public long Id { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
