using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Banchmark.Dto
{
    public class ProviderBenchmarkResult
    {
        public string Provider { get; set; } = "";
        public bool EfCoreTracking { get; set; }
        public int Iterations { get; set; }

        //Ellenőrző érték, a providereknek azonos adatot kell visszaadniuk
        public long RowsReturned { get; set; }

        public double MinMs { get; set; }
        public double MeanMs { get; set; }
        public double MedianMs { get; set; }
        public double P95Ms { get; set; }
        public double MaxMs { get; set; }
        public double StdDevMs { get; set; }
        public double TotalMs { get; set; }
        public IReadOnlyList<double> TimingsMs { get; set; } = [];
    }

}
