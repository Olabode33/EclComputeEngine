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
        public decimal OutStandingBalance { get; set; }
        public decimal Pre_ECL_Best_Estimate { get; set; }
        public decimal Pre_ECL_Optimistic { get; set; }
        public decimal Pre_ECL_Downturn { get; set; }
        public decimal Pre_Impairment_ModelOutput { get; set; }

        public decimal Post_ECL_Best_Estimate { get; set; }
        public decimal Post_ECL_Optimistic { get; set; }
        public decimal Post_ECL_Downturn { get; set; }
        public decimal Post_Impairment_ModelOutput { get; set; }

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

    }

    public class ResultDetailDataMore: ResultDetailData
    {
       
        public int Stage { get; set; }
        public decimal Outstanding_Balance { get; set; }
        public decimal ECL_Best_Estimate { get; set; }
        public decimal ECL_Optimistic { get; set; }
        public decimal ECL_Downturn { get; set; }
        public decimal Impairment_ModelOutput { get; set; }
        public int Overrides_Stage { get; set; }
        public decimal Overrides_TTR_Years { get; set; }
        public decimal Overrides_FSV { get; set; }
        public decimal Overrides_Overlay { get; set; }
        public decimal Overrides_ECL_Best_Estimate { get; set; }
        public decimal Overrides_ECL_Optimistic { get; set; }
        public decimal Overrides_ECL_Downturn { get; set; }
        public decimal Overrides_Impairment_Manual { get; set; }
    }

    public class ReportDetailExtractor
    {
        public int NumberOfContracts { get; set; }
        public decimal SumOutStandingBalance { get; set; }
        public decimal Pre_EclBestEstimate { get; set; }
        public decimal Pre_Optimistic { get; set; }
        public decimal Pre_Downturn { get; set; }
        public decimal Post_EclBestEstimate { get; set; }
        public decimal Post_Optimistic { get; set; }
        public decimal Post_Downturn { get; set; }
        public decimal UserInput_EclBE { get; set; }
        public decimal UserInput_EclO { get; set; }
        public decimal UserInput_EclD { get; set; }
    }

    public class TempFinalEclResult
    {
        public int Stage { get; set; }
        public decimal FinalEclValue { get; set; }
        public int Scenerio { get; set; }
        //public int EclMonth { get; set; }
        public string ContractId { get; set; }

        public int StageOverride { get; set; }
        public decimal FinalEclValueOverride { get; set; }
        public int ScenerioOverride { get; set; }
        //public int EclMonthOverride { get; set; }
        public string ContractIdOverride { get; set; }

    }

    public class TempEadInput
    {
        public string ContractId { get; set; }
        public decimal Value { get; set; }
        //public int Months { get; set; }
    }
}
