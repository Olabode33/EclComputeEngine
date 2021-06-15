using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration
{
    public class CalibrationResult_EAD_Behavioural
    {
        public double Expired { get; set; }
        public double NonExpired { get; set; }
        public double FrequencyExpired { get; set; }
        public double FrequencyNonExpired { get; set; }
    }
}
