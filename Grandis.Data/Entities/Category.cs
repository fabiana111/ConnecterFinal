using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Entities
{
    public class Category
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigációs tulajdonság EF Core-hoz
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
