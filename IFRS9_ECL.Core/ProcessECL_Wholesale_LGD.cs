using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Raw;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace IFRS9_ECL.Core
{
    public class ProcessECL_Wholesale_LGD
    {
        public static readonly ProcessECL_Wholesale_LGD i = new ProcessECL_Wholesale_LGD();

        public bool ProcessTask(Guid masterGuid)
        {
            //Get Data Excel/Database
            var qry = Queries.Raw_Data;
            var _lstRaw = DataAccess.i.GetData(qry);

            var lstRaw = new List<Loanbook_Data>();
            foreach (DataRow dr in _lstRaw.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new Loanbook_Data(), dr);
                itm.ContractId = ECLTasks.i.GenerateContractId(itm);
                lstRaw.Add(itm);
            }
            Console.WriteLine($"Finished Extracting Raw Data - {DateTime.Now}");

            //Next Line to be removed
            //lstRaw = lstRaw.Where(o => o.ContractStartDate == null && o.ContractEndDate == null).Take(5).ToList();

            var LGDPreCalc = ECLTasks.i.LGDPreCalculation(lstRaw);
            Console.WriteLine($"Done with LGD Precalculation - {DateTime.Now}");

            var collateral_R = ECLTasks.i.Collateral_OMV_FSV(lstRaw, LGDPreCalc);
            Console.WriteLine($"Computed Collateral OVM - {DateTime.Now}");
            //Insert to Database
            ExecuteNative.SaveLGDCollaterals(collateral_R, masterGuid);
            Console.WriteLine($"Save LGD Collateral - {DateTime.Now}");

            var corTable = ECLTasks.i.CalculateCoR_Main(LGDPreCalc, lstRaw, collateral_R);
            Console.WriteLine($"Done with Calculate CoR main - {DateTime.Now}");

            var accountData = ECLTasks.i.AccountData(lstRaw, LGDPreCalc, collateral_R, corTable);
            Console.WriteLine($"Done Calculating Account data - {DateTime.Now}");

            //Insert to Database
            ExecuteNative.SaveLGDAccountdata(accountData, masterGuid);
            Console.WriteLine($"Saved LGD Account Data - {DateTime.Now}");

            return true;
        }
    }
}
