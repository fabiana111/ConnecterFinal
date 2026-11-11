using Grandis.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Interfaces
{
    public interface IOrderRepository
    {
        DataProviderType ProviderType { get; }

        /// <summary>
        /// A legutóbbi <paramref name="count"/> rendelés a vevővel és a tételekkel
        /// (tételenként a termékkel) együtt. Mindhárom implementáció egyetlen,
        /// join-olt lekérdezéssel dolgozik, hogy a mérés összevethető legyen.
        /// </summary>
        Task<IEnumerable<Order>> GetRecentOrdersWithItemsAsync(int count, CancellationToken ct = default);
    }
}
