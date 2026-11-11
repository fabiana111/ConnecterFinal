using Grandis.Banchmark.Dto;
using Grandis.Data.Db;
using Grandis.Data.Entities;
using Grandis.Data.Interfaces;
using Grandis.Data.Repositories.EfCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Grandis.Benchmarks;

/// <summary>
/// A mérési motor: egy szcenáriót futtat egy vagy mindhárom adatelérési technológiával.
/// A korrekt összehasonlítás érdekében:
///  - minden provider ugyanazt az előre generált bemenet-sorozatot kapja (Id-k, árak),
///  - minden sorozat előtt warmup-iterációk futnak (JIT, terv-cache, buffer pool),
///  - a sorozatok között GC fut, hogy az előző provider allokációi ne torzítsanak,
///  - repository-szinten mérünk, így a DTO-mapping költsége nem szerepel az időkben.
/// </summary>
public class BenchmarkService
{
    private readonly IServiceProvider _serviceProvider;
    IDbConnectionFactory connectionFactory;

    private const string BenchProductPrefix = "BENCH_";
    private const int InputRandomSeed = 1337;

    public BenchmarkService(IServiceProvider serviceProvider, IDbConnectionFactory connectionFactory)
    {
        _serviceProvider = serviceProvider;
        this.connectionFactory = connectionFactory;
    }

    public static IReadOnlyList<ScenarioInfo> Scenarios { get; } =
    [
        new() { Key = nameof(BenchmarkScenario.GetAllProducts), Name = "Összes termék (GetAll)",
                Description = "A teljes Product tábla beolvasása és entitás-listává alakítása.", SupportsTracking = true },
        new() { Key = nameof(BenchmarkScenario.GetProductById), Name = "Termék Id alapján",
                Description = "Egyetlen sor lekérése kulcs alapján, iterációnként más-más (előre sorsolt) Id-val.", SupportsTracking = true },
        new() { Key = nameof(BenchmarkScenario.GetPagedProducts), Name = "Szűrt + lapozott lista",
                Description = "WHERE + ORDER BY + OFFSET/FETCH lekérdezés összdarabszámmal.", SupportsTracking = true },
        new() { Key = nameof(BenchmarkScenario.GetRecentOrdersWithItems), Name = "Rendelések tételekkel (join)",
                Description = "A legutóbbi N rendelés vevővel, tételekkel és termékekkel — objektumgráf felépítése.", SupportsTracking = true },
        new() { Key = nameof(BenchmarkScenario.InsertProduct), Name = "Termék beszúrása",
                Description = "Iterációnként egy új sor beszúrása (a mérés után automatikus takarítás).", SupportsTracking = false },
        new() { Key = nameof(BenchmarkScenario.UpdateProductPrice), Name = "Ár módosítása",
                Description = "Egyetlen sor UPDATE kulcs alapján, iterációnként más-más sorra.", SupportsTracking = false }
    ];

    public async Task ReSeedDataBase(DataProviderType provider)
    {
        var databaseRepository = _serviceProvider.GetRequiredKeyedService<IDataBaseRepository>(provider)
            ?? throw new NotSupportedException($"Provider {provider} not supported.");

        await databaseRepository.ReSeedDataBase();
    }

    public async Task<Dictionary<string, long>> GetRowCountsAsync(DataProviderType provider, CancellationToken ct = default)
    {
        var databaseRepository = _serviceProvider.GetRequiredKeyedService<IDataBaseRepository>(provider)
            ?? throw new NotSupportedException($"Provider {provider} not supported.");

        return await databaseRepository.GetRowCountsAsync(ct);
    }

    public async Task<BenchmarkCompareResult> CompareAsync(BenchmarkRequest request, CancellationToken ct = default)
    {
        Clamp(request);
        var inputs = await PrepareInputsAsync(request, ct);

        var runs = new List<(DataProviderType Provider, bool Tracking)>
        {
            (DataProviderType.Dapper, false),
            (DataProviderType.AdoNet, false),            
            (DataProviderType.EfCore, false)
        };
        if (request.IncludeEfCoreTracking && IsReadScenario(request.Scenario))
        {
            runs.Add((DataProviderType.EfCore, true));
        }

        var startedAt = DateTime.UtcNow;
        var total = Stopwatch.StartNew();
        var results = new List<ProviderBenchmarkResult>();
        foreach (var (provider, tracking) in runs)
        {
            results.Add(await RunProviderAsync(request, provider, tracking, inputs, ct));
        }

        return new BenchmarkCompareResult
        {
            Scenario = request.Scenario.ToString(),
            Iterations = request.Iterations,
            Warmup = request.Warmup,
            StartedAtUtc = startedAt,
            TotalElapsedMs = total.Elapsed.TotalMilliseconds,
            Results = results,
            RowCountsMatch = results.Select(r => r.RowsReturned).Distinct().Count() == 1,
            Note = request.Scenario == BenchmarkScenario.InsertProduct
                ? "A beszúrt BENCH_ prefixű sorok a mérés után törlődtek."
                : null
        };
    }

