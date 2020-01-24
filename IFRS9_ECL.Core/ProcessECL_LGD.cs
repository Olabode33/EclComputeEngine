using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
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
    public class ProcessECL_LGD
    {
        Guid _eclId;
        EclType _eclType;

        ECLTasks _eclTask;
        public ProcessECL_LGD(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
            _eclTask = new ECLTasks(eclId, eclType);
        }
        public bool ProcessTask()
        {
            //try
            //{
            //Get Data Excel/Database
            var qry = Queries.Raw_Data(_eclId, _eclType);
            var _lstRaw = DataAccess.i.GetData(qry);

            var lstRaw = new List<Loanbook_Data>();
            foreach (DataRow dr in _lstRaw.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new Loanbook_Data(), dr);
                itm.ContractId = _eclTask.GenerateContractId(itm);
                lstRaw.Add(itm);
            }
            Console.WriteLine($"Finished Extracting Raw Data - {DateTime.Now}");



            var threads = lstRaw.Count / 1000;
            threads = threads + 1;

            var taskLst = new List<Task>();

            for (int i = 0; i < threads; i++)
            {
                var sub_LoanBook = lstRaw.Skip(i * 1000).Take(1000).ToList();

                var task = Task.Run(() => {
                    RunLGDJob(sub_LoanBook, _eclId, _eclType);
                });
                taskLst.Add(task);
            }
            Console.WriteLine($"Total Task : {taskLst.Count()}");

            var completedTask = taskLst.Where(o => o.Status == TaskStatus.RanToCompletion).Count();
            Console.WriteLine($"Task Completed: {completedTask}");

            while (!taskLst.Any(o => o.Status == TaskStatus.RanToCompletion))
            {
                var newCount = taskLst.Where(o => o.Status == TaskStatus.RanToCompletion).Count();
                if (completedTask != newCount)
                {
                    Console.WriteLine($"Task Completed: {completedTask}");
                }
                //Do Nothing
            }

            return true;
        }

        private bool RunLGDJob(List<Loanbook_Data> lstRaw, Guid _eclId, EclType _eclType)
        {

            //Next Line to be removed
            //lstRaw = lstRaw.Where(o => o.ContractStartDate == null && o.ContractEndDate == null).Take(5).ToList();

            var LGDPreCalc = _eclTask.LGDPreCalculation(lstRaw);
            Console.WriteLine($"Done with LGD Precalculation - {DateTime.Now}");

            var collateral_R = _eclTask.Collateral_OMV_FSV(lstRaw, LGDPreCalc);
            Console.WriteLine($"Computed Collateral OVM - {DateTime.Now}");

            ///
            /// Save Collateral OMV_FSV
            ///

            //Insert to Database
            ExecuteNative.SaveLGDCollaterals(collateral_R, _eclId, _eclType);
            Console.WriteLine($"Save LGD Collateral - {DateTime.Now}");

            var corTable = _eclTask.CalculateCoR_Main(LGDPreCalc, lstRaw, collateral_R);
            Console.WriteLine($"Done with Calculate CoR main - {DateTime.Now}");

            var accountData = _eclTask.AccountData(lstRaw, LGDPreCalc, collateral_R, corTable);
            Console.WriteLine($"Done Calculating Account data - {DateTime.Now}");

            //Insert to Database
            ExecuteNative.SaveLGDAccountdata(accountData, _eclId, _eclType);
            Console.WriteLine($"Saved LGD Account Data - {DateTime.Now}");

            return true;
        }

        public List<LGDAccountData> GetLgdContractData()
        {
            var qry = Queries.LGD_LgdAccountDatas(_eclId, _eclType);
            var dt = DataAccess.i.GetData(qry);
            var lgdAccountData = new List<LGDAccountData>();

            foreach (DataRow dr in dt.Rows)
            {
                lgdAccountData.Add(DataAccess.i.ParseDataToObject(new LGDAccountData(), dr));
            }

            return lgdAccountData;
        }

        public List<LGDCollateralData> GetLGDCollateralData()
        {
            var qry = Queries.LGD_WholesaleLgdCollateralDatas(_eclId, _eclType);
            var dt = DataAccess.i.GetData(qry);
            var lgdCollateralData = new List<LGDCollateralData>();

            foreach (DataRow dr in dt.Rows)
            {
                lgdCollateralData.Add(DataAccess.i.ParseDataToObject(new LGDCollateralData(), dr));
            }

            return lgdCollateralData;
        }
    }
}
