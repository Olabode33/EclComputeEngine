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
    public class ScenarioLifetimePd
    {
        private ECL_Scenario _scenario;
        protected ScenarioMarginalPd _scenarioMarginalPd;

        Guid _eclId;
        EclType _eclType;
        public ScenarioLifetimePd(ECL_Scenario scenario, Guid eclId, EclType eclType)
        {
            _scenario = scenario;
            this._eclId = eclId;
            this._eclType = eclType;
            _scenarioMarginalPd = new ScenarioMarginalPd(_scenario, eclId, this._eclType);
        }

        public string Run()
        {
            var output = ComputeLifetimePd();
            var dt = new DataTable();

            var c = new LifeTimeObject();

            Type myObjOriginalType = c.GetType();
            PropertyInfo[] myProps = myObjOriginalType.GetProperties();

            for (int i = 0; i < myProps.Length; i++)
            {
                dt.Columns.Add(myProps[i].Name, myProps[i].PropertyType);
            }

            foreach (var _d in output)
            {
                _d.Id = Guid.NewGuid();
                _d.WholesaleEclId = _eclId;
                dt.Rows.Add(new object[]
                    {
                            _d.Id, _d.PdGroup, _d.Month, _d.Value, _d.WholesaleEclId
                    });
            }
            var tableName = "";

            if(_scenario== ECL_Scenario.Best)
            {
                tableName = ECLStringConstants.i.PdLifetimeBests_Table(this._eclType);
            }
            else if (_scenario == ECL_Scenario.Downturn)
            {
                tableName = ECLStringConstants.i.PdLifetimeDownturns_Table(this._eclType);
            }
            else if (_scenario == ECL_Scenario.Optimistic)
            {
                tableName = ECLStringConstants.i.PdLifetimeOptimistics_Table(this._eclType);
            }

            var r = DataAccess.i.ExecuteBulkCopy(dt, tableName);

            return r > 0 ? "" : $"Could not Bulk Insert [{tableName}]";
        }

        public List<LifeTimeObject> ComputeLifetimePd()
        {
            var lifetimePd = new List<LifeTimeObject>();

            var marginalPd = GetScenarioMarginalPd();

            foreach (var row in marginalPd)
            {
                double month1 = GetMonth1MarginalPdForPdGroup(marginalPd, row.PdGroup, row.Month);
                double pd = row.Value;

                var dr = new LifeTimeObject();
                dr.PdGroup = row.PdGroup;
                dr.Month = row.Month;
                dr.Value = row.Month == 1 ? row.Value : month1 * row.Value;

                lifetimePd.Add(dr);
            }

            return lifetimePd;
        }

        protected double GetMonth1MarginalPdForPdGroup(List<LifeTimeObject> marginalPd, string pdGroup, int month)
        {
            var range = marginalPd.AsEnumerable()
                            .Where(x => x.PdGroup == pdGroup
                                                && x.Month < (month == 1 ? 2 : month))
                            .Select(x => x.Value).ToArray();
            var aggr = range.Aggregate(1.0, (acc, x) => acc * (1.0 - x));
            return aggr;
        }


        protected List<LifeTimeObject> GetScenarioMarginalPd()
        {
            return _scenarioMarginalPd.ComputeMaginalPd();
        }
    }
}
