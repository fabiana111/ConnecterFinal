using Grandis.Data.Entities;
using Grandis.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Repositories.EfCore
{
    public class EfCoreOrderRepository : IOrderRepository
    {
        private readonly IDbContextFactory<GrandisDbContext> _context;

        public DataProviderType ProviderType => DataProviderType.EfCore;
        public bool UseTracking { get; set; }

        public EfCoreOrderRepository(IDbContextFactory<GrandisDbContext> context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersWithItemsAsync(int count, CancellationToken ct = default)
        {
            await using var ctx = await _context.CreateDbContextAsync(ct);

            // Include-os, egy lekérdezéses (single query) gráf-betöltés — ugyanaz a join-forma,
            // mint az ADO.NET és Dapper implementációban.
            IQueryable<Order> query = ctx.Orders
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.Id)
                .Take(count)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product);

            if (!UseTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync(ct);
        }
    }
}
