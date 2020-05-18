using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace IFRS9_ECL.Data
{
    public static class Queries
    {
        public static string LifetimePD_Query(string tableName, Guid eclId, EclType eclType)
        {
            return $"select Id, PdGroup, Month, Value, {eclType.ToString()}EclId from {tableName} where {eclType.ToString()}EclId ='{eclId}'";
        }
        
        public static string EclsRegister(string eclType)
        {
            return $"select top 1 Id, ReportingDate, IsApproved, Status, EclType=-1 from {eclType.ToString()}Ecls where status=2";
        }

        public static string Raw_Data(Guid guid, EclType eclType)
        {
            //return $"select * from {eclType.ToString()}EclDataLoanBooks where ContractNo='1762533824' and ContractNo not like ' %EXP%' and {eclType.ToString()}EclUploadId='{guid.ToString()}' ";
            return $"select top 10 * from {eclType.ToString()}EclDataLoanBooks where {eclType.ToString()}EclUploadId='{guid.ToString()}' ";
        }

        public static string PaymentSchedule(Guid guid, EclType eclType)
        {
                return $"Select ContractRefNo, StartDate, Component, NoOfSchedules, Frequency, Amount  from {eclType.ToString()}EclDataPaymentSchedules where {eclType.ToString()}EclUploadId='{guid.ToString()}' and COMPONENT!='GH_INTLN'";
        }

        public static string LGD_Assumption { get { return "Select [collateral value] collateral_value,debenture, cash, inventory, plant_and_equipment, residential_property, commercial_property, shares, vehicle, [Cost of Recovery] costOfRecovery from LGD_Assumptions"; } }

        public static string EAD_GetEIRProjections(Guid eclId, EclType eclType)
        {
            return $"select eir_group,month months,value from {eclType.ToString()}EadEirProjections where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }

        public static string EAD_GetLifeTimeProjections(Guid eclId, EclType eclType)
        {
            return $"select Contract_no, Eir_Group, Cir_Group, Month, Value from {eclType.ToString()}EadLifetimeProjections where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }

        public static string PD_GetSIRCInputResult(Guid eclId, EclType eclType)
        {
            return $"select ContractId, Pd12Month, LifetimePd, RedefaultLifetimePd, Stage1Transition, Stage2Transition, DaysPastDue from {eclType.ToString()}PdMappings where {eclType.ToString()}EclId ='{eclId.ToString()}'";
        }

        public static string LGD_LgdAccountDatas(Guid eclId, EclType eclType)
        {
            //xxxxxxxxxxxxxxxxxxxxxx
            return $"select Id, CONTRACT_NO, TTR_YEARS, COST_OF_RECOVERY, GUARANTOR_PD, GUARANTOR_LGD, GUARANTEE_VALUE, GUARANTEE_LEVEL from {eclType.ToString()}LGDAccountData where {eclType.ToString()}EclId ='{eclId.ToString()}'";
        }

        public static string Credit_Index(Guid eclId, EclType eclType)
        {
            return $"select Id, ProjectionMonth,BestEstimate, Optimistic, Downturn, {eclType.ToString()}EclId from {ECLStringConstants.i.PDCreditIndex_Table(eclType)} where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }

        public static string LGD_LgdCollateralDatas(Guid eclId, EclType eclType)
        {
            return $"select Id, contract_no, customer_no, debenture_omv, cash_omv, inventory_omv, plant_and_equipment_omv, residential_property_omv, commercial_property_omv, receivables_omv, shares_omv, vehicle_omv, total_omv, debenture_fsv, cash_fsv, inventory_fsv, plant_and_equipment_fsv, residential_property_fsv, commercial_property_fsv, receivables_fsv, shares_fsv, vehicle_fsv from {eclType.ToString()}LGDCollateral where {eclType.ToString()}EclId ='{eclId}'";
        }

        public static string WholesaleEadCirProjections(Guid eclId, EclType eclType)
        {
            return $"select cir_group, month months, value, cir_effective from {eclType.ToString()}EadCirProjections where {eclType.ToString()}EclId ='{eclId}'";
        }

        public static string LgdCollateralProjection(Guid eclId, int collateralProjectionType, EclType eclType)
        {
            return $"select CollateralProjectionType, Debenture, Cash, Inventory, Plant_And_Equipment, Residential_Property, Commercial_Property, Receivables, Shares, Vehicle, Month from {eclType.ToString()}LgdCollateralProjection where {eclType.ToString()}EclId = '{eclId}' and CollateralProjectionType={collateralProjectionType}";
        }

        public static string PdMapping(Guid eclId, EclType eclType)
        {
            return $"select p.Id, p.ContractId, l.AccountNo, l.ProductType, p.PdGroup, p.TtmMonths, p.MaxDpd, p.MaxClassificationScore, p.Pd12Month, p.LifetimePd, p.RedefaultLifetimePD, p.Stage1Transition, p.Stage2Transition, p.DaysPastDue, l.RatingModel, l.Segment, RatingUsed=0, ClassificationScore=0,  p.{eclType.ToString()}EclId from {eclType.ToString()}PdMappings p left join {eclType.ToString()}EclDataLoanBooks l on (p.ContractId=l.contractno) where p.{eclType.ToString()}EclId ='{eclId}' and l.{eclType.ToString()}EclUploadId ='{eclId}' and l.ContractNo not like '%EXP%'";
        }

        public static string LGD_InputAssumptions_UnsecuredRecovery(Guid eclId, EclType eclType)
        {
            return $"select [Key] Segment_Product_Type, Value Cure_Rate, Value Days_0, Days_90=0, Days_180=0, Days_270=0, Days_360=0, Downturn_Days_0=0, Downturn_Days_90=0, Downturn_Days_180=0, Downturn_Days_270=0, Downturn_Days_360=0 from LgdInputAssumptions where LgdGroup in (1,2) order by 1";// where {eclType.ToString()}EclId='{eclId}'";
        }

        public static string eclFrameworkAssumptions(Guid eclId, EclType eclType)
        {
            return $"select [Key], Value, AssumptionGroup from {eclType.ToString()}EclAssumptions where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }
        public static string eclLGDAssumptions(Guid eclId, EclType eclType)
        {
            return $"select [Key], Value, LgdGroup AssumptionGroup from {eclType.ToString()}EclLgdAssumptions where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }

        public static string Get_AffiliateMEVBackDateValues(Guid eclId, EclType eclType)
        {
            return $"select BackwardOffset, MacroeconomicVariableId from AffiliateMacroEconomicVariableOffsets where AffiliateId=(select OrganizationUnitId from {eclType.ToString()}Ecls where Id='{eclId.ToString()}' )";
        }
    }
}
