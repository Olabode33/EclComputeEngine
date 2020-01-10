using IFRS9_ECL.Core.PDComputation.cmPD;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.PDComputation
{
    public class IndexForecastWorkings
    {
        ECL_Scenario _Scenario;
        Guid _eclId;
        public IndexForecastWorkings(ECL_Scenario eCL_Scenario, Guid eclId)
        {
            this._Scenario = eCL_Scenario;
            this._eclId = eclId;
        }

        public List<IndexForecast> ComputeIndexForecast()
        {
            List<IndexForecast> indexForecast = new List<IndexForecast>();

            var principalData = ComputeScenarioPrincipalComponents();

            foreach (var itm in principalData)
            {
                double actual = itm.Principal1 * ECLNonStringConstants.i.IndexWeight1 + itm.Principal2 * ECLNonStringConstants.i.IndexWeight2;
                    
                double indexStandardDeviation = ComputeHistoricIndexStandardDeviation();
                double indexMean = ComputeHistoricIndexMean();

                var dr = new IndexForecast();
                dr.Date = itm.Date;
                dr.Actual = actual;
                dr.Standardised = indexStandardDeviation == 0 ? 0 : (actual - indexMean) / indexStandardDeviation;

                indexForecast.Add(dr);
            }

            return indexForecast;
        }

        protected List<IndexForecast> ComputeScenarioPrincipalComponents()
        {
            var principalData = new List<IndexForecast>();

            var statisticalInputs = GetStatisticalInputData();
            var standardisedData = ComputeScenarioStandardisedData(statisticalInputs);
            standardisedData=standardisedData.OrderBy(o => o.Date).ToList();
            var macroeconomicPrincipal1 = statisticalInputs.Where(o => o.Mode == StatisticalInputsRowKeys.PrincipalScore1).ToList();
            var macroeconomicPrincipal2 = statisticalInputs.Where(o => o.Mode == StatisticalInputsRowKeys.PrincipalScore2).ToList();


            var groupedDate = standardisedData.GroupBy(x => x.Date).Select(x => new { Date = x.Key, Cnt = x.Count() }).ToList();
            var macroEconomicCount= groupedDate.Max(r => r.Cnt);

            foreach (var dt in groupedDate)
            {
                var date_standardisedData = standardisedData.Where(o => o.Date == dt.Date).ToList();

                double[] standardised = new double[standardisedData.Count];
                double[] principal1 = new double[macroeconomicPrincipal1.Count];
                double[] principal2 = new double[macroeconomicPrincipal2.Count];

                for (int i = 0; i < date_standardisedData.Count; i++)
                {
                    standardised[i] = date_standardisedData[i].MacroEconomicValue;

                    if(principal1.Length-1>i)
                    {
                        var p1 = macroeconomicPrincipal1.FirstOrDefault(o => o.MacroEconomicVariableId == date_standardisedData[i].MacroEconomicVariableId);
                        principal1[i] = p1 != null ? p1.MacroEconomicValue : 0;
                    }

                    if (principal2.Length - 1 > i)
                    {
                        var p2 = macroeconomicPrincipal2.FirstOrDefault(o => o.MacroEconomicVariableId == date_standardisedData[i].MacroEconomicVariableId);
                        principal2[i] = p2 != null ? p2.MacroEconomicValue : 0;
                    }

                    var dr = new IndexForecast();
                    dr.Date = dt.Date;
                    dr.Principal1 = ExcelFormulaUtil.SumProduct(standardised, principal1);
                    dr.Principal2 = ExcelFormulaUtil.SumProduct(standardised, principal2);

                    principalData.Add(dr);
                }
            }

            return principalData;
        }

        protected List<IndexForecast> ComputeScenarioStandardisedData(List<PDI_StatisticalInputs> statisticalInputs)
        {
            List<IndexForecast> standardisedData = new List<IndexForecast>();

            //var statisticalInputs = GetStatisticalInputData();
            var originalData = GetScenarioProjectionOriginalData();
            var macroeconomicMean = statisticalInputs.FirstOrDefault(o => o.Mode == StatisticalInputsRowKeys.Mean);
            var macroeconomicStandardDeviation = statisticalInputs.FirstOrDefault(o => o.Mode == StatisticalInputsRowKeys.StandardDeviation);


            foreach (var row in originalData)
            {
                var dr = new IndexForecast();
                dr.Date = row.Date;
                dr.MacroEconomicVariableId = row.MacroEconomicVariableId;
                dr.MacroEconomicValue = row.MacroEconomicValue - macroeconomicMean.MacroEconomicValue / macroeconomicStandardDeviation.MacroEconomicValue;

                standardisedData.Add(dr);
            }

            return standardisedData;
        }

        protected List<IndexForecast> GetScenarioProjectionOriginalData()
        {
            var originalData = new List<IndexForecast>();

            var projections = GetScenarioProjectionData();

            //originalData = projections.AsEnumerable()
            //                    .Where(row => row.Field<DateTime>(MacroeconomicProjectionColumns.Date) > TempEclData.ReportDate)
            //                    .CopyToDataTable();

            for (int i = 0; i < projections.Count; i++)
            {
                if (projections[i].Date > ECLNonStringConstants.i.reportingDate) // && i > 3
                {
                    var dr = new IndexForecast();
                    dr.Date = projections[i].Date;
                    dr.MacroEconomicVariableId = projections[i].MacroEconomicVariableId;
                    if(this._Scenario== ECL_Scenario.Best)
                        dr.MacroEconomicValue = projections[i].BestEstimateMacroEconomicValue;
                    if (this._Scenario == ECL_Scenario.Downturn)
                        dr.MacroEconomicValue = projections[i].DownturnMacroEconomicValue;
                    if (this._Scenario == ECL_Scenario.Optimistic)
                        dr.MacroEconomicValue = projections[i].OptimisticMacroEconomicValue;

                    originalData.Add(dr);
                }
            }


            return originalData;
        }

        protected double ComputeHistoricIndexStandardDeviation()
        {
            return ExcelFormulaUtil.CalculateStdDev(new ProcessECL_Wholesale_PD(this._eclId).Get_PDI_HistoricIndex().Select(o=>o.Actual));
        }
        protected double ComputeHistoricIndexMean()
        {
            return new ProcessECL_Wholesale_PD(this._eclId).Get_PDI_HistoricIndex().Average(o => o.Actual);
        }

        private List<PDI_MacroEconomics> GetScenarioProjectionData()
        {
            var obj = new ProcessECL_Wholesale_PD(this._eclId).Get_PDI_MacroEconomics();
            return obj;
        }

        protected List<PDI_StatisticalInputs> GetStatisticalInputData()
        {
            var obj = new ProcessECL_Wholesale_PD(this._eclId).Get_PDI_StatisticalInputs();
            return obj;
        }

        
        
    }
}
