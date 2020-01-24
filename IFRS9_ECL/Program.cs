using IFRS9_ECL.Core;
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

            //ProcessECL_EAD.i.ProcessTask(masterGuid, EclType.Wholesale);

            Console.WriteLine("Done Done Done");
            Console.ReadKey();
           // return;
            new ProcessECL_LGD(masterGuid, EclType.Retail).ProcessTask();


            Console.WriteLine("Done Done Done");
            Console.ReadKey();
             return;

            new ProcessECL_PD(masterGuid, EclType.Retail).ProcessTask();
            Console.WriteLine($"End Time {DateTime.Now}");
        }
    }
}