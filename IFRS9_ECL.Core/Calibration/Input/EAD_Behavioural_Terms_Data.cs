using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class EAD_Behavioural_Terms_Data: BaseObject
    {
        public string Customer_No { get; set; }
        public string Account_No { get; set; }
        public string Contract_No { get; set; }
        public string Customer_Name { get; set; }
        public DateTime Snapshot_Date { get; set; }
        public string Classification { get; set; }
        public double Original_Balance_Lcy { get; set; }
        public double Outstanding_Balance_Lcy { get; set; }
        public double Outstanding_Balance_Acy { get; set; }
        public DateTime? Contract_Start_Date { get; set; }
        public DateTime? Contract_End_Date { get; set; }
        public int Restructure_Indicator { get; set; }
        public string Restructure_Type { get; set; }
        public DateTime? Restructure_Start_Date { get; set; }
        public DateTime? Restructure_End_Date { get; set; }
        public int BatchId { get; set; }
    }
}
