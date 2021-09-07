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
using System.Reflection;
using System.Runtime.InteropServices;
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


            var inputFile = JsonConvert.SerializeObject(input);
            var inputFilePath = Path.Combine(input.BasePath, AppSettings.ModelInputFileEto);
            var inputFilePathTemp = inputFilePath.Replace(AppSettings.Drive, AppSettings.ECLServer3);
            File.WriteAllText(inputFilePathTemp, inputFile);

            File.Copy(loanbook, loanbookTemp, true);

            var modelTemp = model.Replace(AppSettings.Drive, AppSettings.ECLServer3);
            model = model.Replace(AppSettings.new_, string.Empty);
            File.Copy(model, modelTemp, true);

            File.WriteAllText(Path.Combine(new FileInfo(loanbookTemp).Directory.FullName, AppSettings.TransferComplete), string.Empty);

            //while (!File.Exists(modelTemp.Replace(AppSettings.new_, AppSettings.complete_)) && !File.Exists(modelTemp.Replace(AppSettings.new_, AppSettings.complete_)))
            //{
            //    Thread.Sleep(AppSettings.ServerCallWaitTime);
            //}

            //if (File.Exists(modelTemp.Replace(AppSettings.new_, AppSettings.complete_)))
            //{
            //    File.Copy(modelTemp.Replace(AppSettings.new_, AppSettings.complete_), model, true);
            //}
            //if (File.Exists(modelTemp.Replace(AppSettings.new_, AppSettings.error_)))
            //{
            //    File.Copy(modelTemp.Replace(AppSettings.new_, AppSettings.error_), model, true);
            //    //Log error in Db
            //}

            return true;
        }


        public bool ExecuteLGDMacro(string filepath)
        {
            try
            {
                var basePath = new FileInfo(filepath).DirectoryName;
                var inputFileText = File.ReadAllText(Path.Combine(basePath, AppSettings.ModelInputFileEto));
                var input = JsonConvert.DeserializeObject<LGDParameters>(inputFileText);
                string txtLocation = filepath;

                object _missingValue = System.Reflection.Missing.Value;
                Application excel = new Application();
                excel.DisplayAlerts = false;
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
                    // Garbage collecting
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    theWorkbook.Close(true, Missing.Value, Missing.Value);
                    excel.Quit();
                    Marshal.FinalReleaseComObject(theWorkbook);
                    Marshal.FinalReleaseComObject(excel);

                    return true;

                }
                catch (Exception ex)
                {
                    Log4Net.Log.Error(ex);
                    Log4Net.Log.Info(DateTime.Now);
                    Log4Net.Log.Info(input.LoanBookFileName);

                    // Garbage collecting
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    theWorkbook.Close(true, Missing.Value, Missing.Value);
                    excel.Quit();
                    Marshal.FinalReleaseComObject(theWorkbook);
                    Marshal.FinalReleaseComObject(excel);


                    return false;
                }
                finally
                {
                    // excel.Quit();
                }
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                Log4Net.Log.Info(DateTime.Now);

                return false;
            }



        }

    }
}
