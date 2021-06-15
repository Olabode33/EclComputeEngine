using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Entities
{
    public class RvImpairment_ScenarioOption
    {
        public Guid Id { get; set; }
        public virtual Guid RegisterId { get; set; }

		public virtual string ScenarioOption { get; set; }

		public virtual string ApplyOverridesBaseScenario { get; set; }

		public virtual string ApplyOverridesOptimisticScenario { get; set; }

		public virtual string ApplyOverridesDownturnScenario { get; set; }

		public virtual double BestScenarioOverridesValue { get; set; }

		public virtual double OptimisticScenarioOverridesValue { get; set; }

		public virtual double DownturnScenarioOverridesValue { get; set; }

		public virtual double BaseScenario { get; set; }

		public virtual double OptimisticScenario { get; set; }

	}
}
