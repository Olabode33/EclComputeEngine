using System;
using System.Collections.Generic;
using System.Text;

namespace IFRS9_ECL.Models
{
    public class LifeTimeEADs
    {
        public string contract_no{get; set;}
            public string segment{get; set;}
            public string credit_limit_lcy{get; set;}
            public DateTime? start_date{get; set;}
            public DateTime? end_date{get; set;}
            public string remaining_ip{get; set;}
            public string revised_base{get; set;}
            public string cir_premium{get; set;}
            public string eir_premium{get; set;}
            public string cir_base_premium{get; set;}
            public string eir_base_premium{get; set;}
            public string mths_in_force{get; set;}
            public string rem_interest_moritorium{get; set;}
            public string mths_to_expiry{get; set;}
            public string interest_divisor{get; set;}
            public string first_interest_month{get; set;}
            public double LIM_MONTH { get; set;}
    }


    public class EAD_Inputs
    {
        public double months_in_force { get; set; }
        public string first_interest_month { get; set; }

        public string restructure_start_dt {get; set;}
        public string restructure_end_dt {get; set;}
        public string restructure_indicator {get; set;}
        public string contract_end_dt {get; set;}
        public string contract_start_dt {get; set;}
        public string introductory_period {get; set;}
        public string start_date {get; set;}
        public string contract_no {get; set;}
        public string revised_base {get; set;}
        public string interest_rate_type {get; set;}
        public string base_rate {get; set;}
        public string current_contractual_ir {get; set;}
        public string post_ip_contractural_ir {get; set;}
        public double outstanding_balance_lcy {get; set;}
        public string product_type {get; set;}
        public double months_to_expiry {get; set;}
        public string segment {get; set;}
        public double rem_interest_moritorium {get; set;}
        public double credit_limit_lcy {get; set;}
        public string interest_divisor {get; set;}

    }
}
