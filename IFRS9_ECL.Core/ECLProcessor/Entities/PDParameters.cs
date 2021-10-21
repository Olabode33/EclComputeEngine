using IFRS9_ECL.Core.Calibration.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.ECLProcessor.Entities
{
    public class PDParameters
    {
        public string BasePath { get; set; }
        public string ModelFileName { get; set; }
        public string LoanBookFileName { get; set; }
        public double RedefaultAdjustmentFactor { get; set; }
        public string SandPMapping { get; set; }
        public double Expired { get; set; }
        public double NonExpired { get; set; }
        public DateTime ReportDate { get; set; }


        public CreditPdParam CreditPd { get; set; }
        public CreditPolicyParam CreditPolicy { get; set; }



        public List<CalibrationResult_PD_CommsCons_MarginalDefaultRate> CommCons { get; set; }
        public List<CalibrationResult_PD_CummulativeDefaultRate> CummulativeDefaultRates { get; set; }
    }

    public class CreditPdParam
    {
        public double CrPD_CreditPd1 { get; set; }
        public double CrPD_CreditPd2 { get; set; }
        public double CrPD_CreditPd3 { get; set; }
        public double CrPD_CreditPd4 { get; set; }
        public double CrPD_CreditPd5 { get; set; }
        public double CrPD_CreditPd6 { get; set; }
        public double CrPD_CreditPd7 { get; set; }
        public double CrPD_CreditPd8 { get; set; }
        public double CrPD_CreditPd9 { get; set; }
        public double CrPD_CreditPd10 { get; set; }
    }

    public class CreditPolicyParam
    {
        public string CrPD_CreditPolicy1 { get; set; }
        public string CrPD_CreditPolicy2 { get; set; }
        public string CrPD_CreditPolicy3 { get; set; }
        public string CrPD_CreditPolicy4 { get; set; }
        public string CrPD_CreditPolicy5 { get; set; }
        public string CrPD_CreditPolicy6 { get; set; }
        public string CrPD_CreditPolicy7 { get; set; }
        public string CrPD_CreditPolicy8 { get; set; }
        public string CrPD_CreditPolicy9 { get; set; }
        public string CrPD_CreditPolicy10 { get; set; }
    }
}
