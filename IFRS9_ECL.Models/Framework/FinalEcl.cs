using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.Framework
{
    public class FinalEcl
    {
        public string ContractId { get; set; }
        public int EclMonth { get; set; }
        public double MonthlyEclValue { get; set; }
        public double FinalEclValue { get; set; }
        public int Stage { get; set; }
        public int eCL_Scenario { get; set; }
    }
}
