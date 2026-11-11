using Grandis.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Banchmark.Dto
{
    //Egyetlen provider futtatása
    public class BenchmarkRunRequest : BenchmarkRequest
    {
        public DataProviderType Provider { get; set; }
        public bool EfCoreTracking { get; set; }
    }
}
