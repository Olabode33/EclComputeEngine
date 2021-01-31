using IFRS9_ECL.Core.Macro.Entities;
using IFRS9_ECL.Core.PDComputation;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core
{
    public class ProcessECL_PD
    {
        
        Guid _eclId;
        EclType _eclType;
        public ProcessECL_PD(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
        }
        public bool ProcessTask(List<Loanbook_Data> loanbooks)
        {

            try
            {

             

                // Compute Credit Index
                var crdIndx = new CreditIndex(this._eclId, this._eclType);
                crdIndx.Run();



                if (loanbooks.Count <= 1000) //1 !=1)//
                {
                    RunPDJob(loanbooks);
                }
                else
                {
                    //var checker = loanbooks.Count / 60;

                    var groupedLoanBook = new List<List<Loanbook_Data>>();
                    var threads = loanbooks.Count / 500;
                    threads = threads + 1;

                    for (int i = 0; i < threads; i++)
                    {
                        var sub_items = loanbooks.Skip(i * 500).Take(500).ToList();
                        if (sub_items.Count > 0)
                            groupedLoanBook.Add(sub_items);
                    }

                    var allAccountsGrouped = false;

                    try
                    {
                        while (!allAccountsGrouped)
                        {
                            allAccountsGrouped = true;
                            for (int i = 1; i < groupedLoanBook.Count; i++)
                            {
                                var lstfromPrev = groupedLoanBook[i - 1].LastOrDefault();
                                var fstfromCurr = groupedLoanBook[i].FirstOrDefault();
                                if (lstfromPrev.AccountNo == fstfromCurr.AccountNo)
                                {
                                    groupedLoanBook[i - 1].Add(fstfromCurr);
                                    groupedLoanBook[i].RemoveAt(0);
                                    allAccountsGrouped = false;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    var taskLst = new List<Task>();

                    //threads = 1;
                    for (int i = 0; i < threads; i++)
                    {
                        var sub_LoanBook = groupedLoanBook[i];//.Skip(i * 500).Take(500).ToList();

                        var task = Task.Run(() =>
                        {
                            RunPDJob(sub_LoanBook);
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
                }
                

                // Compute Scenario Life time Pd -- best
                var _slt_Pd_b = new ScenarioLifetimePd(ECL_Scenario.Best, this._eclId, this._eclType);
                _slt_Pd_b.Run();

                // Compute Scenario Redefault Lifetime Pds  -- best
                var sRedefault_lt_pd_b = new ScenarioRedefaultLifetimePds(Util.ECL_Scenario.Best, this._eclId, this._eclType);
                sRedefault_lt_pd_b.Run();

                // Compute Scenario Life time Pd -- Optimistic
                var _slt_Pd_o = new ScenarioLifetimePd(ECL_Scenario.Optimistic, this._eclId, this._eclType);
                _slt_Pd_o.Run();

                // Compute Scenario Redefault Lifetime Pds  -- Optimistic
                var sRedefault_lt_pd_o = new ScenarioRedefaultLifetimePds(Util.ECL_Scenario.Optimistic, this._eclId, this._eclType);
                sRedefault_lt_pd_o.Run();



                // Compute Scenario Life time Pd -- Downturn
                var slt_Pd_de = new ScenarioLifetimePd(ECL_Scenario.Downturn, this._eclId, this._eclType);
                slt_Pd_de.Run();

                // Compute Scenario Redefault Lifetime Pds  -- Downturn
                var sRedefault_lt_pd_de = new ScenarioRedefaultLifetimePds(Util.ECL_Scenario.Downturn, this._eclId, this._eclType);
                sRedefault_lt_pd_de.Run();


                return true;
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                return true;
            }
        }



        private void RunPDJob(List<Loanbook_Data> sub_LoanBook)
        {
            try
            {
                // Compute PD mapping
                var pDMapping = new PDMapping(this._eclId, this._eclType);
                pDMapping.Run(sub_LoanBook);
            }
            catch(Exception ex)
            {
                var cc = ex;
                Log4Net.Log.Error(ex);
            }
        }




        public List<PDI_Assumptions> Get_PDI_Assumptions()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_pdInputAssumptionsQuery(this._eclId, this._eclType));
            var data = new List<PDI_Assumptions>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PDI_Assumptions(), dr);
                itm.PdGroup=PDInputs.GetPDAssumptionEnum(int.Parse(dr["PdGroup"].ToString()));
                data.Add(itm);
            }
            return data;
        }

        public List<PDI_MacroEconomicProjections> Get_PDI_MacroEconomicProjections()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_MacroEconomicProjections);
            var data = new List<PDI_MacroEconomicProjections>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PDI_MacroEconomicProjections(), dr);
                data.Add(itm);
            }
            return data;
        }

        public List<PDI_MacroEconomics> Get_PDI_MacroEconomics()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_macroEconomicsQuery(this._eclId, this._eclType));
            var data = new List<PDI_MacroEconomics>();

            //Log4Net.Log.Info($"**************************");
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PDI_MacroEconomics(), dr);
                //Log4Net.Log.Info($"{itm.Date},{itm.MacroEconomicVariableId},{itm.BestEstimateMacroEconomicValue},{itm.OptimisticMacroEconomicValue},{itm.DownturnMacroEconomicValue}");
                data.Add(itm);
            }
            return data;
        }



        public List<PDI_HistoricIndex> Get_PDI_HistoricIndex()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_historicIndexQuery(this._eclId, this._eclType));
            var data = new List<PDI_HistoricIndex>();
            foreach (DataRow dr in dt.Rows)
            {
                var o = DataAccess.i.ParseDataToObject(new MacroResult_IndexData(), dr);
                var itm = new PDI_HistoricIndex();
                itm.Actual = o.Index;
                itm.Standardised = o.StandardIndex;
                itm.Date = GetPeriodDate(o.Period);
                data.Add(itm);
            }
            data = data.OrderBy(o => o.Date).ToList();
            return data;//.Take(32).ToList();
        }

        //public List<PDI_MacroEcoBest> Get_PDI_MacroEcoBest()
        //{
        //    return new List<PDI_MacroEcoBest>();
        //}

        //public List<PDI_MacroEcoDownturn> Get_PDI_MacroEcoDownturn()
        //{
        //    return new List<PDI_MacroEcoDownturn>();
        //}

        //public List<PDI_MacroEcoOptimisit> Get_PDI_MacroEcoOptimisit()
        //{
        //    return new List<PDI_MacroEcoOptimisit>();
        //}

        public List<PdInputAssumptionNonInternalModels> Get_PDI_NonInternalModelInputs(int MonthId=0)
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_nonInternalmodelInputQuery(this._eclId, this._eclType, MonthId));//(this._eclId)); ;
            var data = new List<PdInputAssumptionNonInternalModels>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PdInputAssumptionNonInternalModels(), dr);
                data.Add(itm);
            }
            return data;
        }

        public List<PDI_SnPCummlativeDefaultRate> Get_PDI_SnPCummlativeDefaultRate()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_snpCummulativeDefaultRateQuery(this._eclId, this._eclType));
            var data = new List<PDI_SnPCummlativeDefaultRate>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PDI_SnPCummlativeDefaultRate(), dr);
                data.Add(itm);
            }
            return data;
        }

        public List<PDI_StatisticalInputs> Get_PDI_StatisticalInputs()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_statisticalInputsQuery(this._eclId, this._eclType));
            var data = new List<PDI_StatisticalInputs>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PDI_StatisticalInputs(), dr);
                data.Add(itm);
            }
            return data;
        }

        public List<PDI_ETI_NPL> Get_PDI_ETI_NPL()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_etiNplQuery(this._eclId, this._eclType));
            var data = new List<PDI_ETI_NPL>();
            foreach (DataRow dr in dt.Rows)
            {
                var o = DataAccess.i.ParseDataToObject(new MacroResult_IndexData(), dr);
                var itm =new PDI_ETI_NPL();
                itm.Series = o.BfNpl;
                itm.Date = GetPeriodDate(o.Period);
                data.Add(itm);
            }
            return data;
        }

        private DateTime GetPeriodDate(string period)
        {
            var periodsl = period.Split(' ');
            var mnth = int.Parse(periodsl[0].Replace("Q", "")) * 3;
            var d = (mnth == 3 || mnth == 12) ? 31 : 30;
            return new DateTime(int.Parse(periodsl[1]),mnth,d);
        }

        //public List<WholesalePdLifetimeBests> Get_WholesalePdLifetimeBests()
        //{
        //    var dt = DataAccess.i.GetData(PD_Queries.Get_statisticalInputsQuery(this._eclId));
        //    var data = new List<WholesalePdLifetimeBests>();
        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        var itm = DataAccess.i.ParseDataToObject(new WholesalePdLifetimeBests(), dr);
        //        data.Add(itm);
        //    }
        //    return data;
        //}

        //public List<WholesalePdLifetimeDownturns> Get_WholesalePdLifetimeDownturns()
        //{
        //    return new List<WholesalePdLifetimeDownturns>();
        //}

        //public List<WholesalePdLifetimeOptimistics> Get_WholesalePdLifetimeOptimistics()
        //{
        //    return new List<WholesalePdLifetimeOptimistics>();
        //}

        //public List<WholesalePdMappings> Get_WholesalePdMappings()
        //{
        //    return new List<WholesalePdMappings>();
        //}

        //public List<WholesalePdRedefaultLifetimeBests> Get_WholesalePdRedefaultLifetimeBests()
        //{
        //    return new List<WholesalePdRedefaultLifetimeBests>();
        //}

        //public List<WholesalePdRedefaultLifetimeDownturns> Get_WholesalePdRedefaultLifetimeDownturns()
        //{
        //    return new List<WholesalePdRedefaultLifetimeDownturns>();
        //}

        //public List<WholesalePdRedefaultLifetimeOptimistics> WholesalePdRedefaultLifetimeOptimistics()
        //{
        //    return new List<WholesalePdRedefaultLifetimeOptimistics>();
        //}
    }
}
