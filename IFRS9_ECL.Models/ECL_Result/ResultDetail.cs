using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.ECL_Result
{
    public class ResultDetail
    {
        public int NumberOfContracts { get; set; }
        public double OutStandingBalance { get; set; }
        public double Pre_ECL_Best_Estimate { get; set; }
        public double Pre_ECL_Optimistic { get; set; }
        public double Pre_ECL_Downturn { get; set; }
        public double Pre_Impairment_ModelOutput { get; set; }

        public double Post_ECL_Best_Estimate { get; set; }
        public double Post_ECL_Optimistic { get; set; }
        public double Post_ECL_Downturn { get; set; }
        public double Post_Impairment_ModelOutput { get; set; }

        public List<ResultDetailDataMore> ResultDetailDataMore { get; set; }
    }
    public class ResultDetailData
    {
        public string ContractNo { get; set; }
        public string AccountNo { get; set; }
        public string CustomerNo { get; set; }
        public string Segment { get; set; }
        public string ProductType { get; set; }
        public string Sector { get; set; }
        public double OriginalOutstandingBalance { get; set; } 

    }

    public class ResultDetailDataMore: ResultDetailData
    {
        public Guid Id { get; set; }
        public int Stage { get; set; }
        public double Outstanding_Balance { get; set; }
        public double ECL_Best_Estimate { get; set; }
        public double ECL_Optimistic { get; set; }
        public double ECL_Downturn { get; set; }
        public double Impairment_ModelOutput { get; set; }
        public int Overrides_Stage { get; set; }
        public double Overrides_TTR_Years { get; set; }
        public double Overrides_FSV { get; set; }
        public double Overrides_Overlay { get; set; }
        public double Overrides_ECL_Best_Estimate { get; set; }
        public double Overrides_ECL_Optimistic { get; set; }
        public double Overrides_ECL_Downturn { get; set; }
        public double Overrides_Impairment_Manual { get; set; }
    }

    public class ReportDetailExtractor
    {
        public int NumberOfContracts { get; set; }
        public double SumOutStandingBalance { get; set; }
        public double Pre_EclBestEstimate { get; set; }
        public double Pre_Optimistic { get; set; }
        public double Pre_Downturn { get; set; }
        public double Post_EclBestEstimate { get; set; }
        public double Post_Optimistic { get; set; }
        public double Post_Downturn { get; set; }
        public double UserInput_EclBE { get; set; }
        public double UserInput_EclO { get; set; }
        public double UserInput_EclD { get; set; }
    }

    public class TempFinalEclResult
    {
        public int Stage { get; set; }
        public double FinalEclValue { get; set; }
        public int Scenario { get; set; }
        //public int EclMonth { get; set; }
        public string ContractId { get; set; }

        public int StageOverride { get; set; }
        public double FinalEclValueOverride { get; set; }
        public int ScenarioOverride { get; set; }
        //public int EclMonthOverride { get; set; }
        public string ContractIdOverride { get; set; }

    }

    public class TempEadInput
    {
        public string ContractId { get; set; }
        public double Value { get; set; }
        //public int Months { get; set; }
    }
}
