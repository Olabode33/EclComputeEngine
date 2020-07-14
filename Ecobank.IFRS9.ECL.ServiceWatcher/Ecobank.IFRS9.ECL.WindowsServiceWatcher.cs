using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Ecobank.IFRS9.ECL.ServiceWatcher
{
    public partial class EcobankIFRS9ServiceWater : ServiceBase
    {
        private Timer timer1 = new Timer();
        public EcobankIFRS9ServiceWater()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Log4Net.Log.Info("Started Service");
                timer1.Elapsed += new System.Timers.ElapsedEventHandler(TmrMain_Elapsed);
                timer1.Interval = AppSettings.WatcherServiceInterval;
                var runTime = (AppSettings.WatcherServiceInterval / (60000));
                Log4Net.Log.InfoFormat("Service will run in the next {0}", runTime);
                timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error("Error Occured", ex);
            }
        }

        private void TmrMain_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer1.Interval = AppSettings.WatcherServiceInterval;
            timer1.Enabled = false;

            Log4Net.Log.Info("Timer Disabled");

            ProcessWatchServices();

            Log4Net.Log.Info("Task Completed!");

            timer1.Enabled = true;
            timer1.Interval = AppSettings.ServiceInterval;
            Log4Net.Log.Info("Timer Re- Enabled");
        }

        protected override void OnStop()
        {
        }



        public bool ProcessWatchServices()
        {
            var serviceCount = AppSettings.ServiceCount;
            var ServiceSleepMaxInterval = AppSettings.ServiceSleepMaxInterval;

            Log4Net.Log.Info("Checking Service Status");

            for (int i = 1; i <= serviceCount; i++)
            {

                ServiceController service = new ServiceController($"IFRS9_ECL{i}");
                try
                {
                    string serviceLogFile = AppSettings.ServiceLogFile;
                    serviceLogFile = serviceLogFile.Replace("[i]", i.ToString());

                    var requiresRestart = false;
                    if (!File.Exists(serviceLogFile))
                    {
                        requiresRestart = true;
                    }
                    else
                    {
                        var lastwritetime = new FileInfo(serviceLogFile).LastWriteTime;
                        var span = DateTime.Now - lastwritetime;
                        if (span.Minutes > ServiceSleepMaxInterval)
                        {
                            requiresRestart = true;
                        }
                    }

                    if (service.Status == ServiceControllerStatus.Running && requiresRestart)
                    {
                        Log4Net.Log.Info($"Service {i} has stopped working though its still running. It will be restarted.");
                        //restart service
                        var timeoutMilliseconds = 60000;
                        int millisec1 = Environment.TickCount;
                        TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                        Log4Net.Log.Info($"Service {i} stopped");
                        // count the rest of the timeout
                        int millisec2 = Environment.TickCount;
                        timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                        Log4Net.Log.Info($"Service {i} Started back");
                    }

                }
                catch (Exception ex)
                {
                    Log4Net.Log.Error(ex);
                }
            }


            return true;
        }

    }
}
