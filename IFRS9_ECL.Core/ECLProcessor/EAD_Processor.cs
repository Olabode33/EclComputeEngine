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
    public class EAD_Processor
    {

        //        public bool ProcessEAD(EADParameters input)
        //        {


        //            var loanbook = Path.Combine(input.BasePath, input.LoanBookFileName);
        //            var paymentschedule = Path.Combine(input.BasePath, input.PaymentScheduleFileName);
        //            var model = Path.Combine(input.BasePath, input.ModelFileName);

        //            var loanbookTemp = loanbook.Replace(AppSettings.DDrive, AppSettings.ECLServer2);

        //            Directory.CreateDirectory(Path.GetFullPath(loanbookTemp));
        //            File.Copy(loanbook, loanbookTemp, true);

        //            var paymentscheduleTemp = paymentschedule.Replace(AppSettings.DDrive, AppSettings.ECLServer2);
        //            File.Copy(paymentschedule, paymentscheduleTemp, true);

        //            var modelTemp = model.Replace(AppSettings.DDrive, AppSettings.ECLServer2);
        //            File.Copy(model, modelTemp,true);

        //            string txtLocation = modelTemp;// Path.GetFullPath(modelTemp);

        //            object _missingValue = System.Reflection.Missing.Value;
        //            Application excel = new Application();
        //            var theWorkbook = excel.Workbooks.Open(txtLocation,
        //                                                                    _missingValue,
        //                                                                    false,
        //                                                                    _missingValue,
        //                                                                    _missingValue,
        //                                                                    _missingValue,
        //                                                                    true,
        //                                                                    _missingValue,
        //                                                                    _missingValue,
        //                                                                    true,
        //                                                                    _missingValue,
        //                                                                    _missingValue,
        //                                                                    _missingValue);

        //            try
        //            {
        //                Worksheet startSheet = theWorkbook.Sheets[1];
        //                startSheet.Unprotect(AppSettings.SheetPassword);

        //                startSheet.Cells[9, 5] = input.ReportDate.ToString("dd MMMM yyyy");
        //                startSheet.Cells[12, 5] = input.ConversionFactorObe;
        //                startSheet.Cells[13, 5] = input.PrePaymentFactor;
        //                startSheet.Cells[15, 5] = input.NonExpired;
        //                startSheet.Cells[16, 5] = input.Expired;

        //                startSheet.Cells[20, 5] = loanbookTemp;
        //                var fileName = new FileInfo(loanbookTemp).Name;
        //                startSheet.Cells[21, 5] = fileName;
        //                startSheet.Cells[22, 5] = paymentscheduleTemp;
        //                var psfileName = new FileInfo(paymentscheduleTemp).Name;
        //                startSheet.Cells[23, 5] = psfileName;
        ////                theWorkbook.Save();

        //                excel.Run("extract_ead_data");

        //                Worksheet projection = theWorkbook.Sheets[3];
        //                projection.Unprotect(AppSettings.SheetPassword);


        //                for(int i=4; i<40; i++)
        //                {
        //                    var key = Convert.ToString(projection.Cells[i, 2].Value);
        //                    if (key == "CORPORATE")
        //                    {
        //                        projection.Cells[i, 3] = input.CCF_Corporate;
        //                    }
        //                    if (key == "COMMERCIAL")
        //                    {
        //                        projection.Cells[i, 3] = input.CCF_Commercial;
        //                    }
        //                    if (key == "CONSUMER")
        //                    {
        //                        projection.Cells[i, 3] = input.CCF_Consumer;
        //                    }
        //                    if (key == "OBE")
        //                    {
        //                        projection.Cells[i, 3] = input.CCF_OBE;
        //                    }

        //                    if(input.VariableInterestRates.Any(o=>o.VIR_Name==key))
        //                    {
        //                        projection.Cells[i, 3]=input.VariableInterestRates.FirstOrDefault(o => o.VIR_Name == key).Value;
        //                    }

        //                    if (input.ExchangeRates.Any(o => o.Currency == key))
        //                    {
        //                        projection.Cells[i, 3] = input.ExchangeRates.FirstOrDefault(o => o.Currency == key).Value;
        //                    }
        //                    if(projection.Cells[i, 3].Value==null)
        //                    {
        //                        projection.Cells[i, 3] = 0;
        //                    }
        //                }

        //                theWorkbook.Save();

        //                excel.Run("calculate_lifetime_eads");

        //                theWorkbook.Save();
        //                theWorkbook.Close(true);

        //                File.Copy(modelTemp, model, true);
        //                return true;

        //            }
        //            catch(Exception ex)
        //            {
        //                Log4Net.Log.Error(ex);
        //                Log4Net.Log.Info(DateTime.Now);
        //                Log4Net.Log.Info(input.LoanBookFileName);

        //                theWorkbook.Close(true);
        //                excel.Quit();

        //                File.Copy(modelTemp, model, true);
        //                return false;
        //            }
        //            finally
        //            {
        //                excel.Quit();
        //            }



        //        }


        public bool ProcessEAD(EADParameters input)
        {

            var loanbook = Path.Combine(input.BasePath, input.LoanBookFileName);
            var paymentschedule = Path.Combine(input.BasePath, input.PaymentScheduleFileName);
            var model = Path.Combine(input.BasePath, $"{AppSettings.new_}{input.ModelFileName}");

            var loanbookTemp = loanbook.Replace(AppSettings.Drive, AppSettings.ECLServer2);

            if (!(new FileInfo(loanbookTemp).Directory.Exists))
                Directory.CreateDirectory(new FileInfo(loanbookTemp).Directory.FullName);


            var inputFile = JsonConvert.SerializeObject(input);
            var inputFilePath = Path.Combine(input.BasePath, AppSettings.ModelInputFileEto);
            var inputFilePathTemp = inputFilePath.Replace(AppSettings.Drive, AppSettings.ECLServer2);

            File.WriteAllText(inputFilePathTemp, inputFile);

            File.Copy(loanbook, loanbookTemp, true);

            var paymentscheduleTemp = paymentschedule.Replace(AppSettings.Drive, AppSettings.ECLServer2);
            File.Copy(paymentschedule, paymentscheduleTemp, true);

            var modelTemp = model.Replace(AppSettings.Drive, AppSettings.ECLServer2);
            model = model.Replace(AppSettings.new_, string.Empty);
            File.Copy(model, modelTemp, true);

            File.WriteAllText(Path.Combine(new FileInfo(loanbookTemp).Directory.FullName, AppSettings.TransferComplete),string.Empty);



            return true;
        }


        public bool ExecuteEADMacro(string filepath)
        {
            try
            {
                var basePath = new FileInfo(filepath).DirectoryName;
                var inputFileText = File.ReadAllText(Path.Combine(basePath, AppSettings.ModelInputFileEto));
                var input = JsonConvert.DeserializeObject<EADParameters>(inputFileText);
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

                    startSheet.Cells[9, 5] = input.ReportDate.ToString("dd MMMM yyyy");
                    startSheet.Cells[12, 5] = input.ConversionFactorObe;
                    startSheet.Cells[13, 5] = input.PrePaymentFactor;
                    startSheet.Cells[15, 5] = input.NonExpired;
                    startSheet.Cells[16, 5] = input.Expired;

                    startSheet.Cells[20, 5] = Path.Combine(basePath, new FileInfo(input.LoanBookFileName).Name);
                    var fileName = new FileInfo(input.LoanBookFileName).Name;
                    startSheet.Cells[21, 5] = fileName;
                    startSheet.Cells[22, 5] = Path.Combine(basePath, new FileInfo(input.PaymentScheduleFileName).Name);
                    var psfileName = new FileInfo(input.PaymentScheduleFileName).Name;
                    startSheet.Cells[23, 5] = psfileName;
                    //                theWorkbook.Save();

                    excel.Run("extract_ead_data");

                    Worksheet projection = theWorkbook.Sheets[3];
                    projection.Unprotect(AppSettings.SheetPassword);


                    for (int i = 4; i < 40; i++)
                    {
                        var key = Convert.ToString(projection.Cells[i, 2].Value);
                        if (key == "CORPORATE")
                        {
                            projection.Cells[i, 3] = input.CCF_Corporate;
                        }
                        if (key == "COMMERCIAL")
                        {
                            projection.Cells[i, 3] = input.CCF_Commercial;
                        }
                        if (key == "CONSUMER")
                        {
                            projection.Cells[i, 3] = input.CCF_Consumer;
                        }
                        if (key == "OBE")
                        {
                            projection.Cells[i, 3] = input.CCF_OBE;
                        }

                        if (input.VariableInterestRates.Any(o => o.VIR_Name == key))
                        {
                            projection.Cells[i, 3] = input.VariableInterestRates.FirstOrDefault(o => o.VIR_Name == key).Value;
                        }

                        if (input.ExchangeRates.Any(o => o.Currency == key))
                        {
                            projection.Cells[i, 3] = input.ExchangeRates.FirstOrDefault(o => o.Currency == key).Value;
                        }
                        if (projection.Cells[i, 3].Value == null)
                        {
                            projection.Cells[i, 3] = 0;
                        }
                    }

                    theWorkbook.Save();

                    excel.Run("calculate_lifetime_eads");

                    theWorkbook.Save();
                    Log4Net.Log.Info("Save");
                    // Garbage collecting
                    GC.Collect();
                    Log4Net.Log.Info("Collect");
                    GC.WaitForPendingFinalizers();
                    Log4Net.Log.Info("Finalize");
                    theWorkbook.Close(true, Missing.Value, Missing.Value);
                    Log4Net.Log.Info("Close");
                    //excel.Quit();

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
                    //excel.Quit();
                    Marshal.FinalReleaseComObject(theWorkbook);
                    Marshal.FinalReleaseComObject(excel);

                    return false;
                }
                finally
                {
                    //// Garbage collecting
                    //try
                    //{
                    //    GC.Collect();
                    //    GC.WaitForPendingFinalizers();
                    //    theWorkbook.Close(true, Missing.Value, Missing.Value);
                    //    excel.Quit();
                    //    Marshal.FinalReleaseComObject(theWorkbook);
                    //    Marshal.FinalReleaseComObject(excel);
                    //}
                    //catch { }
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
