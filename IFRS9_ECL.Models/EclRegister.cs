using IFRS9_ECL.Util;
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
        public long OrganizationUnitId { get; set; }
        public EclType eclType { get; set; }

    }
}
