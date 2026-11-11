using Grandis.Data.Interfaces;
using Grandis.Order;
using Grandis.Product;
using Microsoft.AspNetCore.Mvc;

namespace Grandis.WebService.Controllers
{
    [Route("api/dapper/products")]
    public sealed class DapperProductsController(ProductService products) : ProductControllerBase(products)
    {
        protected override DataProviderType Provider => DataProviderType.Dapper;
    }

    [Route("api/dapper/orders")]
    public sealed class DapperOrdersController(OrderService orders) : OrderControllerBase(orders)
    {
        protected override DataProviderType Provider => DataProviderType.Dapper;
    }
}
