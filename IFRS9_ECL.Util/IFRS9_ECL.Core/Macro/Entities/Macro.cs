using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Macro.Entities
{
    public class Macro
    {
        public int Id { get; set; }
        public long AffiliateId { get; set; }
        public int MacroStatusEnum { get; set; }
    }
}
