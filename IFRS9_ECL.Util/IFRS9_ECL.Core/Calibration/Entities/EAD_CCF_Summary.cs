using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class EAD_CCF_Summary: BaseObject
    {
        public string Customer_No { get; set; }
        public string Account_No { get; set; }
        public string Settlement_Account { get; set; }
        public string Product_Type { get; set; }
        public int Snapshot_Date { get; set; }
        public DateTime? Contract_Start_Date { get; set; }
        public DateTime? Contract_End_Date { get; set; }
        public double Limit { get; set; }
        public double Outstanding_Balance { get; set; }
        public string Classification { get; set; }
        public Guid CalibrationId { get; set; }
    }

}
