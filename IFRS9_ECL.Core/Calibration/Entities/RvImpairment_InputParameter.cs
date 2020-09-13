using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Entities
{
    public class RvImpairment_InputParameter
    {
        public Guid Id { get; set; }
        public virtual DateTime ReportingDate { get; set; }

		public virtual double CostOfCapital { get; set; }

		public virtual double LoanAmount { get; set; }

		public virtual Guid RegisterId { get; set; }
	}
}
