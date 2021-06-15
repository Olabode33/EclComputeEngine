using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.PDComputation.cmPD
{
    public class VasicekEtiNplIndex
    {
        public DateTime Date { get; set; }
        public int Month { get; set; }
        public double EtiNpl { get; set; }
        public double Index { get; set; }
        public double Fitted { get; set; }
        public double Residuals { get; set; }
        public double ScenarioPd { get; set; }
        public double ScenarioIndex { get; set; }
        public double ScenarioFactor { get; set; }
    }
}
