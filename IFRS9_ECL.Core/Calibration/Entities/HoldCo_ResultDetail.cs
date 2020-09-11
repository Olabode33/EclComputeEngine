using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Entities
{
    public class HoldCo_ResultDetail
    {
        public Guid Id { get; set; }
		public Guid RegistrationId { get; set; }
		public string AssetType { get; set; }
		public string AssetDescription { get; set; }
		public double Stage { get; set; }
		public double OutstandingBalance { get; set; }
		public double BestEstimate { get; set; }
		public double Optimistic { get; set; }
		public double Downturn { get; set; }
		public double Impairment { get; set; }
	}
}
