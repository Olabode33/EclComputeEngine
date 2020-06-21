using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Macro.Entities
{

    public class MacroResult_PrincipalComponentSummary
    {
        public int Id { get; set; }
        public double? Value { get; set; }
        public int PrincipalComponentIdA { get; set; }
        public int PrincipalComponentIdB { get; set; }
        public string PricipalComponentLabelA { get; set; }
        public string PricipalComponentLabelB { get; set; }
        public int MacroId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
