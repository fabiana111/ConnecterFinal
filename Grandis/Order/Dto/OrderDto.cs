using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Order.Dto
{
    public class OrderDto
    {
        public long Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public OrderCustomerDto? Customer { get; set; }
        public IReadOnlyList<OrderItemDto> Items { get; set; } = [];
    }
}
