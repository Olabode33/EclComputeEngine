using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Macro.Input
{
    public class MacroResult_PrincipalComponent
    {
        public int Id { get; set; }
        public double? PrincipalComponent1 { get; set; }
        public double? PrincipalComponent2 { get; set; }
        public double? PrincipalComponent3 { get; set; }
        public double? PrincipalComponent4 { get; set; }
        public int MacroId { get; set; }
        public DateTime DateCreated { get; set; }


    }
}
