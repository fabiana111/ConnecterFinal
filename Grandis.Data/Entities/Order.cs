using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Entities
{
    public class Order
    {
        public long Id { get; set; }
        public DateTime OrderDate { get; set; }
        public long CustomerId { get; set; }
        public int StatusId { get; set; }
        public decimal TotalAmount { get; set; }

        public Customer? Customer { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
