using IFRS9_ECL.Core.PDComputation.cmPD;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.PDComputation
{
    public class CreditIndex
    {
        protected const int _maxCreditIndexMonth = 60;
        protected VasicekWorkings _vasicekWorkings;
        protected IndexForecastWorkings _indexForecastBest;
        protected IndexForecastWorkings _indexForecastOptimistics;
        protected IndexForecastWorkings _indexForecastDownturn;

        Guid _eclId;
        EclType _eclType;
        
        public CreditIndex(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
            _vasicekWorkings = new VasicekWorkings(ECL_Scenario.Best, this._eclId, this._eclType);
            _indexForecastBest = new IndexForecastWorkings(ECL_Scenario.Best, this._eclId, this._eclType);
            _indexForecastOptimistics = new IndexForecastWorkings(ECL_Scenario.Optimistic, this._eclId, this._eclType);
            _indexForecastDownturn = new IndexForecastWorkings(ECL_Scenario.Downturn, this._eclId, this._eclType);
            
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


        public string Run()
        {
            var creditIndices = ComputeCreditIndex();

            var r = Util.FileSystemStorage<CreditIndex_Output>.WriteCsvData(_eclId, ECLStringConstants.i.PDCreditIndex_Table(_eclType), creditIndices);

            return r? "" : $"Could not Bulk Insert [{ECLStringConstants.i.PDCreditIndex_Table(this._eclType)}]";
        }

        public List<CreditIndex_Output> GetCreditIndexResult()
        {
            var creditIndex = Util.FileSystemStorage<CreditIndex_Output>.ReadCsvData(this._eclId, ECLStringConstants.i.PDCreditIndex_Table(this._eclType));
           
            return creditIndex;
        }

        private List<CreditIndex_Output> ComputeCreditIndex()
        {
            var creditIndices = new List<CreditIndex_Output>();

            var indexForecastBest = GetScenarioIndexForecasting(_indexForecastBest);
            var indexForecastOptimistic = GetScenarioIndexForecasting(_indexForecastOptimistics);
            var indexForecastDownturn = GetScenarioIndexForecasting(_indexForecastDownturn);
            indexForecastBest = indexForecastBest.OrderBy(o => o.Date).Take(24).ToList();
            indexForecastOptimistic = indexForecastOptimistic.OrderBy(o => o.Date).Take(24).ToList();
            indexForecastDownturn = indexForecastDownturn.OrderBy(o => o.Date).Take(24).ToList();

            double vasicekIndexUsed = GetScenarioVasicekIndex();
            var rpDate = GetReportingDate(_eclType, _eclId);
            for (int month = 0; month <= _maxCreditIndexMonth; month++)
            {
                int monthOffset = Convert.ToInt32((month - 1) / 3) * 3 + 3;
                
                var eoMonth = ExcelFormulaUtil.EOMonth(rpDate, monthOffset);
               
                var dr = new CreditIndex_Output();
                dr.ProjectionMonth = month;

                //***************************************************
                double standard = 0;
                var _indexForecastBest = indexForecastBest.FirstOrDefault(o => o.Date == eoMonth);
                if (_indexForecastBest != null)
                    standard = _indexForecastBest.Standardised;

                dr.BestEstimate = month < 1 ? vasicekIndexUsed : standard;

                standard = 0;
                var _indexForecastOptimistic = indexForecastOptimistic.FirstOrDefault(o => o.Date == eoMonth);
                if (_indexForecastOptimistic != null)
                    standard = _indexForecastOptimistic.Standardised;

                dr.Optimistic = month < 1 ? vasicekIndexUsed : standard;


                standard = 0;
                var _indexForecastDownturn = indexForecastDownturn.FirstOrDefault(o => o.Date == eoMonth);
                if (_indexForecastDownturn != null)
                    standard = _indexForecastDownturn.Standardised;

                dr.Downturn = month < 3 ? vasicekIndexUsed : standard;

                creditIndices.Add(dr);
            }

            return creditIndices;
        }

        protected List<IndexForecast> GetScenarioIndexForecasting(IndexForecastWorkings indexForecastWorkings)
        {
            return indexForecastWorkings.ComputeIndexForecast();
        }
        protected double GetScenarioVasicekIndex()
        {
            try { return _vasicekWorkings.ComputeEtiNplIndex().Last().Index; } catch { return 0; }
        }
    }
}
