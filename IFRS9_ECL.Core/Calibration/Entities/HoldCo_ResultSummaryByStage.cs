using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Entities
{
    public class HoldCo_ResultSummaryByStage
    {
		public double StageOneExposure { get; set; }
		public double StageTwoExposure { get; set; }
		public double StageThreeExposure { get; set; }
		public double TotalExposure { get; set; }
		public double StageOneImpairment { get; set; }
		public double StageTwoImpairment { get; set; }
		public double StageThreeImpairment { get; set; }
		public double StageOneImpairmentRatio { get; set; }
		public double StageTwoImpairmentRatio { get; set; }
		public double TotalImpairment { get; set; }
		public double StageThreeImpairmentRatio { get; set; }
		public double TotalImpairmentRatio { get; set; }
		public Guid RegistrationId { get; set; }
	}
}
