using IFRS9_ECL.Core;
using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Core.Report;
using IFRS9_ECL.Models.ECL_Result;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace IFRS_Test1
{
    class Program
    {
        static void Main(string[] args)
        {
            Core c = new Core();
            c.ProcessRunTask();
            
            // Generate Macro Data
            //var affId = 6; //new Guid("4FE329C8-C57F-4EB2-8F7F-08D75BC1F14A");


            //// 55C3EDDB - 94F5 - 47BF - 7B86 - 08D78395353F
            //var caliId = new Guid("55C3EDDB-94F5-47BF-7B86-08D78395353F");
            //Macro_Processor m = new Macro_Processor();
            
            ////m.ProcessMacro(1,affId);


            //Console.WriteLine("Started Behavioural");
            //Console.WriteLine(DateTime.Now);
            //CalibrationInput_EAD_Behavioural_Terms_Processor p = new CalibrationInput_EAD_Behavioural_Terms_Processor();
            //p.ProcessCalibration(caliId, affId);

            //Console.WriteLine("Started CCF");
            //Console.WriteLine(DateTime.Now);
            //CalibrationInput_EAD_CCF_Summary_Processor q = new CalibrationInput_EAD_CCF_Summary_Processor();
            //q.ProcessCalibration(caliId, affId);

            //Console.WriteLine("Started Haircut");
            //Console.WriteLine(DateTime.Now);
            //CalibrationInput_LGD_Haricut_Processor r = new CalibrationInput_LGD_Haricut_Processor();
            //r.ProcessCalibration(caliId, affId);

            //Console.WriteLine("Started CureRate");
            //Console.WriteLine(DateTime.Now);
            //CalibrationInput_LGD_RecoveryRate_Processor s = new CalibrationInput_LGD_RecoveryRate_Processor();
            //s.ProcessCalibration(caliId, affId);

            //Console.WriteLine("Started PD");
            //Console.WriteLine(DateTime.Now);
            //CalibrationInput_PD_CR_RD_Processor t = new CalibrationInput_PD_CR_RD_Processor();
            //t.ProcessCalibration(caliId, affId);
            //Console.WriteLine("Ended All");
            //Console.WriteLine(DateTime.Now);


            //var masterGuid = Guid.NewGuid();
            //masterGuid = Guid.Parse("4140a69e-a729-4269-a078-91a01b5e0cd0");
            //var rc = new ReportComputation();
            //rc.GenerateEclReport(EclType.Wholesale, masterGuid);
            //try
            //{
            //    Console.WriteLine($"Start Time {DateTime.Now}");
            //    //Process Wholesale
            //    var masterGuid = Guid.NewGuid();
            //    // ProcessECL_Wholesale_EAD.i.ProcessTask(masterGuid);
            //    new ProcessECL_LGD(masterGuid, EclType.Retail).ProcessTask();

            //    //new ProcessECL_Wholesale_PD(masterGuid).ProcessTask();
            //    Console.WriteLine($"End Time {DateTime.Now}");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}
            //Console.ReadKey();


            //Console.ReadLine();
        }
    }
}
