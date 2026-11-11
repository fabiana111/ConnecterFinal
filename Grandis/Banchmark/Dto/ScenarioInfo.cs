using System;
using System.Collections.Generic;
using System.Text;

namespace Grandis.Banchmark.Dto
{
    public class ScenarioInfo
    {
        public string Key { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool SupportsTracking { get; set; }
    }
}
