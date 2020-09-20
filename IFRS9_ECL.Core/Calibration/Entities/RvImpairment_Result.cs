using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Entities
{
    public class RvImpairment_Result
    {
        public Guid Id { get; set; }
        public virtual Guid RegisterId { get; set; }

		public virtual double BaseScenarioExposure { get; set; }

		public virtual double OptimisticScenarioExposure { get; set; }

		public virtual double DownturnScenarioExposure { get; set; }

		public virtual double ResultsExposure { get; set; }

		public virtual double BaseScenarioPreOverlay { get; set; }

		public virtual double OptimisticScenarioPreOverlay { get; set; }

		public virtual double DownturnScenarioPreOverlay { get; set; }

		public virtual double ResultPreOverlay { get; set; }

		public virtual double BaseScenarioOverrideImpact { get; set; }

		public virtual double OptimisticScenarioOverrideImpact { get; set; }

		public virtual double DownturnScenarioOverrideImpact { get; set; }

		public virtual double ResultOverrideImpact { get; set; }

		public virtual double BaseScenarioIPO { get; set; }

		public virtual double OptimisticScenarioIPO { get; set; }

		public virtual double DownturnScenarioIPO { get; set; }

		public virtual double ResultIPO { get; set; }

		public virtual double BaseScenarioOverlay { get; set; }

		public virtual double OptimisticScenarioOverlay { get; set; }

		public virtual double DownturnScenarioOverlay { get; set; }

		public virtual double ResultOverlay { get; set; }

		public virtual double BaseScenarioFinalImpairment { get; set; }

		public virtual double OptimisticScenarioFinalImpairment { get; set; }

		public virtual double DownturnScenarioFinalImpairment { get; set; }

		public virtual double ResultFinalImpairment { get; set; }
	}
}
