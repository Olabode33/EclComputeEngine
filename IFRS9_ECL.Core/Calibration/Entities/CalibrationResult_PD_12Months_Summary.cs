using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class CalibrationResult_PD_12Months_Summary
    {
        public int Id { get; set; }

        public double? Normal_12_Months_PD { get; set; }

        public double? DefaultedLoansA { get; set; }

        public double? DefaultedLoansB { get; set; }

        public double? CuredLoansA { get; set; }

        public double? CuredLoansB { get; set; }

        public double? Cure_Rate { get; set; }

        public double? CuredPopulationA { get; set; }

        public double? CuredPopulationB { get; set; }

        public double? RedefaultedLoansA { get; set; }

        public double? RedefaultedLoansB { get; set; }

        public double? Redefault_Rate { get; set; }

        public double? Redefault_Factor { get; set; }
        public double? Commercial_CureRate { get; set; }
        public double? Commercial_RedefaultRate { get; set; }
        public double? Consumer_CureRate { get; set; }
        public double? Consumer_RedefaultRate { get; set; }

        public string Comment { get; set; }

        public int? Status { get; set; }

        public DateTime? DateCreated { get; set; }

        public Guid? CalibrationId { get; set; }

    }

}
