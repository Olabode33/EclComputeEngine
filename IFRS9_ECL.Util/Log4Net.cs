using System.Reflection;
using log4net;
using log4net.Config;

namespace IFRS9_ECL.Util
{
    public static class Log4Net
    {
 
        private static readonly ILog _Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static Log4Net()
        {
            //System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            XmlConfigurator.Configure(); //only once
        }
        public static ILog Log 
        {
            get { return _Log; }
        }
   }
}
