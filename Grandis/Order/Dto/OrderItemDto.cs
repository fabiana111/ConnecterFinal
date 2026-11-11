using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Order.Dto
{
    public class OrderItemDto
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