    public async Task<ProviderBenchmarkResult> RunAsync(BenchmarkRunRequest request, CancellationToken ct = default)
    {
        Clamp(request);
        var inputs = await PrepareInputsAsync(request, ct);
        var tracking = request.EfCoreTracking
            && request.Provider == DataProviderType.EfCore
            && IsReadScenario(request.Scenario);
        return await RunProviderAsync(request, request.Provider, tracking, inputs, ct);
    }

    private async Task<ProviderBenchmarkResult> RunProviderAsync(
        BenchmarkRequest request, DataProviderType provider, bool tracking, BenchmarkInputs inputs, CancellationToken ct)
    {
        var productRepository = _serviceProvider.GetRequiredKeyedService<IProductRepository>(provider)
            ?? throw new NotSupportedException($"Provider {provider} not supported.");
        var orderRepository = _serviceProvider.GetRequiredKeyedService<IOrderRepository>(provider)
            ?? throw new NotSupportedException($"Provider {provider} not supported.");

        if (productRepository is EfCoreProductRepository efProducts)
        {
            efProducts.UseTracking = tracking;
        }
        if (orderRepository is EfCoreOrderRepository efOrders)
        {
            efOrders.UseTracking = tracking;
        }

        var operation = BuildOperation(request, provider, productRepository, orderRepository, inputs, ct);

        // Kiegyenlített indulás: az előző sorozat allokációi ne itt szemétgyűjtődjenek.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        for (var w = 0; w < request.Warmup; w++)
        {
            await operation(w);
        }

        long rowsReturned = 0;
        var timings = new double[request.Iterations];
        var sw = new Stopwatch();
        for (var i = 0; i < request.Iterations; i++)
        {
            sw.Restart();
            rowsReturned = await operation(request.Warmup + i);
            sw.Stop();
            timings[i] = sw.Elapsed.TotalMilliseconds;
        }

        if (request.Scenario == BenchmarkScenario.InsertProduct)
        {
            await CleanupBenchProductsAsync(ct);
        }

        return BuildResult(provider, tracking, timings, rowsReturned);
    }

    private Func<int, Task<long>> BuildOperation(
        BenchmarkRequest request, DataProviderType provider,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        BenchmarkInputs inputs, CancellationToken ct)
    {
        return request.Scenario switch
        {
            BenchmarkScenario.GetAllProducts => async _ =>
            {
                var products = await productRepository.GetAllAsync(ct);
                return products.LongCount();
            }
            ,
            BenchmarkScenario.GetProductById => async i =>
            {
                var product = await productRepository.GetByIdAsync(inputs.ProductIds[i], ct);
                return product is null ? 0 : 1;
            }
            ,
            BenchmarkScenario.GetPagedProducts => async _ =>
            {
                var page = await productRepository.GetPagedAsync(new ProductQuery
                {
                    IsActive = true,
                    MinPrice = 50,
                    MaxPrice = 500,
                    Page = request.Page,
                    PageSize = request.PageSize
                }, ct);
                return page.TotalCount;
            }
            ,
            BenchmarkScenario.GetRecentOrdersWithItems => async _ =>
            {
                var orders = await orderRepository.GetRecentOrdersWithItemsAsync(request.RecentOrdersCount, ct);
                return orders.Sum(o => (long)o.OrderItems.Count);
            }
            ,
            BenchmarkScenario.InsertProduct => async i =>
            {
                await productRepository.InsertAsync(new Data.Entities.Product
                {
                    CreatedAt = DateTime.UtcNow,
                    Name = $"{BenchProductPrefix}{provider}_{i}",
                    Description = "Benchmark közben beszúrt, automatikusan törlődő sor.",
                    CategoryId = inputs.ValidCategoryId,
                    SupplierId = inputs.ValidSupplierId,
                    Price = inputs.Prices[i],
                    StockQuantity = 1,
                    IsActive = false
                }, ct);
                return 1;
            }
            ,
            BenchmarkScenario.UpdateProductPrice => async i =>
                await productRepository.UpdatePriceAsync(inputs.ProductIds[i], inputs.Prices[i], ct),
            _ => throw new NotSupportedException($"Scenario {request.Scenario} not supported.")
        };
    }

