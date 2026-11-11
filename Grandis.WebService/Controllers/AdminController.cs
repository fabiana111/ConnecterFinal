using Grandis.Benchmarks;
using Grandis.Data.Db;
using Grandis.Product;
using Microsoft.AspNetCore.Mvc;

namespace Grandis.WebService.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly BenchmarkService benchmarkService;

    public AdminController(
        BenchmarkService benchmarkService)
    {
        this.benchmarkService = benchmarkService;

    }

    [HttpGet("status")]
    public async Task<ActionResult> GetStatus(CancellationToken ct)
    {
        var counts = await benchmarkService.GetRowCountsAsync(Data.Interfaces.DataProviderType.Dapper, ct);
        return Ok(new
        {
            database = "Grandis.TX",
            rowCounts = counts
        });
    }

    /// <summary>Minden adat törlése és újragenerálása a Seed beállítások szerint.Hosszú művelet!</summary>
    [HttpPost("reseed")]
    public async Task<ActionResult> Reseed(CancellationToken ct)
    {
        await benchmarkService.ReSeedDataBase(Data.Interfaces.DataProviderType.Dapper);
        var counts = await benchmarkService.GetRowCountsAsync(Data.Interfaces.DataProviderType.Dapper, ct);
        return Ok(new { message = "Újratöltés kész.", rowCounts = counts });
    }
}