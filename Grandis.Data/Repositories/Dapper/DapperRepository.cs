using Dapper;
using Grandis.Data.Db;
using Grandis.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Grandis.Data.Repositories.Dapper
{
    public class DapperRepository : IDataBaseRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DataProviderType ProviderType => DataProviderType.Dapper;

        public DapperRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task ReSeedDataBase(CancellationToken ct = default)
        {
            await using var conn = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                "dbo.SeedDatabase",
                new { Reset = true },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 600,
                cancellationToken: ct);

            await conn.ExecuteAsync(command);
        }

        public async Task<Dictionary<string, long>> GetRowCountsAsync(CancellationToken ct = default)
        {
            var result = new Dictionary<string, long>();
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
            SELECT 'Addressee', COUNT_BIG(*) FROM dbo.Addressee
            UNION ALL SELECT 'Category', COUNT_BIG(*) FROM dbo.Category
            UNION ALL SELECT 'Supplier', COUNT_BIG(*) FROM dbo.Supplier
            UNION ALL SELECT 'Customer', COUNT_BIG(*) FROM dbo.Customer
            UNION ALL SELECT 'Product', COUNT_BIG(*) FROM dbo.Product
            UNION ALL SELECT 'Order', COUNT_BIG(*) FROM dbo.[Order]
            UNION ALL SELECT 'OrderItem', COUNT_BIG(*) FROM dbo.OrderItem
            """;
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result[reader.GetString(0)] = reader.GetInt64(1);
            }
            return result;
        }
    }
}
