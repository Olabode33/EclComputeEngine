using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Entities
{
    public class RvImpairment_Haircut
    {
        public Guid Id { get; set; }
		public virtual double CashRecovery { get; set; }

		public virtual double Property { get; set; }

		public virtual double Shares { get; set; }

		public virtual double LoanSale { get; set; }

		public virtual Guid RegisterId { get; set; }
	}
}
