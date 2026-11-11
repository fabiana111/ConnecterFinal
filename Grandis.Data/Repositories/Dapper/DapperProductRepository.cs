using Dapper;
using Grandis.Data.Db;
using Grandis.Data.Entities;
using Grandis.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Grandis.Data.Repositories.Dapper
{
    public class DapperProductRepository : IProductRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DataProviderType ProviderType => DataProviderType.Dapper;

        private const string Columns = "[Id], [CreatedAt], [Name], [Description], [CategoryId], [SupplierId], [Price], [StockQuantity], [IsActive]";

        public DapperProductRepository(IDbConnectionFactory connectionFactory)
        { 
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
            SELECT Id, CreatedAt, Name, Description, CategoryId,
                   SupplierId, Price, StockQuantity, IsActive
            FROM Product";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<Product>(sql);
        }

        public async Task<Product?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            return await conn.QuerySingleOrDefaultAsync<Product>(
                new CommandDefinition($"SELECT {Columns} FROM [dbo].[Product] WHERE [Id] = @Id;",
                    new { Id = id }, cancellationToken: ct));
        }

        public async Task<PagedResult<Product>> GetPagedAsync(ProductQuery query, CancellationToken ct = default)
        {
            var conditions = new List<string>();
            if (query.CategoryId is not null) conditions.Add("[CategoryId] = @CategoryId");
            if (query.MinPrice is not null) conditions.Add("[Price] >= @MinPrice");
            if (query.MaxPrice is not null) conditions.Add("[Price] <= @MaxPrice");
            if (query.IsActive is not null) conditions.Add("[IsActive] = @IsActive");
            var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            var sql = $"""
            SELECT COUNT(*) FROM [dbo].[Product] {where};
            SELECT {Columns} FROM [dbo].[Product] {where}
            ORDER BY [Id]
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

            var args = new
            {
                query.CategoryId,
                query.MinPrice,
                query.MaxPrice,
                query.IsActive,
                Offset = (query.Page - 1) * query.PageSize,
                query.PageSize
            };

            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var multi = await conn.QueryMultipleAsync(new CommandDefinition(sql, args, cancellationToken: ct));
            var total = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<Product>()).ToList();

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
            const string sql = """
            INSERT INTO [dbo].[Product] ([CreatedAt], [Name], [Description], [CategoryId], [SupplierId], [Price], [StockQuantity], [IsActive])
            OUTPUT INSERTED.[Id]
            VALUES (@CreatedAt, @Name, @Description, @CategoryId, @SupplierId, @Price, @StockQuantity, @IsActive);
            """;
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            var id = await conn.ExecuteScalarAsync<long>(new CommandDefinition(sql, product, cancellationToken: ct));
            product.Id = id;
            return id;
        }

        public async Task<int> UpdatePriceAsync(long id, decimal price, CancellationToken ct = default)
        {
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            return await conn.ExecuteAsync(new CommandDefinition(
                "UPDATE [dbo].[Product] SET [Price] = @Price WHERE [Id] = @Id;",
                new { Id = id, Price = price }, cancellationToken: ct));
        }
    }
}
