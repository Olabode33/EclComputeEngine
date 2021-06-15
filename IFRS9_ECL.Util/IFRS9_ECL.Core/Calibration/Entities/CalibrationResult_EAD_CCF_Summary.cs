using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class CalibrationResult_EAD_CCF_Summary
    {
        public int Id { get; set; }

        public double? OD_TotalLimitOdDefaultedLoan { get; set; }

        public double? OD_BalanceAtDefault { get; set; }

        public double? OD_Balance12MonthBeforeDefault { get; set; }

        public double? OD_TotalConversation { get; set; }

        public double? OD_CCF { get; set; }

        public double? Card_TotalLimitOdDefaultedLoan { get; set; }

        public double? Card_BalanceAtDefault { get; set; }

        public double? Card_Balance12MonthBeforeDefault { get; set; }

        public double? Card_TotalConversation { get; set; }

        public double? Card_CCF { get; set; }

        public double? Overall_TotalLimitOdDefaultedLoan { get; set; }

        public double? Overall_BalanceAtDefault { get; set; }

        public double? Overall_Balance12MonthBeforeDefault { get; set; }

        public double? Overall_TotalConversation { get; set; }

        public double? Overall_CCF { get; set; }

        public string Comment { get; set; }

        public int? Status { get; set; }

        public DateTime? DateCreated { get; set; }

        public Guid? CalibrationId { get; set; }

    }

}
