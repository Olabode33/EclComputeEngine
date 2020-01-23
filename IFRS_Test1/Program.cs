using IFRS9_ECL.Core;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS_Test1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine($"Start Time {DateTime.Now}");
                //Process Wholesale
                var masterGuid = Guid.NewGuid();
                // ProcessECL_Wholesale_EAD.i.ProcessTask(masterGuid);
                new ProcessECL_LGD(masterGuid, EclType.Retail).ProcessTask();

                //new ProcessECL_Wholesale_PD(masterGuid).ProcessTask();
                Console.WriteLine($"End Time {DateTime.Now}");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadKey();
        }
    }
}
