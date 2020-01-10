using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class PD_Consumer: BaseObject
    {
        public string Customer_No { get; set; }
        public string Account_No { get; set; }
        public string Contract_No { get; set; }
        public int Current_Rating { get; set; }
        public int Days_Past_Due { get; set; }
        public string Classification { get; set; }
        public double Credit_Limit_Lcy { get; set; }
        public double Original_Balance_Lcy { get; set; }
        public double Outstanding_Balance_Lcy { get; set; }
        public double Contract_End_Date { get; set; }
        public int RAPPDate { get; set; }
        public ConsumerType ConsumerType { get; set; }
        public int BatchId { get; set; }
    }

    public enum ConsumerType
    {
        Consumer1=1,
        Consumer2,
        Consumer3
    }
}
