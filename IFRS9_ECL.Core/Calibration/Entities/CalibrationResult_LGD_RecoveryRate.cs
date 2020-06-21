using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class CalibrationResult_LGD_RecoveryRate

    {

        public double Overall_Exposure_At_Default { get; set; }
        public double Overall_PvOfAmountReceived { get; set; }
        public double Overall_Count { get; set; }
        public double Overall_RecoveryRate { get; set; }

        public double Corporate_Exposure_At_Default { get; set; }
        public double Corporate_PvOfAmountReceived { get; set; }
        public double Corporate_Count { get; set; }
        public double Corporate_RecoveryRate { get; set; }


        public double Commercial_Exposure_At_Default { get; set; }
        public double Commercial_PvOfAmountReceived { get; set; }
        public double Commercial_Count { get; set; }
        public double Commercial_RecoveryRate { get; set; }


        public double Consumer_Exposure_At_Default { get; set; }
        public double Consumer_PvOfAmountReceived { get; set; }
        public double Consumer_Count { get; set; }
        public double Consumer_RecoveryRate { get; set; }

        public string Comment { get; set; }

        public int? Status { get; set; }

        public DateTime? DateCreated { get; set; }

        public Guid? CalibrationId { get; set; }
    }
}
