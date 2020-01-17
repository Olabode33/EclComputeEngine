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
        public int EirIndex { get; set; }
        public int TtrMonths { get; set; }
        public int ProjectionMonth { get; set; }
        public double ProjectionValue { get; set; }
    }
}
