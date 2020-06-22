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

        public ProcessECL_EAD(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
            
        }
        public bool ProcessTask(List<Loanbook_Data> loanbooks)
        {

            try
            {

                var threads = loanbooks.Count / 1000;
                threads = threads + 1;

                var taskLst = new List<Task>();

                for (int i = 0; i < threads; i++)
                {
                    var sub_LoanBook = loanbooks.Skip(i * 1000).Take(1000).ToList();

                    var task = Task.Run(() =>
                    {
                        RunEADJob(sub_LoanBook, this._eclId, this._eclType);
                    });

                    taskLst.Add(task);
                }
                Console.WriteLine($"Total Task : {taskLst.Count()}");

                //var completedTask = taskLst.Where(o => o.Status == TaskStatus.RanToCompletion).Count();
                //Console.WriteLine($"Task Completed: {completedTask}");

                while (taskLst.Count != tasks.Count)
                {
                    //Do Nothing
                }

                Console.WriteLine($"Completed all Tasks");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                
                return false;
            }
        }

        private bool RunEADJob(List<Loanbook_Data> _loanBookData, Guid eclId, EclType eclType)
        {
            var qry = "";
            Console.WriteLine("Completed pass raw data to object");

            var refined_lstRaw = new ECLTasks(eclId, this._eclType).GenerateContractIdandRefinedData(_loanBookData);

            Console.WriteLine("Completed GenerateContractIdandRefinedData");

            var lifeTimeEAD = new ECLTasks(eclId, this._eclType).GenerateLifeTimeEAD(refined_lstRaw);

            Console.WriteLine("Completed GenerateLifeTimeEAD");

            var lstContractIds = refined_lstRaw.Select(o => o.contract_no).Distinct().OrderBy(p => p).ToList();

            //EIR

            Task.Run(() => {
                DoEIRProjectionTask(lifeTimeEAD, lstContractIds, eclId);
            });
            // DoEIRProjectionTask(lifeTimeEAD, lstContractIds, masterGuid);

            //populate for CIR projections
            var cirProjections = new ECLTasks(this._eclId, this._eclType).EAD_CIRProjections(lifeTimeEAD, lstContractIds);
            Console.WriteLine("Completed EAD_CIRProjections");
            //insert into DB
            ExecuteNative.SaveCIRProjections(cirProjections, eclId, eclType);
            Console.WriteLine("Completed SaveCIRProjections");

            qry = Queries.PaymentSchedule(this._eclId, this._eclType);
            var _payment_schedule = DataAccess.i.GetData(qry);
            Console.WriteLine("Completed Getting Payment Schedule");

            var payment_schedule = new List<PaymentSchedule>();
            foreach (DataRow dr in _payment_schedule.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new TempPaymentSchedule(), dr);
                payment_schedule.Add(new PaymentSchedule { Amount = itm.Amount, Component = itm.Component, ContractRefNo = itm.ContractRefNo, StartDate = itm.StartDate, Frequency = itm.Frequency, NoOfSchedules = itm.NoOfSchedules });
            }

            Console.WriteLine("Completed Parsing Payment Schedule to object");

            var ps_contract_ref_nos = payment_schedule.Select(o => o.ContractRefNo).Distinct().OrderBy(o => o).ToList();
            var PaymentScheduleProjection = new ECLTasks(this._eclId, this._eclType).PaymentSchedule_Projection(payment_schedule, ps_contract_ref_nos);
            Console.WriteLine("Completed Parsing PaymentSchedule_Projection");

            //populate for LifeTime  projections
            var lifetimeProjections = new ECLTasks(this._eclId, this._eclType).EAD_LifeTimeProjections(refined_lstRaw, lifeTimeEAD, lstContractIds, cirProjections, PaymentScheduleProjection);
            Console.WriteLine("Completed EAD_LifeTimeProjections");

            ExecuteNative.SaveLifeTimeProjections(lifetimeProjections, eclId, _eclType);
            Console.WriteLine("All Jobs Completed");
            Console.ReadKey();

            tasks.Add(true);
            return true;
        }

        private void DoEIRProjectionTask(List<LifeTimeEADs> lifeTimeEAD, List<string> lstContractIds, Guid masterGuid)
        {

            //populate for EIR projections
            var eirProjections = new ECLTasks(this._eclId, this._eclType).EAD_EIRProjections(lifeTimeEAD, lstContractIds);
            Console.WriteLine("Completed EAD_EIRProjections");
            //insert into DB
            ExecuteNative.SaveEIRProjections(eirProjections, masterGuid, this._eclType);
            Console.WriteLine("Completed SaveEIRProjections");
        }

    }
}

