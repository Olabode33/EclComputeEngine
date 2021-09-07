using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IFRS9_LGDModelProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var tasks = new List<Task>();
            while(0<1)
            {
                Thread.Sleep(AppSettings.ServerCallWaitTime);

                var basePath = AppSettings.ECLServer3;

                var di = new DirectoryInfo(basePath);

                tasks = tasks.Where(o => o.Status == TaskStatus.Running).ToList();

                if (tasks.Count>4)
                {
                    continue;
                }

                var files = di.GetFiles("*", SearchOption.AllDirectories).Where(o => o.Name.StartsWith(AppSettings.new_) && o.Name.EndsWith("LGD.xlsb")).ToList();

                foreach (var file in files.OrderBy(o=>o.Name).ToList())
                {
                    var task1 = Task.Run(() =>
                    {
                        ProcessFile(file);
                    });
                    tasks.Add(task1);
                }

            }

        }

        public static bool ProcessFile(FileInfo file)
        {

            if (!File.Exists(Path.Combine(file.Directory.FullName, AppSettings.TransferComplete)))
                return false;

            //Process LGD
            var processingFileName = file.FullName.Replace(AppSettings.new_, AppSettings.processing_);
            File.Move(file.FullName, processingFileName);


            var tryCounter = 0;
            var eadProcessor = false;
            while (!eadProcessor && tryCounter <= 3)
            {
                eadProcessor = new LGD_Processor().ExecuteLGDMacro(processingFileName);
            }
            if (eadProcessor)
            {
                var completedProcessingFileName = processingFileName.Replace(AppSettings.processing_, AppSettings.complete_);
                if(!File.Exists(completedProcessingFileName))
                    File.Move(processingFileName, completedProcessingFileName);

                //transfer file back to master server

                File.Copy(completedProcessingFileName, completedProcessingFileName.Replace(AppSettings.ECLServer3, AppSettings.ECLServer1), true);
                File.Delete(completedProcessingFileName.Replace(AppSettings.ECLServer3, AppSettings.ECLServer1).Replace(AppSettings.complete_, string.Empty));
                File.WriteAllText(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer3, AppSettings.ECLServer1)).Directory.FullName, AppSettings.LGDComputeComplete), string.Empty);

                // Move FrameworkFile
                if (File.Exists(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer3, AppSettings.ECLServer1)).Directory.FullName, AppSettings.EADComputeComplete)) && File.Exists(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer3, AppSettings.ECLServer1)).Directory.FullName, AppSettings.PDComputeComplete)))
                {
                    new Framework_Processor().TransferFrameworkInputFiles(completedProcessingFileName.Replace(AppSettings.ECLServer3, AppSettings.ECLServer1), AppSettings.LGD);
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
