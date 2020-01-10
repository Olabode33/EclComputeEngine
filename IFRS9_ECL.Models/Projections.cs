using System;
using System.Collections.Generic;
using System.Text;

namespace IFRS9_ECL.Models
{
    public class EIRProjections
    {
        public string eir_group { get; set; }
        public int months { get; set; }
        public double value { get; set; }
    }

    public class CIRProjections
    {
        public string cir_group { get; set; }
        public int months { get; set; }
        public double value { get; set; }
        public double cir_effective { get; set; }
    }

    public class LifeTimeProjections
    {
        public string contract_no { get; set; }
        public string eir_group { get; set; }
        public string cir_group { get; set; }
        public int months { get; set; }
        public double value { get; set; }
    }
}
