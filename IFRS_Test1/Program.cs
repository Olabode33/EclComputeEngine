﻿using IFRS9_ECL.Core;
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
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace IFRS_Test1
{
    class Program
    {

        static void DeployServices()
        {
            var serviceCount = AppSettings.ServiceCount;
            
            Log4Net.Log.Info("Deploying...");
            Log4Net.Log.Info("Should services be started? (YES/NO):");
            var val=Console.ReadLine();


            for (int i = 1; i <= serviceCount; i++)
            {

                
                try
                {
                    string serviceDirectory = AppSettings.ServiceFolder;
                    serviceDirectory = serviceDirectory.Replace("[i]", i.ToString());

                    if (!Directory.Exists(serviceDirectory))
                    {
                        continue;
                    }
                    ServiceController service= new ServiceController();
                    try
                    {
                        service = new ServiceController($"IFRS9_ECL{i}");
                        if (service.Status == ServiceControllerStatus.Running)
                        {
                            Log4Net.Log.Info($"Service {i} has been stopped");
                            //restart service
                            var timeoutMilliseconds = 60000;
                            int millisec1 = Environment.TickCount;
                            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                            service.Stop();
                            service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                            Log4Net.Log.Info($"Service {i} stopped");

                        }
                    }
                    catch { }

                    //Replace files

                    var deployFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Deploy");
                    var files = new DirectoryInfo(deployFolderPath).GetFiles();
                    foreach(var fl in files)
                    {
                        File.Copy(fl.FullName, Path.Combine(serviceDirectory, fl.Name),true);
                    }

                    if(val=="YES")
                    {
                        //Start Service
                        service.Start();
                        Log4Net.Log.Info($"Service {i} Started");
                    }

                }
                catch (Exception ex)
                {
                    Log4Net.Log.Info(ex);
                    Console.ReadKey();
                }
            }
            Log4Net.Log.Info("Done!");
            Console.ReadKey();

        }
        static void Main(string[] args)
        {

            //DeployServices();

            Core c = new Core();
            c.ProcessRunTask();
            
            // Generate Macro Data
            //var affId = 6; //new Guid("4FE329C8-C57F-4EB2-8F7F-08D75BC1F14A");


            //// 55C3EDDB - 94F5 - 47BF - 7B86 - 08D78395353F
            //var caliId = new Guid("55C3EDDB-94F5-47BF-7B86-08D78395353F");
            //Macro_Processor m = new Macro_Processor();
            
            ////m.ProcessMacro(1,affId);


            //Log4Net.Log.Info("Started Behavioural");
            //Log4Net.Log.Info(DateTime.Now);
            //CalibrationInput_EAD_Behavioural_Terms_Processor p = new CalibrationInput_EAD_Behavioural_Terms_Processor();
            //p.ProcessCalibration(caliId, affId);

            //Log4Net.Log.Info("Started CCF");
            //Log4Net.Log.Info(DateTime.Now);
            //CalibrationInput_EAD_CCF_Summary_Processor q = new CalibrationInput_EAD_CCF_Summary_Processor();
            //q.ProcessCalibration(caliId, affId);

            //Log4Net.Log.Info("Started Haircut");
            //Log4Net.Log.Info(DateTime.Now);
            //CalibrationInput_LGD_Haricut_Processor r = new CalibrationInput_LGD_Haricut_Processor();
            //r.ProcessCalibration(caliId, affId);

            //Log4Net.Log.Info("Started CureRate");
            //Log4Net.Log.Info(DateTime.Now);
            //CalibrationInput_LGD_RecoveryRate_Processor s = new CalibrationInput_LGD_RecoveryRate_Processor();
            //s.ProcessCalibration(caliId, affId);

            //Log4Net.Log.Info("Started PD");
            //Log4Net.Log.Info(DateTime.Now);
            //CalibrationInput_PD_CR_RD_Processor t = new CalibrationInput_PD_CR_RD_Processor();
            //t.ProcessCalibration(caliId, affId);
            //Log4Net.Log.Info("Ended All");
            //Log4Net.Log.Info(DateTime.Now);


            //var masterGuid = Guid.NewGuid();
            //masterGuid = Guid.Parse("4140a69e-a729-4269-a078-91a01b5e0cd0");
            //var rc = new ReportComputation();
            //rc.GenerateEclReport(EclType.Wholesale, masterGuid);
            //try
            //{
            //    Log4Net.Log.Info($"Start Time {DateTime.Now}");
            //    //Process Wholesale
            //    var masterGuid = Guid.NewGuid();
            //    // ProcessECL_Wholesale_EAD.i.ProcessTask(masterGuid);
            //    new ProcessECL_LGD(masterGuid, EclType.Retail).ProcessTask();

            //    //new ProcessECL_Wholesale_PD(masterGuid).ProcessTask();
            //    Log4Net.Log.Info($"End Time {DateTime.Now}");
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
