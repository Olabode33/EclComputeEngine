using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Data
{
    public static class PD_Queries
    {
      
        public static string Get_etiNplQuery(Guid eclId, EclType eclType)
        {
            return $"SELECT Period,BfNpl FROM MacroResult_IndexData where MacroId=(select top 1 Id from CalibrationRunMacroAnalysis where OrganizationUnitId=(select top 1 OrganizationUnitId from {eclType.ToString()}Ecls where Id='{eclId.ToString()}'))";
        }

        public static string Get_historicIndexQuery(Guid eclId, EclType eclType)
        {
            return $"SELECT Period,Index,StandardIndex FROM MacroResult_IndexData where MacroId=(select top 1 Id from CalibrationRunMacroAnalysis where OrganizationUnitId=(select top 1 OrganizationUnitId from {eclType.ToString()}Ecls where Id='{eclId.ToString()}'))";
        }

        public static string Get_macroEconomicsQuery(Guid eclId, EclType eclType)
        {
            return $"SELECT [Date],MacroeconomicVariableId MacroEconomicVariableId,BestValue BestEstimateMacroEconomicValue,OptimisticValue OptimisticMacroEconomicValue, DownturnValue DowntimeMacroEconomicValue, {eclType.ToString()}EclId EclId FROM {eclType.ToString()}EclPdAssumptionMacroeconomicProjections where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }
        public static string Get_nonInternalmodelInputQuery(Guid eclId, EclType eclType, int month)
        {

            var subQry = "";
            if(month>0)
            {
                subQry=$" and Month ={month} ";
            }
            
            return $"SELECT Month, PdGroup, MarginalDefaultRate, CummulativeSurvival FROM {eclType.ToString()}EclPdAssumptionNonInternalModels where {eclType.ToString()}EclId='{eclId.ToString()}' {subQry}";
        }
        public static string Get_snpCummulativeDefaultRateQuery(Guid eclId, EclType eclType)
        {
            return $"SELECT [Rating],[Years],[Value] FROM {eclType.ToString()}EclPdSnPCummulativeDefaultRates where {eclType.ToString()}EclId = '{eclId.ToString()}'";
        }
        
        public static string Get_statisticalInputsQuery(Guid eclId, EclType eclType)
        {
            return $"SELECT InputName [Mode], MacroeconomicVariableId MacroEconomicVariableId,Value MacroEconomicValue, {eclType.ToString()}EclId EclId FROM {eclType.ToString()}EclPdAssumptionMacroeconomicInputs where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }
        //Fields for MacroEconomicVariableId in Table Get_statisticalInputsQuery 
        public static string Get_MacroEconomicProjections
        {
            get { return "SELECT Id,Name VariableName FROM MacroeconomicVariables"; }
        }
        public static string Get_pdInputAssumptionsQuery(Guid eclId, EclType eclType)
        {
            return $"SELECT PdGroup, [Key],Value, InputName, {eclType.ToString()}EclId FROM {eclType.ToString()}EclPdAssumptions where {eclType.ToString()}EclId='{eclId.ToString()}'";
        }


    }
}
