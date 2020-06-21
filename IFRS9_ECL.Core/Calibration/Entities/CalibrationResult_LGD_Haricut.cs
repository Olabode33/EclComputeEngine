using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class CalibrationResult_LGD_HairCut
    {
        public DateTime Period { get; set; }
        public double Debenture { get; set; }
        public double Cash { get; set; }
        public double Inventory { get; set; }
        public double Plant_And_Equipment { get; set; }
        public double Residential_Property { get; set; }
        public double Commercial_Property { get; set; }
        public double Receivables { get; set; }
        public double Shares { get; set; }
        public double Vehicle { get; set; }
        public string CalibrationId { get; set; }
    }
}
