using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Framework;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.FrameworkComputation
{
    public class ScenarioEclWorkings
    {
        //protected ScenarioLifetimeLGD _lifetimeLgd;
        protected LifetimeEadWorkings _lifetimeEad;
        protected IrFactorWorkings _irFactorWorkings;
        protected SicrWorkings _sicrWorkings;
        protected LifetimeEadWorkings _lifetimeEadWorkings;

        private ECL_Scenario _scenario;
        Guid _eclId;
        EclType _eclType;
        public ScenarioEclWorkings(Guid eclId, ECL_Scenario scenario, EclType eclType)
        {
            this._eclId = eclId;
            _scenario = scenario;
            this._eclType = eclType;

            _lifetimeEad = new LifetimeEadWorkings(eclId, this._eclType);
            //_lifetimeLgd = new ScenarioLifetimeLGD(eclId,this._eclType);
            _irFactorWorkings = new IrFactorWorkings(eclId, this._eclType);
            _sicrWorkings = new SicrWorkings(eclId, this._eclType);

            _lifetimeEadWorkings = new LifetimeEadWorkings(eclId, this._eclType);
        }

        List<FinalEcl> finalEcl;
        List<FinalEcl> monthlyEcl;
        List<IrFactor> cummulativeDiscountFactor;
        List<LifeTimeProjections> eadInput;
        List<LifetimeEad> lifetimeEad;
        List<LifetimeLgd> lifetimeLGD;

        public List<FinalEcl> ComputeFinalEcl(List<LifetimeEad> _lifetimeEad, List<LifetimeLgd> _lifetimeLGD, List<LifeTimeProjections> _eadInput, List<IrFactor> _cummulativeDiscountFactor, List<LifeTimeObject> lifetimePds, List<StageClassification> stageClassification)
        {
            finalEcl = new List<FinalEcl>();
            eadInput = new List<LifeTimeProjections>();
            
            eadInput = _eadInput;// = GetTempEadInputData(loanbook);
            cummulativeDiscountFactor = new List<IrFactor>();
            cummulativeDiscountFactor = _cummulativeDiscountFactor;

            lifetimeEad = new List<LifetimeEad>();
            lifetimeLGD = new List<LifetimeLgd>();
            
            lifetimeEad = _lifetimeEad;
            lifetimeLGD = _lifetimeLGD;

            //lifetimeEad = GetLifetimeEadResult(loanbook);
            //lifetimeLGD = GetLifetimeLgdResult(loanbook);

            

            monthlyEcl = ComputeMonthlyEcl(lifetimeLGD, lifetimeEad, lifetimePds);

            var stageClassifcation = stageClassification;// GetStageClassification(loanbook);

            var threads = stageClassifcation.Count / 500;
            threads = threads + 1;

            var taskLst = new List<Task>();

            //threads = 1;
            for (int i = 0; i < threads; i++)
            {
                var sub_stageClassification = stageClassifcation.Skip(i * 500).Take(500).ToList();

                var task = Task.Run(() =>
                {
                    RunFinalJob(sub_stageClassification);
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


            return finalEcl;
        }

        private void RunFinalJob(List<StageClassification>  stageClassifcation)
        {
            var _finalEcl = new List<FinalEcl>();
            foreach (var rowStage in stageClassifcation)
            {
                try
                {
                    string contractId = rowStage.ContractId;
                    int stage = rowStage.Stage;
                    //xxxxxxxxxxxxxxxx
                    string eirGroup = "";
                    try
                    {
                        eirGroup = eadInput.FirstOrDefault(x => x.Contract_no == contractId).Eir_Group;
                    }
                    catch { try { eirGroup = eadInput.FirstOrDefault().Eir_Group; } catch { } }
                    double finalEclValue = ComputeFinalEclValue(monthlyEcl, cummulativeDiscountFactor, lifetimeEad, lifetimeLGD, contractId, rowStage, eirGroup);

                    var newRow = new FinalEcl();
                    newRow.ContractId = contractId;
                    newRow.Stage = stage;
                    newRow.FinalEclValue = finalEclValue;
                    var mntECL = monthlyEcl.Where(o => o.ContractId == rowStage.ContractId).ToList();

                    foreach (var itm in mntECL)
                    {
                        itm.FinalEclValue = finalEclValue;
                        itm.Stage = stage;
                        _finalEcl.Add(itm);
                    }
                }
                catch (Exception ex)
                {
                    Log4Net.Log.Error(ex);
                    var cc = ex;
                }
            }
            finalEcl.AddRange(_finalEcl);
        }

        private double ComputeFinalEclValue(List<FinalEcl> monthlyEcl, List<IrFactor> cummulativeDiscountFactor, List<LifetimeEad> lifetimeEad, List<LifetimeLgd> lifetimeLGD, string contractId, StageClassification stage, string eirGroup)
        {
            //xxxxxxxxxxxxxxxxxxx
            double lifetimeLgdMonth0Value = 0;
            try { lifetimeLgdMonth0Value = lifetimeLGD.FirstOrDefault(o => o.ContractId == contractId && o.Month == 0).Value; } catch { }
            double lifetimeEadMonth0Value = 0;
            try
            {
                lifetimeEadMonth0Value = lifetimeEad.FirstOrDefault(o => o.ContractId == contractId && o.ProjectionMonth == 0).ProjectionValue;
            }
            catch { }

            double finalEclValue = 0;

            switch (stage.Stage)
            {
                case 1:
                    double[] monthEclArray = monthlyEcl.Where(o => o.ContractId == contractId && o.EclMonth >= 1 && o.EclMonth <= FrameworkConstants.ScenerioWorkingMaxMonth).Select(n => n.MonthlyEclValue).ToArray();
                    double[] monthCdfArray = cummulativeDiscountFactor.Where(o => o.EirGroup == eirGroup && o.ProjectionMonth >= 1 && o.ProjectionMonth <= FrameworkConstants.ScenerioWorkingMaxMonth).Select(n => n.ProjectionValue).ToArray();

                    finalEclValue = ExcelFormulaUtil.SumProduct(monthEclArray, monthCdfArray);
                    break;
                case 2:
                    double[] monthEclArray2 = monthlyEcl.Where(o => o.ContractId == contractId && o.EclMonth >= 1).Select(n => n.MonthlyEclValue).ToArray();// && o.EclMonth <= FrameworkConstants.ProjectionMonth).Select(n => n.MonthlyEclValue).ToArray();
                    double[] monthCdfArray2 = cummulativeDiscountFactor.Where(o => o.EirGroup == eirGroup && o.ProjectionMonth >= 1).Select(n => n.ProjectionValue).ToArray();// && o.ProjectionMonth < FrameworkConstants.ProjectionMonth).Select(n => n.ProjectionValue).ToArray(); //FrameworkConstants.ProjectionMonth
                    finalEclValue = ExcelFormulaUtil.SumProduct(monthEclArray2, monthCdfArray2);
                    break;
                default:
                    finalEclValue = lifetimeEadMonth0Value * lifetimeLgdMonth0Value;
                    break;

            }

            return finalEclValue;
        }

   
        public List<FinalEcl> ComputeMonthlyEcl(List<LifetimeLgd> lifetimeLgds, List<LifetimeEad> lifetimeEads, List<LifeTimeObject> lifetimePds)
        {
            var monthlyEcl = new List<FinalEcl>();


            // lifetimeLgds = lifetimeLgds.Where(x => x.Month != 0).ToList();

            // var allContracts = lifetimeLgds.Select(o => o.ContractId).ToList();

            //var allContracts = lifetimeLgds.Select(o => o.ContractId).ToList();

            //foreach (var _contractId in allContracts)
            //{
            //    var contract_lifetimeLgds = lifetimeLgds.Where(o => o.ContractId == _contractId).ToList();
            //    var contract_lifetimeEads = lifetimeEads.Where(o => o.ContractId == _contractId).ToList();
            //    foreach (var row in contract_lifetimeLgds)
            //    {
            //        string contractId = row.ContractId;
            //        string pdGroup = row.PdIndex;
            //        int month = row.Month;

            //        double monthlyEclValue = ComputeMonthlyEclValue(lifetimePds, contract_lifetimeEads, row, contractId, pdGroup, month);

            //        var newRow = new FinalEcl();
            //        newRow.ContractId = contractId;
            //        newRow.EclMonth = month;
            //        newRow.MonthlyEclValue = monthlyEclValue;

            //        monthlyEcl.Add(newRow);
            //    }
            //}
            foreach (var row in lifetimeLgds)
            {
                string contractId = row.ContractId;
                string pdGroup = row.PdIndex;
                int month = row.Month;
                
                double monthlyEclValue = ComputeMonthlyEclValue(lifetimePds, lifetimeEads, row, contractId, pdGroup, month);

                var newRow = new FinalEcl();
                newRow.ContractId = contractId;
                newRow.EclMonth = month;
                newRow.MonthlyEclValue = monthlyEclValue;

                monthlyEcl.Add(newRow);
            }

            return monthlyEcl;
        }

        private double ComputeMonthlyEclValue(List<LifeTimeObject> lifetimePds, List<LifetimeEad> lifetimeEads, LifetimeLgd row, string contractId, string pdGroup, int month)
        {
            double lgdValue = row.Value;
            double pdValue = GetLifetimePdValueFromTable(lifetimePds, pdGroup, month);
            double eadValue = GetLifetimeEadValueFromTable(lifetimeEads, contractId, month);

            //if(contractId.Contains("10123603239201"))
            //{
            //    var cc = 0;
            //}

            if(pdGroup==ECLStringConstants.i.ExpiredContractsPrefix)
            {
                pdValue = 1;
            }
            double monthlyEclValue = pdValue * lgdValue * eadValue;
            return monthlyEclValue;
        }

        private double GetLifetimeEadValueFromTable(List<LifetimeEad> lifetimeEads, string contractId, int month)
        {
            //xxxxxxxxxxxxxxxxxxxxx
            try { return lifetimeEads.FirstOrDefault(x => x.ContractId == contractId && x.ProjectionMonth == month).ProjectionValue; }
            catch { return 0; }
        }

        private double GetLifetimePdValueFromTable(List<LifeTimeObject> lifetimePds, string pdGroup, int month)
        {
            lifetimePds = lifetimePds.OrderBy(o => o.Month).ToList();
            try { return lifetimePds.FirstOrDefault(x => x.PdGroup == pdGroup && x.Month == (month > 120 ? 120 : month)).Value; } catch { return 0; }
        }

        public double[] ComputeMonthArray(DataTable dataTable, string contractColumnName, string monthColumnName, string valueColumnName, string contractId, int maxMonth)
        {
            double[] monthlyArray = dataTable.AsEnumerable()
                                                     .Where(x => x.Field<string>(contractColumnName) == contractId
                                                              && x.Field<int>(monthColumnName) >= 1
                                                              && x.Field<int>(monthColumnName) <= maxMonth)
                                                     .Select(x =>
                                                     {
                                                         return x.Field<double>(valueColumnName);
                                                     }).ToArray();

            return monthlyArray;
        }


        public List<LifeTimeObject> Get_LifetimePd_And_RedefaultLifetimePD_Result()
        {

            var qry = "";
            switch (_scenario)
            {
                case ECL_Scenario.Best:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.PdLifetimeBests_Table(this._eclType), this._eclId, this._eclType);
                    break;
                case ECL_Scenario.Optimistic:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.PdLifetimeOptimistics_Table(this._eclType), this._eclId, this._eclType);
                    break;
                case ECL_Scenario.Downturn:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.PdLifetimeDownturns_Table(this._eclType), this._eclId, this._eclType);
                    break;
                default:
                    return null;
            }

            var _lstRaw = DataAccess.i.GetData(qry);

            var lifetimePd = new List<LifeTimeObject>();
            foreach (DataRow dr in _lstRaw.Rows)
            {
                lifetimePd.Add(DataAccess.i.ParseDataToObject(new LifeTimeObject(), dr));
            }
            Log4Net.Log.Info("Completed pass data to object");

            return lifetimePd;
        }
       


    }
}
