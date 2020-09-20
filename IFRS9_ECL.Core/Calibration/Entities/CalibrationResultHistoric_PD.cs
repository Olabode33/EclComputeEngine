using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Entities
{
    public class CalibrationResultHistoric_PD_Output
    {
        public int Id { get; set; }
        public int Affiliate_ID { get; set; }
        public double Rating_1 { get; set; }
        public double Rating_2 { get; set; }
        public double Rating_3 { get; set; }
        public double Rating_4 { get; set; }
        public double Rating_5 { get; set; }
        public double Rating_6 { get; set; }
        public double Rating_7 { get; set; }
        public double Rating_8 { get; set; }
        public double Rating_9 { get; set; }
        public double Rating_10 { get; set; }
        public double Rating_Comm { get; set; }
        public double Rating_Cons { get; set; }
        public double Defaulted_Loan { get; set; }
        public double Cured_Loan { get; set; }
        public double Redefaulted_Loans { get; set; }
    }

    public class CalibrationResultHistoric_PD_Corporate
    {
        public int Id { get; set; }
        public int Affiliate_ID { get; set; }
        public int RAPPDATE { get; set; }
        public double OutstandingBalance_1 { get; set; }
        public double OutstandingBalance_2 { get; set; }
        public double OutstandingBalance_3 { get; set; }
        public double OutstandingBalance_4 { get; set; }
        public double OutstandingBalance_5 { get; set; }
        public double OutstandingBalance_6 { get; set; }
        public double OutstandingBalance_7 { get; set; }
        public double OutstandingBalance_8 { get; set; }
        public double OutstandingBalance_9 { get; set; }
        public double OutstandingBalance_10 { get; set; }
        public double Balance_1 { get; set; }
        public double Balance_2 { get; set; }
        public double Balance_3 { get; set; }
        public double Balance_4 { get; set; }
        public double Balance_5 { get; set; }
        public double Balance_6 { get; set; }
        public double Balance_7 { get; set; }
        public double Balance_8 { get; set; }
        public double Balance_9 { get; set; }
        public double Balance_10 { get; set; }
    }

    public class CalibrationResultHistoric_PD_CommsCons
    {
        public int Id { get; set; }
        public int Affiliate_ID { get; set; }
        public int Stage { get; set; }
        public double Comm_1 { get; set; }
        public double Comm_2 { get; set; }
        public double Comm_3 { get; set; }
        public double Cons_1 { get; set; }
        public double Cons_2 { get; set; }
        public double Cons_3 { get; set; }
    }
}
