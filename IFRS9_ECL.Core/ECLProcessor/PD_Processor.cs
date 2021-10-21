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
    public class PD_Processor
    {

        public bool ProcessPD(PDParameters input)
        {

            var loanbook = Path.Combine(input.BasePath, input.LoanBookFileName);
            var model = Path.Combine(input.BasePath, $"{AppSettings.new_}{input.ModelFileName}");

            var loanbookTemp = loanbook.Replace(AppSettings.Drive, AppSettings.ECLServer4);


            if (!(new FileInfo(loanbookTemp).Directory.Exists))
                Directory.CreateDirectory(new FileInfo(loanbookTemp).Directory.FullName);


            var inputFile = JsonConvert.SerializeObject(input);
            var inputFilePath = Path.Combine(input.BasePath, AppSettings.ModelInputFileEto);
            var inputFilePathTemp = inputFilePath.Replace(AppSettings.Drive, AppSettings.ECLServer4);
            File.WriteAllText(inputFilePathTemp, inputFile);

            File.Copy(loanbook, loanbookTemp, true);

            var modelTemp = model.Replace(AppSettings.Drive, AppSettings.ECLServer4);
            model = model.Replace(AppSettings.new_, string.Empty);

            File.Copy(model, modelTemp, true);

            File.WriteAllText(Path.Combine(new FileInfo(loanbookTemp).Directory.FullName, AppSettings.TransferComplete), string.Empty);


            return true;
        }

        public bool ExecutePDMacro(string filepath)
        {
            try
            {
                var basePath = new FileInfo(filepath).DirectoryName;
                var inputFileText = File.ReadAllText(Path.Combine(basePath, AppSettings.ModelInputFileEto));
                var input = JsonConvert.DeserializeObject<PDParameters>(inputFileText);
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
                    startSheet.Cells[12, 5] = input.RedefaultAdjustmentFactor;
                    startSheet.Cells[13, 5] = input.SandPMapping;

                    startSheet.Cells[15, 5] = input.NonExpired;
                    startSheet.Cells[16, 5] = input.Expired;

                    startSheet.Cells[20, 5] = Path.Combine(basePath, new FileInfo(input.LoanBookFileName).Name);

                    var fileName = new FileInfo(input.LoanBookFileName).Name;
                    startSheet.Cells[21, 5] = fileName;


                    Worksheet internalModelSheet = theWorkbook.Sheets[3];
                    var rowFour = 4;
                    var colThree = 3;
                    var colFour = 4;

                    internalModelSheet.Cells[rowFour, colThree] = input.CreditPd.CrPD_CreditPd1;
                    internalModelSheet.Cells[rowFour + 1, colThree] = input.CreditPd.CrPD_CreditPd2;
                    internalModelSheet.Cells[rowFour + 2, colThree] = input.CreditPd.CrPD_CreditPd3;
                    internalModelSheet.Cells[rowFour + 3, colThree] = input.CreditPd.CrPD_CreditPd4;
                    internalModelSheet.Cells[rowFour + 4, colThree] = input.CreditPd.CrPD_CreditPd5;
                    internalModelSheet.Cells[rowFour + 5, colThree] = input.CreditPd.CrPD_CreditPd6;
                    internalModelSheet.Cells[rowFour + 6, colThree] = input.CreditPd.CrPD_CreditPd7;
                    internalModelSheet.Cells[rowFour + 7, colThree] = input.CreditPd.CrPD_CreditPd8;
                    internalModelSheet.Cells[rowFour + 8, colThree] = input.CreditPd.CrPD_CreditPd9;
                    internalModelSheet.Cells[rowFour + 9, colThree] = input.CreditPd.CrPD_CreditPd10;

                    internalModelSheet.Cells[rowFour, colFour] = input.CreditPolicy.CrPD_CreditPolicy1;
                    internalModelSheet.Cells[rowFour + 1, colFour] = input.CreditPolicy.CrPD_CreditPolicy2;
                    internalModelSheet.Cells[rowFour + 2, colFour] = input.CreditPolicy.CrPD_CreditPolicy3;
                    internalModelSheet.Cells[rowFour + 3, colFour] = input.CreditPolicy.CrPD_CreditPolicy4;
                    internalModelSheet.Cells[rowFour + 4, colFour] = input.CreditPolicy.CrPD_CreditPolicy5;
                    internalModelSheet.Cells[rowFour + 5, colFour] = input.CreditPolicy.CrPD_CreditPolicy6;
                    internalModelSheet.Cells[rowFour + 6, colFour] = input.CreditPolicy.CrPD_CreditPolicy7;
                    internalModelSheet.Cells[rowFour + 7, colFour] = input.CreditPolicy.CrPD_CreditPolicy8;
                    internalModelSheet.Cells[rowFour + 8, colFour] = input.CreditPolicy.CrPD_CreditPolicy9;
                    internalModelSheet.Cells[rowFour + 9, colFour] = input.CreditPolicy.CrPD_CreditPolicy10;

                    var grouped=input.CummulativeDefaultRates.GroupBy(o => o.Rating);

                    var aaa = input.CummulativeDefaultRates.Where(o => o.Rating.Equals("AAA", StringComparison.InvariantCultureIgnoreCase)).OrderBy(n => n.Years).ToList();
                    var aa = input.CummulativeDefaultRates.Where(o => o.Rating.Equals("AA", StringComparison.InvariantCultureIgnoreCase)).OrderBy(n => n.Years).ToList();
                    var a = input.CummulativeDefaultRates.Where(o => o.Rating.Equals("A", StringComparison.InvariantCultureIgnoreCase)).OrderBy(n => n.Years).ToList();
                    var bbb = input.CummulativeDefaultRates.Where(o => o.Rating.Equals("BBB", StringComparison.InvariantCultureIgnoreCase)).OrderBy(n => n.Years).ToList();
                    var bb = input.CummulativeDefaultRates.Where(o => o.Rating.Equals("BB", StringComparison.InvariantCultureIgnoreCase)).OrderBy(n => n.Years).ToList();
                    var b = input.CummulativeDefaultRates.Where(o => o.Rating.Equals("B", StringComparison.InvariantCultureIgnoreCase)).OrderBy(n => n.Years).ToList();
                    var ccc = input.CummulativeDefaultRates.Where(o => o.Rating.Equals("CCC", StringComparison.InvariantCultureIgnoreCase)).OrderBy(n => n.Years).ToList();

                    var rowFive = 5;
                    var valueSeven = 7;
                    for (int i = 8; i <= 22; i++)
                    {
                        try { internalModelSheet.Cells[rowFive, i] = aaa.FirstOrDefault(o => o.Years == (i - valueSeven)).Value; } catch { };
                        try{internalModelSheet.Cells[rowFive+1, i] = aa.FirstOrDefault(o => o.Years == (i - valueSeven)).Value; } catch { };
                        try{internalModelSheet.Cells[rowFive+2, i] = a.FirstOrDefault(o => o.Years == (i - valueSeven)).Value; } catch { };
                        try{internalModelSheet.Cells[rowFive+3, i] = bbb.FirstOrDefault(o => o.Years == (i - valueSeven)).Value; } catch { };
                        try{internalModelSheet.Cells[rowFive+4, i] = bb.FirstOrDefault(o => o.Years == (i - valueSeven)).Value; } catch { };
                        try{internalModelSheet.Cells[rowFive+5, i] = b.FirstOrDefault(o => o.Years == (i - valueSeven)).Value; } catch { };
                        try{internalModelSheet.Cells[rowFive+6, i] = ccc.FirstOrDefault(o => o.Years == (i - valueSeven)).Value; } catch { };
                    }


                    excel.ScreenUpdating = false;


                    excel.Run("unhide_unprotect");
                    excel.Run("primary_condition_extractor");
                    excel.Run("centre_sheets");
                    excel.Run("hide_protect");

                    excel.Run("unhide_unprotect");
                    excel.Run("primary_condition_extractor");
                    excel.Run("centre_sheets");
                    excel.Run("hide_protect");

                    excel.Run("unhide_unprotect");
                    excel.Run("resize_pd_workbook");
                    excel.Run("centre_sheets");
                    excel.Run("hide_protect");

                    excel.ScreenUpdating = true;

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

                    theWorkbook.Close(true);
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
                    //excel.Quit();
                }
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                Log4Net.Log.Info(DateTime.Now);

                return false;
            }

        }

        //excel.SheetFollowHyperlink += Excel_SheetFollowHyperlink;

        //private void Excel_SheetFollowHyperlink(object Sh, Hyperlink Target)
        //{
        //    Target.
        //    throw new NotImplementedException();
        //}
    }
}