    /// <summary>
    /// Providerektől független, bemenetek (cél-Id-k, árak) előállítása,
    /// hogy minden provider pontosan ugyanazokat a műveleteket hajtsa végre.
    /// </summary>
    private async Task<BenchmarkInputs> PrepareInputsAsync(BenchmarkRequest request, CancellationToken ct)
    {
        var inputs = new BenchmarkInputs();
        var totalCalls = request.Warmup + request.Iterations;
        var rnd = new Random(InputRandomSeed);

        using var conn = connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);

        if (request.Scenario is BenchmarkScenario.GetProductById or BenchmarkScenario.UpdateProductPrice)
        {
            var ids = new List<long>();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT [Id] FROM [dbo].[Product] WHERE [Name] NOT LIKE 'BENCH\_%' ESCAPE '\';";
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                ids.Add(reader.GetInt64(0));
            }
            if (ids.Count == 0)
            {
                throw new InvalidOperationException("A Product tábla üres — előbb töltsd fel seed-adatokkal (POST /api/admin/reseed).");
            }
            inputs.ProductIds = Enumerable.Range(0, totalCalls).Select(_ => ids[rnd.Next(ids.Count)]).ToArray();
        }

        if (request.Scenario is BenchmarkScenario.InsertProduct or BenchmarkScenario.UpdateProductPrice)
        {
            inputs.Prices = Enumerable.Range(0, totalCalls)
                .Select(_ => Math.Round((decimal)(rnd.NextDouble() * 999 + 1), 2)).ToArray();
        }

        if (request.Scenario == BenchmarkScenario.InsertProduct)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TOP (1) [Id] FROM [dbo].[Category]; ";
            inputs.ValidCategoryId = (long)(await cmd.ExecuteScalarAsync(ct)
                ?? throw new InvalidOperationException("Nincs kategória az adatbázisban — előbb futtass seedet."));
            cmd.CommandText = "SELECT TOP (1) [Id] FROM [dbo].[Supplier];";
            inputs.ValidSupplierId = (long)(await cmd.ExecuteScalarAsync(ct)
                ?? throw new InvalidOperationException("Nincs beszállító az adatbázisban — előbb futtass seedet."));
        }

        return inputs;
    }

    private async Task CleanupBenchProductsAsync(CancellationToken ct)
    {
        await using var conn = connectionFactory.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"DELETE FROM [dbo].[Product] WHERE [Name] LIKE 'BENCH\_%' ESCAPE '\';";
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static ProviderBenchmarkResult BuildResult(
        DataProviderType provider, bool tracking, double[] timings, long rowsReturned)
    {
        var sorted = timings.Order().ToArray();
        var n = sorted.Length;
        var mean = sorted.Average();
        var median = n % 2 == 1 ? sorted[n / 2] : (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
        var p95 = sorted[Math.Min(n - 1, (int)Math.Ceiling(0.95 * n) - 1)];
        var stdDev = Math.Sqrt(sorted.Sum(t => (t - mean) * (t - mean)) / n);

        return new ProviderBenchmarkResult
        {
            Provider = provider.ToString(),
            EfCoreTracking = tracking,
            Iterations = n,
            RowsReturned = rowsReturned,
            MinMs = sorted[0],
            MaxMs = sorted[^1],
            MeanMs = mean,
            MedianMs = median,
            P95Ms = p95,
            StdDevMs = stdDev,
            TotalMs = sorted.Sum(),
            TimingsMs = timings
        };
    }

    private static bool IsReadScenario(BenchmarkScenario scenario) => scenario
        is BenchmarkScenario.GetAllProducts
        or BenchmarkScenario.GetProductById
        or BenchmarkScenario.GetPagedProducts
        or BenchmarkScenario.GetRecentOrdersWithItems;

    private static void Clamp(BenchmarkRequest request)
    {
        request.Iterations = Math.Clamp(request.Iterations, 1, 500);
        request.Warmup = Math.Clamp(request.Warmup, 0, 50);
        request.RecentOrdersCount = Math.Clamp(request.RecentOrdersCount, 1, 5000);
        request.Page = Math.Max(1, request.Page);
        request.PageSize = Math.Clamp(request.PageSize, 1, 500);
    }

    private sealed class BenchmarkInputs
    {
        public long[] ProductIds = [];
        public decimal[] Prices = [];
        public long ValidCategoryId;
        public long ValidSupplierId;
    }
}
