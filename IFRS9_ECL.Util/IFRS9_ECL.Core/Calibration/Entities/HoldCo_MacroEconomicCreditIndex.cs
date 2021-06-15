using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Entities
{
    public class HoldCo_MacroEconomicCreditIndex
    {
        public Guid Id { get; set; }
		public Guid RegistrationId { get; set; }
		public int Month { get; set; }
		public double BestEstimate { get; set; }
		public double Optimistic { get; set; }
		public double Downturn { get; set; }
	}
}
