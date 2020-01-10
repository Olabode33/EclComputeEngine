using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.PD
{


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
        public string Assumptions { get; set; }
        public string Value { get; set; }
        public Guid EclId { get; set; }
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
        public double _1 { get; set; }
        public double _2 { get; set; }
        public double _3 { get; set; }
        public double _4 { get; set; }
        public double _5 { get; set; }
        public double _6 { get; set; }
        public double _7 { get; set; }
        public double _8 { get; set; }
        public double _9 { get; set; }
        public double _10 { get; set; }
        public double _11 { get; set; }
        public double _12 { get; set; }
        public double _13 { get; set; }
        public double _14 { get; set; }
        public double _15 { get; set; }
        public double PD { get; set; }
        public Guid EclId { get; set; }

    }


}
