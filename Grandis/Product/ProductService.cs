using Grandis.Data.Entities;
using Grandis.Data.Interfaces;
using Grandis.Product.Dto;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Product
{
    public class ProductService
    {
        private readonly IServiceProvider _serviceProvider;

        public ProductService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(DataProviderType provider, CancellationToken ct = default)
        {
            var entities = await Repository(provider).GetAllAsync(ct);
            return entities.Select(Map).ToList();
        }

        private IProductRepository Repository(DataProviderType provider)
        {
            return _serviceProvider.GetRequiredKeyedService<IProductRepository>(provider)
                    ?? throw new NotSupportedException($"Provider {provider} not supported.");
        }

        public async Task<ProductDto?> GetProductAsync(DataProviderType provider, long id, CancellationToken ct = default)
        {
            var entity = await Repository(provider).GetByIdAsync(id, ct);
            return entity is null ? null : Map(entity);
        }

        public async Task<PagedProductsDto> GetPagedAsync(DataProviderType provider, ProductQuery query, CancellationToken ct = default)
        {
            var page = await Repository(provider).GetPagedAsync(query, ct);
            return new PagedProductsDto
            {
                Items = page.Items.Select(Map).ToList(),
                TotalCount = page.TotalCount,
                Page = page.Page,
                PageSize = page.PageSize
            };
        }

        public Task<long> CreateAsync(DataProviderType provider, CreateProductRequest request, CancellationToken ct = default)
        {
            var product = new Data.Entities.Product
            {
                CreatedAt = DateTime.UtcNow,
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                SupplierId = request.SupplierId,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                IsActive = request.IsActive
            };
            return Repository(provider).InsertAsync(product, ct);
        }

        public Task<int> UpdatePriceAsync(DataProviderType provider, long id, decimal price, CancellationToken ct = default)
        { 
            return Repository(provider).UpdatePriceAsync(id, price, ct);
        }

        private static ProductDto Map(Data.Entities.Product p) => new()
        {
            Id = p.Id,
            CreatedAt = p.CreatedAt,
            Name = p.Name,
            Description = p.Description,
            CategoryId = p.CategoryId,
            SupplierId = p.SupplierId,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            IsActive = p.IsActive
        };
    }
}
