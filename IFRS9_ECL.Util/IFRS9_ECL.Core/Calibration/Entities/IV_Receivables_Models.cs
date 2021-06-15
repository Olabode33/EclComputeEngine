using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Entities
{
    public class ReceivablesForecasts
    {
        public Guid Id { get; set; }

        public string Period { get; set; }

        public double Optimistic { get; set; }

        public double Base { get; set; }

        public double Downturn { get; set; }

        public Guid RegisterId { get; set; }

    }


    public class ReceivablesInputs
    {
        public Guid Id { get; set; }

        public DateTime ReportingDate { get; set; }

        public double ScenarioOptimistic { get; set; }

        public int LossDefinition { get; set; }

        public double LossRate { get; set; }

        public bool FLIOverlay { get; set; }

        public double OverlayOptimistic { get; set; }

        public double OverlayBase { get; set; }

        public double OverlayDownturn { get; set; }

        public double InterceptCoefficient { get; set; }

        public double IndexCoefficient { get; set; }

        public double LossRateCoefficient { get; set; }

        public Guid RegisterId { get; set; }

        public double ScenarioBase { get; set; }

    }

    public class ReceivablesRegisters
    {
        public Guid Id { get; set; }

        public DateTime CreationTime { get; set; }

        public long? CreatorUserId { get; set; }

        public DateTime? LastModificationTime { get; set; }

        public long? LastModifierUserId { get; set; }

        public bool IsDeleted { get; set; }

        public long? DeleterUserId { get; set; }

        public DateTime? DeletionTime { get; set; }

        public int Status { get; set; }

    }


    public class ReceivablesResults
    {
        public Guid Id { get; set; }

        public double TotalExposure { get; set; }

        public double TotalImpairment { get; set; }

        public double AdditionalProvision { get; set; }

        public double Coverage { get; set; }

        public double OptimisticExposure { get; set; }

        public double BaseExposure { get; set; }

        public double DownturnExposure { get; set; }

        public double ECLTotalExposure { get; set; }

        public double OptimisticImpairment { get; set; }

        public double BaseImpairment { get; set; }

        public double DownturnImpairment { get; set; }

        public double ECLTotalImpairment { get; set; }

        public double OptimisticCoverageRatio { get; set; }

        public double BaseCoverageRatio { get; set; }

        public double DownturnCoverageRatio { get; set; }

        public double TotalCoverageRatio { get; set; }

        public Guid RegisterId { get; set; }

    }

    public class ReceivablesCurrentPeriodDates
    {
        public Guid Id { get; set; }

        public string Account { get; set; }

        public double ZeroTo90 { get; set; }

        public double NinetyOneTo180 { get; set; }

        public double OneEightyOneTo365 { get; set; }

        public double Over365 { get; set; }

        public Guid RegisterId { get; set; }

    }

}
