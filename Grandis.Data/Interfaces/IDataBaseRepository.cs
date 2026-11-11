using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Data.Interfaces
{
    public interface IDataBaseRepository
    {
        DataProviderType ProviderType { get; }

        Task ReSeedDataBase(CancellationToken ct = default);

        Task<Dictionary<string, long>> GetRowCountsAsync(CancellationToken ct = default);
    }
}
