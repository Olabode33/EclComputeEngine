using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Macro.Input
{
    public class AffiliateMacroEconomicVariableOffsets
    {
        public int Id { get; set; }
        public int MacroeconomicVariableId { get; set; }
        public long AffiliateId { get; set; }
        public int BackwardOffset { get; set; }
        [NotMapped]
        public string varTitle { get; set; }
    }
}
