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
    }

    public class PdMappings
    {
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
    }

    public static class PdAssumptionsRowKey
    {
        public const string AssumptionsColumn = "Assumptions";
        public const string ValuesColumn = "Value";
        public const string SnpMappingValueBestFit = "Best Fit";
        public const string SnpMappingValueEtiCreditPolicy = "ETI Credit Policy";
    }

    public class SicrInputs
    {
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

    }



}
