using Grandis.Data.Interfaces;
using Grandis.Order;
using Grandis.Product;
using Microsoft.AspNetCore.Mvc;

namespace Grandis.WebService.Controllers
{
    [Route("api/adonet/products")]
    public class AdoNetController(ProductService products) : ProductControllerBase(products)
    {
        protected override DataProviderType Provider => DataProviderType.AdoNet;
    }

    [Route("api/adonet/orders")]
    public sealed class AdoNetOrdersController(OrderService orders) : OrderControllerBase(orders)
    {
        protected override DataProviderType Provider => DataProviderType.AdoNet;
    }
}
