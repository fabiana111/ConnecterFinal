using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Product.Dto
{
    public class PagedProductsDto
    {
        public IReadOnlyList<ProductDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
