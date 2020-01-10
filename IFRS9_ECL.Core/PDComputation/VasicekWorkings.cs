﻿using IFRS9_ECL.Core.PDComputation.cmPD;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.PDComputation
{
    public class VasicekWorkings
    {
        private ECL_Scenario _scenario;

        Guid _eclId;

        public VasicekWorkings(ECL_Scenario screnario, Guid eclId)
        {
            this._eclId = eclId;
            _scenario = screnario;
        }

        public void Run()
        {
            List<VasicekEtiNplIndex> vasicek = ComputeVasicekScenario();
            string stop = "Stop";
        }

        public List<VasicekEtiNplIndex> ComputeVasicekScenario()
        {
            var vasicek = new List<VasicekEtiNplIndex>();
            double pdTtc = ComputePdTtc();
            double averageFittedIndex = ComputeVasicekAverageFitted();
            var indexForecast = GetScenarioIndexForecastResult();
            int month = 1;

            foreach (var row in indexForecast)
            {
                double scenarioPd = ComputeVasicekIndex(row.Standardised, pdTtc, ECLNonStringConstants.i.Rho);

                var dr = new VasicekEtiNplIndex();
                dr.Date = row.Date;
                dr.Month = month;
                dr.ScenarioIndex = row.Standardised;
                dr.ScenarioPd = scenarioPd;
                dr.ScenarioFactor = averageFittedIndex == 0 ? 1 : scenarioPd / averageFittedIndex;

                vasicek.Add(dr);
                month++;
            }

            return vasicek;
        }
        protected List<IndexForecast> GetScenarioIndexForecastResult()
        {
            var indexForecastWorkings = new IndexForecastWorkings(_scenario, this._eclId);
            return indexForecastWorkings.ComputeIndexForecast();
        }
        protected double ComputeVasicekAverageFitted()
        {
            var fitted = ComputeEtiNplIndex();
            return fitted.Where(m => m.Date >= new DateTime(ECLNonStringConstants.i.reportingDate.Year - 3, ECLNonStringConstants.i.reportingDate.Month, ECLNonStringConstants.i.reportingDate.Day))
                    .Average(o => o.Fitted);
        }
        public List<VasicekEtiNplIndex> ComputeEtiNplIndex()
        {
            var etiNpl = new ProcessECL_Wholesale_PD(this._eclId).Get_PDI_ETI_NPL();
            var historicIndex = new ProcessECL_Wholesale_PD(this._eclId).Get_PDI_HistoricIndex();
            double pdTtc = ComputePdTtc();

            var vasicekEtiNplIndex = new List<VasicekEtiNplIndex>();

            foreach (var etiNplRecord in etiNpl)
            {
                double index = historicIndex.FirstOrDefault(o => o.Date == etiNplRecord.Date).Standardised;

                var newRecord = new VasicekEtiNplIndex();
                newRecord.Date = etiNplRecord.Date;
                newRecord.EtiNpl = etiNplRecord.Series;
                newRecord.Index = index;
                newRecord.Fitted = ComputeVasicekIndex(index, pdTtc, ECLNonStringConstants.i.Rho);
                newRecord.Residuals = etiNplRecord.Series - ComputeVasicekIndex(index, pdTtc, ECLNonStringConstants.i.Rho);

                vasicekEtiNplIndex.Add(newRecord);
            }

            return vasicekEtiNplIndex;
        }
        protected double ComputeVasicekIndex(double index, double pd_ttc, double rho)
        {
            //var t1 = ExcelFormulaUtil.NormSInv(pd_ttc);
            //var t2 = Math.Sqrt(rho);
            //var t3 = Math.Sqrt(1 - rho);
            //var t4 = (ExcelFormulaUtil.NormSInv(pd_ttc) + Math.Sqrt(rho) * index);
            //var t5 = (ExcelFormulaUtil.NormSInv(pd_ttc) + Math.Sqrt(rho) * index) / Math.Sqrt(1 - rho);
            //var tF = ExcelFormulaUtil.NormSDist((ExcelFormulaUtil.NormSInv(pd_ttc) + Math.Sqrt(rho) * index) / Math.Sqrt(1 - rho));
            return ExcelFormulaUtil.NormSDist((ExcelFormulaUtil.NormSInv(pd_ttc) + Math.Sqrt(rho) * index) / Math.Sqrt(1 - rho));
        }
        protected double ComputePdTtc()
        {
            return new ProcessECL_Wholesale_PD(this._eclId).Get_PDI_ETI_NPL().Average(o=>o.Series);
        }

        
    }
}
