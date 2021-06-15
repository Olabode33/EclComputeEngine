using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IFRS9_FrameworkModelProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            while (0 < 1)
            {
                Thread.Sleep(AppSettings.ServerCallWaitTime);

                var basePath = AppSettings.ECLServer5;

                var di = new DirectoryInfo(basePath);
                var files = di.GetFiles("*", SearchOption.AllDirectories).Where(o => o.Name.StartsWith(AppSettings.new_) && o.Name.EndsWith("Framework.xlsb")).ToList();

                foreach (var file in files)
                {
                    var task1 = Task.Run(() =>
                    {
                        ProcessFile(file);
                    });
                }
            }
        }

        public static bool ProcessFile(FileInfo file)
        {
            //Process EAD
            var processingFileName = file.FullName.Replace(AppSettings.new_, AppSettings.processing_);
            File.Move(file.FullName, processingFileName);


            var tryCounter = 0;
            var eadProcessor = false;
            while (!eadProcessor && tryCounter <= 3)
            {
                eadProcessor = new Framework_Processor().ExecuteFrameworkMacro(processingFileName);
            }
            if (eadProcessor)
            {
                File.Move(processingFileName, processingFileName.Replace(AppSettings.processing_, AppSettings.complete_));
            }
            else
            {
                File.Move(processingFileName, processingFileName.Replace(AppSettings.processing_, AppSettings.error_));
            }

            return true;
        }
    }
}
