using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class CalibrationResult_PD_12Months
    {
        public int Id { get; set; }

        public double? Rating { get; set; }

        public double? Outstanding_Balance { get; set; }

        public double? Redefault_Balance { get; set; }

        public double? Redefaulted_Balance { get; set; }

        public double? Total_Redefault { get; set; }

        public double? Months_PDs_12 { get; set; }

        public string Comment { get; set; }

        public int? Status { get; set; }

        public DateTime? DateCreated { get; set; }

        public Guid? CalibrationId { get; set; }

    }

    public class PD12Months
    {
        public int Rating { get; set; }
        public double Months_PDs_12 { get; set; }
    }

}
