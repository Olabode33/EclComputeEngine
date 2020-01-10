using System;
using System.Collections.Generic;
using System.Text;

namespace IFRS9_ECL.Models.Raw
{

    public class Loanbook_Data
    {

        public Guid Id { get; set; }

        public DateTime CreationTime { get; set; }

        public long? CreatorUserId { get; set; }

        public DateTime? LastModificationTime { get; set; }

        public long? LastModifierUserId { get; set; }

        public bool IsDeleted { get; set; }

        public long? DeleterUserId { get; set; }

        public DateTime? DeletionTime { get; set; }

        public int? TenantId { get; set; }

        public string CustomerNo { get; set; }

        public string AccountNo { get; set; }

        public string ContractNo { get; set; }

        public string CustomerName { get; set; }

        public DateTime? SnapshotDate { get; set; }

        public string Segment { get; set; }

        public string Sector { get; set; }

        public string Currency { get; set; }

        public string ProductType { get; set; }

        public string ProductMapping { get; set; }

        public string SpecialisedLending { get; set; }

        public string RatingModel { get; set; }

        public int? OriginalRating { get; set; }

        public int? CurrentRating { get; set; }

        public double? LifetimePD { get; set; }

        public double? Month12PD { get; set; }

        public int? DaysPastDue { get; set; }

        public bool WatchlistIndicator { get; set; }

        public string Classification { get; set; }

        public DateTime? ImpairedDate { get; set; }

        public DateTime? DefaultDate { get; set; }

        public double? CreditLimit { get; set; }

        public double? OriginalBalanceLCY { get; set; }

        public double? OutstandingBalanceLCY { get; set; }

        public double? OutstandingBalanceACY { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public bool RestructureIndicator { get; set; }

        public string RestructureRisk { get; set; }

        public string RestructureType { get; set; }

        public DateTime? RestructureStartDate { get; set; }

        public DateTime? RestructureEndDate { get; set; }

        public string PrincipalPaymentTermsOrigination { get; set; }

        public int? PPTOPeriod { get; set; }

        public string InterestPaymentTermsOrigination { get; set; }

        public int? IPTOPeriod { get; set; }

        public string PrincipalPaymentStructure { get; set; }

        public string InterestPaymentStructure { get; set; }

        public string InterestRateType { get; set; }

        public string BaseRate { get; set; }

        public string OriginationContractualInterestRate { get; set; }

        public int? IntroductoryPeriod { get; set; }

        public double? PostIPContractualInterestRate { get; set; }

        public double? CurrentContractualInterestRate { get; set; }

        public double? EIR { get; set; }

        public double? DebentureOMV { get; set; }

        public double? DebentureFSV { get; set; }

        public double? CashOMV { get; set; }

        public double? CashFSV { get; set; }

        public double? InventoryOMV { get; set; }

        public double? InventoryFSV { get; set; }

        public double? PlantEquipmentOMV { get; set; }

        public double? PlantEquipmentFSV { get; set; }

        public double? ResidentialPropertyOMV { get; set; }

        public double? ResidentialPropertyFSV { get; set; }

        public double? CommercialPropertyOMV { get; set; }

        public double? CommercialProperty { get; set; }

        public double? ReceivablesOMV { get; set; }

        public double? ReceivablesFSV { get; set; }

        public double? SharesOMV { get; set; }

        public double? SharesFSV { get; set; }

        public double? VehicleOMV { get; set; }

        public double? VehicleFSV { get; set; }

        public double? CureRate { get; set; }

        public bool GuaranteeIndicator { get; set; }

        public double? GuarantorPD { get; set; }

        public double? GuarantorLGD { get; set; }

        public double? GuaranteeValue { get; set; }

        public double? GuaranteeLevel { get; set; }

        public string ContractId { get; set; }

        public Guid WholesaleEclUploadId { get; set; }

        public long OrganizationUnitId { get; set; }

    }
  
}
