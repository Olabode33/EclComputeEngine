using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models
{
    public class LGD_Assumptions_CollateralType_TTR_Years
    {
        public double collateral_value { get; set; }
        public double debenture { get; set; }
        public double cash { get; set; }
        public double inventory { get; set; }
        public double plant_and_equipment { get; set; }
        public double residential_property { get; set; }
        public double commercial_property { get; set; }
        public double Receivables { get; set; }
        public double shares { get; set; }
        public double vehicle { get; set; }
    }

    public class LGD_Assumptions_2
    {
        public string COLLATERAL_TYPE { get; set; }
        public double ttr_years { get; set; }
        //public string new_contract_no { get; set; }
        //public string contract_no { get; set; }
        //public double guarantee_value { get; set; }
        //public string customer_no { get; set; }
        //public double costOfRecovery { get; set; }
        //public double cor { get; set; }
    }
}
