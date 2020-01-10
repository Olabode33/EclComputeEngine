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
        public string Contract_No { get; set; }
        public string Customer_Name { get; set; }
        public DateTime Snapshot_Date { get; set; }
        public string Product_Type { get; set; }
        public double Outstanding_Balance_Lcy { get; set; }
        public string Correct_Segment { get; set; }
        public int Tracker { get; set; }
        public double Limit { get; set; }
        public CCFs_Month_Type CCFs_Month_Type { get; set; }
        public int BatchId { get; set; }
    }

    public enum CCFs_Month_Type
    {
        ThreeMonths,
        SixMonths,
        NineMonths,
        TwelveMonths
    }
}
