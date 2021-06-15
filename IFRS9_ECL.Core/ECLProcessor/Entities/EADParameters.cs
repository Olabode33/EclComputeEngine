using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.ECLProcessor.Entities
{
    public class EADParameters
    {
        public string BasePath { get; set; }
        public string ModelFileName { get; set; }
        public string LoanBookFileName { get; set; }
        public string PaymentScheduleFileName { get; set; }

        public DateTime ReportDate { get; set; }
        public double ConversionFactorObe { get; set; }
        public double Expired { get; set; }
        public double NonExpired { get; set; }
        public double CCF_Commercial { get; set; }
        public double CCF_Corporate { get; set; }
        public double CCF_Consumer { get; set; }
        public double CCF_OBE { get; set; }
        public List<ExchangeRate> ExchangeRates { get; set; }
        public List<VariableInterestRate> VariableInterestRates { get; set; }
        public double PrePaymentFactor  { get; set; }
    }


    public class ExchangeRate
    {
        public string Currency { get; set; }
        public double Value { get; set; }
    }
    public class VariableInterestRate
    {
        public string VIR_Name { get; set; }
        public double Value { get; set; }
    }
}
