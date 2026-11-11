using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Order.Dto
{
    public class OrderCustomerDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
