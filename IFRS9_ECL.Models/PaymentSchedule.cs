using System;
using System.Collections.Generic;
using System.Text;

namespace IFRS9_ECL.Models
{
    public class PaymentSchedule
    {
        public string ContractRefNo { get; set; }
        public DateTime StartDate { get; set; }
        public string Component { get; set; }
        public int NoOfSchedules { get; set; }
        public string Frequency { get; set; }
        public double Amount { get; set; }
        public string ContractId { get; set; }
        public string PaymentType { get; set; }
        public string Months { get; set; }
        public double Value { get; set; }
    }

    public class TempPaymentSchedule
    {
        public string ContractRefNo { get; set; }
        public DateTime StartDate { get; set; }
        public string Component { get; set; }
        public int NoOfSchedules { get; set; }
        public string Frequency { get; set; }
        public double Amount { get; set; }
    }

}
