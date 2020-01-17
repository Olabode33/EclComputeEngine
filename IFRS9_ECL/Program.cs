using IFRS9_ECL.Core;
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
            Console.WriteLine($"Start Time {DateTime.Now}");
            //Process Wholesale
            var masterGuid = Guid.NewGuid();
             ProcessECL_Wholesale_EAD.i.ProcessTask(masterGuid);
            ProcessECL_Wholesale_LGD.i.ProcessTask(masterGuid);

            masterGuid = Guid.Parse("4140a69e-a729-4269-a078-91a01b5e0cd0");

            new ProcessECL_Wholesale_PD(masterGuid).ProcessTask();
            Console.WriteLine($"End Time {DateTime.Now}");
        }
    }
}