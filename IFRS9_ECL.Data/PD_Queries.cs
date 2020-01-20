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
        public static string Get_nonInternalmodelInputQuery()//(Guid eclId)
        {
            return $"SELECT Month, PdGroup, MarginalDefaultRate, CummulativeSurvival FROM PdInputAssumptionNonInternalModels";// where EclId='{eclId}'";
            //return $"SELECT Month,CONS_STAGE_1,CONS_STAGE_2,COMM_STAGE_1,COMM_STAGE_2, EclId FROM PDI_NonInternalModelInputs where EclId='{eclId}'";
        }
        public static string Get_snpCummulativeDefaultRateQuery(Guid eclId)
        {
            return $"SELECT [Rating],[Years],[Value] FROM [WholesaleEclPdSnPCummulativeDefaultRates] where WholesaleEclId = '{eclId}'";
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
            return $"SELECT [PdGroup], [Key],[Value], InputName, WholesaleEclId FROM [WholesaleEclPdAssumptions] where WholesaleEclId='{eclId}'";
        }
        //public static string Get_pdInputAssumptionsQuery(Guid eclId)
        //{
        //    return $"SELECT [Assumptions],[Value], EclId FROM [PDI_Assumptions] where EclId='{eclId}'";
        //}

    }
}
