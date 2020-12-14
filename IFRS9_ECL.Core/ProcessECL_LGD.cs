using IFRS9_ECL.Core.FrameworkComputation;
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
            
        }
        public bool ProcessTask(List<Loanbook_Data> loanbooks)
        {
            try
            {

                Log4Net.Log.Info($"Finished Extracting Raw Data - {DateTime.Now}");


                if (1!=1)//loanbooks.Count <= 1000)
                {
                    RunLGDJob(loanbooks, _eclId, _eclType);
                    return true;
                }
                //var checker = loanbooks.Count / 60;

                var threads = loanbooks.Count / 500;
                threads = threads + 1;

                var taskLst = new List<Task>();

                //threads = 1;
                for (int i = 0; i < threads; i++)
                {
                    var sub_LoanBook = loanbooks.Skip(i * 500).Take(500).ToList();

                    var task = Task.Run(() =>
                    {
                        RunLGDJob(sub_LoanBook, _eclId, _eclType);
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

                return true;
            }catch(Exception ex)
            {
                Log4Net.Log.Error(ex);
                return true;
            }
        }

        private bool RunLGDJob(List<Loanbook_Data> lstRaw, Guid _eclId, EclType _eclType)
        {

            //lstRaw = lstRaw.Where(o => o.ContractNo.Contains("182NIFC162940002") || o.ContractId.Contains("182NIFC162940002")).ToList();
            //Next Line to be removed
            //lstRaw = lstRaw.Where(o => o.ContractStartDate == null && o.ContractEndDate == null).Take(5).ToList();
            _eclTask = new ECLTasks(_eclId, _eclType);
            var LGDPreCalc = _eclTask.LGDPreCalculation(lstRaw);
            Log4Net.Log.Info($"Done with LGD Precalculation - {DateTime.Now}");

            var collateral_R = _eclTask.Collateral_OMV_FSV(lstRaw, LGDPreCalc);
            Log4Net.Log.Info($"Computed Collateral OVM - {DateTime.Now}");

            ///
            /// Save Collateral OMV_FSV
            ///

            //Insert to Database
            ExecuteNative.SaveLGDCollaterals(collateral_R, _eclId, _eclType);
            Log4Net.Log.Info($"Save LGD Collateral - {DateTime.Now}");

            var corTable = _eclTask.CalculateCoR_Main(LGDPreCalc, lstRaw, collateral_R);
            Log4Net.Log.Info($"Done with Calculate CoR main - {DateTime.Now}");
            
            var accountData = _eclTask.AccountData(lstRaw, LGDPreCalc, collateral_R, corTable);
            Log4Net.Log.Info($"Done Calculating Account data - {DateTime.Now}");

            //Insert to Database
            ExecuteNative.SaveLGDAccountdata(accountData, _eclId, _eclType);
            Log4Net.Log.Info($"Saved LGD Account Data - {DateTime.Now}");

            return true;
        }

        public List<LGDAccountData> GetLgdContractData(List<Loanbook_Data> loanbook)
        {
            var lgdAccountData = Util.FileSystemStorage<LGDAccountData>.ReadCsvData(this._eclId, ECLStringConstants.i.LGDAccountData_Table(this._eclType));

            var contract_Ids = loanbook.Select(o => o.ContractId).ToList();
            var filteredList = lgdAccountData.Where(o=> contract_Ids.Contains(o.CONTRACT_NO)).ToList();
            foreach(var itm in filteredList)
            {
                try { itm.LIM_MONTHS = loanbook.FirstOrDefault(o => o.ContractId == itm.CONTRACT_NO).LIM_MONTH; } catch { }
            }
            return filteredList;
        }

        public List<LGDCollateralData> GetLGDCollateralData()
        {

            var lgdCollateralData = Util.FileSystemStorage<LGDCollateralData>.ReadCsvData(this._eclId, ECLStringConstants.i.LGDCollateral_Table(this._eclType));
            
            return lgdCollateralData;
        }
        /// <summary>
        /// 1 = Fsv
        /// 2 = Stage
        /// 3 = TTr
        /// </summary>
        /// <param name="dataRequest"></param>
        /// <returns></returns>
        public List<EclOverrides> GetOverrideData(int dataRequest)
        {
            var qry = "";
            if(dataRequest==1)
            {
                qry = Queries.EclOverridesFsv(_eclId, _eclType);
            }
            if (dataRequest == 2)
            {
                qry = Queries.EclOverridesStage(_eclId, _eclType);
            }
            if (dataRequest == 3)
            {
                qry = Queries.EclOverridesTTr(_eclId, _eclType);
            }

            var dt = DataAccess.i.GetData(qry);
            var eclOverrides = new List<EclOverrides>();

            foreach (DataRow dr in dt.Rows)
            {
                eclOverrides.Add(DataAccess.i.ParseDataToObject(new EclOverrides(), dr));
            }

            return eclOverrides;
        }

    }
}
