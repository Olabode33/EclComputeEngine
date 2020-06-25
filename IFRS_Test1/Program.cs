using IFRS9_ECL.Core;
using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Core.Report;
using IFRS9_ECL.Models.ECL_Result;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace IFRS_Test1
{
    class Program
    {

        static void Start()
        {
            // This method takes ten seconds to terminate.
            Thread.Sleep(10000);
            task.Add(true);
        }

        static List<bool> task = new List<bool>();
        static void Main(string[] args)
        {



            //var stopwatch = Stopwatch.StartNew();
            //// Create an array of Thread references.
            //Thread[] array = new Thread[4];
            //for (int i = 0; i < array.Length; i++)
            //{
            //    // Start the thread with a ThreadStart.
            //    array[i] = new Thread(new ThreadStart(Start));
            //    array[i].Start();
            //}

            //while(task.Count!= array.Count())
            //{
            //    Log4Net.Log.Info("Still running");
            //}
          
            //Log4Net.Log.Info("DONE: {0}", stopwatch.ElapsedMilliseconds);



            Core c = new Core();
            c.ProcessRunTask();
            
            // Generate Macro Data
            //var affId = 6; //new Guid("4FE329C8-C57F-4EB2-8F7F-08D75BC1F14A");


            //// 55C3EDDB - 94F5 - 47BF - 7B86 - 08D78395353F
            //var caliId = new Guid("55C3EDDB-94F5-47BF-7B86-08D78395353F");
            //Macro_Processor m = new Macro_Processor();
            
            ////m.ProcessMacro(1,affId);


            //Log4Net.Log.Info("Started Behavioural");
            //Console.WriteLine(DateTime.Now);
            //CalibrationInput_EAD_Behavioural_Terms_Processor p = new CalibrationInput_EAD_Behavioural_Terms_Processor();
            //p.ProcessCalibration(caliId, affId);

            //Log4Net.Log.Info("Started CCF");
            //Console.WriteLine(DateTime.Now);
            //CalibrationInput_EAD_CCF_Summary_Processor q = new CalibrationInput_EAD_CCF_Summary_Processor();
            //q.ProcessCalibration(caliId, affId);

            //Log4Net.Log.Info("Started Haircut");
            //Console.WriteLine(DateTime.Now);
            //CalibrationInput_LGD_Haricut_Processor r = new CalibrationInput_LGD_Haricut_Processor();
            //r.ProcessCalibration(caliId, affId);

            //Log4Net.Log.Info("Started CureRate");
            //Console.WriteLine(DateTime.Now);
            //CalibrationInput_LGD_RecoveryRate_Processor s = new CalibrationInput_LGD_RecoveryRate_Processor();
            //s.ProcessCalibration(caliId, affId);

            //Log4Net.Log.Info("Started PD");
            //Console.WriteLine(DateTime.Now);
            //CalibrationInput_PD_CR_RD_Processor t = new CalibrationInput_PD_CR_RD_Processor();
            //t.ProcessCalibration(caliId, affId);
            //Log4Net.Log.Info("Ended All");
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
            //    Log4Net.Log.Error(ex.ToString());
            //}
            ////Console.ReadKey();


            //Console.ReadLine();
        }
    }
}
