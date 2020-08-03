using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace IFRS9_ECL.Util
{
    public static class AppSettings
    {
        public static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["IFRS9_DB"].ConnectionString; } }
        public static string CalibrationModelPath { get { return ConfigurationManager.AppSettings["CalibrationModelPath"]; } }
        public static string MacroModelPath { get { return ConfigurationManager.AppSettings["MacroModelPath"]; } }
        public static string RScriptPath { get { return ConfigurationManager.AppSettings["RScriptPath"]; } }

        public static double ServiceInterval
        {
            get
            {
                return 10000;
            }
        }

        public static double WatcherServiceInterval
        {
            get
            {
                return 300000;
            }
        }

        public static int ServiceCount { get { return int.Parse(ConfigurationManager.AppSettings["ServiceCount"]); } }
        public static int ServiceSleepMaxInterval { get { return int.Parse(ConfigurationManager.AppSettings["ServiceSleepMaxInterval"]); } }

        public static string GetCounter(long cnt)
        {
            //return $"_{cnt.ToString()}";
            var counter = 6000;
            if (cnt > 6000 && cnt <= 10000)
                counter = 6000;

            if (cnt > 10000 && cnt <= 15000)
                counter = 15000;

            if (cnt > 15000 && cnt <= 20000)
                counter = 20000;

            if (cnt > 20000 && cnt <= 30000)
                counter = 30000;

            if (cnt > 30000 && cnt <= 60000)
                counter = 60000;

            if (cnt > 60000)// && cnt <= 100000)
                counter = 100000;

            //if (cnt > 100000)
            //    counter = 150000;

            return $"C{counter}";
        }

        public static string ServiceLogFile { get { return ConfigurationManager.AppSettings["ServiceLogFile"]; } }
        public static string ServiceFolder { get { return ConfigurationManager.AppSettings["ServiceFolder"]; } }


    }
}
