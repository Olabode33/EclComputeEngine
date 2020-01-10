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
    public class ProcessECL_Wholesale_EAD
    {
        public static readonly ProcessECL_Wholesale_EAD i = new ProcessECL_Wholesale_EAD();

        public bool ProcessTask(Guid masterGuid)
        {
            try
            {
                //Get Data Excel/Database
                var qry = Queries.Raw_Data;
                Console.WriteLine("Started");
                var _lstRaw = DataAccess.i.GetData(qry);
                Console.WriteLine("Selected Raw Data from table");
                var lstRaw = new List<Loanbook_Data>();
                foreach (DataRow dr in _lstRaw.Rows)
                {
                    lstRaw.Add(DataAccess.i.ParseDataToObject(new Loanbook_Data(), dr));
                }
                Console.WriteLine("Completed pass raw data to object");

                var refined_lstRaw = ECLTasks.i.GenerateContractIdandRefinedData(lstRaw);

                Console.WriteLine("Completed GenerateContractIdandRefinedData");

                var lifeTimeEAD = ECLTasks.i.GenerateLifeTimeEAD(refined_lstRaw);

                Console.WriteLine("Completed GenerateLifeTimeEAD");

                var lstContractIds = refined_lstRaw.Select(o => o.contract_no).Distinct().OrderBy(p => p).ToList();

                //EIR

                Task.Run(() => {
                    DoEIRProjectionTask(lifeTimeEAD, lstContractIds, masterGuid);
                });
               // DoEIRProjectionTask(lifeTimeEAD, lstContractIds, masterGuid);

                //populate for CIR projections
                var cirProjections = ECLTasks.i.EAD_CIRProjections(lifeTimeEAD, lstContractIds);
                Console.WriteLine("Completed EAD_CIRProjections");
                //insert into DB
                ExecuteNative.SaveCIRProjections(cirProjections, masterGuid);
                Console.WriteLine("Completed SaveCIRProjections");

                qry = Queries.PaymentSchedule;
                var _payment_schedule = DataAccess.i.GetData(qry);
                Console.WriteLine("Completed Getting Payment Schedule");

                var payment_schedule = new List<PaymentSchedule>();
                foreach (DataRow dr in _payment_schedule.Rows)
                {
                    var itm = DataAccess.i.ParseDataToObject(new TempPaymentSchedule(), dr);
                    payment_schedule.Add(new PaymentSchedule { AMOUNT = itm.AMOUNT, COMPONENT = itm.COMPONENT, CONTRACT_REF_NO = itm.CONTRACT_REF_NO, START_DATE = itm.START_DATE, FREQUENCY = itm.FREQUENCY, NO_OF_SCHEDULES = itm.NO_OF_SCHEDULES });
                }

                Console.WriteLine("Completed Parsing Payment Schedule to object");

                var ps_contract_ref_nos = payment_schedule.Select(o => o.CONTRACT_REF_NO).Distinct().OrderBy(o => o).ToList();
                var PaymentScheduleProjection = ECLTasks.i.PaymentSchedule_Projection(payment_schedule, ps_contract_ref_nos);
                Console.WriteLine("Completed Parsing PaymentSchedule_Projection");

                //populate for LifeTime  projections
                var lifetimeProjections = ECLTasks.i.EAD_LifeTimeProjections(refined_lstRaw, lifeTimeEAD, lstContractIds, cirProjections, PaymentScheduleProjection);
                Console.WriteLine("Completed EAD_LifeTimeProjections");

                ExecuteNative.SaveLifeTimeProjections(lifetimeProjections, masterGuid);
                Console.WriteLine("All Jobs Completed");
                Console.ReadKey();
                return true;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
                return false;
            }
        }

        private void DoEIRProjectionTask(List<LifeTimeEADs> lifeTimeEAD, List<string> lstContractIds, Guid masterGuid)
        {

            //populate for EIR projections
            var eirProjections = ECLTasks.i.EAD_EIRProjections(lifeTimeEAD, lstContractIds);
            Console.WriteLine("Completed EAD_EIRProjections");
            //insert into DB
            ExecuteNative.SaveEIRProjections(eirProjections, masterGuid);
            Console.WriteLine("Completed SaveEIRProjections");
        }

    }
}

