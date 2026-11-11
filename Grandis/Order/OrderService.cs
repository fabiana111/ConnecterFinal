using Grandis.Data;
using Grandis.Data.Interfaces;
using Grandis.Order.Dto;
using Grandis.Helper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Order
{
    public class OrderService
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IReadOnlyList<OrderDto>> GetRecentOrdersAsync(DataProviderType provider, int count, CancellationToken ct = default)
        {
            var orders = await Repository(provider).GetRecentOrdersWithItemsAsync(count, ct);
            return orders.Select(Map).ToList();
        }

        private IOrderRepository Repository(DataProviderType provider)
        {
            return _serviceProvider.GetRequiredKeyedService<IOrderRepository>(provider)
                ?? throw new NotSupportedException($"Provider {provider} not supported.");
        }

        private static OrderDto Map(Data.Entities.Order o) => new()
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            Status = ((OrderStatus)o.StatusId).GetDescription(),
            TotalAmount = o.TotalAmount,
            Customer = o.Customer is null ? null : new OrderCustomerDto
            {
                Id = o.Customer.Id,
                FullName = $"{o.Customer.LastName} {o.Customer.FirstName}",
                Email = o.Customer.Email
            },
            Items = o.OrderItems.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.Quantity * i.UnitPrice
            }).ToList()
        };
    }
}
