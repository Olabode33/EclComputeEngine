using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core
{
    public class Core
    {
        public bool ProcessTask()
        {
            //return true;
            var retailEcls=Queries.EclsRegister(EclType.Retail.ToString());
            var wholesaleEcls = Queries.EclsRegister(EclType.Wholesale.ToString());
            var obeEcls = Queries.EclsRegister(EclType.Obe.ToString());

            var dtR=DataAccess.i.GetData(retailEcls);
            var dtW = DataAccess.i.GetData(wholesaleEcls);
            var dtO = DataAccess.i.GetData(obeEcls);

            var eclRegister = new EclRegister { EclType=-1 };

            if(dtR.Rows.Count>0)
            {
                var itm = DataAccess.i.ParseDataToObject(new EclRegister(), dtR.Rows[0]);
                itm.EclType = 0;
                eclRegister = itm;
            }
            if (dtW.Rows.Count > 0 && eclRegister.EclType==-1)
            {
                var itm = DataAccess.i.ParseDataToObject(new EclRegister(), dtW.Rows[0]);
                itm.EclType = 1;
                eclRegister = itm;
            }
            if (dtO.Rows.Count > 0 && eclRegister.EclType == -1)
            {
                var itm = DataAccess.i.ParseDataToObject(new EclRegister(), dtO.Rows[0]);
                itm.EclType = 2;
                eclRegister = itm;
            }

            if (eclRegister.EclType==-1)
            {
                Log4Net.Log.Info("Found No Pending ECL");
                return true;
            }
            var eclType = GetECLType(eclRegister.EclType);
                Log4Net.Log.Info($"Found ECL with ID {eclRegister.Id} of Type [{eclType.ToString()}]. Running will commence if it has not been picked by another Job");
            


            var masterGuid =eclRegister.Id;//Guid.NewGuid();
            //masterGuid = Guid.Parse("4140a69e-a729-4269-a078-91a01b5e0cd0");


            Console.WriteLine($"Start Time {DateTime.Now}");
            
            // Process EAD
            ProcessECL_EAD.i.ProcessTask(masterGuid, eclType);

            //Process LGD
            new ProcessECL_LGD(masterGuid, eclType).ProcessTask();

            //Process PD
            new ProcessECL_PD(masterGuid, eclType).ProcessTask();

            //Process PD
            new ProcessECL_Framework(masterGuid, ECL_Scenario.Best, eclType).ProcessTask();

            Console.WriteLine($"End Time {DateTime.Now}");
            return true;
        }

        private EclType GetECLType(int id)
        {
            if (id == 0)
                return EclType.Retail;

            if (id == 1)
                return EclType.Wholesale;

            if (id == 2)
                return EclType.Obe;

            return EclType.None;

        }
    }
}
