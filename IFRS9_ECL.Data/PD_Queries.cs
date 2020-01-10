using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Data
{
    public static class PD_Queries
    {
        public static string Get_loanBookQuery(string testAccountNo)
        {
            return "SELECT ContractId,CustomerNo,AccountNo,ContractNo,CustomerName,SnapshotDate" +
                ",Segment,Sector,Currency,ProductType,ProductMapping,SpecialisedLending,RatingModel" +
                ",OriginalRating,CurrentRating,LifetimePD,Month12PD,DaysPastDue,WatchlistIndicator" +
                ",Classification,ImpairedDate,DefaultDate,CreditLimit,OriginalBalanceLCY,OutstandingBalanceLCY,OutstandingBalanceACY" +
                ",ContractStartDate,ContractEndDate,RestructureIndicator,RestructureRisk,RestructureType,RestructureStartDate" +
                ",RestructureEndDate,PrincipalPaymentTermsOrigination,PPTOPeriod,InterestPaymentTermsOrigination" +
                ",IPTOPeriod,PrincipalPaymentStructure,InterestPaymentStructure,InterestRateType,BaseRate,OriginationContractualInterestRate" +
                ",IntroductoryPeriod,PostIPContractualInterestRate,CurrentContractualInterestRate,EIR,DebentureOMV,DebentureFSV" +
                ",CashOMV,CashFSV,InventoryOMV,InventoryFSV,PlantEquipmentOMV,PlantEquipmentFSV,ResidentialPropertyOMV,ResidentialPropertyFSV" +
                ",CommercialPropertyOMV,CommercialProperty,ReceivablesOMV,ReceivablesFSV,SharesOMV,SharesFSV,VehicleOMV" +
                ",VehicleFSV,CureRate,GuaranteeIndicator,GuarantorPD,GuarantorLGD,GuaranteeValue,GuaranteeLevel" +
                "FROM WholesaleEclDataLoanBooks" +
                $"Where ContractNo = '{testAccountNo}'";
        }
        public static string Get_12MonthsPdQuery(Guid eclId)
        {
            return $"SELECT [Credit Rating] Rating,PD,[S&P Mapping(ETI Credit Policy)] Policy,[S&P Mapping(Best Fit)] Fit, EclId FROM PDI_12MonthPds where EclId='{eclId}'";
        }

        public static string Get_etiNplQuery()
        {
            return $"SELECT [Date],EtiNplSeries Series FROM PdInputAssumptionNplIndexes";
        }

        public static string Get_historicIndexQuery()
        {
            return $"SELECT [Date], [Actual], Standardised FROM PdInputAssumptionNplIndexes";
        }

        //public static string Get_macroEcoBestQuery
        //{
        //    get { return "SELECT [Date],[Prime Lending Rate(%)] Prime_Lending_Rate,[Oil Exports(USD'm)] Oil_Exports_USD,[Real GDP Growth Rate(%)] Real_GDP_Growth_Rate,[Differenced Real GDP Growth Rate(%)] Differenced_Real_GDP_Growth_Rate FROM PDI_MacroEcoBest"; }
        //}
        public static string Get_macroEconomicsQuery(Guid eclId)
        {
            return $"SELECT [Date],MacroeconomicVariableId MacroEconomicVariableId,BestValue BestEstimateMacroEconomicValue,OptimisticValue OptimisticMacroEconomicValue, DownturnValue DowntimeMacroEconomicValue, WholesaleEclId EclId FROM WholesaleEclPdAssumptionMacroeconomicProjections where WholesaleEclId='{eclId}'";
        }
        public static string Get_nonInternalmodelInputQuery(Guid eclId)
        {
            return $"SELECT Month,CONS_STAGE_1,CONS_STAGE_2,COMM_STAGE_1,COMM_STAGE_2, EclId FROM PDI_NonInternalModelInputs where EclId='{eclId}'";
        }
        public static string Get_snpCummulativeDefaultRateQuery(Guid eclId)
        {
            return $"SELECT [Rating],[1] _1,[2] _2,[3] _3,[4] _4,[5] _5,[6] _6,[7] _7,[8] _8,[9] _9,[10] _10,[11] _11,[12] _12,[13] _13,[14] _14,[15] _15,[12 Month PD] _12_Month_PD, EclId FROM [PDI_SnPCummlativeDefaultRate] where EclId = '{eclId}'";
        }
        //public static string Get_statisticalInputsQuery
        //{
        //    get { return "SELECT [Mode],[Prime Lending Rate(%)] Prime_Lending_Rate,[Oil Exports(USD'm)] Oil_Exports_USD,[Real GDP Growth Rate(%)] Real_GDP_Growth_rate FROM [PDI_StatisticalInputs]"; }
        //}
        public static string Get_statisticalInputsQuery(Guid eclId)
        {
            return $"SELECT InputName [Mode], MacroeconomicVariableId MacroEconomicVariableId,Value MacroEconomicValue, WholesaleEclId EclId FROM [PDI_StatisticalInputs] where WholesaleEclId='{eclId}'";
        }
        //Fields for MacroEconomicVariableId in Table Get_statisticalInputsQuery 
        public static string Get_MacroEconomicProjections
        {
            get { return "SELECT Id,Name VariableName FROM MacroeconomicVariables"; }
        }
        public static string Get_pdInputAssumptionsQuery(Guid eclId)
        {
            return $"SELECT [Assumptions],[Value], EclId FROM [PDI_Assumptions] where EclId='{eclId}'";
        }

        public static string Get_impairmentAssumptionsQuery(Guid eclId)
        {
            return $"SELECT [Assumption] FROM [ImpairmentAssumptions] where EclId='{eclId}'";
        }
        public static string Get_tempLgdContractDataQuery(string testAccountNo)
        {
            return $"SELECT [CONTRACT_NO] ,[ACCOUNT_NO] ,[CUSTOMER_NO] ,[PRODUCT_TYPE] ,[TTR_YEARS] ,[COST_OF_RECOVERY_%] COST_OF_RECOVERY, [GUARANTOR_PD], [GUARANTOR_LGD], [GUARANTEE_VALUE], [GUARANTEE_LEVEL] FROM [TempLGDContractData] Where [CONTRACT_NO] = '{testAccountNo}'"; 
        }
        public static string Get_tempLgdCollateralProjectionOptimisticQuery
        {
            get { return "SELECT [MONTH],[CASH],[COMMERCIAL_PROPERTY],[DEBENTURE],[INVENTORY],[PLANT_AND_EQUIPMENT],[RECEIVABLES],[RESIDENTIAL_PROPERTY],[SHARES],[VEHICLE] FROM [TempLGDCollateralProjectionOptimistic]"; }
        }
        public static string Get_tempLgdCollateralProjectionDownturnQuery
        {
            get { return "SELECT [MONTH],[CASH],[COMMERCIAL_PROPERTY],[DEBENTURE],[INVENTORY],[PLANT_AND_EQUIPMENT],[RECEIVABLES],[RESIDENTIAL_PROPERTY],[SHARES],[VEHICLE] FROM [TempLGDCollateralProjectionDownturn]"; }
        }
        public static string Get_tempLgdCollateralProjectionBestQuery
        {
            get { return "SELECT [MONTH],[CASH],[COMMERCIAL_PROPERTY],[DEBENTURE],[INVENTORY],[PLANT_AND_EQUIPMENT],[RECEIVABLES],[RESIDENTIAL_PROPERTY],[SHARES],[VEHICLE] FROM [TempLGDCollateralProjectionBest]"; }
        }
        public static string Get_tempLgdCollateralTypeOmvQuery
        {
            get { return "SELECT [CONTRACT_NO],[CASH],[COMMERCIAL_PROPERTY],[DEBENTURE],[INVENTORY],[PLANT_AND_EQUIPMENT],[RECEIVABLES],[RESIDENTIAL_PROPERTY],[SHARES],[VEHICLE] FROM [TempLGDCollateralTypeOMV]"; }
        }
        public static string Get_tempLgdCollateralTypeFsvQuery
        {
            get
            {
                return "SELECT [CONTRACT_NO],[CASH],[COMMERCIAL_PROPERTY],[DEBENTURE],[INVENTORY],[PLANT_AND_EQUIPMENT],[RECEIVABLES],[RESIDENTIAL_PROPERTY],[SHARES],[VEHICLE] FROM [TempLGDCollateralTypeFSV]";
            }
        }
        public static string Get_tempEadInputQuery
        {
            get { return "SELECT [CONTRACT_ID] ,[EIR_GROUP] ,[CIR_GROUP], [Month] from TempEADInputs"; }
        }
        public static string Get_tempEirProjectionsQuery
        {
            get { return "SELECT [EIR_GROUPS],[Month] from TempEADEirProjections"; }
        }
        public static string Get_tempCirProjectionQuery
        {
            get { return "SELECT [CIR_GROUPS],[Month] from TempEADCirProjections"; }
        }
        public static string Get_tempLgdInputAssumptions
        {
            get { return "SELECT [SEGMENT_PRODUCT_TYPE],[CURE_RATE],[Scenario],[0] _0,[90] _90,[180] _180,[270] _270,[360] _360 FROM [LgdInputAssumptions]"; }
        }
       
    }
}
