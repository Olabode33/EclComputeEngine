using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.Framework
{
    public class LgdCollateralProjection: LgdCollateralGrowth_DepricationAssumption
    {
        public int Month { get; set; }
    }
    public class LgdCollateralFsvProjectionUpdate : LgdCollateralGrowth_DepricationAssumption
    {
        public string ContractNo { get; set; }
    }

    public class LgdCollateralGrowth_DepricationAssumption
    {
        public ECL_Scenario CollateralProjectionType { get; set; }
        public double Debenture { get; set; }
        public double Cash { get; set; }
        public double Inventory { get; set; }
        public double Plant_And_Equipment { get; set; }
        public double Residential_Property { get; set; }
        public double Commercial_Property { get; set; }
        public double Receivables { get; set; }
        public double Shares { get; set; }
        public double Vehicle { get; set; }
    }


}
