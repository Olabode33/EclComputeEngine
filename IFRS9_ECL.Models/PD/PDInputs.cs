using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.PD
{

    public static class PDInputs
    {
        public static PdInputAssumptionGroupEnum GetPDAssumptionEnum(int id)
        {
            if (id == 0)
            {
                return PdInputAssumptionGroupEnum.General;
            }
            if (id == 1)
            {
                return PdInputAssumptionGroupEnum.CreditPD;
            }
            if (id == 2)
            {
                return PdInputAssumptionGroupEnum.CreditEtiPolicy;
            }
            if (id == 3)
            {
                return PdInputAssumptionGroupEnum.CreditBestFit;
            }
            if (id == 4)
            {
                return PdInputAssumptionGroupEnum.StatisticsIndexWeights;
            }
            if (id == 5)
            {
                return PdInputAssumptionGroupEnum.InvestmentAssumption;
            }
            if (id == 6)
            {
                return PdInputAssumptionGroupEnum.InvestmentMacroeconomicScenario;
            }
            return PdInputAssumptionGroupEnum.General;
        }
    }


    public class PDI_StatisticalInputs
    {
        public string Mode { get; set; }
        public int MacroEconomicVariableId { get; set; }
        public double MacroEconomicValue { get; set; }
        public Guid EclId { get; set; }
    }

    public static class StatisticalInputsRowKeys
    {
        public const string Mean = "Mean";
        public const string StandardDeviation = "Standard Deviation";
        public const string Eigenvalues = "Eigenvalues";
        public const string PrincipalScore1 = "Principal Component Score 1";
        public const string PrincipalScore2 = "Principal Component Score 2";
    }


    public class PDI_MacroEconomicProjections
    {
        public int Id { get; set; }
        public string VariableName { get; set; }
    }


    public class PDI_MacroEconomics
    {
        public DateTime Date { get; set; }
        public int MacroEconomicVariableId { get; set; }
        public double BestEstimateMacroEconomicValue { get; set; }
        public double OptimisticMacroEconomicValue { get; set; }
        public double DownturnMacroEconomicValue { get; set; }
        public Guid EclId { get; set; }
    }



    public class PDI_12MonthPds
    {
        public double Rating { get; set; }
        public double PD { get; set; }
        public string Policy { get; set; }
        public string Fit { get; set; }
        public Guid EclId { get; set; }
    }

    public class PDI_Assumptions
    {
        public PdInputAssumptionGroupEnum PdGroup { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string InputName { get; set; }
        public Guid EclId { get; set; }
    }

    public enum PdInputAssumptionGroupEnum
    {
        General, CreditPD, CreditEtiPolicy, CreditBestFit, StatisticsIndexWeights, InvestmentAssumption, InvestmentMacroeconomicScenario
    }

    public class PDI_HistoricIndex
    {
        public DateTime Date { get; set; }
        public double Actual { get; set; }
        public double Standardised { get; set; }

    }
    public class PDI_ETI_NPL
    {
        public DateTime Date { get; set; }
        public double Series { get; set; }

    }

    public class PDI_NonInternalModelInputs
    {
        public double Month { get; set; }
        public double CONS_STAGE_1 { get; set; }
        public double CONS_STAGE_2 { get; set; }
        public double COMM_STAGE_1 { get; set; }
        public double COMM_STAGE_2 { get; set; }
        public Guid EclId { get; set; }

    }

    public class PDI_SnPCummlativeDefaultRate
    {
        public string Rating { get; set; }
        public int Years { get; set; }
        public double Value { get; set; }
        //public double PD { get; set; }
        //public Guid EclId { get; set; }

    }


}
