using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Banchmark.Dto
{
    public class BenchmarkCompareResult
    {
        public string Scenario { get; set; } = "";
        public int Iterations { get; set; }
        public int Warmup { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public double TotalElapsedMs { get; set; }
        public IReadOnlyList<ProviderBenchmarkResult> Results { get; set; } = [];

        //Igaz, ha minden provider azonos sorszámot adott vissza
        public bool RowCountsMatch { get; set; }

        public string? Note { get; set; }
    }
}
