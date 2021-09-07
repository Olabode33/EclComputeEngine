using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.ECLProcessor.Entities
{
    public class FrameworkParameters
    {
        public string BasePath { get; set; }
        public string ModelFileName { get; set; }
        public string EadFileName { get; set; }
        public string LgdFile { get; set; }
        public string PdFileName { get; set; }
        public string ReportFolderName { get; set; }
        public DateTime ReportDate { get; set; }
        public Guid EclId { get; set; }
        public EclType EclType { get; set; }
    }
}
