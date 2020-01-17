using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.Framework
{
    public class IrFactor
    {
        public int Rank { get; set; }
        public string CirGroup { get; set; }
        public string EirGroup { get; set; }
        public int ProjectionMonth { get; set; }
        public double ProjectionValue { get; set; }
    }
}
