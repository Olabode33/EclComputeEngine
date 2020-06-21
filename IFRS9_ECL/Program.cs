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

            
            Console.WriteLine($"Start Time {DateTime.Now}");
            //Process Wholesale
            var masterGuid = Guid.NewGuid();
            masterGuid = Guid.Parse("4140a69e-a729-4269-a078-91a01b5e0cd0");

            new ProcessECL_EAD(masterGuid, EclType.Wholesale).ProcessTask(new List<Loanbook_Data>());

            //Console.WriteLine("Done Done Done");
            //Console.ReadKey();
            // return;
           // new ProcessECL_LGD(masterGuid, EclType.Wholesale).ProcessTask();

            //Console.WriteLine("Done Done Done");
            //Console.ReadKey();
            // return;

            //new ProcessECL_PD(masterGuid, EclType.Wholesale).ProcessTask();

            //new ProcessECL_PD(masterGuid, EclType.Wholesale).ProcessTask();

            new ProcessECL_Framework(masterGuid, ECL_Scenario.Best, EclType.Wholesale).ProcessTask(new List<Loanbook_Data>());
            Console.WriteLine($"Best Time {DateTime.Now}");
            new ProcessECL_Framework(masterGuid, ECL_Scenario.Optimistic, EclType.Wholesale).ProcessTask(new List<Loanbook_Data>());
            Console.WriteLine($"Optimistic Time {DateTime.Now}");
            new ProcessECL_Framework(masterGuid, ECL_Scenario.Downturn, EclType.Wholesale).ProcessTask(new List<Loanbook_Data>());
            Console.WriteLine($"Downturn Time {DateTime.Now}");
            Console.WriteLine($"End Time {DateTime.Now}");
            //Console.WriteLine("Done Done Done");
            //Console.ReadKey();
            // return;
        }
    }
}