using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class CalibrationBatch: BaseObject
    {
        public int EAD_Behavioural_Terms_Data { get; set; }
        public int EAD_CCF_Summary { get; set; }
        public int LGD_CureRates { get; set; }
        public int LGD_HairCut { get; set; }
        public int LGD_RecoveryRate { get; set; }
        public int LGD_RecoveryRate_AccountSummary { get; set; }
        public int PD_Commercial { get; set; }
        public int PD_Consumer { get; set; }
        public int PD_Contract_Level_Data { get; set; }
        public int PD_MacroEconomics_RawData { get; set; }
        public int PD_Wholesale_Input_Template { get; set; }

        public StageStatus StageStatus { get; set; }
    }

    public enum StageStatus
    {
        Upload,
        Migrating,
        Migrated
    }
}
