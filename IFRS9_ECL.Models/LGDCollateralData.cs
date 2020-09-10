using System;
using System.Collections.Generic;
using System.Text;

namespace IFRS9_ECL.Models
{
    public class LGDCollateralData
    {
        public Guid Id { get; set; }
        public string contract_no { get; set; }
        public string customer_no { get; set; }
        public double debenture_omv { get; set; }
        public double cash_omv { get; set; }
        public double inventory_omv { get; set; }
        public double plant_and_equipment_omv { get; set; }
        public double residential_property_omv { get; set; }
        public double commercial_property_omv { get; set; }
        public double receivables_omv { get; set; }
        public double shares_omv { get; set; }
        public double vehicle_omv { get; set; }
        public double total_omv { get; set; }
        public double debenture_fsv { get; set; }
        public double cash_fsv { get; set; }
        public double inventory_fsv { get; set; }
        public double plant_and_equipment_fsv { get; set; }
        public double residential_property_fsv { get; set; }
        public double commercial_property_fsv { get; set; }
        public double receivables_fsv { get; set; }
        public double shares_fsv { get; set; }
        public double vehicle_fsv { get; set; }
        public double total_fsv { get; set; }
    }
}
