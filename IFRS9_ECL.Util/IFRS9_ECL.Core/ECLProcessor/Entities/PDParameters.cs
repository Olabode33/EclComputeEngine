using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.ECLProcessor.Entities
{
    public class PDParameters
    {
        public string BasePath { get; set; }
        public string ModelFileName { get; set; }
        public string LoanBookFileName { get; set; }
        public double RedefaultAdjustmentFactor { get; set; }
        public string SandPMapping { get; set; }
        public double Expired { get; set; }
        public double NonExpired { get; set; }
        public DateTime ReportDate { get; set; }
    }
}
