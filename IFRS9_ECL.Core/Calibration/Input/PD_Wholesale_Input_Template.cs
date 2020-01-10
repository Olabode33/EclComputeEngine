using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class PD_Wholesale_Input_Template: BaseObject
    {
        public string Customer_No { get; set; }
        public string Account_No { get; set; }
        public string Contract_No { get; set; }
        public int Current_Rating { get; set; }
        public int Days_Past_Due { get; set; }
        public string Classification { get; set; }
        public double Outstanding_Balance_Lcy { get; set; }
        public double Contract_Start_Date { get; set; }
        public double Contract_End_Date { get; set; }
        public int RAPPDate { get; set; }
        public string Correct_Product_Type { get; set; }
        public string CS_IND { get; set; }
        public string PD_Group { get; set; }
        public int BatchId { get; set; }
    }
}
