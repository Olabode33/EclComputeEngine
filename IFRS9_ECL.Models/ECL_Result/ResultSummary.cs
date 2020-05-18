using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.ECL_Result
{
    public class ResultSummary
    {
        public List<ReportBreakdown> Overrall { get; set; }
        public List<ReportBreakdown> Scenario { get; set; }
        public List<ReportBreakdown> Segment { get; set; }
        public List<ReportBreakdown> Stage { get; set; }
        public List<ReportBreakdown> DynamicBreakdown_Segment_ProductType { get; set; }
        public List<ReportBreakdown> ProductType { get; set; }
        public List<ReportBreakdown> SegmentAndStage { get; set; }
        public List<ReportBreakdown> TopExposureSummary { get; set; }
    }


    public class ReportBreakdown
    {
        public string Field1 { get; set; }
        public string Exposure_Pre { get; set; }
        public string Impairment_Pre { get; set; }
        public string CoverageRatio_Pre { get; set; }
        public string Exposure_Post { get; set; }
        public string Impairment_Post { get; set; }
        public string CoverageRatio_Post { get; set; }

        public Section Section { get; set; }
    }

    public enum Section
    {
        Overrall,
        Scenario,
        Segment,
        DynamicBreakdown_Segment_ProductType,
        ProductType,
        SegmentAndStage,
        TopExposureSummary
    }

    

}
