using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace IFRS9_ECL.Util
{
    public static class AppSettings
    {
        public static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["IFRS9_AUTOMATION_DB"].ConnectionString; } }
    }
}
