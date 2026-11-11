using Grandis.Data.Entities;
using Grandis.Data.Interfaces;
using Grandis.Order;
using Grandis.Product;
using Grandis.Product.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Grandis.WebService.Controllers
{
    [ApiController]
    public abstract class ProductControllerBase : Controller
    {
        protected abstract DataProviderType Provider { get; }

        private readonly ProductService productService;

        public ProductControllerBase(
            ProductService productService)
        {
            this.productService = productService;

        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken ct)
        { 
            return Ok(await productService.GetProductsAsync(Provider, ct));
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ProductDto>> GetById(long id, CancellationToken ct)
        {
            var product = await productService.GetProductAsync(Provider, id, ct);
            return product is null ? NotFound() : Ok(product);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedProductsDto>> GetPaged(
            [FromQuery] long? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var query = new ProductQuery
            {
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                IsActive = isActive,
                Page = Math.Max(1, page),
                PageSize = Math.Clamp(pageSize, 1, 500)
            };
            return Ok(await productService.GetPagedAsync(Provider, query, ct));
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
        {
            var id = await productService.CreateAsync(Provider, request, ct);
            return Created($"{Request.Path}/{id}", new { id });
        }

        [HttpPut("{id:long}/price")]
        public async Task<ActionResult> UpdatePrice(long id, [FromBody] UpdatePriceRequest request, CancellationToken ct)
        {
            var affected = await productService.UpdatePriceAsync(Provider, id, request.Price, ct);
            return affected == 0 ? NotFound() : NoContent();
        }
    }
}
