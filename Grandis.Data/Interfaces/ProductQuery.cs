using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Interfaces
{
    public class ProductQuery
    {
        public long? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsActive { get; set; }

        /// <summary>1-alapú lapszám.</summary>
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 50;
    }
}
