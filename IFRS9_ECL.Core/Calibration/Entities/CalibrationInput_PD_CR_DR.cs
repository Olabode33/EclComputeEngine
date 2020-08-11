using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration.Input
{
    public class CalibrationInput_PD_CR_DR
    {
        public int Id { get; set; }

        public string Customer_No { get; set; }

        public string Account_No { get; set; }

        public string Contract_No { get; set; }

        public string Product_Type { get; set; }
public string Current_Rating { get; set; }
        public int? Days_Past_Due { get; set; }

        public string Classification { get; set; }

        public double? Outstanding_Balance_Lcy { get; set; }

        public DateTime? Contract_Start_Date { get; set; }

        public DateTime? Contract_End_Date { get; set; }

        public int? RAPP_Date { get; set; }

        public DateTime? DateCreated { get; set; }

        public Guid? CalibrationId { get; set; }

    }

}
