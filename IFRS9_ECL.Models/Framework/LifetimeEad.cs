using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.Framework
{
    public class LifetimeEad
    {
        public string ContractId { get; set; }
        public int CirIndex { get; set; }
        public string ProductType { get; set; }
        public long MonthsPastDue { get; set; }
        public int ProjectionMonth { get; set; }
        public double ProjectionValue { get; set; }
    }
}
