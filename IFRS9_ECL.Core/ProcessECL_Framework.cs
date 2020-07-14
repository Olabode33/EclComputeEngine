using IFRS9_ECL.Core.FrameworkComputation;
using IFRS9_ECL.Core.Report;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.ECL_Result;
using IFRS9_ECL.Models.Framework;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Models.Raw;
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
        public ProcessECL_Framework(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;

        }

        public string ProcessTask(List<Loanbook_Data> loanbook, List<LifetimeEad> lifetimeEad, List<LifetimeLgd> lifetimeLGD, List<IrFactor> cummulativeDiscountFactor, List<LifeTimeProjections> eadInput)
        {
            var threads = loanbook.Count / 500;
            threads = threads + 1;

            var taskLst = new List<Task>(); 
            var lifetimePds = new ScenarioEclWorkings(this._eclId, this._Scenario, this._eclType).Get_LifetimePd_And_RedefaultLifetimePD_Result();
            //threads = 1;
            for (int i = 0; i < threads; i++)
            {
                var sub_LoanBook = loanbook.Skip(i * 500).Take(500).ToList();

                var task = Task.Run(() =>
                {
                    RunFrameWorkJob(sub_LoanBook, lifetimeEad, lifetimeLGD, cummulativeDiscountFactor, eadInput, lifetimePds);
                });
                taskLst.Add(task);
            }
            Log4Net.Log.Info($"Total Task : {taskLst.Count()}");

            var completedTask = taskLst.Where(o => o.IsCompleted).Count();
            Log4Net.Log.Info($"Task Completed: {completedTask}");

            //while (!taskLst.Any(o => o.IsCompleted))
            var tskStatusLst = new List<TaskStatus> { TaskStatus.RanToCompletion, TaskStatus.Faulted };
            while (0 < 1)
            {
                if (taskLst.All(o => tskStatusLst.Contains(o.Status)))
                {
                    break;
                }
                //Do Nothing
            }

            return "";

        }


        public string ProcessResultDetails(List<Loanbook_Data> loanbook)
        {


            // Gennerate Result Details
            var rd = new ReportComputation().GetResultDetail(this._eclType, this._eclId, loanbook);

            var c = new ResultDetailDataMore();

            Type myObjOriginalType = c.GetType();
            PropertyInfo[] myProps = myObjOriginalType.GetProperties();

            var dt = new DataTable();
            for (int i = 0; i < myProps.Length; i++)
            {
                dt.Columns.Add(myProps[i].Name, myProps[i].PropertyType);
            }

            dt.Columns.Add($"{this._eclType.ToString()}EclId", typeof(Guid));



            foreach (var _d in rd.ResultDetailDataMore)
            {
                var Id = Guid.NewGuid();
                dt.Rows.Add(new object[]
                    {
                            Id, _d.Stage, _d.Outstanding_Balance, _d.ECL_Best_Estimate, _d.ECL_Optimistic, _d.ECL_Downturn, _d.Impairment_ModelOutput,
                            _d.Overrides_Stage, _d.Overrides_TTR_Years, _d.Overrides_FSV, _d.Overrides_Overlay, _d.Overrides_ECL_Best_Estimate, _d.Overrides_ECL_Optimistic, _d.Overrides_ECL_Downturn, _d.Overrides_Impairment_Manual, _d.ContractNo, _d.AccountNo,
                            _d.CustomerNo, _d.Segment, _d.ProductType, _d.Sector, this._eclId
                    });
            }

            var qry = Queries.ClearFrameworkReportTable(this._eclId, this._eclType);
            DataAccess.i.ExecuteQuery(qry);

            //Save to Report Detail
            var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.EclFramworkReportDetail(this._eclType));

            return "";
        }

        private void RunFrameWorkJob(List<Loanbook_Data> loanBook, List<LifetimeEad> lifetimeEad, List<LifetimeLgd> lifetimeLGD, List<IrFactor> cummulativeDiscountFactor, List<LifeTimeProjections> eadInput, List<LifeTimeObject> lifetimePds)
        {

            var obj = new ScenarioEclWorkings(this._eclId, this._Scenario, this._eclType);

            var d = obj.ComputeFinalEcl(loanBook, lifetimeEad, lifetimeLGD, eadInput, cummulativeDiscountFactor, lifetimePds);

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
            if (this._Scenario == ECL_Scenario.Best)
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
            var qry=Queries.EclOverrideExist(this._eclId, this._eclType);
            var cnt = DataAccess.i.getCount(qry);
            if(cnt>0)
            {
                //Save to Framwork Override table
                Log4Net.Log.Info($"Inserting into override table {dt.Rows.Count}");
                var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.FrameworkResultOverride(this._eclType));
            }
            else
            {
                //save to Framework table
                Log4Net.Log.Info($"Inserting into Non override table {dt.Rows.Count}");
                var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.FrameworkResult(this._eclType));
            }
            

        }
    }
}
