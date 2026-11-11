using Grandis.Banchmark.Dto;
using Grandis.Benchmarks;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/benchmark")]
public class BenchmarkController(BenchmarkService benchmark) : ControllerBase
{
    /// <summary>A felület számára: a futtatható szcenáriók listája.</summary>
    [HttpGet("scenarios")]
    public ActionResult<IReadOnlyList<ScenarioInfo>> GetScenarios()
    {
        return Ok(BenchmarkService.Scenarios);
    }

    /// <summary>Egy szcenárió futtatása egyetlen providerrel.</summary>
    [HttpPost("run")]
    public async Task<ActionResult<ProviderBenchmarkResult>> Run(
        [FromBody] BenchmarkRunRequest request, CancellationToken ct)
    { 
        return Ok(await benchmark.RunAsync(request, ct));
    }

    /// <summary>Egy szcenárió futtatása mindhárom providerrel, összehasonlító eredménnyel.</summary>
    [HttpPost("compare")]
    public async Task<ActionResult<BenchmarkCompareResult>> Compare(
        [FromBody] BenchmarkRequest request, CancellationToken ct)
    { 
        return Ok(await benchmark.CompareAsync(request, ct));
    }
}
