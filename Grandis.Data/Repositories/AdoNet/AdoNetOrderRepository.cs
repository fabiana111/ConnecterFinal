using Grandis.Data.Db;
using Grandis.Data.Entities;
using Grandis.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Repositories.AdoNet
{
    public class AdoNetOrderRepository : IOrderRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DataProviderType ProviderType => DataProviderType.AdoNet;

        private const string Sql = """
        SELECT o.[Id], o.[OrderDate], o.[CustomerId], o.[StatusId], o.[TotalAmount],
               c.[Id], c.[CreditedAt], c.[FirstName], c.[LastName], c.[Email], c.[PhoneNumber], c.[AddresseeId],
               oi.[Id], oi.[OrderId], oi.[ProductId], oi.[Quantity], oi.[UnitPrice],
               p.[Id], p.[CreatedAt], p.[Name], p.[Description], p.[CategoryId], p.[SupplierId], p.[Price], p.[StockQuantity], p.[IsActive]
        FROM (SELECT TOP (@Count) [Id], [OrderDate], [CustomerId], [StatusId], [TotalAmount]
              FROM [dbo].[Order]
              ORDER BY [OrderDate] DESC, [Id] DESC) o
        INNER JOIN [dbo].[Customer] c ON c.[Id] = o.[CustomerId]
        INNER JOIN [dbo].[OrderItem] oi ON oi.[OrderId] = o.[Id]
        INNER JOIN [dbo].[Product] p ON p.[Id] = oi.[ProductId]
        ORDER BY o.[OrderDate] DESC, o.[Id] DESC;
        """;

        public AdoNetOrderRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersWithItemsAsync(int count, CancellationToken ct = default)
        {
            var orders = new Dictionary<long, Order>();

            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = Sql;
            cmd.Parameters.AddWithValue("@Count", count);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var orderId = reader.GetInt64(0);
                if (!orders.TryGetValue(orderId, out var order))
                {
                    order = new Order
                    {
                        Id = orderId,
                        OrderDate = reader.GetDateTime(1),
                        CustomerId = reader.GetInt64(2),
                        StatusId = reader.GetInt32(3),
                        TotalAmount = reader.GetDecimal(4),
                        Customer = new Customer
                        {
                            Id = reader.GetInt64(5),
                            CreditedAt = reader.GetDateTime(6),
                            FirstName = reader.GetString(7),
                            LastName = reader.GetString(8),
                            Email = reader.GetString(9),
                            PhoneNumber = reader.IsDBNull(10) ? null : reader.GetString(10),
                            AddresseeId = reader.GetInt64(11)
                        }
                    };
                    orders.Add(orderId, order);
                }

                order.OrderItems.Add(new OrderItem
                {
                    Id = reader.GetInt64(12),
                    OrderId = reader.GetInt64(13),
                    ProductId = reader.GetInt64(14),
                    Quantity = reader.GetInt32(15),
                    UnitPrice = reader.GetDecimal(16),
                    Product = new Product
                    {
                        Id = reader.GetInt64(17),
                        CreatedAt = reader.GetDateTime(18),
                        Name = reader.GetString(19),
                        Description = reader.IsDBNull(20) ? null : reader.GetString(20),
                        CategoryId = reader.GetInt64(21),
                        SupplierId = reader.GetInt64(22),
                        Price = reader.GetDecimal(23),
                        StockQuantity = reader.GetInt32(24),
                        IsActive = reader.GetBoolean(25)
                    }
                });
            }

            return orders.Values.ToList();
        }
    }
}
