using System;
using System.Collections.Generic;
using System.Text;

namespace IFRS9_ECL.Models
{
    public class Refined_Raw_Wholesale
    {
            public string contract_no {get; set;}
            public string segment {get; set;}
            public string currency {get; set;}
            public string product_type {get; set;}
            public double? credit_limit_lcy {get; set;}
            public string original_bal_lcy {get; set;}
            public string OUTSTANDING_BALANCE_LCY {get; set;}
            public DateTime? CONTRACT_START_DATE {get; set;}
            public DateTime? CONTRACT_END_DATE {get; set;}
            public int RESTRUCTURE_INDICATOR {get; set;}
            public DateTime? RESTRUCTURE_START_DATE {get; set;}
            public DateTime? RESTRUCTURE_END_DATE {get; set;}
            public string IPT_O_PERIOD {get; set;}
            public string PRINCIPAL_PAYMENT_STRUCTURE {get; set;}
            public string INTEREST_PAYMENT_STRUCTURE {get; set;}
            public string INTEREST_RATE_TYPE {get; set;}
            public string BASE_RATE {get; set;}
            public string ORIGINATION_CONTRACTUAL_INTEREST_RATE {get; set;}
            public string INTRODUCTORY_PERIOD {get; set;}
            public string POST_IP_CONTRACTUAL_INTEREST_RATE {get; set;}
            public string CURRENT_CONTRACTUAL_INTEREST_RATE {get; set;}
            public string EIR {get; set;}
            public double LIM_MONTH { get; set; }
    }
}
