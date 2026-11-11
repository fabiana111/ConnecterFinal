using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Banchmark.Dto
{
    public enum BenchmarkScenario
    {
        GetAllProducts,
        GetProductById,
        GetPagedProducts,
        GetRecentOrdersWithItems,
        InsertProduct,
        UpdateProductPrice
    }
}
