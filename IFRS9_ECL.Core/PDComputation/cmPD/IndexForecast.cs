using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.PDComputation.cmPD
{
    public class IndexForecast
    {
        public DateTime Date { get; set; }
        public int MacroEconomicVariableId { get; set; }
        public double MacroEconomicValue { get; set; }
        public double Principal1 { get; set; }
        public double Principal2 { get; set; }
        public double Principal3 { get; set; }
        public double Principal4 { get; set; }
        public double Actual { get; set; }
        public double Standardised { get; set; }
    }
}
