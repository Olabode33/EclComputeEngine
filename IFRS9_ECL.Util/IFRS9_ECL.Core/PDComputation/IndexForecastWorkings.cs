using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Core.PDComputation.cmPD;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
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

        private DateTime GetReportingDate(EclType _eclType, Guid eclId)
        {
            var ecls = Queries.EclsRegister(_eclType.ToString(), _eclId.ToString());
            var dtR = DataAccess.i.GetData(ecls);
            if (dtR.Rows.Count > 0)
            {
                var itm = DataAccess.i.ParseDataToObject(new EclRegister(), dtR.Rows[0]);
                return itm.ReportingDate;
            }
            return DateTime.Now;
        }


        public List<IndexForecast> ComputeIndexForecast()
        {
            List<IndexForecast> indexForecast = new List<IndexForecast>();

            var statisticalInputs = GetStatisticalInputData();
            var principalData = ComputeScenarioPrincipalComponents(statisticalInputs);

            double indexStandardDeviation = ComputeHistoricIndexStandardDeviation();
            double indexMean = ComputeHistoricIndexMean();

            var cp = new Macro_Processor().GetMacroResult_Statistics(this._eclId, this._eclType);

            var engenValues = statisticalInputs.Where(o => o.Mode == StatisticalInputsRowKeys.Eigenvalues).Select(p=>p.MacroEconomicValue).ToList();

            for (int i = 0; i < engenValues.Count; i++)
            {
                if (i == 0)
                    cp.IndexWeight1 = engenValues[i] / engenValues.Take(2).Sum();

                if (i == 1)
                    cp.IndexWeight2 = engenValues[i] / engenValues.Take(2).Sum();

                if (i == 2)
                    cp.IndexWeight3 = 0;// engenValues[i] / engenValues.Take(3).Sum();

                if (i == 3)
                    cp.IndexWeight4 = 0;// engenValues[i] / engenValues.Take(4).Sum();
            }

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

        protected List<IndexForecast> ComputeScenarioPrincipalComponents(List<PDI_StatisticalInputs> statisticalInputs)
        {
            var principalData = new List<IndexForecast>();

            
            var standardisedData = ComputeScenarioStandardisedData(statisticalInputs);
            standardisedData=standardisedData.OrderBy(o => o.Date).ToList();
            var macroeconomicPrincipal1 = statisticalInputs.Where(o => o.Mode == StatisticalInputsRowKeys.PrincipalScore1).ToList();
            var macroeconomicPrincipal2 = statisticalInputs.Where(o => o.Mode == StatisticalInputsRowKeys.PrincipalScore2).ToList();
            var macroeconomicPrincipal3 = statisticalInputs.Where(o => o.Mode == StatisticalInputsRowKeys.PrincipalScore3).ToList();
            var macroeconomicPrincipal4 = statisticalInputs.Where(o => o.Mode == StatisticalInputsRowKeys.PrincipalScore4).ToList();


            var groupedDate = standardisedData.GroupBy(x => x.Date).Select(x => new { Date = x.Key, Cnt = x.Count() }).ToList();
            var macroEconomicCount = 0;
            if (groupedDate.Count > 0)
            {
                macroEconomicCount=groupedDate.Max(r => r.Cnt);
            }

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

                    if (principal1.Length - 1 >= i)
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
                    if (principal4.Length - 1 >= i)
                    {
                        var p4 = macroeconomicPrincipal4.FirstOrDefault(o => o.MacroEconomicVariableId == date_standardisedData[i].MacroEconomicVariableId);
                        principal4[i] = p4 != null ? p4.MacroEconomicValue : 0;
                    }
                }

                var itm = standardisedData.FirstOrDefault(o => o.Date == dt.Date);

                itm.Principal1 = ExcelFormulaUtil.SumProduct(standardised, principal1);
                itm.Principal2 = ExcelFormulaUtil.SumProduct(standardised, principal2);
                itm.Principal3 = ExcelFormulaUtil.SumProduct(standardised, principal3);
                itm.Principal4 = ExcelFormulaUtil.SumProduct(standardised, principal4);

                //Log4Net.Log.Info($"{itm.Date},{itm.MacroEconomicVariableId},{itm.MacroEconomicValue},{this._Scenario.ToString()}++++++");
                principalData.Add(itm);
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
                //****************************************
                if(macroeconomicMean==null)
                    macroeconomicMean = new PDI_StatisticalInputs { MacroEconomicValue = 0, MacroEconomicVariableId = 0 };

                if(macroeconomicStandardDeviation== null)
                    macroeconomicStandardDeviation = new PDI_StatisticalInputs { MacroEconomicValue = 0, MacroEconomicVariableId = 0 };

                var dr = new IndexForecast();
                dr.Date = row.Date;
                dr.MacroEconomicVariableId = row.MacroEconomicVariableId;
                if(macroeconomicStandardDeviation.MacroEconomicValue==0)
                {
                    macroeconomicStandardDeviation.MacroEconomicValue = 1;
                }
                dr.MacroEconomicValue = (row.MacroEconomicValue - macroeconomicMean.MacroEconomicValue) / macroeconomicStandardDeviation.MacroEconomicValue;

                standardisedData.Add(dr);
                //Log4Net.Log.Info($"{dr.Date},{dr.MacroEconomicVariableId},{dr.MacroEconomicValue},{this._Scenario.ToString()}++++++");
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


            var qry = Queries.Get_AffiliateId(this._eclId, this._eclType);
            var dt = DataAccess.i.GetData(qry);

            var affiliateId = Convert.ToInt32(dt.Rows[0][0]);

            qry = Queries.Get_AffiliateMEVBackDateValues(this._eclId, this._eclType);
                dt = DataAccess.i.GetData(qry);

            

            var MEVBackDate = new List<AffiliateMEVBackDateValues>();

            foreach (DataRow dr in dt.Rows)
            {
                MEVBackDate.Add(DataAccess.i.ParseDataToObject(new AffiliateMEVBackDateValues(), dr));
            }

            var lastMacroVariableId = MEVBackDate.Select(o => o.MicroEconomicId).Distinct().Last();

            projections = projections.OrderByDescending(o => o.Date).ToList();

            for (int i = 0; i < projections.Count; i++)
            {
                if (1!=1)//(affiliateId == 5 || affiliateId == 46 || affiliateId == 47) && projections[i].MacroEconomicVariableId == lastMacroVariableId)
                {
                    var prevItm = projections.FirstOrDefault(o => o.MacroEconomicVariableId == lastMacroVariableId && o.Date < projections[i].Date);
                    if(prevItm!=null)
                    {
                        projections[i].BestEstimateMacroEconomicValue = projections[i].BestEstimateMacroEconomicValue - prevItm.BestEstimateMacroEconomicValue;
                        projections[i].OptimisticMacroEconomicValue = projections[i].OptimisticMacroEconomicValue - prevItm.OptimisticMacroEconomicValue;
                        projections[i].DownturnMacroEconomicValue = projections[i].DownturnMacroEconomicValue - prevItm.DownturnMacroEconomicValue;
                    }
                    else
                    {
                        projections[i].BestEstimateMacroEconomicValue = 0;
                        projections[i].OptimisticMacroEconomicValue = 0;
                        projections[i].DownturnMacroEconomicValue = 0;
                    }
                }
            }


            var reportingDate= GetReportingDate(_eclType, _eclId);
            //Log4Net.Log.Info("=================");
            for (int i = 0; i < projections.Count; i++)
            {
                if (projections[i].Date > reportingDate)// && i > 3
                {

                    var itm = projections[i];
                    var bdate = MEVBackDate.FirstOrDefault(o => o.MicroEconomicId == itm.MacroEconomicVariableId);
                    var _bdate = 0;
                    if (bdate != null)
                    {
                        //if (bdate.BackDateQuarters == 1)
                        //{
                        //    bdate.BackDateQuarters = bdate.BackDateQuarters;
                        //}
                        _bdate = bdate.BackDateQuarters * 3;
                    }
                    var _dt = itm.Date.AddMonths(-_bdate);
                    var _itm = projections.OrderBy(p => p.Date).FirstOrDefault(o => o.MacroEconomicVariableId == itm.MacroEconomicVariableId && o.Date.Month == _dt.Month && o.Date.Year == _dt.Year); // == GetLastDayOfMonth(itm.Date.AddMonths(-_bdate))
                    if (_itm == null)
                    {
                        if (projections.Count > 0)
                            _itm = projections.Last();

                    }
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


                    //Log4Net.Log.Info($"{dr.Date},{dr.MacroEconomicVariableId},{dr.MacroEconomicValue},{this._Scenario.ToString()}");
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
            return Util.Computation.GetStandardDeviationS(new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_HistoricIndex().Select(o=>o.Actual).ToList());
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

            var prinCSummary = new Macro_Processor().GetMacroResult_PCSummary(this._eclId, this._eclType);

            var itms = new List<PDI_StatisticalInputs>();

            for(int i=0; i<actualMacEcoVar.Count; i++)
            {
                var sub = prinCSummary.Where(o => o.PrincipalComponentIdB == i + 4).OrderBy(p => p.PricipalComponentLabelA).ToList();
                foreach(var _v in sub)
                {
                    itms.Add(new PDI_StatisticalInputs { EclId= this._eclId, Mode= _v.PricipalComponentLabelA, MacroEconomicValue= _v.Value??0, MacroEconomicVariableId= actualMacEcoVar[i].MacroeconomicVariableId });
                }
            }

            return itms;

            //foreach (var itm in prinCSummary)
            //{
            //    var o = new PDI_StatisticalInputs();

            //    if (itm.PrincipalComponentIdA == 1)
            //    {
            //        if(actualMacEcoVar.Count> itm.PrincipalComponentIdB - varBle)
            //        {
            //            o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
            //            o.MacroEconomicValue = itm.Value.Value;
            //            o.Mode = itm.PricipalComponentLabelA;
            //        }
            //    }
            //    if (itm.PrincipalComponentIdA == 2)
            //    {
            //        if (actualMacEcoVar.Count > itm.PrincipalComponentIdB - varBle)
            //        {
            //            o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
            //            o.MacroEconomicValue = itm.Value.Value;
            //            o.Mode = itm.PricipalComponentLabelA;
            //        }
            //    }
            //    if (itm.PrincipalComponentIdA == 3)
            //    {
            //        if (actualMacEcoVar.Count > itm.PrincipalComponentIdB - varBle)
            //        {
            //            o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
            //            o.MacroEconomicValue = itm.Value.Value;
            //            o.Mode = itm.PricipalComponentLabelA;
            //        }
            //    }
            //    if (itm.PrincipalComponentIdA == 4)
            //    {
            //        if (actualMacEcoVar.Count > itm.PrincipalComponentIdB - varBle)
            //        {
            //            o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
            //            o.MacroEconomicValue = itm.Value.Value;
            //            o.Mode = itm.PricipalComponentLabelA;
            //        }
            //    }
            //    if (itm.PrincipalComponentIdA == 5)
            //    {
            //        if (actualMacEcoVar.Count > itm.PrincipalComponentIdB - varBle)
            //        {
            //            o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
            //            o.MacroEconomicValue = itm.Value.Value;
            //            o.Mode = itm.PricipalComponentLabelA;
            //        }
            //    }
            //    if (itm.PrincipalComponentIdA == 6)
            //    {
            //        if (actualMacEcoVar.Count > itm.PrincipalComponentIdB - varBle)
            //        {
            //            o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
            //            o.MacroEconomicValue = itm.Value.Value;
            //            o.Mode = itm.PricipalComponentLabelA;
            //        }
            //    }
            //    if (itm.PrincipalComponentIdA == 7)
            //    {
            //        if (actualMacEcoVar.Count > itm.PrincipalComponentIdB - varBle)
            //        {
            //            o.MacroEconomicVariableId = actualMacEcoVar[itm.PrincipalComponentIdB - varBle].MacroeconomicVariableId;
            //            o.MacroEconomicValue = itm.Value.Value;
            //            o.Mode = itm.PricipalComponentLabelA;
            //        }
            //    }
            //    itms.Add(o);
            //}



            //var obj = new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_StatisticalInputs();
            //return obj;
        }

        
        
    }
}
