﻿using IFRS9_ECL.Util;
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
            return $"select Id=-1, Customer_No,Account_No,Contract_No,Customer_Name,Snapshot_Date,Classification,Original_Balance_Lcy,Outstanding_Balance_Lcy,Outstanding_Balance_Acy,Contract_Start_Date,Contract_End_Date,Restructure_Indicator,Restructure_Type,Restructure_Start_Date,Restructure_End_Date from CalibrationInput_EAD_Behavioural_Terms where CalibrationID = '{calibrationId}' union all select Id, Customer_No,Account_No,Contract_No,Customer_Name,Snapshot_Date,Classification,Original_Balance_Lcy,Outstanding_Balance_Lcy,Outstanding_Balance_Acy,Contract_Start_Date,Contract_End_Date,Restructure_Indicator,Restructure_Type,Restructure_Start_Date,Restructure_End_Date from CalibrationHistory_EAD_Behavioural_Terms where AffiliateId = (select OrganizationUnitId from CalibrationRunEadBehaviouralTerms where Id = '{calibrationId}') Order by Account_No, Contract_No, Snapshot_Date";// order by Snapshot_Date desc";
        }

        public static string VariableInterestRate(string eclType, string eclId)
        {
            return $"select Value, InputName from {eclType}EclEadInputAssumptions where {eclType}EclId ='{eclId}' and [Key] like '%VariableInterestRateProjection%'";
        }

        public static string CalibrationResult_EAD_Behavioural_Terms_Update(Guid calibrationId, string assumption_nonExpired, string freq_nonExpired, string assumption_Expired, string freq_Expired)
        {
            return $"delete from CalibrationResult_EAD_Behavioural_Terms where CalibrationID ='{calibrationId.ToString()}'; insert into CalibrationResult_EAD_Behavioural_Terms(Assumption_NonExpired, Freq_NonExpired, Assumption_Expired, Freq_Expired, Comment, Status, CalibrationId, DateCreated) values ('{assumption_nonExpired}', '{freq_nonExpired}', '{assumption_Expired}', '{freq_Expired}', '', 1, '{calibrationId.ToString()}', GetDate())";
        }

        public static string GetEADBehaviouralData(Guid eclId, string eclType)
        {
            return $"select top 1 * from CalibrationResult_EAD_Behavioural_Terms where CalibrationID=(select BehaviouralCalibrationId from WholesaleEcls where Id='{eclId}')";
        }
        public static string GetEADCCFData(Guid eclId, string eclType)
        {
            return $"select top 1 * from CalibrationResult_EAD_CCF_Summary where CalibrationID=(select CCFCalibrationId from WholesaleEcls where Id='{eclId}')";
        }
        //public static string GetLGDHaircutSummaryData(Guid eclId, string eclType)
        //{
        //    return $"select top 1 * from CalibrationResult_LGD_HairCut_Summary where CalibrationID=(select top 1 Id from CalibrationRunLgdHairCut where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        //}

        public static string GetLGDHaircutSummaryData(Guid eclId, string eclType)
        {
            return $"select top 1 * from CalibrationResult_LGD_HairCut_Summary where CalibrationID=(select HaircutCalibrationId from WholesaleEcls where Id='{eclId}')";
        }

        //public static string GetLGDRecoveryRateData(Guid eclId, string eclType)
        //{
        //    return $"select top 1 * from CalibrationResult_LGD_RecoveryRate where CalibrationID=(select top 1 Id from CalibrationRunLgdRecoveryRate where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        //}
        public static string GetLGDRecoveryRateData(Guid eclId, string eclType)
        {
            return $"select top 1 * from CalibrationResult_LGD_RecoveryRate where CalibrationID=(select RecoveryRateCalibrationId from WholesaleEcls where Id='{eclId}')";
        }
        public static string GetPD12MonthsPD(Guid eclId, string eclType)
        {
            return $"select Rating, Months_PDs_12 from CalibrationResult_PD_12Months where CalibrationID=(select PDCrDrCalibrationId from WholesaleEcls where Id='{eclId}')";
        }
        
        public static string GetPDIndexData(Guid eclId, string eclType)
        {
            return $"select Period, Index, StandardIndex, BfNpl from MacroResult_IndexData where MacroId=(select top 1 Id from CalibrationRunMacroAnalysis where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        }
        public static string GetPDStatistics(Guid eclId, string eclType)
        {
            return $"select top 1 IndexWeight1, IndexWeight2,IndexWeight3, IndexWeight4, Average, StandardDev from MacroResult_Statistics where MacroId=(select Id from CalibrationRunMacroAnalysis where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        }
        public static string GetPrincipalComponentSummary(Guid eclId, string eclType)
        {
            return $"select * from MacroResult_PrincipalComponentSummary where MacroId=(select Id from CalibrationRunMacroAnalysis where OrganizationUnitId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') and Status=7)";
        }
        public static string ClearFrameworkReportTable(Guid eclId, EclType eclType)
        {
            return $"delete from {eclType.ToString()}EclFramworkReportDetail where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }

        public static string GetSelectMacroVariables(Guid eclId, string eclType)
        {
            return $"select s.*, m.Description, m.Name from MacroResult_SelectedMacroEconomicVariables s left join MacroeconomicVariables m on (m.Id=s.MacroeconomicVariableId) where s.AffiliateId=(select OrganizationUnitId from {eclType}Ecls where Id='{eclId.ToString()}') ";
        }
        public static string GetPDRedefaultFactor(Guid eclId, string eclType)
        {
            return $"select top 1 Redefault_Factor, Cure_Rate,Commercial_CureRate, Consumer_CureRate from CalibrationResult_PD_12Months_Summary where CalibrationID=(select PDCrDrCalibrationId from WholesaleEcls where Id='{eclId}')";
        }

        public static string Affiliate_MacroeconomicVariable(long affiliateId)
        {
            return $"select a.* from AffiliateMacroEconomicVariableOffsets a left join MacroeconomicVariables v on (a.MacroeconomicVariableId=v.Id) where a.AffiliateId={affiliateId} order by a.id";
        }

        public static string Macro_Analysis(int macroid)
        {
            return $"select * from MacroenonomicData where macroId ={macroid} order by id";
        }

        public static string CalibrationInput_EAD_CCF(Guid calibrationId)
        {
            return $"select Id, Customer_No,Account_No,Product_Type,Snapshot_Date,Contract_Start_Date,Contract_End_Date,Limit,Outstanding_Balance,Classification,Settlement_Account from ( select Id=-1, Customer_No,Account_No,Product_Type,Snapshot_Date,Contract_Start_Date,Contract_End_Date,Limit,Outstanding_Balance,Classification,Settlement_Account from CalibrationInput_EAD_CCF_Summary  where CalibrationID = '{calibrationId.ToString()}' union all select Id, Customer_No,Account_No,Product_Type,Snapshot_Date,Contract_Start_Date,Contract_End_Date,Limit,Outstanding_Balance,Classification,Settlement_Account from CalibrationHistory_EAD_CCF_Summary where AffiliateId = (select OrganizationUnitId from CalibrationRunEadCcfSummary where Id = '{calibrationId}')) s";
        }

        public static string CalibrationResult_IVReceivables(Guid calibrationId, double totalExposure, double totalImpairment, double additionalProvision, double coverage, double optimisticExposure, double baseExposure, double downturnExposure, double eCLTotalExposure, double optimisticImpairment, double baseImpairment, double downturnImpairment, double eCLTotalImpairment, double optimisticCoverageRatio, double baseCoverageRatio, double downturnCoverageRatio, double totalCoverageRatio)
        {
            return $"delete from ReceivablesResults where RegisterId ='{calibrationId.ToString()}'; " +
                   $"insert into ReceivablesResults(id, totalExposure, totalImpairment, additionalProvision, coverage, optimisticExposure, " +
                   $" baseExposure, downturnExposure, eCLTotalExposure, optimisticImpairment, baseImpairment, downturnImpairment, eCLTotalImpairment, " +
                   $" optimisticCoverageRatio, baseCoverageRatio, downturnCoverageRatio, totalCoverageRatio, RegisterId) " +
                   $" values (newid(), {totalExposure}, {totalImpairment}, {additionalProvision}, {coverage}, {optimisticExposure}, {baseExposure}, {downturnExposure}, {eCLTotalExposure}, " +
                   $" {optimisticImpairment}, {baseImpairment} , {downturnImpairment}, {eCLTotalImpairment}, {optimisticCoverageRatio}, {baseCoverageRatio}, {downturnCoverageRatio}, {totalCoverageRatio}, '{calibrationId.ToString()}') ";
        }

        public static string CalibrationResult_EAD_CCF_Summary_Update(Guid calibrationId, double? oD_TotalLimitOdDefaultedLoan, double? oD_BalanceAtDefault, double? oD_Balance12MonthBeforeDefault, double? oD_TotalConversation, double? oD_CCF, double? card_TotalLimitOdDefaultedLoan, double? card_BalanceAtDefault, double? card_Balance12MonthBeforeDefault, double? card_TotalConversation, double? card_CCF, double? overall_TotalLimitOdDefaultedLoan, double? overall_BalanceAtDefault, double? overall_Balance12MonthBeforeDefault, double? overall_TotalConversation, double? overall_CCF)
        {
            return $"delete from CalibrationResult_EAD_CCF_Summary where CalibrationID ='{calibrationId.ToString()}'; insert into CalibrationResult_EAD_CCF_Summary(OD_TotalLimitOdDefaultedLoan, OD_BalanceAtDefault, OD_Balance12MonthBeforeDefault, OD_TotalConversation, OD_CCF, Card_TotalLimitOdDefaultedLoan, Card_BalanceAtDefault, Card_Balance12MonthBeforeDefault, Card_TotalConversation, Card_CCF, Overall_TotalLimitOdDefaultedLoan, " +
                $"Overall_BalanceAtDefault, Overall_Balance12MonthBeforeDefault, Overall_TotalConversation, Overall_CCF, Comment, Status, CalibrationId, DateCreated) values ({oD_TotalLimitOdDefaultedLoan}, {oD_BalanceAtDefault}, {oD_Balance12MonthBeforeDefault}, {oD_TotalConversation}, {oD_CCF}, {card_TotalLimitOdDefaultedLoan}, {card_BalanceAtDefault}, {card_Balance12MonthBeforeDefault}, {card_TotalConversation}, {card_CCF}, {overall_TotalLimitOdDefaultedLoan}, " +
                $"{overall_BalanceAtDefault}, {overall_Balance12MonthBeforeDefault}, {overall_TotalConversation}, {overall_CCF}, '', 1, '{calibrationId.ToString()}', GetDate())";
        }

        public static string CalibrationInput_Haircut(Guid calibrationId)
        {
            return $" select Id=-1, Customer_No,Account_No,Contract_No,Snapshot_Date,Outstanding_Balance_Lcy,Debenture_OMV,Debenture_FSV,Cash_OMV,Cash_FSV,Inventory_OMV,Inventory_FSV,Plant_And_Equipment_OMV,Plant_And_Equipment_FSV,Residential_Property_OMV,Residential_Property_FSV,Commercial_Property_OMV,Commercial_Property_FSV,Receivables_OMV,Receivables_FSV,Shares_OMV,Shares_FSV,Vehicle_OMV,Vehicle_FSV,Guarantee_Value  from CalibrationInput_LGD_HairCut where CalibrationID = '{calibrationId}' union all  select Id, Customer_No,Account_No,Contract_No,Snapshot_Date,Outstanding_Balance_Lcy,Debenture_OMV,Debenture_FSV,Cash_OMV,Cash_FSV,Inventory_OMV,Inventory_FSV,Plant_And_Equipment_OMV,Plant_And_Equipment_FSV,Residential_Property_OMV,Residential_Property_FSV,Commercial_Property_OMV,Commercial_Property_FSV,Receivables_OMV,Receivables_FSV,Shares_OMV,Shares_FSV,Vehicle_OMV,Vehicle_FSV,Guarantee_Value  from CalibrationHistory_LGD_HairCut where AffiliateId = (select OrganizationUnitId from CalibrationRunLgdHairCut where Id = '{calibrationId}')  order by Account_No, Contract_No, Snapshot_Date"; //Snapshot_Date desc
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
            return $"select Id=-1, Customer_No,Account_No,Account_Name,Contract_No,Segment,Days_Past_Due,Classification,Default_Date,Outstanding_Balance_Lcy,Contractual_Interest_Rate,Amount_Recovered,Date_Of_Recovery,Type_Of_Recovery,Product_Type from CalibrationInput_LGD_RecoveryRate where CalibrationID ='{calibrationId}' union all select Id, Customer_No,Account_No,Account_Name,Contract_No,Segment,Days_Past_Due,Classification,Default_Date,Outstanding_Balance_Lcy,Contractual_Interest_Rate,Amount_Recovered,Date_Of_Recovery,Type_Of_Recovery,Product_Type from CalibrationHistory_LGD_RecoveryRate  where AffiliateId = (select OrganizationUnitId from CalibrationRunLgdRecoveryRate where Id = '{calibrationId}')  order by Account_No,Contract_No,Date_Of_Recovery"; //Date_Of_Recovery desc
        }

        public static string CalibrationInput_PD_CR_DR(Guid calibrationId)
        {
            return $"select Id=-1, Customer_No,Account_No,Contract_No,Product_Type,Days_Past_Due,Classification,Outstanding_Balance_Lcy,Contract_Start_Date,Contract_End_Date,RAPP_Date,Current_Rating,Segment from CalibrationInput_PD_CR_DR where CalibrationID ='{calibrationId}' union all select Id, Customer_No,Account_No,Contract_No,Product_Type,Days_Past_Due,Classification,Outstanding_Balance_Lcy,Contract_Start_Date,Contract_End_Date,RAPP_Date,Current_Rating,Segment from CalibrationHistory_PD_CR_DR where AffiliateId =(select OrganizationUnitId from CalibrationRunPdCrDrs where Id='{calibrationId}')  order by Account_No,Contract_No,RAPP_Date"; //RAPP_Date desc
        }

        public static string Calibration_ReceivablesRegisters()
        {
            return $"select top 1 * from ReceivablesRegisters where Status=2";
        }
        public static string CalibrationInput_IVReceivables_CurrentPeriodDates(Guid calibrationId)
        {
            return $"select * from CurrentPeriodDates where RegisterId ='{calibrationId}'";
        }
        public static string CalibrationInput_IVReceivables_ReceivablesInputs(Guid calibrationId)
        {
            return $"select * from ReceivablesInputs where RegisterId ='{calibrationId}'";
        }
        public static string CalibrationInput_IVReceivables_ReceivablesForecasts(Guid calibrationId)
        {
            return $"select * from ReceivablesForecasts where RegisterId ='{calibrationId}'";
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

        public static string CalibrationResult_PD_CommCons_Update(long AffiliateId, int Month, double? Comm1, double? Cons1, double? Comm2, double? Cons2, double? CummComm1, double? CummCons1, double? CummComm2, double? CummCons2)
        {
            return $"INSERT INTO [dbo].[PdInputAssumptionNonInternalModels] " +
                   $"   ([Id],[CreationTime],[CreatorUserId],[LastModificationTime],[LastModifierUserId],[IsDeleted]," +
                   $"    [Key],[Month],[PdGroup],[MarginalDefaultRate],[CummulativeSurvival],[IsComputed],[CanAffiliateEdit],[RequiresGroupApproval],[Framework],[OrganizationUnitId],[Status])     " +
                   $"   VALUES (newid() ,getdate() ,2 ,GETDATE() ,2 ,0 ,'NonInternalModelCOMM_STAGE_1DefaultRate' ,{Month} ,'COMM_STAGE_1' ,{Comm1} ,{CummComm1} ,1 ,0 ,1 ,1 ,{AffiliateId} ,2 ); " +
                   $"INSERT INTO [dbo].[PdInputAssumptionNonInternalModels] " +
                   $"   ([Id],[CreationTime],[CreatorUserId],[LastModificationTime],[LastModifierUserId],[IsDeleted]," +
                   $"    [Key],[Month],[PdGroup],[MarginalDefaultRate],[CummulativeSurvival],[IsComputed],[CanAffiliateEdit],[RequiresGroupApproval],[Framework],[OrganizationUnitId],[Status])     " +
                   $"   VALUES (newid() ,getdate() ,2 ,GETDATE() ,2 ,0 ,'NonInternalModelCOMM_STAGE_2DefaultRate' ,{Month} ,'COMM_STAGE_2' ,{Comm2} ,{CummComm2} ,1 ,0 ,1 ,1 ,{AffiliateId} ,2 ); " +
                   $"INSERT INTO [dbo].[PdInputAssumptionNonInternalModels] " +
                   $"   ([Id],[CreationTime],[CreatorUserId],[LastModificationTime],[LastModifierUserId],[IsDeleted]," +
                   $"    [Key],[Month],[PdGroup],[MarginalDefaultRate],[CummulativeSurvival],[IsComputed],[CanAffiliateEdit],[RequiresGroupApproval],[Framework],[OrganizationUnitId],[Status])     " +
                   $"   VALUES (newid() ,getdate() ,2 ,GETDATE() ,2 ,0 ,'NonInternalModelCONS_STAGE_1DefaultRate' ,{Month} ,'CONS_STAGE_1' ,{Cons1} ,{CummCons1} ,1 ,0 ,1 ,1 ,{AffiliateId} ,2 ); " +
                   $"INSERT INTO [dbo].[PdInputAssumptionNonInternalModels] " +
                   $"   ([Id],[CreationTime],[CreatorUserId],[LastModificationTime],[LastModifierUserId],[IsDeleted]," +
                   $"    [Key],[Month],[PdGroup],[MarginalDefaultRate],[CummulativeSurvival],[IsComputed],[CanAffiliateEdit],[RequiresGroupApproval],[Framework],[OrganizationUnitId],[Status])     " +
                   $"   VALUES (newid() ,getdate() ,2 ,GETDATE() ,2 ,0 ,'NonInternalModelCONS_STAGE_2DefaultRate' ,{Month} ,'CONS_STAGE_2' ,{Cons2} ,{CummCons2} ,1 ,0 ,1 ,1 ,{AffiliateId} ,2 ); ";



            //return $" insert into CalibrationResult_PD_CommsCons_MarginalDefaultRate(Month, Comm1, Cons1, Comm2, Cons2, Comment, Status, CalibrationId, DateCreated) " +
            //       $" values({Month}, {Comm1}, {Cons1}, {Comm2}, {Cons2}, '', 1, '{calibrationId.ToString()}', GetDate()); ";
        }


        public static string Get_EclPdSnPCummulativeDefaultRates(Guid eclId)
        {
            return $"select top 1 * from WholesaleEclPdSnPCummulativeDefaultRates where WholesaleEclId='{eclId}'";
        }

        public static string Get_PD_Comm_Cons_Result(Guid eclId)
        {
            return $"select top 1 * from CalibrationResult_Comm_Cons_PD where CalibrationId=(select CommConsCalibrationId from WholesaleEcls where Id='{eclId}') order by Month";
        }

        public static string CalibrationResult_PD_Update_Summary(Guid calibrationId, string lstQry, string commsCons, double? Normal_12_Months_PD, double? DefaultedLoansA, double? DefaultedLoansB, double? CuredLoansA, double? CuredLoansB, double? Cure_Rate, double? CuredPopulationA, double? CuredPopulationB, double? RedefaultedLoansA, double? RedefaultedLoansB, double? Redefault_Rate, double? Redefault_Factor, 
                                                                 double? Commercial_CureRate, double? Commercial_RedefaultRate, double? Consumer_CureRate, double? Consumer_RedefaultRate, long affiliateId)
        {
            return $"delete from CalibrationResult_PD_12Months where CalibrationID ='{calibrationId.ToString()}'; "+
                $"delete from CalibrationResult_PD_12Months_Summary where CalibrationID ='{calibrationId.ToString()}'; {lstQry} " +
                $"delete from [PdInputAssumptionNonInternalModels] where OrganizationUnitId ={affiliateId} and Framework = 1; {commsCons} " +
                $"insert into CalibrationResult_PD_12Months_Summary(Normal_12_Months_PD, DefaultedLoansA, DefaultedLoansB, CuredLoansA, CuredLoansB, Cure_Rate, " +
                $"CuredPopulationA, CuredPopulationB, RedefaultedLoansA, RedefaultedLoansB, Redefault_Rate, Redefault_Factor, " +
                $"Commercial_CureRate, Commercial_RedefaultRate, Consumer_CureRate, Consumer_RedefaultRate, " +
                $"Comment, Status, CalibrationId, DateCreated) values({Normal_12_Months_PD}, {DefaultedLoansA}, {DefaultedLoansB}, {CuredLoansA}, {CuredLoansB}, {Cure_Rate}, " +
                $"{CuredPopulationA}, {CuredPopulationB}, {RedefaultedLoansA}, {RedefaultedLoansB}, {Redefault_Rate}, {Redefault_Factor},  " +
                $"{Commercial_CureRate}, {Commercial_RedefaultRate}, {Consumer_CureRate}, {Consumer_RedefaultRate}, " +
                $"'', 1, '{calibrationId.ToString()}', GetDate()); ";
        }
        public static string DeleteAffiliateMacroData(int macroId, long affiliateId)
        {
            return $"delete from MacroResult_CorMat where MacroId ={macroId};  {Environment.NewLine}" +
                $" delete from MacroResult_IndexData where MacroId ={macroId};  {Environment.NewLine}" +
                $" delete from MacroResult_PrincipalComponent where MacroId ={macroId};  {Environment.NewLine}" +
                $" delete from MacroResult_PrincipalComponentSummary where MacroId ={macroId};  {Environment.NewLine}" +
                $" delete from MacroResult_Statistics where MacroId ={macroId};  {Environment.NewLine}" +
                $" delete from MacroResult_SelectedMacroEconomicVariables where affiliateId={affiliateId};  {Environment.NewLine}";

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
            return $"select top 1 Id, convert(date, ReportingDate) ReportingDate, IsApproved, Status, EclType=-1, OrganizationUnitId from {eclType.ToString()}Ecls where status in (3,12)";
        }
        public static string EclsRegister(string eclType, string eclId)
        {
            return $"select top 1 Id, convert(date, ReportingDate) ReportingDate, IsApproved, Status, EclType=-1, OrganizationUnitId from {eclType.ToString()}Ecls where Id='{eclId}'";
        }
        public static string UpdateIntTableServiceId(string TableName, int serviceId, int recordId)
        {
            return $"update {TableName} set ServiceId={serviceId} where Id ={recordId}";
        }
        public static string UpdateGuidTableServiceId(string TableName, int serviceId, Guid recordId)
        {
            return $"update {TableName} set ServiceId={serviceId} where Id ='{recordId.ToString()}' and (ServiceId =0 or (ServiceId>0 and Status=12))";
        }
        public static string UpdateEclStatus(string eclType, string eclId, int status, string exception, int overrideStatus=-1)
        {
            return $"update {eclType.ToString()}Ecls set status={status}, ExceptionComment='{exception}' where Id ='{eclId}'";
        }
        public static string DeleteDataOnWholesaleEclFramworkReportDetail(string eclId)
        {
            return $"delete from WholesaleEclFramworkReportDetail where WholesaleEclId='{eclId}'";
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

        public static string CalibrationRegisterUpdate(string caliId, int status, string tableName)
        {
            return $"update {tableName} set Status={status} where Id ='{caliId}'";
        }

        public static string MacroRegisterUpdate(int macroId, int status, string exceptionComment)
        {
            return $"update CalibrationRunMacroAnalysis set Status={status}, ExceptionComment='{exceptionComment}' where Id ={macroId}";
        }

        public static string Raw_Data(Guid guid, EclType eclType)
        {
            return $"select * from {eclType.ToString()}EclDataLoanBooks where {eclType.ToString()}EclUploadId='{guid.ToString()}' order by CustomerNo,AccountNo,ContractNo";// and contractNo='0000000000000000'";// and customerNo in (select customerNo from {eclType.ToString()}EclDataLoanBooks where {eclType.ToString()}EclUploadId='{ guid.ToString()}' and contractNo in ('A36NSLL183060001'))";//,'701NTIC173240002','7010121400372000','7030121405800900','703NTIC172710001','703NTIC172300001','701CRLA171390001','703NTIC172790001','703NTIC171840001','701SFLN172480001','703NTIC173040001','701CRLN173130001','703NTIC173000002','701CRLA172720001','701CRLN171950001','703NTIC172910001','701NBDD173210001','701NBDD173250001','703NTIC173000001','702NTIC173620001','703ATIC172710001','703NTIC172970001','701NBDD173340001','701NBDD173250002','702NTIC172840001','701STCI171800002','701NTIC172990001','701NBDD173630001','701LTLA161890001','722NTIC172990001','702NTIC173260001','701ATIC172430001','701NBDD173260001','701NBDD173210002','7010121417860300','702NTIC172830001','7010221402653700','703ATIC173620001','701CRLA173630001','701CTLN152390001','708STCI173630101','701ILTL160550001','705NTIC173110001','701NTIC173180001','701CRLA161760001','7010121401941400','702NTIC173320001','722NTIC173250001','701NTIC173490001','703NBDD173550002','7030121405789000','701CTLN153030001','703NBDD173550001','701LTLA142860001','708STCI173630102','708CRLN171810103','701STCI173630001','703NBDD173550003','702ETLA172850101','7010121400250900','703CRLA172710001','701LTLN131470001','7060181422160700'))";// like '%14004156%'";// and ContractNo like ' %701010142124400%'"; //and customerNo like '%14025993%'";//10513603600101  and ContractNo='001SMGA121180002'";// and ContractNo like '%10123600327101%'"; // and customerno = '36019901'// and ContractNo='001BADP173340003' ";
        }

        public static string PaymentSchedule(Guid guid, EclType eclType)
        {
            return $"Select ContractRefNo, StartDate, Component, NoOfSchedules, Frequency, Amount  from {eclType.ToString()}EclDataPaymentSchedules where {eclType.ToString()}EclUploadId='{guid.ToString()}' and COMPONENT!='GH_INTLN'";// and contractRefNo in ('0000000000000000')";//,'701NTIC173240002','7010121400372000','7030121405800900','703NTIC172710001','703NTIC172300001','701CRLA171390001','703NTIC172790001','703NTIC171840001','701SFLN172480001','703NTIC173040001','701CRLN173130001','703NTIC173000002','701CRLA172720001','701CRLN171950001','703NTIC172910001','701NBDD173210001','701NBDD173250001','703NTIC173000001','702NTIC173620001','703ATIC172710001','703NTIC172970001','701NBDD173340001','701NBDD173250002','702NTIC172840001','701STCI171800002','701NTIC172990001','701NBDD173630001','701LTLA161890001','722NTIC172990001','702NTIC173260001','701ATIC172430001','701NBDD173260001','701NBDD173210002','7010121417860300','702NTIC172830001','7010221402653700','703ATIC173620001','701CRLA173630001','701CTLN152390001','708STCI173630101','701ILTL160550001','705NTIC173110001','701NTIC173180001','701CRLA161760001','7010121401941400','702NTIC173320001','722NTIC173250001','701NTIC173490001','703NBDD173550002','7030121405789000','701CTLN153030001','703NBDD173550001','701LTLA142860001','708STCI173630102','708CRLN171810103','701STCI173630001','703NBDD173550003','702ETLA172850101','7010121400250900','703CRLA172710001','701LTLN131470001','7060181422160700')"; //
        }

        public static string LGD_Assumption { get { return "Select [collateral value] collateral_value,debenture, cash, inventory, plant_and_equipment, residential_property, commercial_property, shares, vehicle, [Cost of Recovery] costOfRecovery from LGD_Assumptions"; } }

        public static string EclOverridesFsv(Guid eclId, EclType eclType)
        {
            return $"select * from {eclType.ToString()}EclOverrides where {eclType.ToString()}EclDataLoanBookId ='{eclId}'";
        }
        public static string CheckOverrideDataExist(Guid eclId, EclType eclType)
        {
            return $"select count(Id) from {eclType.ToString()}EclOverrides where {eclType.ToString()}EclDataLoanBookId ='{eclId}' and Status=2";
        }
        public static string EclOverridesStage(Guid eclId, EclType eclType)
        {
            return $"select ContractId, Stage from {eclType.ToString()}EclOverrides where {eclType.ToString()}EclDataLoanBookId ='{eclId}'";
        }
        public static string EclOverridesTTr(Guid eclId, EclType eclType)
        {
            return $"select ContractId, TtrYears from {eclType.ToString()}EclOverrides where {eclType.ToString()}EclDataLoanBookId ='{eclId}'";
        }
        public static string EclOverridesAllData(Guid eclId, EclType eclType)
        {
            return $"select * from {eclType.ToString()}EclOverrides where {eclType.ToString()}EclDataLoanBookId ='{eclId}'";
        }
        public static string EclOverrideExist(Guid eclId, EclType eclType)
        {
            return $"select count(*) from {eclType.ToString()}EclOverrides where {eclType.ToString()}EclDataLoanBookId ='{eclId}'";
        }

        public static string EclOverrideIsRunning(Guid eclId)
        {
            return $"select ExceptionComment from WholesaleEcls where Id ='{eclId}'";
        }



        public static string LgdCollateralProjection(Guid eclId, int collateralProjectionType, EclType eclType)
        {
            //return $"select CollateralProjectionType, Debenture, Cash, Inventory, Plant_And_Equipment, Residential_Property, Commercial_Property, Receivables, Shares, Vehicle, Month from {eclType.ToString()}LgdCollateralProjection where {eclType.ToString()}EclId = '{eclId}' and CollateralProjectionType={collateralProjectionType}";
            return $"select  [Key], [Value], LgdGroup from {eclType.ToString()}EclLgdAssumptions where {eclType.ToString()}EclId ='{eclId}' and LgdGroup = {collateralProjectionType}";
        }

        public static string LGD_InputAssumptions_UnsecuredRecovery(Guid eclId, EclType eclType)
        {
            return $"select [Key] Segment_Product_Type, Value Cure_Rate, Value Days_0, Days_90=0, Days_180=0, Days_270=0, Days_360=0, Downturn_Days_0=0, Downturn_Days_90=0, Downturn_Days_180=0, Downturn_Days_270=0, Downturn_Days_360=0 from LgdInputAssumptions where LgdGroup in (1,2) order by 1";// where {eclType.ToString()}EclId='{eclId}'";
        }

        public static string eclFrameworkAssumptions(Guid eclId, EclType eclType)
        {
            return $"select [Key], Value, AssumptionGroup from {eclType.ToString()}EclAssumptions where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }
        public static string eclEadInputAssumptions(Guid eclId, EclType eclType)
        {
            return $"select [Key], InputName, [Value] from {eclType.ToString()}EclEadInputAssumptions where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }

        public static string GetPDIndexData(object eclId, object p)
        {
            throw new NotImplementedException();
        }

        public static string eclLGDAssumptions(Guid eclId, EclType eclType)
        {
            return $"select [Key], Value, LgdGroup AssumptionGroup from {eclType.ToString()}EclLgdAssumptions where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }
        public static string eclPDAssumptions(Guid eclId, EclType eclType)
        {
            return $"select [Key], Value, PdGroup AssumptionGroup from {eclType.ToString()}EclPdAssumptions where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }

        public static string Get_AffiliateMEVBackDateValues(Guid eclId, EclType eclType)
        {
            return $"select BackwardOffset BackDateQuarters, MacroeconomicVariableId MicroEconomicId from MacroResult_SelectedMacroEconomicVariables where AffiliateId=(select OrganizationUnitId from {eclType.ToString()}Ecls where Id='{eclId.ToString()}' ) order by Id";
        }


        public static string Calibration_HoldingCo_Registers()
        {
            return $"select top 1 * from HoldCoRegisters where Status=2";
        }
        public static string CalibrationInput_HoldingCo_AssetBooks(Guid calibrationId)
        {
            return $"select * from AssetBooks where RegistrationId ='{calibrationId}'";
        }
        public static string CalibrationInput_HoldingCo_Parameter(Guid calibrationId)
        {
            return $"select * from HoldCoInputParameters where RegistrationId ='{calibrationId}'";
        }
        public static string CalibrationInput_HoldingCo_MacroEconomicCreditIndices(Guid calibrationId)
        {
            return $"select * from MacroEconomicCreditIndices where RegistrationId ='{calibrationId}'";
        }

        public static string CalibrationResult_HoldingCo_ResultSummary(Guid calibrationId, double BestEstimateExposure, double OptimisticExposure, double DownturnExposure, 
                double BestEstimateTotal, double OptimisticTotal, double DownturnTotal, double BestEstimateImpairmentRatio, double OptimisticImpairmentRatio, double DownturnImpairmentRatio, 
                double Exposure, double Total, double ImpairmentRatio, int Check, double Diff)
        {
            return
                $"delete from [HoldCoResultSummaries] where [RegistrationId] ='{calibrationId.ToString()}'; " +
                $"INSERT INTO [dbo].[HoldCoResultSummaries] ([id],[CreationTime],[CreatorUserId],[IsDeleted],[BestEstimateExposure],[OptimisticExposure],[DownturnExposure], " +
                $" [BestEstimateTotal],[OptimisticTotal],[DownturnTotal],[BestEstimateImpairmentRatio],[OptimisticImpairmentRatio],[DownturnImpairmentRatio], " +
                $" [Exposure],[Total],[ImpairmentRatio],[Check],[Diff],[RegistrationId]) " +
                $"VALUES (newid(), getdate(), 2, 0, {BestEstimateExposure}, {OptimisticExposure},{DownturnExposure}, " +
                $" {BestEstimateTotal}, {OptimisticTotal}, '{DownturnTotal}', " +
                $" {BestEstimateImpairmentRatio}, {OptimisticImpairmentRatio}, {DownturnImpairmentRatio}, " +
                $" {Exposure}, {Total}, {ImpairmentRatio}, {Check}, '{Diff}', '{calibrationId.ToString()}' ); ";
        }

        public static string CalibrationResult_HoldingCo_ResultSummaryByStage(Guid calibrationId, double StageOneExposure, double StageTwoExposure, double StageThreeExposure, double TotalExposure, 
                                                                              double StageOneImpairment, double StageTwoImpairment, double StageThreeImpairment, double TotalImpairment,
                                                                              double StageOneImpairmentRatio, double StageTwoImpairmentRatio, double StageThreeImpairmentRatio, double TotalImpairmentRatio)
        {
            return
                $"delete from [ResultSummaryByStages] where [RegistrationId] ='{calibrationId.ToString()}'; " +
                $"INSERT INTO[dbo].[ResultSummaryByStages] ([id],[CreationTime],[CreatorUserId],[IsDeleted], " +
                $" [StageOneExposure],[StageTwoExposure],[StageThreeExposure],[TotalExposure], " +
                $" [StageOneImpairment],[StageTwoImpairment],[StageThreeImpairment],[TotalImpairment], " +
                $" [StageOneImpairmentRatio],[StageTwoImpairmentRatio],[StageThreeImpairmentRatio],[TotalImpairmentRatio],[RegistrationId]) " +
                $"VALUES (newid(), getdate(), 2, 0 , " +
                $" {StageOneExposure}, {StageTwoExposure}, {StageThreeExposure}, {TotalExposure}, " +
                $" {StageOneImpairment}, {StageTwoImpairment}, {StageThreeImpairment}, {TotalImpairment}, " +
                $" {StageOneImpairmentRatio}, {StageTwoImpairmentRatio}, { StageThreeImpairmentRatio}, {TotalImpairmentRatio}, '{calibrationId.ToString()}'); ";

        }


        public static string CalibrationResult_HoldingCo_ResultDetail_Items(Guid calibrationId, string AssetType, string AssetDescription, double Stage, double OutstandingBalance, 
                                                                       double BestEstimate, double Optimistic, double Downturn, double Impairment)
        {
            return
                $"INSERT INTO[dbo].[HoldCoInterCompanyResults] ([id],[CreationTime],[CreatorUserId],[IsDeleted], " +
                $" [RegistrationId] ,[AssetType],[AssetDescription],[Stage],[OutstandingBalance],[BestEstimate],[Optimistic],[Downturn],[Impairment]) " +
                $" VALUES (newid(), getdate(), 2, 0, '{calibrationId.ToString()}', '{AssetType}', '{AssetDescription}', {Stage}, {OutstandingBalance}, {BestEstimate}, {Optimistic}, {Downturn}, {Impairment}); ";

        }

        public static string CalibrationResult_HoldingCo_ResultDetails(Guid calibrationId, StringBuilder qry)
        {
            return $"delete from [HoldCoInterCompanyResults] where [RegistrationId] ='{calibrationId.ToString()}'; \n" + qry.ToString() + ";";
        }


        public static string CalibrationResultHistoric_PD_CommsCons(long AffiliateId)
        {
            return $"SELECT  Id, Affiliate_ID, Stage, Comm_1, Comm_2, Comm_3, Cons_1, Cons_2, Cons_3 FROM  CalibrationResultHistoric_PD_CommsCons where  Affiliate_ID = {AffiliateId};";
        }
        public static string CalibrationResultHistoric_PD_Corporate(long AffiliateId)
        {
            return $"SELECT Id ,Affiliate_ID ,RAPPDATE, " +
                   $" OutstandingBalance_1 ,OutstandingBalance_2 ,OutstandingBalance_3 ,OutstandingBalance_4 ,OutstandingBalance_5 ,OutstandingBalance_6 ,OutstandingBalance_7 ,OutstandingBalance_8 ,OutstandingBalance_9 ,OutstandingBalance_10, " +
                   $" Balance_1 ,Balance_2 ,Balance_3 ,Balance_4 ,Balance_5 ,Balance_6 ,Balance_7 ,Balance_8 ,Balance_9 ,Balance_10 " +
                   $" FROM CalibrationResultHistoric_PD_Corporate where Affiliate_ID = {AffiliateId} order by RAPPDATE; ";
        }
        public static string CalibrationResultHistoric_PD_Output(long AffiliateId)
        {
            return $"SELECT Id, Affiliate_ID, " +
                   $" Rating_1, Rating_2, Rating_3, Rating_4, Rating_5, Rating_6, Rating_7, Rating_8, Rating_9, Rating_10, " +
                   $" Rating_Comm, Rating_Cons, Defaulted_Loan, Cured_Loan, Redefaulted_Loans " +
                   $" FROM  CalibrationResultHistoric_PD_Output where Affiliate_ID = {AffiliateId};";
        }



        /*************** RV Impairment Query *************************/
        //TODO:: Update Queries
        public static string Calibration_RvImpairment_Registers()
        {
            return $"select top 1 * from [LoanImpairmentRegisters] where Status=2 or status=12";
        }
        public static string CalibrationInput_RvImpairment_Recoverys(Guid calibrationId)
        {
            return $"select * from LoanImpairmentRecoveries where RegisterId ='{calibrationId}' order by Recovery";
        }
        public static string CalibrationInput_RvImpairment_ScenarioOptions(Guid calibrationId)
        {
            return $"select * from LoanImpairmentScenarios where RegisterId ='{calibrationId}'";
        }
        public static string CalibrationInput_RvImpairment_Haircut(Guid calibrationId)
        {
            return $"select * from LoanImpairmentHaircuts where RegisterId ='{calibrationId}'";
        }
        public static string CalibrationInput_RvImpairment_Parameters(Guid calibrationId)
        {
            return $"select * from LoanImpairmentInputParameters where RegisterId ='{calibrationId}'";
        }
        public static string CalibrationInput_RvImpairment_Calibration(Guid calibrationId)
        {
            return $"select * from LoanImpairmentKeyParameters where RegisterId ='{calibrationId}' order by Year";
        }
        public static string CalibrationInput_RvImpairment_ResultImpairmentOverlay(Guid calibrationId)
        {
            return $"select BaseScenarioOverlay, OptimisticScenarioOverlay, DownturnScenarioOverlay from LoanImpairmentModelResults where RegisterId ='{calibrationId}';";
        }

        public static string CalibrationResult_RvImpairment(Guid calibrationId, double  BaseScenarioExposure, double BaseScenarioFinalImpairment, double BaseScenarioIPO, double  BaseScenarioOverlay, double  BaseScenarioOverrideImpact, double  BaseScenarioPreOverlay, 
                                                            double DownturnScenarioExposure, double DownturnScenarioFinalImpairment, double DownturnScenarioIPO, double DownturnScenarioOverlay, double DownturnScenarioOverrideImpact, double DownturnScenarioPreOverlay, 
                                                            double OptimisticScenarioExposure, double OptimisticScenarioFinalImpairment, double OptimisticScenarioIPO, double OptimisticScenarioOverlay, double OptimisticScenarioOverrideImpact, double OptimisticScenarioPreOverlay,
                                                            double ResultFinalImpairment, double ResultIPO, double ResultOverlay, double ResultOverrideImpact, double ResultPreOverlay, double ResultsExposure)
        {
            return
                $"delete from [LoanImpairmentModelResults] where [RegisterId] ='{calibrationId.ToString()}'; " +
                $"INSERT INTO [dbo].[LoanImpairmentModelResults] ([Id],[CreationTime],[CreatorUserId],[IsDeleted],[RegisterId], " +
                $"  [BaseScenarioExposure],[BaseScenarioFinalImpairment],[BaseScenarioIPO],[BaseScenarioOverlay],[BaseScenarioOverrideImpact],[BaseScenarioPreOverlay], " +
                $"  [DownturnScenarioExposure],[DownturnScenarioFinalImpairment],[DownturnScenarioIPO],[DownturnScenarioOverlay],[DownturnScenarioOverrideImpact],[DownturnScenarioPreOverlay], " +
                $"  [OptimisticScenarioExposure],[OptimisticScenarioFinalImpairment],[OptimisticScenarioIPO],[OptimisticScenarioOverlay],[OptimisticScenarioOverrideImpact],[OptimisticScenarioPreOverlay], " +
                $"  [ResultFinalImpairment],[ResultIPO],[ResultOverlay],[ResultOverrideImpact],[ResultPreOverlay],[ResultsExposure]) " +
                $"VALUES (newid(), getdate(), 2, 0, '{calibrationId.ToString()}', " +
                $"  {BaseScenarioExposure}, {BaseScenarioFinalImpairment}, {BaseScenarioIPO}, {BaseScenarioOverlay}, {BaseScenarioOverrideImpact}, {BaseScenarioPreOverlay}, " +
                $"  {DownturnScenarioExposure}, {DownturnScenarioFinalImpairment}, {DownturnScenarioIPO}, {DownturnScenarioOverlay}, {DownturnScenarioOverrideImpact}, {DownturnScenarioPreOverlay}, " +
                $"  {OptimisticScenarioExposure}, {OptimisticScenarioFinalImpairment}, {OptimisticScenarioIPO}, {OptimisticScenarioOverlay}, {OptimisticScenarioOverrideImpact}, {OptimisticScenarioPreOverlay}, " +
                $"  {ResultFinalImpairment}, {ResultIPO}, {ResultOverlay}, {ResultOverrideImpact}, {ResultPreOverlay}, {ResultsExposure}); ";

        }

        public static string Get_AffiliateId(Guid eclId, EclType eclType)
        {
            return $"select OrganizationUnitId from {eclType.ToString()}Ecls where Id='{eclId.ToString()}'";
        }
    }
}
