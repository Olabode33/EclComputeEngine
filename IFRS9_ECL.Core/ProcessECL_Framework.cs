using IFRS9_ECL.Core.FrameworkComputation;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core
{
    public class ProcessECL_Framework
    {
        Guid _eclId;
        ECL_Scenario _scenario;
        EclType _eclType;
        public ProcessECL_Framework(Guid eclId, ECL_Scenario scenario, EclType eclType)
        {
            this._eclId = eclId;
            this._scenario = scenario;
            this._eclType = eclType;
        }


        public void ProcessTask()
        {
            var obj = new ScenarioEclWorkings(this._eclId, this._scenario, this._eclType);
            obj.ComputeFinalEcl();
        }
    }
}
