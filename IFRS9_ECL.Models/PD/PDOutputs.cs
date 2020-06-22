using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.PD
{
    public class CreditIndex_Output
    {
        public Guid Id { get; set; }
        public int ProjectionMonth { get; set; }
        public double BestEstimate { get; set; }
        public double Optimistic { get; set; }
        public double Downturn { get; set; }
        public Guid WholesaleEclId { get; set; }
    }

    public class PdMappings
    {
        public Guid Id { get; set; }
        public string ContractId { get; set; }
        public string AccountNo { get; set; }
        public string ProductType { get; set; }
        public string PdGroup { get; set; }
        public int TtmMonths { get; set; }
        public int MaxDpd { get; set; }
        public int MaxClassificationScore { get; set; }
        public double Pd12Month { get; set; }
        public double LifetimePd { get; set; }
        public double RedefaultLifetimePd { get; set; }
        public int Stage1Transition { get; set; }
        public int Stage2Transition { get; set; }
        public int DaysPastDue { get; set; }
        public string RatingModel { get; set; }
        public string Segment { get; set; }
        public int RatingUsed { get; set; }
        public int ClassificationScore { get; set; }
        public Guid WholesaleEclId { get; set; }
    }

    public static class PdAssumptionsRowKey
    {
        public const string AssumptionsColumn = "Assumptions";
        public const string ValuesColumn = "Value";
        public const string ReDefaultAdjustmentFactor = "ReDefaultAdjustmentFactor";
        public const string SnpMapping = "SnpMapping";
        public const string NonExpired = "NonExpired"; ///OD_PERFORMANCE_PAST_EXPIRY
        public const string Expired = "Expired"; ///EXP_OD_PERFORMANCE_PAST_REPORTING
        public const string SnpMappingValueBestFit = "Best Fit";
        public const string SnpMappingValueEtiCreditPolicy = "ETI Credit Policy";
    }

    public class SicrInputs
    {
       // public Guid Id { get; set; }
        public string ContractId { get; set; }
        //public int RestructureIndicator { get; set; }
        //public int RestructureRisk { get; set; }
        //public int WatchlistIndicator { get; set; }
        //public int CurrentRating { get; set; }
        public double Pd12Month { get; set; }
        public double LifetimePd { get; set; }
        public double RedefaultLifetimePd { get; set; }
        public int Stage1Transition { get; set; }
        public int Stage2Transition { get; set; }
        public int DaysPastDue { get; set; }
        //public int OriginationRating { get; set; }
        //public double Origination12MonthPd { get; set; }
        //public double OriginationLifetimePd { get; set; }
        //public DateTime ImpairedDate { get; set; }
        //public DateTime DefaultDate { get; set; }
        //public Guid WholesaleEclId { get; set; }
    }

    /// <summary>
    ///  Model for all scenerio (Lifetime, Marginal, RedefaultLifeTime) [Optimistic, Downturn, Best]
    /// </summary>
    public class LifeTimeObject
    {
        public Guid Id { get; set; }
        public string PdGroup { get; set; }
        public int Month { get; set; }
        public double Value { get; set; }
        public Guid WholesaleEclId { get; set; }

    }



}
