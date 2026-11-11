using Grandis.Data.Entities;
using Grandis.Data.Interfaces;
using Grandis.Order;
using Grandis.Order.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Grandis.WebService.Controllers
{
    [ApiController]
    public abstract class OrderControllerBase : Controller
    {
        protected abstract DataProviderType Provider { get; }

        private readonly OrderService orderService;

        public OrderControllerBase(
            OrderService orderService)
        {
            this.orderService = orderService;
            
        }

        [HttpGet("recent")]
        public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetRecent([FromQuery] int count = 100, CancellationToken ct = default)
        { 
            return Ok(await orderService.GetRecentOrdersAsync(Provider, Math.Clamp(count, 1, 5000), ct));
        }
    }
}
