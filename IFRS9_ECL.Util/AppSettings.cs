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
    }
}
