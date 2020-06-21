using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Macro.Entities
{
    public class MacroResult_CorMat
    {
        public int Id { get; set; }
        public double? Value { get; set; }
        public int MacroEconomicIdA { get; set; }
        public int MacroEconomicIdB { get; set; }
        public string MacroEconomicLabelA { get; set; }
        public string MacroEconomicLabelB { get; set; }
        public int MacroId { get; set; }
        public DateTime DateCreated { get; set; }
    }

}
