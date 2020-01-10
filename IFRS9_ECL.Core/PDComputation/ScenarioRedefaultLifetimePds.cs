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
    public class ScenarioRedefaultLifetimePds
    {
        private ECL_Scenario _scenario;
        protected ScenarioMarginalPd _scenarioMarginalPd;
        Guid _eclId;
        public ScenarioRedefaultLifetimePds(ECL_Scenario scenario, Guid eclId)
        {
            _scenario = scenario;
            this._eclId = eclId;
            _scenarioMarginalPd = new ScenarioMarginalPd(_scenario, this._eclId);
        }
        public string Run()
        {
            var output = ComputeRedefaultLifetimePd();

            var dt = new DataTable();
            foreach (var _d in output)
            {
                _d.Id = Guid.NewGuid();
                _d.Id = _eclId;
                dt.Rows.Add(new object[]
                    {
                            _d.Id, _d.PdGroup, _d.Month, _d.Value, _d.WholesaleEclId
                    });
            }
            var tableName = "";

            if (_scenario == ECL_Scenario.Best)
            {
                tableName = ECLStringConstants.i.WholesalePdRedefaultLifetimeBests_Table;
            }
            else if (_scenario == ECL_Scenario.Downturn)
            {
                tableName = ECLStringConstants.i.WholesalePdRedefaultLifetimeDownturns_Table;
            }
            else if (_scenario == ECL_Scenario.Optimistic)
            {
                tableName = ECLStringConstants.i.WholesalePdRedefaultLifetimeOptimistics_Table;
            }

            var r = DataAccess.i.ExecuteBulkCopy(dt, tableName);

            return r > 0 ? "" : $"Could not Bulk Insert [{tableName}]";

        }

        public List<LifeTimeObject> ComputeRedefaultLifetimePd()
        {
            var redefaultLifetimePd = new List<LifeTimeObject>();

            var marginalPd = GetScenarioMarginalPd();
            double readjustmentFactor = GetRedefaultAdjustmentFactor();

            double test = GetMonthMarginalPdForPdGroup(marginalPd, "1", 10, readjustmentFactor);
            double test2 = marginalPd.FirstOrDefault(row => row.PdGroup == "1" && row.Month == 10).Value;

            double test3 = test2 * readjustmentFactor;
            double test4 = test3 * test;

            foreach (var row in marginalPd)
            {
                double prevValue = GetMonthMarginalPdForPdGroup(marginalPd, row.PdGroup,
                                                                            row.Month,
                                                                            readjustmentFactor);
                double marginalPdValue = row.Value;

                var dr = new LifeTimeObject();
                dr.PdGroup = row.PdGroup;
                dr.Month = row.Month;
                dr.Value = row.Month == 1 ? Math.Min(marginalPdValue * readjustmentFactor, 1.0) : prevValue * Math.Min(marginalPdValue * readjustmentFactor, 1.0);

                redefaultLifetimePd.Add(dr);
            }

            return redefaultLifetimePd;
        }
        //0.98867548257824434 || 0.00154465569130425
        protected double GetMonthMarginalPdForPdGroup(List<LifeTimeObject> marginalPd, string pdGroup, int month, double readjustmentFactor)
        {
            var range = marginalPd.Where(x => x.PdGroup == pdGroup && x.Month < (month == 1 ? 2 : month))
                            .Select(x => {
                                double value = x.Value;

                                return Math.Min(value * readjustmentFactor, 1.0);
                            }).ToArray();
            var aggr = range.Aggregate(1.0, (acc, x) => acc * (1.0 - x));
            return aggr;
        }
        protected double GetRedefaultAdjustmentFactor()
        {
            return Convert.ToDouble(GetPdInputAssumptions().FirstOrDefault(row => row.Assumptions == PdAssumptionsRowKey.ReDefaultAdjustmentFactor).Value);
        }
        protected List<PDI_Assumptions> GetPdInputAssumptions()
        {
            return new ProcessECL_Wholesale_PD(this._eclId).Get_PDI_Assumptions();
        }
        protected List<LifeTimeObject> GetScenarioMarginalPd()
        {
            return _scenarioMarginalPd.ComputeMaginalPd();
        }
    }
}
