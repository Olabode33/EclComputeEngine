using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class CalibrationResult_PD_CommsCons_MarginalDefaultRate
    {
        public int Month { get; set; }
        public double? Comm1 { get; set; }
        public double? Cons1 { get; set; }
        public double? Comm2 { get; set; }
        public double? Cons2 { get; set; }
        public string Comment { get; set; }
        public int? Status { get; set; }
        public DateTime? DateCreated { get; set; }
        public Guid? CalibrationId { get; set; }
    }
}
