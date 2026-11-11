using Grandis.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Grandis.Data.Repositories.EfCore
{
    public class EfCoreRepository : IDataBaseRepository
    {
        private readonly IDbContextFactory<GrandisDbContext> _context;

        public DataProviderType ProviderType => DataProviderType.EfCore;

        public EfCoreRepository(IDbContextFactory<GrandisDbContext> context)
        {
            _context = context;
        }

        public async Task ReSeedDataBase(CancellationToken ct = default)
        {
            await using GrandisDbContext ctx = await _context.CreateDbContextAsync(ct);

            ctx.Database.SetCommandTimeout(600);

            await ctx.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC dbo.SeedDatabase @Reset = {true}", ct);
        }

        public async Task<Dictionary<string, long>> GetRowCountsAsync(CancellationToken ct = default)
        {
            await using GrandisDbContext ctx = await _context.CreateDbContextAsync(ct);

            return new Dictionary<string, long>
            {
                ["Addressee"] = await ctx.Addressees.LongCountAsync(ct),
                ["Category"] = await ctx.Categories.LongCountAsync(ct),
                ["Supplier"] = await ctx.Suppliers.LongCountAsync(ct),
                ["Customer"] = await ctx.Customers.LongCountAsync(ct),
                ["Product"] = await ctx.Products.LongCountAsync(ct),
                ["Order"] = await ctx.Orders.LongCountAsync(ct),
                ["OrderItem"] = await ctx.OrderItems.LongCountAsync(ct)
            };
        }
    }
}
