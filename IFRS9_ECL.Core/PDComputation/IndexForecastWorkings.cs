using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Core.PDComputation.cmPD;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
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

            var cp = new Macro_Processor().GetMacroResult_Statistics(this._eclId, this._eclType);

            foreach (var itm in principalData)
            {
                double actual = (itm.Principal1 * cp.IndexWeight1.Value) + (itm.Principal2 * cp.IndexWeight2.Value) + (itm.Principal3 * cp.IndexWeight3.Value) + (itm.Principal4 * cp.IndexWeight4.Value);
              
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
            var macroeconomicPrincipal3 = statisticalInputs.Where(o => o.Mode == StatisticalInputsRowKeys.PrincipalScore3).ToList();
            var macroeconomicPrincipal4 = statisticalInputs.Where(o => o.Mode == StatisticalInputsRowKeys.PrincipalScore4).ToList();


            var groupedDate = standardisedData.GroupBy(x => x.Date).Select(x => new { Date = x.Key, Cnt = x.Count() }).ToList();
            var macroEconomicCount= groupedDate.Max(r => r.Cnt);

            foreach (var dt in groupedDate)
            {
                var date_standardisedData = standardisedData.Where(o => o.Date == dt.Date).ToList();

                double[] standardised = new double[date_standardisedData.Count];
                double[] principal1 = new double[macroeconomicPrincipal1.Count];
                double[] principal2 = new double[macroeconomicPrincipal2.Count];
                double[] principal3 = new double[macroeconomicPrincipal3.Count];
                double[] principal4 = new double[macroeconomicPrincipal4.Count];

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
                    if (principal3.Length - 1 >= i)
                    {
                        var p3 = macroeconomicPrincipal3.FirstOrDefault(o => o.MacroEconomicVariableId == date_standardisedData[i].MacroEconomicVariableId);
                        principal3[i] = p3 != null ? p3.MacroEconomicValue : 0;
                    }
                    if (principal2.Length - 1 >= i)
                    {
                        var p4 = macroeconomicPrincipal4.FirstOrDefault(o => o.MacroEconomicVariableId == date_standardisedData[i].MacroEconomicVariableId);
                        principal4[i] = p4 != null ? p4.MacroEconomicValue : 0;
                    }
                }

                foreach(var itm in standardisedData.Where(o=>o.Date== dt.Date).ToList())
                {
                    itm.Principal1 = ExcelFormulaUtil.SumProduct(standardised, principal1);
                    itm.Principal2 = ExcelFormulaUtil.SumProduct(standardised, principal2);
                    itm.Principal3 = ExcelFormulaUtil.SumProduct(standardised, principal3);
                    itm.Principal4 = ExcelFormulaUtil.SumProduct(standardised, principal4);

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


            
            
                var qry = Queries.Get_AffiliateMEVBackDateValues(this._eclId, this._eclType);
                var dt = DataAccess.i.GetData(qry);
            var MEVBackDate = new List<AffiliateMEVBackDateValues>();

            foreach (DataRow dr in dt.Rows)
            {
                MEVBackDate.Add(DataAccess.i.ParseDataToObject(new AffiliateMEVBackDateValues(), dr));
            }

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
            var actualMacEcoVar = new Macro_Processor().Get_MacroResult_SelectedMacroEconomicVariables(this._eclId, this._eclType.ToString());

            var statInput = new Macro_Processor().GetMacroResult_PCSummary(this._eclId, this._eclType);

            var itms = new List<PDI_StatisticalInputs>();

            var varBle = statInput.Max(o => o.PrincipalComponentIdB) - 3;
            foreach (var itm in statInput)
            {
                var o = new PDI_StatisticalInputs();

                if (itm.PrincipalComponentIdA == 1)
                {
                    o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
                    o.MacroEconomicValue = itm.Value.Value;
                    o.Mode = itm.PricipalComponentLabelA;
                }
                if (itm.PrincipalComponentIdA == 2)
                {
                    o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
                    o.MacroEconomicValue = itm.Value.Value;
                    o.Mode = itm.PricipalComponentLabelA;
                }
                if (itm.PrincipalComponentIdA == 3)
                {
                    o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
                    o.MacroEconomicValue = itm.Value.Value;
                    o.Mode = itm.PricipalComponentLabelA;
                }
                if (itm.PrincipalComponentIdA == 4)
                {
                    o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
                    o.MacroEconomicValue = itm.Value.Value;
                    o.Mode = itm.PricipalComponentLabelA;
                }
                if (itm.PrincipalComponentIdA == 5)
                {
                    o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
                    o.MacroEconomicValue = itm.Value.Value;
                    o.Mode = itm.PricipalComponentLabelA;
                }
                if (itm.PrincipalComponentIdA == 6)
                {
                    o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
                    o.MacroEconomicValue = itm.Value.Value;
                    o.Mode = itm.PricipalComponentLabelA;
                }
                if (itm.PrincipalComponentIdA == 7)
                {
                    o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
                    o.MacroEconomicValue = itm.Value.Value;
                    o.Mode = itm.PricipalComponentLabelA;
                }
                itms.Add(o);
            }
            return itms;

            //var obj = new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_StatisticalInputs();
            //return obj;
        }

        
        
    }
}
