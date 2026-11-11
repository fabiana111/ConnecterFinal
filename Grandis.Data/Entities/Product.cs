using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Entities
{
    public class Product
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long CategoryId { get; set; }
        public long SupplierId { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigációs tulajdonságok (EF Core-nak hasznos, ADO/Dapper esetén elhagyható vagy DTO-nál tölthető)
        public Category? Category { get; set; }
        public Supplier? Supplier { get; set; }
    }
}
