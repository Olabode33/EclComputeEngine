using System;
using System.Collections.Generic;
using System.Text;

namespace IFRS9_ECL.Models
{
    public class LGD_PD_Assumptions
    {
        public string pd_group { get; set; }
        public double pd { get; set; }
        public Guid eclId { get; set; }
    }
}
