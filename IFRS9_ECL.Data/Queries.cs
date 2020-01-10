using System;
using System.Collections.Generic;
using System.Text;

namespace IFRS9_ECL.Data
{
    public static class Queries
    {
        public static string PD_AssumptionSelectQry
        {
            get
            {
                return "select * from PDI_Assumptions";
            }
        }

        public static string LGD_PD_AssumptionSelectQry
        {
            get
            {
                return "select * from Wholesale_LGD_PD_Assumptions";
            }
        }

        public static string Raw_Data
        {
            get
            {
                return "select * from EclRawDataLoanBooks"; // where contractno='52003720'
            }
        }

        public static string PaymentSchedule
        {
            get
            {
                return "Select * from PaymentScheduleNew where COMPONENT!='GH_INTLN'";
            }
        }

        public static string LGD_Assumption_2 { get { return "Select COLLATERAL_TYPE, TTR_YEARS from LGD_Assumptions_2"; } }

        public static string LGD_Assumption { get { return "Select [collateral value] collateral_value,debenture, cash, inventory, plant_and_equipment, residential_property, commercial_property, shares, vehicle, [Cost of Recovery] costOfRecovery from LGD_Assumptions"; } }
    }
}
