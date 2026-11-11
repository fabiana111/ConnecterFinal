using Grandis.Data.Entities;
using Grandis.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Repositories.EfCore
{
    public class EfCoreProductRepository : IProductRepository
    {
        private readonly IDbContextFactory<GrandisDbContext> _context;

        public DataProviderType ProviderType => DataProviderType.EfCore;
        public bool UseTracking { get; set; }

        public EfCoreProductRepository(IDbContextFactory<GrandisDbContext> context)
        { 
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
        {
            await using var ctx = await _context.CreateDbContextAsync(ct);
            return await Query(ctx).ToListAsync(ct);
        }

        public async Task<Product?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            await using var ctx = await _context.CreateDbContextAsync(ct);
            return await Query(ctx).FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<PagedResult<Product>> GetPagedAsync(ProductQuery query, CancellationToken ct = default)
        {
            await using var ctx = await _context.CreateDbContextAsync(ct);

            var filtered = Query(ctx);
            if (query.CategoryId is not null) filtered = filtered.Where(p => p.CategoryId == query.CategoryId);
            if (query.MinPrice is not null) filtered = filtered.Where(p => p.Price >= query.MinPrice);
            if (query.MaxPrice is not null) filtered = filtered.Where(p => p.Price <= query.MaxPrice);
            if (query.IsActive is not null) filtered = filtered.Where(p => p.IsActive == query.IsActive);

            // EF-idiomatikus lapozás: külön Count + lap-lekérdezés (két adatbázis-kör).
            var total = await filtered.CountAsync(ct);
            var items = await filtered
                .OrderBy(p => p.Id)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(ct);

            return new PagedResult<Product>
            {
                Items = items,
                TotalCount = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<long> InsertAsync(Product product, CancellationToken ct = default)
        {
            await using var ctx = await _context.CreateDbContextAsync(ct);
            ctx.Products.Add(product);
            await ctx.SaveChangesAsync(ct);
            return product.Id;
        }

        public async Task<int> UpdatePriceAsync(long id, decimal price, CancellationToken ct = default)
        {
            await using var ctx = await _context.CreateDbContextAsync(ct);
            // ExecuteUpdate: egyetlen UPDATE utasítás, szemantikailag azonos a másik két
            // implementációval (a klasszikus betölt+módosít+SaveChanges két kört jelentene).
            return await ctx.Products
                .Where(p => p.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Price, price), ct);
        }

        private IQueryable<Product> Query(GrandisDbContext ctx) =>
            UseTracking ? ctx.Products : ctx.Products.AsNoTracking();
    }
}
