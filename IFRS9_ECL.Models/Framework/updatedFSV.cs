using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.Framework
{
    public class updatedFSV
    {
        public string ContractNo { get; set; }
        public double Cash { get; set; }
        public double CommercialProperty { get; set; }
        public double Debenture { get; set; }
        public double Inventory { get; set; }
        public double PlantAndEquipment { get; set; }
        public double Receivables { get; set; }
        public double ResidentialProperty { get; set; }
        public double Shares { get; set; }
        public double Vehicle { get; set; }
        public int? Override_TTR_Year { get; set; }

    }
}
