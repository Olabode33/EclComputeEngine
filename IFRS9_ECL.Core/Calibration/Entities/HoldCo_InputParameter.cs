using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Entities
{
    public class HoldCo_InputParameter
    {
        public Guid Id { get; set; }
		public Guid RegistrationId { get; set; }
		public DateTime ValuationDate { get; set; }
		public double Optimistic { get; set; }
		public double BestEstimate { get; set; }
		public double Downturn { get; set; }
		public string AssumedRating { get; set; }
		public string DefaultLoanRating { get; set; }
		public double RecoveryRate { get; set; }
		public DateTime AssumedStartDate { get; set; }
		public DateTime AssumedMaturityDate { get; set; }
	}
}
