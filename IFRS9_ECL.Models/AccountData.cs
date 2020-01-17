using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models
{
    public class LGDAccountData
    {        
        public Guid Id { get; set; }
        public string CONTRACT_NO { get; set; }
        public double TTR_YEARS { get; set; }
        public double COST_OF_RECOVERY { get; set; }
        public double GUARANTOR_PD { get; set; }
        public double GUARANTOR_LGD { get; set; }
        public double GUARANTEE_VALUE { get; set; }
        public double GUARANTEE_LEVEL { get; set; }

    }
}
