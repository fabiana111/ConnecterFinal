using Grandis.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Interfaces
{
    public interface IProductRepository
    {
        DataProviderType ProviderType { get; }

        Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default);

        Task<Product?> GetByIdAsync(long id, CancellationToken ct = default);

        Task<PagedResult<Product>> GetPagedAsync(ProductQuery query, CancellationToken ct = default);

        Task<long> InsertAsync(Product product, CancellationToken ct = default);

        Task<int> UpdatePriceAsync(long id, decimal price, CancellationToken ct = default);
    }
}
