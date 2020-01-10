using System;
using System.Collections.Generic;
using System.Text;

namespace IFRS9_ECL.Models
{
    public class PaymentSchedule
    {
        public string CONTRACT_REF_NO { get; set; }
        public string START_DATE { get; set; }
        public string COMPONENT { get; set; }
        public string NO_OF_SCHEDULES { get; set; }
        public string FREQUENCY { get; set; }
        public string AMOUNT { get; set; }
        public string CONTRACT_ID { get; set; }
        public string PAYMENT_TYPE { get; set; }
        public string MONTHS { get; set; }
        public double VALUE { get; set; }
    }

}
