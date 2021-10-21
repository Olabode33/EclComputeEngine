using IFRS9_ECL.Core.Calibration.Input;
using IFRS9_ECL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.ECLProcessor.Entities
{
    public class LGDParameters
    {
        public string BasePath { get; set; }
        public string ModelFileName { get; set; }
        public string LoanBookFileName { get; set; }

        public double Expired { get; set; }
        public double NonExpired { get; set; }
        public double Commercial_CureRate { get; set; }
        public double Consumer_CureRate { get; set; }
        public double Corporate_CureRate { get; set; }
        public double Commercial_RecoveryRate { get; set; }
        public double Consumer_RecoveryRate { get; set; }
        public double Corporate_RecoveryRate { get; set; }
        public double RedefaultFactor { get; set; }
        public DateTime ReportDate { get; set; }

        public LGD_Assumptions_CollateralType_TTR_Years lgd_first { get; set; }
        public LGD_Assumptions_CollateralType_TTR_Years lgd_last { get; set; }

        public double LGDCollateralGrowthAssumption_Debenture { get; set; }
        public double LGDCollateralGrowthAssumption_Cash { get; set; }
        public double LGDCollateralGrowthAssumption_Inventory { get; set; }
        public double LGDCollateralGrowthAssumption_PlantEquipment { get; set; }
        public double LGDCollateralGrowthAssumption_ResidentialProperty { get; set; }
        public double LGDCollateralGrowthAssumption_CommercialProperty { get; set; }
        public double LGDCollateralGrowthAssumption_Receivables { get; set; }
        public double LGDCollateralGrowthAssumption_Shares { get; set; }
        public double LGDCollateralGrowthAssumption_Vehicle { get; set; }

        public double TTR_Debenture { get; set; }
        public double TTR_Cash { get; set; }
        public double TTR_Inventory { get; set; }
        public double TTR_PlantEquipment { get; set; }
        public double TTR_ResidentialProperty { get; set; }
        public double TTR_CommercialProperty { get; set; }
        public double TTR_Receivables { get; set; }
        public double TTR_Shares { get; set; }
        public double TTR_Vehicle { get; set; }


        public double CrPD_CreditPd1 { get; set; }
        public double CrPD_CreditPd2 { get; set; }
        public double CrPD_CreditPd3 { get; set; }
        public double CrPD_CreditPd4 { get; set; }
        public double CrPD_CreditPd5 { get; set; }
        public double CrPD_CreditPd6 { get; set; }
        public double CrPD_CreditPd7 { get; set; }
        public double CrPD_CreditPd8 { get; set; }
        public double CrPD_CreditPd9 { get; set; }
        public double CrPD_CreditPd10 { get; set; }

        public double CrPD_ConsStage1 { get; set; }
        public double CrPD_ConsStage2 { get; set; }
        public double CrPD_CommStage1 { get; set; }
        public double CrPD_CommStage2 { get; set; }
        public double CrPD_Exp { get; set; }


        public CalibrationResult_LGD_HairCut Haircut  { get; set; }

        public CreditPdParam CreditPd { get; set; }


    }
}
