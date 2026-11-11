using Grandis.Data.Interfaces;
using Grandis.Order;
using Grandis.Product;
using Microsoft.AspNetCore.Mvc;

namespace Grandis.WebService.Controllers
{
    [Route("api/efcore/products")]
    public sealed class EfCoreProductsController(ProductService products) : ProductControllerBase(products)
    {
        protected override DataProviderType Provider => DataProviderType.EfCore;
    }

    [Route("api/efcore/orders")]
    public sealed class EfCoreOrdersController(OrderService orders) : OrderControllerBase(orders)
    {
        protected override DataProviderType Provider => DataProviderType.EfCore;
    }
}
