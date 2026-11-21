using Grandis.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Interfaces
{
    public interface IOrderRepository
    {
        DataProviderType ProviderType { get; }

        Task<IEnumerable<Order>> GetRecentOrdersWithItemsAsync(int count, CancellationToken ct = default);
    }
}
