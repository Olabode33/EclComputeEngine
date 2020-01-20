using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.Framework
{
    public class LgdInputAssumptions_UnsecuredRecovery
    {
        public string Segment_Product_Type { get; set; }
        public double Cure_Rate { get; set; }
        public double Days_0 { get; set; }
        public double Days_90 { get; set; }
        public double Days_180 { get; set; }
        public double Days_270 { get; set; }
        public double Days_360 { get; set; }

        public double Downturn_Days_0 { get; set; }
        public double Downturn_Days_90 { get; set; }
        public double Downturn_Days_180 { get; set; }
        public double Downturn_Days_270 { get; set; }
        public double Downturn_Days_360 { get; set; }

    }

    public class EclAssumptions
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public int LgdGroup { get; set; }
    }
}
