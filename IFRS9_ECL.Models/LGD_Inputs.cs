using System;
using System.Collections.Generic;
using System.Text;

namespace IFRS9_ECL.Models
{
    public class LGD_Inputs
    {
        public double total {get; set; }
        public string account_no { get; set; }

        public double debenture_omv { get; set; }
        public double cash_omv {get; set; }
        public double inventory_omv {get; set; }
        public double plant_and_equipment_omv {get; set; }
        public double residential_property_omv {get; set; }
        public double commercial_property_omv { get; set; }
        public double receivables_omv { get; set; }
        public double shares_omv { get; set; }
        public double vehicle_omv { get; set; }

        public double debenture_fsv { get; set; }
        public double cash_fsv { get; set; }
        public double inventory_fsv { get; set; }
        public double plant_and_equipment_fsv { get; set; }
        public double residential_property_fsv { get; set; }
        public double commercial_property_fsv { get; set; }
        public double receivables_fsv { get; set; }
        public double shares_fsv { get; set; }
        public double vehicle_fsv { get; set; }

        public string specialised_lending {get; set; }
        public double pd_x_ead {get; set; }
        public string product_type {get; set; }
        public string new_contract_no {get; set; }
        public bool restructure_indicator {get; set; }
        public DateTime? restructure_end_date {get; set; }
        public DateTime? contract_end_date {get; set; }
        public string rating_model {get; set; }
        public string segment {get; set; }
        public double days_past_due {get; set; }
        public string rating_used {get; set; }
        public int current_rating {get; set; }
        public double month_pd_12 {get; set; }
        public string customer_no {get; set; }
        public double project_finance_ind {get; set; }
        public string guarantee_value {get; set; }
        public string contractid {get; set; }

        //COR sheet
        public double collateral_value {get; set; }

    }
}
