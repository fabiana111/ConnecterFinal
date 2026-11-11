using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Product.Dto
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public long CategoryId { get; set; }
        public long SupplierId { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
