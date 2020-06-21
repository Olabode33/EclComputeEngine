using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Macro.Input
{
    public class MacroData
    {
        public int Id { get; set; }
        public long AffiliateId { get; set; }
        public int MacroeconomicId { get; set; }
        public double Value { get; set; }
        public DateTime Period { get; set; }
    }

    public class GroupMacroData
    {
        public string period { get; set; }
        public double MacroValue1 { get; set; }
        public double MacroValue2 { get; set; }
        public double MacroValue3 { get; set; }
        public double MacroValue4 { get; set; }
        public double NPL { get; set; }
    }
}
