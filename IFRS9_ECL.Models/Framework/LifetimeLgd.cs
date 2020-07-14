using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.Framework
{
    public class LifetimeLgd
    {
        public string ContractId { get; set; }
        public string PdIndex { get; set; }
        public string LgdIndex { get; set; }
        public double RedefaultLifetimePD { get; set; }
        public double CureRate { get; set; }
        public double UrBest { get; set; }
        public double URDownturn { get; set; }
        public double Cor { get; set; }
        public double GPd { get; set; }
        public double GuarantorLgd { get; set; }
        public double GuaranteeValue { get; set; }
        public double GuaranteeLevel { get; set; }
        public int Stage { get; set; }
        public int Month { get; set; }
        public ECL_Scenario Ecl_Scenerio { get; set; }
        public double Value { get; set; }
    }

}
