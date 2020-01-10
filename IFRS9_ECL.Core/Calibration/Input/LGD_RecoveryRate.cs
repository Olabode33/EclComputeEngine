using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class LGD_RecoveryRate: BaseObject
    {
        public string Contract_No { get; set; }
        public string Product_Type { get; set; }
        public int Date { get; set; }
        public string Classification { get; set; }
        public double Balance_at_Start { get; set; }
        public double Total_Recoveries { get; set; }
        public double Cashflow_Recoveries { get; set; }
        public double Collateral_Recoveries { get; set; }
        public double EIR { get; set; }
        public int BatchId { get; set; }
    }
}
