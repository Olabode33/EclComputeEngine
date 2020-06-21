using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class Calibration_LGD_RecoveryRate:BaseObject
    {

        public string Customer_No { get; set; }

        public string Account_No { get; set; }

        public string Account_Name { get; set; }

        public string Contract_No { get; set; }

        public string Segment { get; set; }

        public string Product_Type { get; set; }

        public int? Days_Past_Due { get; set; }

        public string Classification { get; set; }

        public DateTime? Default_Date { get; set; }

        public double? Outstanding_Balance_Lcy { get; set; }

        public double? Contractual_Interest_Rate { get; set; }

        public double? Amount_Recovered { get; set; }

        public DateTime? Date_Of_Recovery { get; set; }

        public string Type_Of_Recovery { get; set; }

        public Guid? CalibrationId { get; set; }

    }

}
