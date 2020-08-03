using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.Framework
{
    public class LifetimeCollateral
    {
        public string ContractId { get; set; }
        public long EirIndex { get; set; }
        public long TtrMonths { get; set; }
        public long ProjectionMonth { get; set; }
        public double ProjectionValue { get; set; }
    }
}
