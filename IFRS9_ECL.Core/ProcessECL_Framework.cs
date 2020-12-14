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
        protected SicrWorkings _sicrWorkings;
        
        public ProcessECL_Framework(Guid eclId, ECL_Scenario scenario, EclType eclType)
        {
            this._eclId = eclId;
            this._Scenario = scenario;
            this._eclType = eclType;
            _sicrWorkings = new SicrWorkings(eclId, this._eclType);

        }
        public ProcessECL_Framework(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
            _sicrWorkings = new SicrWorkings(eclId, this._eclType);

        }

        public string ProcessTask(List<Loanbook_Data> loanbook, List<LifetimeEad> lifetimeEad, List<LifetimeLgd> lifetimeLGD, List<IrFactor> cummulativeDiscountFactor, List<LifeTimeProjections> eadInput, List<StageClassification> stageClassifcation)
        {

            var lifetimePds = new ScenarioEclWorkings(this._eclId, this._Scenario, this._eclType).Get_LifetimePd_And_RedefaultLifetimePD_Result();

            //var stageClassifcation = stageClassifcation;// GetStageClassification(loanbook);

            if (1 != 1)// loanbook.Count <= 1000)
            {
                RunFrameWorkJob(lifetimeEad, lifetimeLGD, cummulativeDiscountFactor, eadInput, lifetimePds, stageClassifcation);
                return "";
            }
            //var checker = loanbook.Count / 60;

            var threads = loanbook.Count / 500;

            threads = threads + 1;

            var taskLst = new List<Task>(); 
            
            //threads = 1;
            for (int i = 0; i < threads; i++)
            {
                var sub_LoanBook = loanbook.Skip(i * 500).Take(500).ToList();
                var contractNo = sub_LoanBook.Select(o => o.ContractId).ToList();
                var sub_stageClassification = stageClassifcation.Where(o => contractNo.Contains(o.ContractId)).ToList();
                var sub_lifetimeEad = lifetimeEad.Where(o => contractNo.Contains(o.ContractId)).ToList();
                var sub_lifetimeLGD = lifetimeLGD.Where(o => contractNo.Contains(o.ContractId)).ToList();
                var sub_eadInput = eadInput.Where(o => contractNo.Contains(o.Contract_no)).ToList();
                var task = Task.Run(() =>
                {
                    RunFrameWorkJob(sub_lifetimeEad, sub_lifetimeLGD, cummulativeDiscountFactor, sub_eadInput, lifetimePds, sub_stageClassification);
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

            //Task t = Task.WhenAll(taskLst);

            //try
            //{
            //    t.Wait();
            //}
            //catch (Exception ex)
            //{
            //    Log4Net.Log.Error(ex);
            //}
            //Log4Net.Log.Info($"All Task status: {t.Status}");

            //if (t.Status == TaskStatus.RanToCompletion)
            //{
            //    Log4Net.Log.Info($"All Task ran to completion");
            //}
            //if (t.Status == TaskStatus.Faulted)
            //{
            //    Log4Net.Log.Info($"All Task ran to fault");
            //}

            return "";

        }

        public List<EclAssumptions> GetECLEADInputAssumptions()
        {
            var qry = Queries.eclEadInputAssumptions(this._eclId, this._eclType);
            var dt = DataAccess.i.GetData(qry);
            var eclAssumptions = new List<EclAssumptions>();

            foreach (DataRow dr in dt.Rows)
            {
                eclAssumptions.Add(DataAccess.i.ParseDataToObject(new EclAssumptions(), dr));
            }

            return eclAssumptions;
        }
        

        public string ProcessResultDetails(List<Loanbook_Data> loanbook)
        {
            var _eclEadInputAssumption=GetECLEADInputAssumptions();
            var CCF_OBE = 0.5;
            try { CCF_OBE = Convert.ToDouble(_eclEadInputAssumption.FirstOrDefault(o => o.Key == "ConversionFactorOBE").Value); } catch { }


            var qry = Queries.ClearFrameworkReportTable(this._eclId, this._eclType);

            DataAccess.i.ExecuteQuery(qry);

            // Gennerate Result Details
            var rd = new ReportComputation().GetResultDetail(this._eclType, this._eclId, loanbook, CCF_OBE);
           
            return "";
        }

        private void RunFrameWorkJob(List<LifetimeEad> lifetimeEad, List<LifetimeLgd> lifetimeLGD, List<IrFactor> cummulativeDiscountFactor, List<LifeTimeProjections> eadInput, List<LifeTimeObject> lifetimePds, List<StageClassification> stageClassification)
        {

            var obj = new ScenarioEclWorkings(this._eclId, this._Scenario, this._eclType);

            var d = obj.ComputeFinalEcl(lifetimeEad, lifetimeLGD, eadInput, cummulativeDiscountFactor, lifetimePds, stageClassification);

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
                _d.eCL_Scenario = _scenerio;
            }
            var qry=Queries.EclOverrideExist(this._eclId, this._eclType);
            var cnt = DataAccess.i.getCount(qry);
            if(cnt>0)
            {
                var r = Util.FileSystemStorage<FinalEcl>.WriteCsvData(this._eclId, ECLStringConstants.i.FrameworkResultOverride(this._eclType), d);
                //Save to Framwork Override table
                Log4Net.Log.Info($"Inserting into override table {d.Count}");
            }
            else
            {
                var r = Util.FileSystemStorage<FinalEcl>.WriteCsvData(this._eclId, ECLStringConstants.i.FrameworkResult(this._eclType), d);
                //save to Framework table
                Log4Net.Log.Info($"Inserting into Non override table {d.Count}");
            }
            

        }

        
    }
}
