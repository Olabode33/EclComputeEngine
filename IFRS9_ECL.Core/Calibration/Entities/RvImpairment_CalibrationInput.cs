using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Entities
{
    public class RvImpairment_CalibrationInput
    {
        public Guid Id { get; set; }
        public Guid RegisterId { get; set; }

		public int Year { get; set; }

		public double ExpectedCashFlow { get; set; }

		public double RevisedCashFlow { get; set; }
	}
}
