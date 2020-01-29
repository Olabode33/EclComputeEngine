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
        EclType _eclType;
        public IndexForecastWorkings(ECL_Scenario eCL_Scenario, Guid eclId, EclType eclType)
        {
            this._Scenario = eCL_Scenario;
            this._eclId = eclId;
            this._eclType = eclType;
        }

        public List<IndexForecast> ComputeIndexForecast()
        {
            List<IndexForecast> indexForecast = new List<IndexForecast>();

            var principalData = ComputeScenarioPrincipalComponents();

            double indexStandardDeviation = ComputeHistoricIndexStandardDeviation();
            double indexMean = ComputeHistoricIndexMean();


            foreach (var itm in principalData)
            {
                double actual = itm.Principal1 * ECLNonStringConstants.i.IndexWeight1 + itm.Principal2 * ECLNonStringConstants.i.IndexWeight2;
              
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

                double[] standardised = new double[date_standardisedData.Count];
                double[] principal1 = new double[macroeconomicPrincipal1.Count];
                double[] principal2 = new double[macroeconomicPrincipal2.Count];

                for (int i = 0; i < date_standardisedData.Count; i++)
                {
                    standardised[i] = date_standardisedData[i].MacroEconomicValue;

                    if(principal1.Length-1>=i)
                    {
                        var p1 = macroeconomicPrincipal1.FirstOrDefault(o => o.MacroEconomicVariableId == date_standardisedData[i].MacroEconomicVariableId);
                        principal1[i] = p1 != null ? p1.MacroEconomicValue : 0;
                    }

                    if (principal2.Length - 1 >= i)
                    {
                        var p2 = macroeconomicPrincipal2.FirstOrDefault(o => o.MacroEconomicVariableId == date_standardisedData[i].MacroEconomicVariableId);
                        principal2[i] = p2 != null ? p2.MacroEconomicValue : 0;
                    }
                }

                foreach(var itm in standardisedData.Where(o=>o.Date== dt.Date).ToList())
                {
                    itm.Principal1 = ExcelFormulaUtil.SumProduct(standardised, principal1);
                    itm.Principal2 = ExcelFormulaUtil.SumProduct(standardised, principal2);

                    principalData.Add(itm);
                }

            }

            return principalData;
        }

        protected List<IndexForecast> ComputeScenarioStandardisedData(List<PDI_StatisticalInputs> statisticalInputs)
        {
            List<IndexForecast> standardisedData = new List<IndexForecast>();

            //var statisticalInputs = GetStatisticalInputData();
            var originalData = GetScenarioProjectionOriginalData();

            //double macroeconomicStandardDeviation = ComputeHistoricIndexStandardDeviation();
            //double macroeconomicMean = ComputeHistoricIndexMean();

            foreach (var row in originalData)
            {
                var macroeconomicMean = statisticalInputs.FirstOrDefault(o => o.MacroEconomicVariableId==row.MacroEconomicVariableId && o.Mode == StatisticalInputsRowKeys.Mean);
                var macroeconomicStandardDeviation = statisticalInputs.FirstOrDefault(o => o.MacroEconomicVariableId == row.MacroEconomicVariableId && o.Mode == StatisticalInputsRowKeys.StandardDeviation);

                var dr = new IndexForecast();
                dr.Date = row.Date;
                dr.MacroEconomicVariableId = row.MacroEconomicVariableId;
                dr.MacroEconomicValue = (row.MacroEconomicValue - macroeconomicMean.MacroEconomicValue) / macroeconomicStandardDeviation.MacroEconomicValue;

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

            var MEVBackDate = new AffiliateMicroEconomicsVariable().AffiliateMEVBackDateValues(_eclId, _eclType);

            for (int i = 0; i < projections.Count; i++)
            {
                if (projections[i].Date > ECLNonStringConstants.i.reportingDate) // && i > 3
                {

                    var itm = projections[i];
                    var bdate = MEVBackDate.FirstOrDefault(o => o.MicroEconomicId == itm.MacroEconomicVariableId);
                    var _bdate = 0;
                    if (bdate != null)
                    {
                        if(bdate.BackDateQuarters==1)
                        {
                            bdate.BackDateQuarters = bdate.BackDateQuarters;
                        }
                        _bdate = bdate.BackDateQuarters * 3;
                    }
                    var _dt = itm.Date.AddMonths(-_bdate);
                    var _itm = projections.OrderBy(p => p.Date).FirstOrDefault(o => o.MacroEconomicVariableId==itm.MacroEconomicVariableId && o.Date.Month== _dt.Month && o.Date.Year== _dt.Year); // == GetLastDayOfMonth(itm.Date.AddMonths(-_bdate))

                    var dr = new IndexForecast();
                    dr.Date = itm.Date;
                    dr.MacroEconomicVariableId = itm.MacroEconomicVariableId;
                    if (this._Scenario == ECL_Scenario.Best)
                        dr.MacroEconomicValue = _itm.BestEstimateMacroEconomicValue;
                    if (this._Scenario == ECL_Scenario.Downturn)
                        dr.MacroEconomicValue = _itm.DownturnMacroEconomicValue;
                    if (this._Scenario == ECL_Scenario.Optimistic)
                        dr.MacroEconomicValue = _itm.OptimisticMacroEconomicValue;

                    originalData.Add(dr);
                }
            }


            return originalData;
        }

        private DateTime GetLastDayOfMonth(DateTime dateTime)
        {
            return dateTime.AddMonths(1).AddDays(-1);
        }

        protected double ComputeHistoricIndexStandardDeviation()
        {
            return ExcelFormulaUtil.CalculateStdDev(new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_HistoricIndex().Select(o=>o.Actual));
        }
        protected double ComputeHistoricIndexMean()
        {
            return new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_HistoricIndex().Average(o => o.Actual);
        }

        private List<PDI_MacroEconomics> GetScenarioProjectionData()
        {
            var obj = new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_MacroEconomics();
            return obj;
        }

        protected List<PDI_StatisticalInputs> GetStatisticalInputData()
        {
            var obj = new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_StatisticalInputs();
            return obj;
        }

        
        
    }
}
