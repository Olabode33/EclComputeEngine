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
            return $"select Id, PdGroup, Month, Value, {eclType.ToString()}EclId from {tableName} where {eclType.ToString()}EclId ='{eclId}' order by id";
        }

        public static string CalibrationInput_EAD_Behavioural_Terms(Guid calibrationId)
        {
            return $"select * from CalibrationInput_EAD_Behavioural_Terms where CalibrationID ='{calibrationId}' order by id";
        }

        public static string CalibrationResult_EAD_Behavioural_Terms_Update(Guid calibrationId, string assumption_nonExpired, string freq_nonExpired, string assumption_Expired, string freq_Expired)
        {
            return $"delete from CalibrationResult_EAD_Behavioural_Terms where CalibrationID ='{calibrationId.ToString()}'; insert into CalibrationResult_EAD_Behavioural_Terms(Assumption_NonExpired, Freq_NonExpired, Assumption_Expired, Freq_Expired, Comment, Status, CalibrationId, DateCreated) values ('{assumption_nonExpired}', '{freq_nonExpired}', '{assumption_Expired}', '{freq_Expired}', '', 1, '{calibrationId.ToString()}', GetDate())";
        }

        public static string GetEADBehaviouralData(Guid eclId, string eclType)
        {
            return $"select top 1 * from CalibrationResult_EAD_Behavioural_Terms where CalibrationID=(select Id from CalibrationRunEadBehaviouralTerms where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        }
        public static string GetEADCCFData(Guid eclId, string eclType)
        {
            return $"select top 1 * from CalibrationInput_EAD_CCF_Summary where CalibrationID=(select Id from CalibrationRunEadCcfSummary where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        }
        public static string GetLGDHaircutSummaryData(Guid eclId, string eclType)
        {
            return $"select top 1 * from CalibrationResult_LGD_HairCut_Summary where CalibrationID=(select Id from CalibrationRunLgdHairCut where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        }

        public static string GetLGDRecoveryRateData(Guid eclId, string eclType)
        {
            return $"select top 1 Overall_RecoveryRate from CalibrationResult_LGD_RecoveryRate where CalibrationID=(select Id from CalibrationRunLgdRecoveryRate where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        }
        public static string GetPD12MonthsPD(Guid eclId, string eclType)
        {
            return $"select Rating, Months_PDs_12 from CalibrationResult_PD_12Months where CalibrationID=(select Id from CalibrationRunPdCrDrs where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        }
        
        public static string GetPDIndexData(Guid eclId, string eclType)
        {
            return $"select Period, Index, StandardIndex, BfNpl from MacroResult_IndexData where MacroId=(select Id from CalibrationRunMacroAnalysis where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        }
        public static string GetPDStatistics(Guid eclId, string eclType)
        {
            return $"select top 1 IndexWeight1, IndexWeight2,IndexWeight3, IndexWeight4, Average, StandardDev from MacroResult_Statistics where MacroId=(select Id from CalibrationRunMacroAnalysis where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        }
        public static string GetSelectMacroVariables(Guid eclId, string eclType)
        {
            return $"select s.*, m.Description, m.Name from MacroResult_SelectedMacroEconomicVariables s left join MacroeconomicVariables m on (m.Id=s.MacroeconomicVariableId) where s.AffiliateId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') ";
        }
        public static string GetPDRedefaultFactor(Guid eclId, string eclType)
        {
            return $"select top 1 Redefault_Factor, Cure_Rate from CalibrationResult_PD_12Months_Summary where CalibrationID=(select Id from CalibrationRunPdCrDrs where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        }

        public static string Affiliate_MacroeconomicVariable(long affiliateId)
        {
            return $"select * from AffiliateMacroEconomicVariableOffsets where AffiliateId={affiliateId} order by id";
        }

        public static string Macro_Analysis(int macroid)
        {
            return $"select * from MacroenonomicData where macroId ={macroid} order by id";
        }

        public static string CalibrationInput_EAD_CCF(Guid calibrationId)
        {
            return $"select * from CalibrationInput_EAD_CCF_Summary where CalibrationID ='{calibrationId.ToString()}' order by id";
        }
        public static string CalibrationResult_EAD_CCF_Summary_Update(Guid calibrationId, double? oD_TotalLimitOdDefaultedLoan, double? oD_BalanceAtDefault, double? oD_Balance12MonthBeforeDefault, double? oD_TotalConversation, double? oD_CCF, double? card_TotalLimitOdDefaultedLoan, double? card_BalanceAtDefault, double? card_Balance12MonthBeforeDefault, double? card_TotalConversation, double? card_CCF, double? overall_TotalLimitOdDefaultedLoan, double? overall_BalanceAtDefault, double? overall_Balance12MonthBeforeDefault, double? overall_TotalConversation, double? overall_CCF)
        {
            return $"delete from CalibrationResult_EAD_CCF_Summary where CalibrationID ='{calibrationId.ToString()}'; insert into CalibrationResult_EAD_CCF_Summary(OD_TotalLimitOdDefaultedLoan, OD_BalanceAtDefault, OD_Balance12MonthBeforeDefault, OD_TotalConversation, OD_CCF, Card_TotalLimitOdDefaultedLoan, Card_BalanceAtDefault, Card_Balance12MonthBeforeDefault, Card_TotalConversation, Card_CCF, Overall_TotalLimitOdDefaultedLoan, " +
                $"Overall_BalanceAtDefault, Overall_Balance12MonthBeforeDefault, Overall_TotalConversation, Overall_CCF, Comment, Status, CalibrationId, DateCreated) values ({oD_TotalLimitOdDefaultedLoan}, {oD_BalanceAtDefault}, {oD_Balance12MonthBeforeDefault}, {oD_TotalConversation}, {oD_CCF}, {card_TotalLimitOdDefaultedLoan}, {card_BalanceAtDefault}, {card_Balance12MonthBeforeDefault}, {card_TotalConversation}, {card_CCF}, {overall_TotalLimitOdDefaultedLoan}, " +
                $"{overall_BalanceAtDefault}, {overall_Balance12MonthBeforeDefault}, {overall_TotalConversation}, {overall_CCF}, '', 1, '{calibrationId.ToString()}', GetDate())";
        }

        public static string CalibrationInput_Haircut(Guid calibrationId)
        {
            return $"select * from CalibrationInput_LGD_HairCut where CalibrationID ='{calibrationId}' order by id";
        }


        public static string CalibrationResult_HairCut_Update(Guid calibrationId, DateTime? Period, double? Debenture, double? Cash, double? Inventory, double? Plant_And_Equipment, double? Residential_Property, double? Commercial_Property, double? Receivables, double? Shares, double? Vehicle)
        {
            var prd = Period == null ? "NULL" : Period.Value.ToString("dd-MMM-yyyy");
            return $" insert into CalibrationResult_LGD_HairCut(Debenture,Cash,Inventory,Plant_And_Equipment,Residential_Property,Commercial_Property,Receivables,Shares,Vehicle, Comment, Status, CalibrationId, DateCreated) " +
                $"values ({Debenture}, {Cash}, {Inventory}, {Plant_And_Equipment}, {Residential_Property}, {Commercial_Property}, {Receivables}, {Shares}, {Vehicle}, '', 1, '{calibrationId.ToString()}', GetDate()); {Environment.NewLine} ";
    //        return $" insert into CalibrationResult_LGD_HairCut([Period],Debenture,Cash,Inventory,Plant_And_Equipment,Residential_Property,Commercial_Property,Receivables,Shares,Vehicle, Comment, Status, CalibrationId, DateCreated) " +
    //$"values ('{prd}', {Debenture}, {Cash}, {Inventory}, {Plant_And_Equipment}, {Residential_Property}, {Commercial_Property}, {Receivables}, {Shares}, {Vehicle}, '', 1, '{calibrationId.ToString()}', GetDate()); {Environment.NewLine} ";
        }

        public static string CalibrationResult_HairCut_Summary_Update(Guid calibrationId, double? Debenture, double? Cash, double? Inventory, double? Plant_And_Equipment, double? Residential_Property, double? Commercial_Property, double? Receivables, double? Shares, double? Vehicle)
        {
            return $" insert into CalibrationResult_LGD_HairCut_Summary(Debenture,Cash,Inventory,Plant_And_Equipment,Residential_Property,Commercial_Property,Receivables,Shares,Vehicle, Comment, Status, CalibrationId, DateCreated) " +
                $"values ({Debenture}, {Cash}, {Inventory}, {Plant_And_Equipment}, {Residential_Property}, {Commercial_Property}, {Receivables}, {Shares}, {Vehicle}, '', 1, '{calibrationId.ToString()}', GetDate()); ";
        }
        public static string CalibrationResult_HairCut_UpdateFinal(Guid calibrationId, string subQry)
        {
            return $"delete from CalibrationResult_LGD_HairCut where CalibrationID ='{calibrationId.ToString()}'; delete from CalibrationResult_LGD_HairCut_Summary where CalibrationID ='{calibrationId.ToString()}'; {subQry}";
        }


        public static string CalibrationInput_RecoveryRate(Guid calibrationId)
        {
            return $"select * from CalibrationInput_LGD_RecoveryRate where CalibrationID ='{calibrationId}' order by id";
        }

        public static string CalibrationInput_PD_CR_DR(Guid calibrationId)
        {
            return $"select * from CalibrationInput_PD_CR_DR where CalibrationID ='{calibrationId}' order by id";
        }

        public static string CalibrationResult_LGD_RecoveryRate_Update(Guid calibrationId, double? overall_Exposure_At_Default, double? overall_PvOfAmountReceived, double? overall_Count, double? overall_RecoveryRate, double? corporate_Exposure_At_Default, double? corporate_PvOfAmountReceived, double? corporate_Count, double? corporate_RecoveryRate, double? commercial_Exposure_At_Default, double? commercial_PvOfAmountReceived, double? commercial_Count, double? commercial_RecoveryRate, double? consumer_Exposure_At_Default, double? consumer_PvOfAmountReceived, double? consumer_Count, double? consumer_RecoveryRate)
        {
            return $"delete from CalibrationResult_LGD_RecoveryRate where CalibrationID ='{calibrationId.ToString()}'; insert into" +
                $" CalibrationResult_LGD_RecoveryRate(overall_Exposure_At_Default,overall_PvOfAmountReceived,overall_Count, overall_RecoveryRate,corporate_Exposure_At_Default, corporate_PvOfAmountReceived, corporate_Count, corporate_RecoveryRate, commercial_Exposure_At_Default, commercial_PvOfAmountReceived, commercial_Count, commercial_RecoveryRate, consumer_Exposure_At_Default, consumer_PvOfAmountReceived, consumer_Count,  consumer_RecoveryRate  , Comment, Status, CalibrationId, DateCreated) " +
                $"                            values({overall_Exposure_At_Default},{overall_PvOfAmountReceived},{overall_Count}, {overall_RecoveryRate}, {corporate_Exposure_At_Default}, {corporate_PvOfAmountReceived}, {corporate_Count}, {corporate_RecoveryRate}, {commercial_Exposure_At_Default}, {commercial_PvOfAmountReceived}, {commercial_Count}, {commercial_RecoveryRate}, {consumer_Exposure_At_Default}, {consumer_PvOfAmountReceived}, {consumer_Count},{consumer_RecoveryRate},  '', 1, '{calibrationId.ToString()}', GetDate()); ";
        }

        public static string CalibrationResult_PD_Update(Guid calibrationId, double? Rating, double? Outstanding_Balance, double? Redefault_Balance, double? Redefaulted_Balance, double? Total_Redefault, double? Months_PDs_12)
        {
            return $" insert into CalibrationResult_PD_12Months(Rating, Outstanding_Balance, Redefault_Balance, Redefaulted_Balance, Total_Redefault, Months_PDs_12, Comment, Status, CalibrationId, DateCreated) values({Rating}, {Outstanding_Balance}, {Redefault_Balance}, {Redefaulted_Balance}, {Total_Redefault}, {Months_PDs_12},'', 1, '{calibrationId.ToString()}', GetDate()); ";
        }

        public static string CalibrationResult_PD_Update_Summary(Guid calibrationId, string lstQry, double? Normal_12_Months_PD, double? DefaultedLoansA, double? DefaultedLoansB, double? CuredLoansA, double? CuredLoansB, double? Cure_Rate, double? CuredPopulationA, double? CuredPopulationB, double? RedefaultedLoansA, double? RedefaultedLoansB, double? Redefault_Rate, double? Redefault_Factor)
        {
            return $"delete from CalibrationResult_PD_12Months where CalibrationID ='{calibrationId.ToString()}'; delete from CalibrationResult_PD_12Months_Summary where CalibrationID ='{calibrationId.ToString()}'; {lstQry} insert into CalibrationResult_PD_12Months_Summary(Normal_12_Months_PD, DefaultedLoansA, DefaultedLoansB, CuredLoansA, CuredLoansB, Cure_Rate, CuredPopulationA, CuredPopulationB, RedefaultedLoansA, RedefaultedLoansB, Redefault_Rate, Redefault_Factor, Comment, Status, CalibrationId, DateCreated) values({Normal_12_Months_PD}, {DefaultedLoansA}, {DefaultedLoansB}, {CuredLoansA}, {CuredLoansB}, {Cure_Rate}, {CuredPopulationA}, {CuredPopulationB}, {RedefaultedLoansA}, {RedefaultedLoansB}, {Redefault_Rate}, {Redefault_Factor},  '', 1, '{calibrationId.ToString()}', GetDate()); ";
        }
        public static string MacroResult_BatchInsert(int macroId, string lstQry, long affiliateId)
        {
            return $"delete from MacroResult_CorMat where MacroId ={macroId};  {Environment.NewLine}" +
                $" delete from MacroResult_IndexData where MacroId ={macroId};  {Environment.NewLine}" +
                $" delete from MacroResult_PrincipalComponent where MacroId ={macroId};  {Environment.NewLine}" +
                $" delete from MacroResult_PrincipalComponentSummary where MacroId ={macroId};  {Environment.NewLine}" +
                $" delete from MacroResult_Statistics where MacroId ={macroId};  {Environment.NewLine}" +
                $" delete from MacroResult_SelectedMacroEconomicVariables where affiliateId={affiliateId};  {Environment.NewLine}" +
                $"{lstQry}";

        }

        public static string MacroResult_PrinC(int macroId, double? p1, double? p2, double? p3, double? p4)
        {
            return $" insert into MacroResult_PrincipalComponent(macroId, PrincipalComponent1, PrincipalComponent2, PrincipalComponent3, PrincipalComponent4, DateCreated) values({macroId}, {p1}, {p2}, {p3}, {p4}, getdate()); {Environment.NewLine}";
        }

        public static string MacroResult_IndxResult(int macroId, string period, double? index, double? standardIndex, double? bfNpl)
        {
            return $" insert into MacroResult_IndexData(macroId, [period], [index], standardIndex, bfNpl, DateCreated) values({macroId}, '{period}', {index}, {standardIndex}, {bfNpl}, getdate());  {Environment.NewLine}";
        }

        public static string EclsRegister(string eclType)
        {
            return $"select top 1 Id, ReportingDate, IsApproved, Status, EclType=-1 from {eclType.ToString()}Ecls where status=2";
        }
        public static string EclsRegisterUpdate(string eclType, string eclId, int status, string exception)
        {
            return $"update {eclType.ToString()}Ecls set status={status}, ExceptionComment='{exception}' where Id ='{eclId}'";
        }
        public static string CalibrationBehavioural()
        {
            return $"select top 1 Id, OrganizationUnitId AffiliateId from CalibrationRunEadBehaviouralTerms where Status=2";
        }
        public static string CalibrationCCF()
        {
            return $"select top 1 Id, OrganizationUnitId AffiliateId from CalibrationRunEadCcfSummary where Status=2";
        }
        public static string CalibrationHaircut()
        {
            return $"select top 1 Id, OrganizationUnitId AffiliateId from CalibrationRunLgdHairCut where Status=2";
        }
        public static string CalibrationRecovery()
        {
            return $"select top 1 Id, OrganizationUnitId AffiliateId from CalibrationRunLgdRecoveryRate where Status=2";
        }
        public static string CalibrationPD()
        {
            return $"select top 1 Id, OrganizationUnitId AffiliateId from CalibrationRunPdCrDrs where Status=2";
        }

        public static string MacroRegister()
        {
            return $"select top 1 Id, OrganizationUnitId AffiliateId from CalibrationRunMacroAnalysis where Status=2";
        }

        public static string MacroResult_StatisticalIndex(int macroId, double? indexWeight1, double? indexWeight2, double? indexWeight3, double? indexWeight4, double? standardDev, double? average, double? correlation, double? tTC_PD)
        {
            return $" insert into MacroResult_Statistics(macroId,indexWeight1,indexWeight2,indexWeight3,indexWeight4,standardDev, average, correlation, tTC_PD, DateCreated) " +
                $"values({macroId},{indexWeight1},{indexWeight2},{indexWeight3},{indexWeight4},{standardDev}, {average}, {correlation}, {tTC_PD}, GetDate());  {Environment.NewLine}";
        }
        public static string MacroResult_CorMat(int macroId, int macroEconomicIdA, int macroEconomicIdB, string macroEconomicLabelA, string macroEconomicLabelB, double? value)
        {
            return $" insert into MacroResult_CorMat(macroId,macroEconomicIdA,macroEconomicIdB,macroEconomicLabelA,macroEconomicLabelB,value, DateCreated) " +
    $"values({macroId},{macroEconomicIdA},{macroEconomicIdB},'{macroEconomicLabelA}','{macroEconomicLabelB}',{value}, GetDate());  {Environment.NewLine}";
        }

        public static string MacroResult_PrincipalComponent(int macroId, int pcIdA, int pcIdB, string pcLabelA, string pcLabelB, double? value)
        {
            return $" insert into MacroResult_PrincipalComponentSummary(macroId,PrincipalComponentIdA,PrincipalComponentIdB,PricipalComponentLabelA,PricipalComponentLabelB,value, DateCreated) " +
                $"values({macroId},{pcIdA},{pcIdB},'{pcLabelA}','{pcLabelB}',{value}, GetDate());  {Environment.NewLine}";
        }
        public static string MacroResult_SelectedMacroEconomicVariables(int backwardLag, long affiliateId, int macroVariableId)
        {
            return $" insert into MacroResult_SelectedMacroEconomicVariables(BackwardOffset,AffiliateId,MacroeconomicVariableId) values({backwardLag},{affiliateId},{macroVariableId});  {Environment.NewLine}";
        }

        public static string MacroeconomicVariable()
        {
            return $"select Name, Description from MacroeconomicVariables";
        }

        public static string CalibrationRegisterUpdate(string caliId, int status, string exceptionComment, string tableName)
        {
            return $"update {tableName} set Status={status}, ExceptionComment='{exceptionComment}' where Id ='{caliId}'";
        }


        public static string MacroRegisterUpdate(int macroId, int status, string exceptionComment)
        {
            return $"update CalibrationRunMacroAnalysis set Status={status}, ExceptionComment='{exceptionComment}' where Id ={macroId}";
        }

        public static string Raw_Data(Guid guid, EclType eclType)
        {
            //******************************************************
            //return $"select * from {eclType.ToString()}EclDataLoanBooks where ContractNo='1762533824' and ContractNo not like ' %EXP%' and {eclType.ToString()}EclUploadId='{guid.ToString()}' ";
            return $"select * from {eclType.ToString()}EclDataLoanBooks where {eclType.ToString()}EclUploadId='{guid.ToString()}' ";
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

        public static string EclOverridesFsv(Guid eclId, EclType eclType)
        {
            return $"select * from {eclType.ToString()}EclOverrides where {eclType.ToString()}EclDataLoanBookId ='{eclId}'";
        }
        public static string CheckOverrideDataExist(Guid eclId, EclType eclType)
        {
            return $"select count(*) from {eclType.ToString()}EclOverrides where {eclType.ToString()}EclDataLoanBookId ='{eclId}'";
        }
        public static string EclOverridesStage(Guid eclId, EclType eclType)
        {
            return $"select ContractId, Stage from {eclType.ToString()}EclOverrides where {eclType.ToString()}EclDataLoanBookId ='{eclId}'";
        }
        public static string EclOverridesTTr(Guid eclId, EclType eclType)
        {
            return $"select ContractId, TtrYears from {eclType.ToString()}EclOverrides where {eclType.ToString()}EclDataLoanBookId ='{eclId}'";
        }

        public static string EclOverrideExist(Guid eclId, EclType eclType)
        {
            return $"select count(*) from {eclType.ToString()}EclOverrides where {eclType.ToString()}EclDataLoanBookId ='{eclId}'";
        }


        public static string EadCirProjections(Guid eclId, EclType eclType)
        {
            return $"select cir_group, month months, value, cir_effective from {eclType.ToString()}EadCirProjections where {eclType.ToString()}EclId ='{eclId}'";
        }

        public static string LgdCollateralProjection(Guid eclId, int collateralProjectionType, EclType eclType)
        {
            //return $"select CollateralProjectionType, Debenture, Cash, Inventory, Plant_And_Equipment, Residential_Property, Commercial_Property, Receivables, Shares, Vehicle, Month from {eclType.ToString()}LgdCollateralProjection where {eclType.ToString()}EclId = '{eclId}' and CollateralProjectionType={collateralProjectionType}";
            return $"select  [Key], [Value], LgdGroup from {eclType.ToString()}EclLgdAssumptions where {eclType.ToString()}EclId ='{eclId}' and LgdGroup = {collateralProjectionType}";
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

        public static string GetPDIndexData(object eclId, object p)
        {
            throw new NotImplementedException();
        }

        public static string eclLGDAssumptions(Guid eclId, EclType eclType)
        {
            return $"select [Key], Value, LgdGroup AssumptionGroup from {eclType.ToString()}EclLgdAssumptions where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }

        public static string Get_AffiliateMEVBackDateValues(Guid eclId, EclType eclType)
        {
            return $"select BackwardOffset, MacroeconomicVariableId from MacroResult_SelectedMacroEconomicVariables where AffiliateId=(select OrganizationUnitId from {eclType.ToString()}Ecls where Id='{eclId.ToString()}' )";
        }
    }
}
