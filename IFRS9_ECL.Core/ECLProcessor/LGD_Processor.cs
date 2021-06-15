using IFRS9_ECL.Core.Calibration.Input;
using IFRS9_ECL.Core.ECLProcessor.Entities;
using IFRS9_ECL.Data;
using IFRS9_ECL.Util;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration
{
    public class LGD_Processor
    {

        public bool ProcessLGD(LGDParameters input)
        {

            var loanbook = Path.Combine(input.BasePath, input.LoanBookFileName);
            var model = Path.Combine(input.BasePath, $"{AppSettings.new_}{input.ModelFileName}");

            var loanbookTemp = loanbook.Replace(AppSettings.Drive, AppSettings.ECLServer3);

            if (!(new FileInfo(loanbookTemp).Directory.Exists))
                Directory.CreateDirectory(new FileInfo(loanbookTemp).Directory.FullName);

            File.Copy(loanbook, loanbookTemp, true);

            var modelTemp = model.Replace(AppSettings.Drive, AppSettings.ECLServer3);
            model = model.Replace(AppSettings.new_, string.Empty);
            File.Copy(model, modelTemp, true);

            var inputFile = JsonConvert.SerializeObject(input);
            var inputFilePath = Path.Combine(input.BasePath, AppSettings.ModelInputFileEto);
            var inputFilePathTemp = inputFilePath.Replace(AppSettings.Drive, AppSettings.ECLServer3);

            File.WriteAllText(inputFilePathTemp, inputFile);

            while (!File.Exists(modelTemp.Replace(AppSettings.new_, AppSettings.complete_)) && !File.Exists(modelTemp.Replace(AppSettings.new_, AppSettings.complete_)))
            {
                Thread.Sleep(AppSettings.ServerCallWaitTime);
            }

            if (File.Exists(modelTemp.Replace(AppSettings.new_, AppSettings.complete_)))
            {
                File.Copy(modelTemp.Replace(AppSettings.new_, AppSettings.complete_), model, true);
            }
            if (File.Exists(modelTemp.Replace(AppSettings.new_, AppSettings.error_)))
            {
                File.Copy(modelTemp.Replace(AppSettings.new_, AppSettings.error_), model, true);
                //Log error in Db
            }

            return true;
        }


        public bool ExecuteLGDMacro(string filepath)
        {
            var basePath = new FileInfo(filepath).DirectoryName;
            var inputFileText = File.ReadAllText(Path.Combine(basePath, AppSettings.ModelInputFileEto));
            var input = JsonConvert.DeserializeObject<LGDParameters>(inputFileText);
            string txtLocation = filepath;

            object _missingValue = System.Reflection.Missing.Value;
            Application excel = new Application();
            var theWorkbook = excel.Workbooks.Open(txtLocation,
                                                                    _missingValue,
                                                                    false,
                                                                    _missingValue,
                                                                    _missingValue,
                                                                    _missingValue,
                                                                    true,
                                                                    _missingValue,
                                                                    _missingValue,
                                                                    true,
                                                                    _missingValue,
                                                                    _missingValue,
                                                                    _missingValue);

            try
            {
                Worksheet startSheet = theWorkbook.Sheets[1];
                startSheet.Unprotect(AppSettings.SheetPassword);

                var reportDate = input.ReportDate.ToString("dd MMMM yyyy");
                startSheet.Cells[9, 5] = reportDate;
                startSheet.Cells[13, 5] = input.NonExpired;
                startSheet.Cells[14, 5] = input.Expired;

                startSheet.Cells[18, 5] = Path.Combine(basePath, new FileInfo(input.LoanBookFileName).Name);
                var fileName = new FileInfo(input.LoanBookFileName).Name;
                startSheet.Cells[19, 5] = fileName;


                excel.Run("unhide_unprotect");
                excel.Run("primary_condition_extractor");
                excel.Run("centre_sheets");
                excel.Run("hide_protect");

                excel.Run("unhide_unprotect");
                excel.Run("resize_LGD_workbook");
                excel.Run("centre_sheets");
                excel.Run("hide_protect");

                theWorkbook.Save();
                theWorkbook.Close(true);

                

                return true;

            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                Log4Net.Log.Info(DateTime.Now);
                Log4Net.Log.Info(input.LoanBookFileName);

                theWorkbook.Close(true);
                excel.Quit();

                

                return false;
            }
            finally
            {
                excel.Quit();
            }




        }

    }
}
