using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IFRS9_EADModelProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var rrr = new EAD_Processor().ExecuteEADMacro(@"C:\Users\Dev-Sys\Desktop\ecl_dump\0\0\processing_0_07863845-2142-4850-3f73-08d94646a174_EAD.xlsb");

            //var tasks = new List<Task>();
            //while (0 < 1)
            //{
            //    Thread.Sleep(AppSettings.ServerCallWaitTime);

            //    var basePath = AppSettings.ECLServer2;

            //    var di = new DirectoryInfo(basePath);
            //    tasks = tasks.Where(o => o.Status == TaskStatus.Running).ToList();

            //    if (tasks.Count > AppSettings.MaxExcelTaskCount)
            //    {
            //        continue;
            //    }

            //    var files = di.GetFiles("*", SearchOption.AllDirectories).Where(o => o.Name.StartsWith(AppSettings.new_) && o.Name.EndsWith("EAD.xlsb")).ToList();

            //    foreach (var file in files.OrderBy(o => o.Name).ToList())
            //    {
            //        var task1 = Task.Run(() =>
            //        {
            //            ProcessFile(file);
            //        });
            //        tasks.Add(task1);
            //    }
            //}
        }

        public static bool ProcessFile(FileInfo file)
        {

            if (!File.Exists(Path.Combine(file.Directory.FullName, AppSettings.TransferComplete)))
                return false;

            //Process EAD
            var processingFileName = file.FullName.Replace(AppSettings.new_, AppSettings.processing_);
            File.Move(file.FullName, processingFileName);


            var tryCounter = 0;
            var eadProcessor = false;
            while (!eadProcessor && tryCounter <= 3)
            {
                eadProcessor = new EAD_Processor().ExecuteEADMacro(processingFileName);
                
            }
            if (eadProcessor)
            {
                var completedProcessingFileName=processingFileName.Replace(AppSettings.processing_, AppSettings.complete_);
                if (!File.Exists(completedProcessingFileName))
                    File.Move(processingFileName, completedProcessingFileName);

                //transfer file back to master server

                File.Copy(completedProcessingFileName, completedProcessingFileName.Replace(AppSettings.ECLServer2, AppSettings.ECLServer1), true);
                try { File.Delete(completedProcessingFileName.Replace(AppSettings.ECLServer2, AppSettings.ECLServer1).Replace(AppSettings.complete_, string.Empty)); } catch { }
                File.WriteAllText(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer2, AppSettings.ECLServer1)).Directory.FullName, AppSettings.EADComputeComplete), string.Empty);

                // Move FrameworkFile
                if (File.Exists(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer2, AppSettings.ECLServer1)).Directory.FullName, AppSettings.LGDComputeComplete)) && File.Exists(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer2, AppSettings.ECLServer1)).Directory.FullName, AppSettings.PDComputeComplete)))
                {
                    new Framework_Processor().TransferFrameworkInputFiles(completedProcessingFileName.Replace(AppSettings.ECLServer2, AppSettings.ECLServer1), AppSettings.EAD);
                }
            }
            else
            {
                File.Move(processingFileName, processingFileName.Replace(AppSettings.processing_, AppSettings.error_));
            }
            return true;
        }
    }
}
