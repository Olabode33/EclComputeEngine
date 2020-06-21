using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Macro.Entities
{
    public class MacroResult_SelectedMacroEconomicVariables
    {
        public int MacroeconomicVariableId { get; set; }
        public long AffiliateId { get; set; }
        public int BackwardOffset { get; set; }
        public string friendlyName { get; set; }
    }
}
