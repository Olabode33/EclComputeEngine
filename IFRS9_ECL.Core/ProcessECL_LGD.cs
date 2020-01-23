using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

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
