using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.FrameworkComputation
{
    public class EclOverrides
    {
        public Guid Id { get; set; }

        public DateTime CreationTime { get; set; }

        public long? CreatorUserId { get; set; }

        public DateTime? LastModificationTime { get; set; }

        public long? LastModifierUserId { get; set; }

        public bool IsDeleted { get; set; }

        public long? DeleterUserId { get; set; }

        public DateTime? DeletionTime { get; set; }

        public int? Stage { get; set; }

        public int? TtrYears { get; set; }

        public double? FSV_Cash { get; set; }

        public double? FSV_CommercialProperty { get; set; }

        public double? FSV_Debenture { get; set; }

        public double? FSV_Inventory { get; set; }

        public double? FSV_PlantAndEquipment { get; set; }

        public double? FSV_Receivables { get; set; }

        public double? FSV_ResidentialProperty { get; set; }

        public double? FSV_Shares { get; set; }

        public double? FSV_Vehicle { get; set; }

        public double? OverlaysPercentage { get; set; }

        public string Reason { get; set; }

        public string ContractId { get; set; }

        public Guid? RetailEclDataLoanBookId { get; set; }

    }

}
