using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.PDComputation.cmPD
{
    public class LogOddRatio
    {
        public int Rank { get; set; }
        public string Rating { get; set; }
        public int Year { get; set; }
        public double LogOddsRatio { get; set; }
    }
}
