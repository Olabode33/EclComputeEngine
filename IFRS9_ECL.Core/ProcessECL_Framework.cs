using IFRS9_ECL.Core.FrameworkComputation;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models.Framework;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core
{
    public class ProcessECL_Framework
    {
        Guid _eclId;
        ECL_Scenario _Scenario;
        EclType _eclType;
        public ProcessECL_Framework(Guid eclId, ECL_Scenario scenario, EclType eclType)
        {
            this._eclId = eclId;
            this._Scenario = scenario;
            this._eclType = eclType;
        }


        public string ProcessTask()
        {
            var obj = new ScenarioEclWorkings(this._eclId, this._Scenario, this._eclType);
            var d=obj.ComputeFinalEcl();


            var c = new FinalEcl();

            Type myObjOriginalType = c.GetType();
            PropertyInfo[] myProps = myObjOriginalType.GetProperties();

            var dt = new DataTable();
            for (int i = 0; i < myProps.Length; i++)
            {
                dt.Columns.Add(myProps[i].Name, myProps[i].PropertyType);
            }
            dt.Columns.Add($"Scenario", typeof(int));
            dt.Columns.Add($"{this._eclType.ToString()}EclId", typeof(Guid));

            var _scenerio = 0;
            if(this._Scenario== ECL_Scenario.Best)
            {
                _scenerio = 1;
            }
            if (this._Scenario == ECL_Scenario.Optimistic)
            {
                _scenerio = 2;
            }
            if (this._Scenario == ECL_Scenario.Downturn)
            {
                _scenerio = 3;
            }

            foreach (var _d in d)
            {
                _d.Id = Guid.NewGuid();
                dt.Rows.Add(new object[]
                    {
                            _d.Id, _d.ContractId, _d.EclMonth, _d.MonthlyEclValue, _d.FinalEclValue, _d.Stage, _scenerio, this._eclId
                    });
            }
            var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.FrameworkResult(this._eclType));

            return r > 0 ? "" : $"Could not Bulk Insert [{ECLStringConstants.i.FrameworkResult(this._eclType)}]";

        }
    }
}
