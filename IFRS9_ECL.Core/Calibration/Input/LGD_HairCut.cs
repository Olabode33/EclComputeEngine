using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class LGD_HairCut: BaseObject
    {
        public string Customer_No { get; set; }
        public string Account_No { get; set; }
        public string Contract_No { get; set; }
        public DateTime Snapshot_Date { get; set; }
        public double Outstanding_Balance_Lcy { get; set; }
        public double Debenture_OMV { get; set; }
        public double Debenture_FSV { get; set; }
        public double Cash_OMV { get; set; }
        public double Cash_FSV { get; set; }
        public double Inventory_OMV { get; set; }
        public double Inventory_FSV { get; set; }
        public double Plant_and_Equipment_OMV { get; set; }
        public double Plant_and_Equipment_FSV { get; set; }
        public double Residential_Property_OMV { get; set; }
        public double Residential_Property_FSV { get; set; }
        public double Commercial_Property_OMV { get; set; }
        public double Commercial_Property_FSV { get; set; }
        public double Receivables_OMV { get; set; }
        public double Receivables_FSV { get; set; }
        public double Shares_OMV { get; set; }
        public double Shares_FSV { get; set; }
        public double Vehicle_OMV { get; set; }
        public double Vehicle_FSV { get; set; }
        public double Guarantee_Value { get; set; }              
        public int BatchId { get; set; }

    }
}
