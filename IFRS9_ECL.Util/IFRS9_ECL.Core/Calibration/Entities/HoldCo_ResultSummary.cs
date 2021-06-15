using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Entities
{
    public class HoldCo_ResultSummary
    {
        public Guid Id { get; set; }
		public double BestEstimateExposure { get; set; }
		public double OptimisticExposure { get; set; }
		public double DownturnExposure { get; set; }
		public double BestEstimateTotal { get; set; }
		public double OptimisticTotal { get; set; }
		public double DownturnTotal { get; set; }
		public double BestEstimateImpairmentRatio { get; set; }
		public double OptimisticImpairmentRatio { get; set; }
		public double DownturnImpairmentRatio { get; set; }
		public double Exposure { get; set; }
		public double Total { get; set; }
		public double ImpairmentRatio { get; set; }
		public bool Check { get; set; }
		public double Diff { get; set; }
		public Guid RegistrationId { get; set; }
	}
}
