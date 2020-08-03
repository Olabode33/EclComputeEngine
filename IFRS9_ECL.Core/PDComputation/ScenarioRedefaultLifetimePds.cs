using IFRS9_ECL.Core.Calibration;
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
    public class ScenarioRedefaultLifetimePds
    {
        private ECL_Scenario _scenario;
        protected ScenarioMarginalPd _scenarioMarginalPd;
        Guid _eclId;
        EclType _eclType;
        public ScenarioRedefaultLifetimePds(ECL_Scenario scenario, Guid eclId, EclType eclType)
        {
            _scenario = scenario;
            this._eclId = eclId;
            this._eclType = eclType;
            _scenarioMarginalPd = new ScenarioMarginalPd(_scenario, this._eclId, this._eclType);
        }
        public string Run()
        {
            var output = ComputeRedefaultLifetimePd();

            var dt = new DataTable();


            var c = new LifeTimeObject();

            Type myObjOriginalType = c.GetType();
            PropertyInfo[] myProps = myObjOriginalType.GetProperties();

            for (int i = 0; i < myProps.Length; i++)
            {
                dt.Columns.Add(myProps[i].Name, myProps[i].PropertyType);
            }
            dt.Columns.Add($"{this._eclType.ToString()}EclId", typeof(Guid));

            foreach (var _d in output)
            {
                _d.Id = Guid.NewGuid();
                dt.Rows.Add(new object[]
                    {
                            _d.Id, _d.PdGroup, _d.Month, _d.Value, _eclId
                    });
            }
            var tableName = "";

            if (_scenario == ECL_Scenario.Best)
            {
                tableName = ECLStringConstants.i.PdRedefaultLifetimeBests_Table(this._eclType);
            }
            else if (_scenario == ECL_Scenario.Downturn)
            {
                tableName = ECLStringConstants.i.PdRedefaultLifetimeDownturns_Table(this._eclType);
            }
            else if (_scenario == ECL_Scenario.Optimistic)
            {
                tableName = ECLStringConstants.i.PdRedefaultLifetimeOptimistics_Table(this._eclType);
            }

            var r = DataAccess.i.ExecuteBulkCopy(dt, tableName);

            return r > 0 ? "" : $"Could not Bulk Insert [{tableName}]";

        }

        public List<LifeTimeObject> ComputeRedefaultLifetimePd()
        {
            var redefaultLifetimePd = new List<LifeTimeObject>();

            var marginalPd = GetScenarioMarginalPd();
            var pdCali = new CalibrationInput_PD_CR_RD_Processor().GetPDRedefaultFactorCureRate(this._eclId, this._eclType);
            double readjustmentFactor = pdCali[0];

            //double test = GetMonthMarginalPdForPdGroup(marginalPd, "2", 10, readjustmentFactor);
            //double test2 = marginalPd.FirstOrDefault(row => row.PdGroup == "2" && row.Month == 10).Value;

            //double test3 = test2 * readjustmentFactor;
            //double test4 = test3 * test;

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
        
        protected List<PDI_Assumptions> GetPdInputAssumptions()
        {
            return new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_Assumptions();
        }
        protected List<LifeTimeObject> GetScenarioMarginalPd()
        {
            return _scenarioMarginalPd.ComputeMaginalPd();
        }
    }
}
