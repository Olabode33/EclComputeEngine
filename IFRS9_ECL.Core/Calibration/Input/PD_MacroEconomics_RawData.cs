using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class PD_MacroEconomics_RawData: BaseObject
    {
        public string Date { get; set; }
        public string InterBank_Fx { get; set; }
        public int MacroVariableId { get; set; }
        public double MacroEconomicVariableId { get; set; }
        public int BatchId { get; set; }
    }
}
