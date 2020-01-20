using IFRS9_ECL.Core.PDComputation.cmPD;
using IFRS9_ECL.Data;
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
        public CreditIndex(Guid eclId)
        {
            this._eclId = eclId;
            _vasicekWorkings = new VasicekWorkings(ECL_Scenario.Best, this._eclId);
            _indexForecastBest = new IndexForecastWorkings(ECL_Scenario.Best, this._eclId);
            _indexForecastOptimistics = new IndexForecastWorkings(ECL_Scenario.Optimistic, this._eclId);
            _indexForecastDownturn = new IndexForecastWorkings(ECL_Scenario.Downturn, this._eclId);
        }

        public string Run()
        {
            var creditIndices = ComputeCreditIndex();

            var dt = new DataTable();
            var c = new CreditIndex_Output();

            Type myObjOriginalType = c.GetType();
            PropertyInfo[] myProps = myObjOriginalType.GetProperties();

            for (int i = 0; i < myProps.Length; i++)
            {
                dt.Columns.Add(myProps[i].Name, myProps[i].PropertyType);
            }

            foreach (var _d in creditIndices)
            {
                _d.Id = Guid.NewGuid();
                _d.WholesaleEclId = _eclId;
                dt.Rows.Add(new object[]
                    {
                            _d.Id, _d.ProjectionMonth, _d.BestEstimate, _d.Optimistic, _d.Downturn, _d.WholesaleEclId
                    });
            }
            var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.WholesalePDCreditIndex_Table);

            return r > 0 ? "" : $"Could not Bulk Insert [{ECLStringConstants.i.WholesalePDCreditIndex_Table}]";
        }

        public List<CreditIndex_Output> GetCreditIndexResult()
        {

            var qry = Queries.Credit_Index(this._eclId);
            var _lstRaw = DataAccess.i.GetData(qry);

            var creditIndex = new List<CreditIndex_Output>();
            foreach (DataRow dr in _lstRaw.Rows)
            {
                creditIndex.Add(DataAccess.i.ParseDataToObject(new CreditIndex_Output(), dr));
            }
            return creditIndex;
        }

        private List<CreditIndex_Output> ComputeCreditIndex()
        {
            var creditIndices = new List<CreditIndex_Output>();

            var indexForecastBest = GetScenarioIndexForecasting(_indexForecastBest);
            var indexForecastOptimistic = GetScenarioIndexForecasting(_indexForecastOptimistics);
            var indexForecastDownturn = GetScenarioIndexForecasting(_indexForecastDownturn);

            for (int month = 0; month <= _maxCreditIndexMonth; month++)
            {
                int monthOffset = Convert.ToInt32((month - 1) / 3) * 3 + 3;
                DateTime eoMonth = ExcelFormulaUtil.EOMonth(ECLNonStringConstants.i.reportingDate, monthOffset);
                double vasicekIndexUsed = GetScenarioVasicekIndex();

                var dr = new CreditIndex_Output();
                dr.ProjectionMonth = month;
                dr.BestEstimate = month < 1 ? vasicekIndexUsed : indexForecastBest.FirstOrDefault(o => o.Date == eoMonth).Standardised;
                dr.Optimistic = month < 1 ? vasicekIndexUsed : indexForecastOptimistic.FirstOrDefault(o => o.Date == eoMonth).Standardised;
                dr.Downturn = month < 3 ? vasicekIndexUsed : indexForecastDownturn.FirstOrDefault(o => o.Date == eoMonth).Standardised;

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
            return _vasicekWorkings.ComputeEtiNplIndex().FirstOrDefault(o => o.Date == ECLNonStringConstants.i.reportingDate).Index;
        }
    }
}
