using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Banchmark.Dto
{
    public class BenchmarkRequest
    {
        public BenchmarkScenario Scenario { get; set; }
        public int Iterations { get; set; } = 20;
        public int Warmup { get; set; } = 3;

        //Olvasó szcenárióknál az EF Core change trackinges változata is lefut külön sorozatként.
        public bool IncludeEfCoreTracking { get; set; }

        public int RecentOrdersCount { get; set; } = 100;
        public int Page { get; set; } = 5;
        public int PageSize { get; set; } = 50;
    }
}
