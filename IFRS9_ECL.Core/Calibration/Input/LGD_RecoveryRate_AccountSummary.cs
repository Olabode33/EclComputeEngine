using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class LGD_RecoveryRate_AccountSummary: BaseObject
    {

        public string Contract_No { get; set; }
        public string Segment { get; set; }
        public int BatchId { get; set; }
    }
}
