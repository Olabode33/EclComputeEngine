using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class CalibrationResult_PD_CummulativeDefaultRate
    {
        public string Key { get; set; }
        public string Rating { get; set; }
        public int? Years { get; set; }
        public double? Value { get; set; }
    }
}
