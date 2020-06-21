using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Macro.Entities
{
    public class MacroResult_Statistics
    {
        public int Id { get; set; }
        public double? IndexWeight1 { get; set; }
        public double? IndexWeight2 { get; set; }
        public double? IndexWeight3 { get; set; }
        public double? IndexWeight4 { get; set; }
        public double? StandardDev { get; set; }
        public double? Average { get; set; }
        public double? Correlation { get; set; }
        public double? TTC_PD { get; set; }

        public int MacroId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
