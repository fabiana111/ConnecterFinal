using Dapper;
using Grandis.Data.Db;
using Grandis.Data.Entities;
using Grandis.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Repositories.Dapper
{
    public class DapperOrderRepository : IOrderRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        // Ugyanaz a join-forma, mint az ADO.NET és EF Core implementációban; a StatusId
        // oszlop Status néven aliasolva, hogy a Dapper az enum property-re tudja mappelni.
        private const string Sql = """
        SELECT o.[Id], o.[OrderDate], o.[CustomerId], o.[StatusId] AS [Status], o.[TotalAmount],
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

        public DataProviderType ProviderType => DataProviderType.Dapper;

        public DapperOrderRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersWithItemsAsync(int count, CancellationToken ct = default)
        {
            var orders = new Dictionary<long, Order>();

            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync(ct);
            await conn.QueryAsync<Order, Customer, OrderItem, Product, Order>(
                new CommandDefinition(Sql, new { Count = count }, cancellationToken: ct),
                (order, customer, item, product) =>
                {
                    if (!orders.TryGetValue(order.Id, out var existing))
                    {
                        existing = order;
                        existing.Customer = customer;
                        orders.Add(order.Id, existing);
                    }
                    item.Product = product;
                    existing.OrderItems.Add(item);
                    return existing;
                },
                splitOn: "Id,Id,Id");

            return orders.Values.ToList();
        }
    }
}
