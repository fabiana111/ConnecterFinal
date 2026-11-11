using Grandis.Data.Db;
using Grandis.Data.Entities;
using Grandis.Data.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Repositories.AdoNet
{
    public class AdoNetProductRepository : IProductRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DataProviderType ProviderType => DataProviderType.AdoNet;

        private const string Columns = "[Id], [CreatedAt], [Name], [Description], [CategoryId], [SupplierId], [Price], [StockQuantity], [IsActive]";

        public AdoNetProductRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
        {
            var result = new List<Product>();
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT {Columns} FROM [dbo].[Product];";
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(Map(reader));
            }
            return result;
        }

        public async Task<Product?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT {Columns} FROM [dbo].[Product] WHERE [Id] = @Id;";
            cmd.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.BigInt) { Value = id });
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? Map(reader) : null;
        }

        public async Task<PagedResult<Product>> GetPagedAsync(ProductQuery query, CancellationToken ct = default)
        {
            var conditions = new List<string>();
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();

            if (query.CategoryId is not null)
            {
                conditions.Add("[CategoryId] = @CategoryId");
                cmd.Parameters.AddWithValue("@CategoryId", query.CategoryId.Value);
            }
            if (query.MinPrice is not null)
            {
                conditions.Add("[Price] >= @MinPrice");
                cmd.Parameters.AddWithValue("@MinPrice", query.MinPrice.Value);
            }
            if (query.MaxPrice is not null)
            {
                conditions.Add("[Price] <= @MaxPrice");
                cmd.Parameters.AddWithValue("@MaxPrice", query.MaxPrice.Value);
            }
            if (query.IsActive is not null)
            {
                conditions.Add("[IsActive] = @IsActive");
                cmd.Parameters.AddWithValue("@IsActive", query.IsActive.Value);
            }

            var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
            cmd.Parameters.AddWithValue("@Offset", (query.Page - 1) * query.PageSize);
            cmd.Parameters.AddWithValue("@PageSize", query.PageSize);

            // Egy körben (egy batch, két eredményhalmaz): összdarabszám + aktuális lap.
            cmd.CommandText = $"""
            SELECT COUNT(*) FROM [dbo].[Product] {where};
            SELECT {Columns} FROM [dbo].[Product] {where}
            ORDER BY [Id]
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            await reader.ReadAsync(ct);
            var total = reader.GetInt32(0);
            await reader.NextResultAsync(ct);

            var items = new List<Product>();
            while (await reader.ReadAsync(ct))
            {
                items.Add(Map(reader));
            }

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
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
            INSERT INTO [dbo].[Product] ([CreatedAt], [Name], [Description], [CategoryId], [SupplierId], [Price], [StockQuantity], [IsActive])
            OUTPUT INSERTED.[Id]
            VALUES (@CreatedAt, @Name, @Description, @CategoryId, @SupplierId, @Price, @StockQuantity, @IsActive);
            """;
            cmd.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
            cmd.Parameters.AddWithValue("@Name", product.Name);
            cmd.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
            cmd.Parameters.AddWithValue("@SupplierId", product.SupplierId);
            cmd.Parameters.AddWithValue("@Price", product.Price);
            cmd.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
            cmd.Parameters.AddWithValue("@IsActive", product.IsActive);
            var id = (long)(await cmd.ExecuteScalarAsync(ct))!;
            product.Id = id;
            return id;
        }

        public async Task<int> UpdatePriceAsync(long id, decimal price, CancellationToken ct = default)
        {
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE [dbo].[Product] SET [Price] = @Price WHERE [Id] = @Id;";
            cmd.Parameters.AddWithValue("@Price", price);
            cmd.Parameters.AddWithValue("@Id", id);
            return await cmd.ExecuteNonQueryAsync(ct);
        }

        private static Product Map(SqlDataReader reader) => new()
        {
            Id = reader.GetInt64(0),
            CreatedAt = reader.GetDateTime(1),
            Name = reader.GetString(2),
            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
            CategoryId = reader.GetInt64(4),
            SupplierId = reader.GetInt64(5),
            Price = reader.GetDecimal(6),
            StockQuantity = reader.GetInt32(7),
            IsActive = reader.GetBoolean(8)
        };
    }
}
