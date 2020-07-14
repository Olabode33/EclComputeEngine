using IFRS9_ECL.Core;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL
{
    class Program
    {
        static void Main(string[] args)
        {
            var lst = new List<int>();

            
            Log4Net.Log.Info($"Start Time {DateTime.Now}");
            //Process Wholesale
            var masterGuid = Guid.NewGuid();
            masterGuid = Guid.Parse("4140a69e-a729-4269-a078-91a01b5e0cd0");

            new ProcessECL_EAD(masterGuid, EclType.Wholesale).ProcessTask(new List<Loanbook_Data>());

            //Log4Net.Log.Info("Done Done Done");
            ////Console.ReadKey();
            // return;
           // new ProcessECL_LGD(masterGuid, EclType.Wholesale).ProcessTask();

            //Log4Net.Log.Info("Done Done Done");
            ////Console.ReadKey();
            // return;

            //new ProcessECL_PD(masterGuid, EclType.Wholesale).ProcessTask();

            //new ProcessECL_PD(masterGuid, EclType.Wholesale).ProcessTask();

            //new ProcessECL_Framework(masterGuid, ECL_Scenario.Best, EclType.Wholesale).ProcessTask(new List<Loanbook_Data>());
            //Log4Net.Log.Info($"Best Time {DateTime.Now}");
            //new ProcessECL_Framework(masterGuid, ECL_Scenario.Optimistic, EclType.Wholesale).ProcessTask(new List<Loanbook_Data>());
            //Log4Net.Log.Info($"Optimistic Time {DateTime.Now}");
            //new ProcessECL_Framework(masterGuid, ECL_Scenario.Downturn, EclType.Wholesale).ProcessTask(new List<Loanbook_Data>());
            //Log4Net.Log.Info($"Downturn Time {DateTime.Now}");
            //Log4Net.Log.Info($"End Time {DateTime.Now}");
            //Log4Net.Log.Info("Done Done Done");
            ////Console.ReadKey();
            // return;
        }
    }
}