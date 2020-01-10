using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class LGD_CureRates: BaseObject
    {
        public string Customer_No { get; set; }
        public string Account_No { get; set; }
        public string Contract_No { get; set; }
        public string Segment { get; set; }
        public string Product_Type { get; set; }
        public DateTime Snapshot_Date { get; set; }
        public int Days_Past_Due { get; set; }
        public string Classification { get; set; }
        public double Outstanding_Balance_Lcy { get; set; }
        public int BatchId { get; set; }

    }

}
