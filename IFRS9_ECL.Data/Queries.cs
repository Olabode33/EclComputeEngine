using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace IFRS9_ECL.Data
{
    public static class Queries
    {
        public static string LifetimePD_Query(string tableName, Guid eclId)
        {
            return $"select PdGroup, Month, Value from {tableName} where WholesaleEclId ='{eclId}'";
        }

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

        public static string EAD_GetEIRProjections(Guid eclId)
        {
            return $"select eir_group,month months,value from WholesaleEadCirProjections where WholesaleEclId='{eclId.ToString()}'";
        }

        public static string EAD_GetLifeTimeProjections(Guid eclId)
        {
            return $"select Contract_no, Eir_Group, Cir_Group, Month, Value from WholesaleEadLifetimeProjections where WholesaleEclId='{eclId.ToString()}'";
        }

        public static string PD_GetSIRCInputResult(Guid eclId)
        {
            return $"select ContractId, Pd12Month, LifetimePd, RedefaultLifetimePd, Stage1Transition, Stage2Transition, DaysPastDue from WholesalePdMappings where WholesaleEclId ='{eclId.ToString()}'";
        }

        public static string LGD_WholesaleLgdAccountDatas(Guid eclId)
        {
            return $"select Id, CONTRACT_NO, TTR_YEARS, COST_OF_RECOVERY, GUARANTOR_PD, GUARANTOR_LGD, GUARANTEE_VALUE, GUARANTEE_LEVEL from WholesaleLGDAccountData where WholesaleEclId ='{eclId.ToString()}'";
        }

        public static string Credit_Index(Guid eclId)
        {
            return $"select ProjectionMonth,BestEstimate, Optimistic, Downturn from {ECLStringConstants.i.WholesalePDCreditIndex_Table} where WholesaleEclId='{eclId.ToString()}'";
        }

        public static string LGD_WholesaleLgdCollateralDatas(Guid eclId)
        {
            return $"select Id, contract_no, customer_no, debenture_omv, cash_omv, inventory_omv, plant_and_equipment_omv, residential_property_omv, commercial_property_omv, receivables_omv, shares_omv, vehicle_omv, total_omv, debenture_fsv, cash_fsv, inventory_fsv, plant_and_equipment_fsv, residential_property_fsv, commercial_property_fsv, receivables_fsv, shares_fsv, vehicle_fsv from WholesaleLGDAccountData where WholesaleEclId ='{eclId}'";
        }

        public static string WholesaleEadCirProjections(Guid eclId)
        {
            return $"select cir_group, month months, value, cir_effective from WholesaleEadCirProjections where WholesaleEclId ='{eclId}'";
        }

        public static string LgdCollateralProjection(Guid eclId, int collateralProjectionType)
        {
            return $"select CollateralProjectionType, Debenture, Cash, Inventory, Plant_And_Equipment, Residential_Property, Commercial_Property, Receivables, Shares, Vehicle, Month from WholesaleLgdCollateralProjection where WholesaleEclId='{eclId}' and CollateralProjectionType={collateralProjectionType}";
        }




        public static string PdMapping(Guid eclId)
        {
            return $"select ContractId, AccountNo, ProductType, PdGroup, TtmMonths, MaxDpd, MaxClassificationScore, Pd12Month, LifetimePd, RedefaultLifetimePD, Stage1Transition, Stage2Transition, DaysPastDue, RatingModel, Segment, RatingUsed, ClassificationScore from WholesalePdMappings where WholesaleEclId ='{eclId}' ";
        }

        public static string LGD_InputAssumptions_UnsecuredRecovery(Guid eclId)
        {
            return $"select Segment_Product_Type, Cure_Rate, Days_0, Days_90=0, Days_180=0, Days_270=0, Days_360=0, Downturn_Days_0=0, Downturn_Days_90=0, Downturn_Days_180=0, Downturn_Days_270=0, Downturn_Days_360=0 from WholesaleLgdInputAssumptions_UnsecuredRecovery where WholesaleEclId='{eclId}'";
        }

        public static string eclAssumptions(Guid eclId)
        {
            return $"select Key, Value, LgdGroup from WholesaleEclAssumptions where WholesaleEclId='{eclId}'";
        }
    }
}
