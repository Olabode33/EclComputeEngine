using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models
{
    public class EclRegister
    {
        public Guid Id { get; set; }
        public DateTime ReportingDate { get; set; }
        public int Status { get; set; }
        public int EclType { get; set; }

    }
}
