using Excel.FinancialFunctions;
using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core
{
    public class ProcessECL_EAD
    {

        EclType _eclType;
        Guid _eclId;

        List<bool> tasks = new List<bool>();
        List<LifeTimeEADs> lifetimeEADs = new List<LifeTimeEADs>();
        List<Refined_Raw_Retail_Wholesale> refined_lstRaws = new List<Refined_Raw_Retail_Wholesale>();
        List<LifeTimeProjections> lifeTimeProjections = new List<LifeTimeProjections>();
        List<PaymentSchedule> paymentScheduleProjections = new List<PaymentSchedule>();
        DateTime reportingDate = new DateTime();
        public ProcessECL_EAD(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
            reportingDate = GetReportingDate(eclType, eclId);
        }

        private DateTime GetReportingDate(EclType _eclType, Guid eclId)
        {
            var ecls = Queries.EclsRegister(_eclType.ToString(), _eclId.ToString());
            var dtR = DataAccess.i.GetData(ecls);
            if (dtR.Rows.Count > 0)
            {
                var itm = DataAccess.i.ParseDataToObject(new EclRegister(), dtR.Rows[0]);
                return itm.ReportingDate;
            }
            return DateTime.Now;
        }

        public bool ProcessTask(List<Loanbook_Data> loanbooks)
        {
            try
            {
                var qry = Queries.PaymentSchedule(this._eclId, this._eclType);
                var _payment_schedule = DataAccess.i.GetData(qry);
                Log4Net.Log.Info("Completed Getting Payment Schedule");

                var payment_schedule = new List<PaymentSchedule>();
                foreach (DataRow dr in _payment_schedule.Rows)
                {
                    var itm = DataAccess.i.ParseDataToObject(new TempPaymentSchedule(), dr);
                    payment_schedule.Add(new PaymentSchedule { Amount = itm.Amount, Component = itm.Component, ContractRefNo = itm.ContractRefNo, StartDate = itm.StartDate, Frequency = itm.Frequency, NoOfSchedules = itm.NoOfSchedules });
                }
                //var ps_contract_nos = payment_schedule.Select(o => o.ContractRefNo).ToList();
                //var non_ps_lb = loanbooks.Where(o => !ps_contract_nos.Contains(o.ContractNo)).ToList();
                //var crt_non_ps_lb = non_ps_lb.Select(o=>o.ContractNo).Distinct().ToList();
                //foreach (var itm in crt_non_ps_lb)
                //{
                //   // payment_schedule.Add(new PaymentSchedule { ContractRefNo = itm, Component= "AMORTISE" });
                //}
                var tskStatusLst = new List<TaskStatus> { TaskStatus.RanToCompletion, TaskStatus.Faulted };

                if (1!=1)//loanbooks.Count<=1000)
                {
                    RunEADJob(loanbooks, this._eclId);
                }
                else
                {
                    //var checker = loanbooks.Count / 30;
                    var threads = loanbooks.Count / 500;
                    threads = threads + 1;

                    var taskLst = new List<Task>();

                    for (int i = 0; i < threads; i++)
                    {
                        var sub_LoanBook = loanbooks.Skip(i * 500).Take(500).ToList();

                        //var contractIds = sub_LoanBook.Select(o => o.ContractNo).ToList();
                        //var sub_payment_schedule = payment_schedule.Where(o => contractIds.Contains(o.ContractRefNo)).ToList();

                        var task = Task.Run(() =>
                        {
                            RunEADJob(sub_LoanBook, this._eclId);
                        });

                        taskLst.Add(task);
                    }
                    Log4Net.Log.Info($"Total Task : {taskLst.Count()}");

                    
                    while (0 < 1)
                    {
                        if (taskLst.All(o => tskStatusLst.Contains(o.Status)))
                        {
                            break;
                        }
                        //Do Nothing
                    }

                }


                //EIR

                Task.Run(() => {
                    DoEIRProjectionTask(lifetimeEADs, this._eclId);
                });
                // DoEIRProjectionTask(lifeTimeEAD, lstContractIds, masterGuid);

                //populate for CIR projections
                var cirProjections = new ECLTasks(this._eclId, this._eclType).EAD_CIRProjections(lifetimeEADs);
                Log4Net.Log.Info("Completed EAD_CIRProjections");
                //insert into DB
                ExecuteNative.SaveCIRProjections(cirProjections, this._eclId, this._eclType);
                Log4Net.Log.Info("Completed SaveCIRProjections");


                Log4Net.Log.Info("Completed Parsing Payment Schedule to object");



                if (1!=1)//payment_schedule.Count <= 1000)
                {
                    
                    var ps_contract_ref_nos = payment_schedule.Select(o => o.ContractRefNo).Distinct().OrderBy(o => o).ToList();
                    PaymentSchedule_Projection(payment_schedule, ps_contract_ref_nos, 1);
                }
                else
                {
                    //var checker = loanbooks.Count / 30;

                    var threads = loanbooks.Count / 500;
                    threads = threads + 1;

                    var taskLst = new List<Task>();

                    for (int i = 0; i < threads; i++)
                    {
                        var sub_payment_schedule = payment_schedule.Skip(i * 500).Take(500).ToList();

                        var task = Task.Run(() =>
                        {
                            var ps_contract_ref_nos = sub_payment_schedule.Select(o => o.ContractRefNo).Distinct().OrderBy(o => o).ToList();
                            PaymentSchedule_Projection(sub_payment_schedule, ps_contract_ref_nos, i);
                        });

                        taskLst.Add(task);
                    }
                    Log4Net.Log.Info($"Total Task : {taskLst.Count()}");


                    //var completedTask = taskLst.Where(o => o.Status == TaskStatus.RanToCompletion).Count();
                    //Log4Net.Log.Info($"Task Completed: {completedTask}");

                    //while (taskLst.Count != tasks.Count)
                    //while (!taskLst.Any(o => o.IsCompleted))

                    while (0 < 1)
                    {
                        if (taskLst.All(o => tskStatusLst.Contains(o.Status)))
                        {
                            break;
                        }
                        //Do Nothing
                    }
                }
                Log4Net.Log.Info("Completed Parsing PaymentSchedule_Projection");


                var ccfData = new CalibrationInput_EAD_CCF_Summary_Processor().GetCCFData(this._eclId, this._eclType);

                ////populate for LifeTime  projections
                //var lifetimeProjections_ = new ECLTasks(this._eclId, this._eclType).EAD_LifeTimeProjections(refined_lstRaws, lifetimeEADs, cirProjections, PaymentScheduleProjection, ccfData);
                //Log4Net.Log.Info("Completed EAD_LifeTimeProjections");

                //ExecuteNative.SaveLifeTimeProjections(lifetimeProjections_, this._eclId, _eclType);
                //Log4Net.Log.Info("All Jobs Completed");

                //refined_lstRaws = refined_lstRaws.Where(o => o.contract_no == "001SFLN172790002").ToList();

                if (loanbooks.Count <= 1000) //1 != 1) //
                {

                    //var _lifetimeProjections = 
                        new ECLTasks(this._eclId, this._eclType).EAD_LifeTimeProjections(refined_lstRaws, lifetimeEADs, cirProjections, paymentScheduleProjections, ccfData);
                    //lifeTimeProjections.AddRange(_lifetimeProjections);
                }
                else
                {
                    //var checker = loanbooks.Count / 60;

                    var threads = loanbooks.Count / 500;
                    threads = threads + 1;

                    var taskLst = new List<Task>();

                    for (int i = 0; i < threads; i++)
                    {
                        var sub_refined_lstRaws = refined_lstRaws.Skip(i * 500).Take(500).ToList();

                        var contractnos = sub_refined_lstRaws.Select(o => o.contract_no).ToList();
                        var sub_lifetimeEADs = lifetimeEADs.Where(o => contractnos.Contains(o.contract_no)).ToList();
                        var sub_PaymentScheduleProjection = paymentScheduleProjections.Where(o => contractnos.Contains(o.ContractId)).ToList();
                        var task = Task.Run(() =>
                        {

                        //populate for LifeTime  projections
                        //var _lifetimeProjections = 
                            new ECLTasks(this._eclId, this._eclType).EAD_LifeTimeProjections(sub_refined_lstRaws, sub_lifetimeEADs, cirProjections, sub_PaymentScheduleProjection, ccfData);

                           // lifeTimeProjections.AddRange(_lifetimeProjections);
                        ////Console.ReadKey();
                        ///
                    });

                        taskLst.Add(task);
                    }
                    Log4Net.Log.Info($"Total Task : {taskLst.Count()}");

                    //var completedTask = taskLst.Where(o => o.Status == TaskStatus.RanToCompletion).Count();
                    //Log4Net.Log.Info($"Task Completed: {completedTask}");

                    //while (taskLst.Count != tasks.Count)
                    //while (!taskLst.Any(o => o.IsCompleted))

                    while (0 < 1)
                    {
                        if (taskLst.All(o => tskStatusLst.Contains(o.Status)))
                        {
                            break;
                        }
                        //Do Nothing
                    }
                   
                }

                Log4Net.Log.Info("Completed EAD_LifeTimeProjections");

                return true;
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex.ToString());
               // //Console.ReadKey();
                return false;
            }
        }

        private void RunEADJob(List<Loanbook_Data> _loanBookData, Guid eclId)
        {
            Log4Net.Log.Info("Completed pass raw data to object");

            var refined_lstRaw = new ECLTasks(eclId, this._eclType).GenerateContractIdandRefinedData(_loanBookData);

            Log4Net.Log.Info("Completed GenerateContractIdandRefinedData");

            var lifeTimeEAD = new ECLTasks(eclId, this._eclType).GenerateLifeTimeEAD(refined_lstRaw);

            refined_lstRaws.AddRange(refined_lstRaw);
            lifetimeEADs.AddRange(lifeTimeEAD);
            
        }

        private void DoEIRProjectionTask(List<LifeTimeEADs> lifeTimeEAD, Guid masterGuid)
        {

            //populate for EIR projections
            var eirProjections = new ECLTasks(this._eclId, this._eclType).EAD_EIRProjections(lifeTimeEAD);
            Log4Net.Log.Info("Completed EAD_EIRProjections");
            //insert into DB
            ExecuteNative.SaveEIRProjections(eirProjections, masterGuid, this._eclType);
            Log4Net.Log.Info("Completed SaveEIRProjections");
        }

        public void PaymentSchedule_Projection(List<PaymentSchedule> ps, List<string> ps_contract_ref_no, int counter)
        {
            var _ps = new List<PaymentSchedule>();

            int wholeIndex = 0;
            foreach(var item in ps)
            { 

                bool start_month_adjustment = false;
                int frequency_factor;
                int no_schedules;
                double amount;
                DateTime start_date;
                double start_month = 0;
                double start_schedule;
                int monthIndex = 1;

                //Determine frequency factor
                //foreach (var item in contractblock)
                //{
                if (string.IsNullOrEmpty(item.Frequency))
                {
                    item.Frequency = "M";
                }
                string frequency = item.Frequency.Trim();
                if (ECLScheduleConstants.Bullet == frequency)
                {
                    frequency_factor = 0;
                }
                else if (ECLScheduleConstants.Monthly == frequency)
                {
                    frequency_factor = ECLScheduleConstants.Monthly_number;
                }
                else if (ECLScheduleConstants.Quarterly == frequency)
                {
                    frequency_factor = ECLScheduleConstants.Quarterly_number;
                }
                else if (ECLScheduleConstants.Yearly == frequency)
                {
                    frequency_factor = ECLScheduleConstants.Yearly_number;
                }
                else if (ECLScheduleConstants.HalfYear == frequency)
                {
                    frequency_factor = ECLScheduleConstants.HalfYear_number;
                }
                else
                {
                    frequency_factor = 0;
                }

                //Run through each schedule
                no_schedules = item.NoOfSchedules;

                //set amount
                amount = item.Amount;

                //Determine the rounded months from the report date at which the entry starts.
                //Allowed for this to be negative. This will be used later.
                start_date = item.StartDate;

                if (start_date > reportingDate)
                {
                    if (!start_month_adjustment)
                    {
                        start_month = Math.Round(Financial.YearFrac(reportingDate, start_date, DayCountBasis.ActualActual) * 12, 0);
                        if (start_month == 0)
                        {
                            start_month_adjustment = true;
                        }
                    }
                    if (start_month_adjustment)
                    {
                        var start_date_ = EndOfMonth(start_date, 0);
                        if (reportingDate < start_date_)
                        {
                            start_month = Math.Round(Financial.YearFrac(reportingDate, start_date_, DayCountBasis.ActualActual) * 12, 0);
                        }
                        else
                        {
                            start_month = 0;
                        }

                    }
                    start_schedule = 0;
                }
                else
                {
                    //'Set negative number of months if the payment entry started in the past. If it is a bullet payment entry it should not pull through.
                    if (start_date < reportingDate)
                    {
                        start_month = -1 * Math.Round(Financial.YearFrac(start_date, reportingDate, DayCountBasis.ActualActual) * 12, 0);
                    }
                    else
                    {
                        start_month = 0;
                    }

                    var projectionMonth = item.NoOfSchedules + start_month;

                    if (frequency_factor != 0)
                    {
                        var w = (-start_month + 1) / frequency_factor;
                        start_schedule = Math.Ceiling(w);
                    }
                    else
                    {
                        start_schedule = no_schedules;
                        //This way if the schedule entry is a bullet payment before the reporting date the function will not step into the loop.
                        //The +1 is to allow for the current months payment.
                    }
                }


                var hasItm = 0;
                //'Check whether the last schedule in this entry is more months from the reporting date than the max_ttm derived from the loan book snapshot.
                var contact_ps = _ps.Where(o => o.ContractId == item.ContractRefNo).ToList();
                for (double schedule = start_schedule; schedule < no_schedules; schedule++)
                {
                    hasItm = hasItm + 1;
                    var __Item = contact_ps.FirstOrDefault(o => o.Months == monthIndex.ToString());
                    if (__Item != null)
                    {
                        _ps.Remove(__Item);
                        __Item.Amount = __Item.Value + amount;
                        _ps.Add(__Item);
                    }
                    else
                    {
                        _ps.Add(new PaymentSchedule { ContractId = item.ContractRefNo, PaymentType = item.Component, Months = monthIndex.ToString(), Value = amount });
                    }

                    wholeIndex++;
                    monthIndex++;
                }

                if (hasItm == 0)
                {
                    _ps.Add(new PaymentSchedule { ContractId = item.ContractRefNo, PaymentType = item.Component, Months = item.Months, StartDate=item.StartDate, Value = 0 });
                }
            }
            

            //_ps.GroupBy(t => new { t.ContractRefNo, t.Months }).Select(group => new { Months=group. });
            paymentScheduleProjections.AddRange(_ps);
            Log4Net.Log.Info($"PS Count - {counter}");
        }


        private DateTime EndOfMonth(DateTime myDate, int numberOfMonths)
        {
            //Update Value ************************************************
            //Update Value ************************************************
            try
            {
                DateTime startOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(numberOfMonths).AddMonths(1).AddDays(-1);
                return endOfMonth;
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                myDate = DateTime.Today;
                DateTime startOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(numberOfMonths).AddMonths(1).AddDays(-1);
                return endOfMonth;
            }
        }

    }
}

