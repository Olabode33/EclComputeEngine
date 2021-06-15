using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Macro.Entities
{
    public class MacroResult_IndexData
    {
        public int Id { get; set; }
        public string Period { get; set; }
        public double Index { get; set; }
        public double StandardIndex { get; set; }
        public double BfNpl { get; set; }
        public int MacroId {get; set;}
        public DateTime DateCreated { get; set; }
    }
}
