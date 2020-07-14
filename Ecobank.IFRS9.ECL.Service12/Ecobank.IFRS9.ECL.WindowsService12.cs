using System;
using IFRS9_ECL.Core;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Ecobank.IFRS9.ECL.Service12
{
    public partial class IFRS9_ECL12 : ServiceBase
    {
        private Timer timer1 = new Timer();
        public IFRS9_ECL12()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Log4Net.Log.Info("Started Service");
                timer1.Elapsed += new System.Timers.ElapsedEventHandler(TmrMain_Elapsed);
                timer1.Interval = AppSettings.ServiceInterval;
                var runTime = (AppSettings.ServiceInterval / (60000));
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
            timer1.Interval = AppSettings.ServiceInterval;
            timer1.Enabled = false;

            Log4Net.Log.Info("Timer Disabled");
            var core = new Core();
            Log4Net.Log.Info("Entering Core");
            core.ProcessRunTask();

            Log4Net.Log.Info("Task Completed!");

            timer1.Enabled = true;
            timer1.Interval = AppSettings.ServiceInterval;
            Log4Net.Log.Info("Timer Re- Enabled");
        }
        protected override void OnStop()
        {
        }
    }
}
