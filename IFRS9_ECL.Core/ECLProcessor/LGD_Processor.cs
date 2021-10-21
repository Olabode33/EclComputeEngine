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

        public bool ProcessLGD(LGDParameters input, PDParameters pdParam)
        {
            input.CreditPd = pdParam.CreditPd;

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

                    Worksheet assumptionSheet = theWorkbook.Sheets[3];
                    var searchColIndex = 2;
                    var colThreeIndex = 3;
                    var colFourIndex = 4;
                    for (int i = 5; i < 67; i++)
                    {
                        var colTwoValue = (string)Convert.ToString(assumptionSheet.Cells[i, searchColIndex].Value);
                        if (colTwoValue.ToLower().StartsWith("commercial"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.Commercial_CureRate;
                            assumptionSheet.Cells[i, colFourIndex] = input.Commercial_RecoveryRate;
                        }
                        if (colTwoValue.ToLower().StartsWith("consumer"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.Consumer_CureRate;
                            assumptionSheet.Cells[i, colFourIndex] = input.Consumer_RecoveryRate;
                        }
                        if (colTwoValue.ToLower().StartsWith("corporate"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.Corporate_CureRate;
                            assumptionSheet.Cells[i, colFourIndex] = input.Corporate_RecoveryRate;
                        }
                        if (colTwoValue.StartsWith("1"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CreditPd.CrPD_CreditPd1;
                        }
                        if (colTwoValue.StartsWith("2"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CreditPd.CrPD_CreditPd2;
                        }
                        if (colTwoValue.StartsWith("3"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CreditPd.CrPD_CreditPd3;
                        }
                        if (colTwoValue.StartsWith("4"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CreditPd.CrPD_CreditPd4;
                        }
                        if (colTwoValue.StartsWith("5"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CreditPd.CrPD_CreditPd5;
                        }
                        if (colTwoValue.StartsWith("6"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CreditPd.CrPD_CreditPd6;
                        }
                        if (colTwoValue.StartsWith("7"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CreditPd.CrPD_CreditPd7;
                        }
                        if (colTwoValue.StartsWith("8"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CreditPd.CrPD_CreditPd8;
                        }
                        if (colTwoValue.StartsWith("9"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CreditPd.CrPD_CreditPd9;
                        }
                        if (colTwoValue.StartsWith("10"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CreditPd.CrPD_CreditPd10;
                        }
                        if (colTwoValue.ToUpper().StartsWith("CONS_STAGE_1"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CrPD_ConsStage1;
                        }
                        if (colTwoValue.ToLower().StartsWith("CONS_STAGE_2"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CrPD_ConsStage2;
                        }
                        if (colTwoValue.ToLower().StartsWith("COMM_STAGE_1"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CrPD_CommStage1;
                        }
                        if (colTwoValue.ToLower().StartsWith("COMM_STAGE_2"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.CrPD_CommStage2;
                        }

                        if (colTwoValue.ToLower().Contains("<"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.lgd_first.collateral_value;
                            assumptionSheet.Cells[i, colThreeIndex + 1] = input.lgd_first.debenture;
                            assumptionSheet.Cells[i, colThreeIndex + 2] = input.lgd_first.cash;
                            assumptionSheet.Cells[i, colThreeIndex + 3] = input.lgd_first.inventory;
                            assumptionSheet.Cells[i, colThreeIndex + 4] = input.lgd_first.plant_and_equipment;
                            assumptionSheet.Cells[i, colThreeIndex + 5] = input.lgd_first.residential_property;
                            assumptionSheet.Cells[i, colThreeIndex + 6] = input.lgd_first.commercial_property;
                            assumptionSheet.Cells[i, colThreeIndex + 7] = input.lgd_first.Receivables;
                            assumptionSheet.Cells[i, colThreeIndex + 8] = input.lgd_first.shares;
                            assumptionSheet.Cells[i, colThreeIndex + 9] = input.lgd_first.vehicle;
                        }

                        if (colTwoValue.ToLower().Contains(">"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.lgd_last.collateral_value;
                            assumptionSheet.Cells[i, colThreeIndex + 1] = input.lgd_last.debenture;
                            assumptionSheet.Cells[i, colThreeIndex + 2] = input.lgd_last.cash;
                            assumptionSheet.Cells[i, colThreeIndex + 3] = input.lgd_last.inventory;
                            assumptionSheet.Cells[i, colThreeIndex + 4] = input.lgd_last.plant_and_equipment;
                            assumptionSheet.Cells[i, colThreeIndex + 5] = input.lgd_last.residential_property;
                            assumptionSheet.Cells[i, colThreeIndex + 6] = input.lgd_last.commercial_property;
                            assumptionSheet.Cells[i, colThreeIndex + 7] = input.lgd_last.Receivables;
                            assumptionSheet.Cells[i, colThreeIndex + 8] = input.lgd_last.shares;
                            assumptionSheet.Cells[i, colThreeIndex + 9] = input.lgd_last.vehicle;
                        }
                        if (colTwoValue.ToLower().StartsWith("best"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.LGDCollateralGrowthAssumption_Debenture;
                            assumptionSheet.Cells[i, colThreeIndex + 1] = input.LGDCollateralGrowthAssumption_Cash;
                            assumptionSheet.Cells[i, colThreeIndex + 2] = input.LGDCollateralGrowthAssumption_Inventory;
                            assumptionSheet.Cells[i, colThreeIndex + 3] = input.LGDCollateralGrowthAssumption_PlantEquipment;
                            assumptionSheet.Cells[i, colThreeIndex + 4] = input.LGDCollateralGrowthAssumption_ResidentialProperty;
                            assumptionSheet.Cells[i, colThreeIndex + 5] = input.LGDCollateralGrowthAssumption_CommercialProperty;
                            assumptionSheet.Cells[i, colThreeIndex + 6] = input.LGDCollateralGrowthAssumption_Receivables;
                            assumptionSheet.Cells[i, colThreeIndex + 7] = input.LGDCollateralGrowthAssumption_Shares;
                            assumptionSheet.Cells[i, colThreeIndex + 8] = input.LGDCollateralGrowthAssumption_Vehicle;
                        }

                        if (colTwoValue.ToLower().StartsWith("debenture"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.TTR_Debenture;
                        }
                        if (colTwoValue.ToLower().StartsWith("cash"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.TTR_Cash;
                        }
                        if (colTwoValue.ToLower().StartsWith("plant"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.TTR_PlantEquipment;
                        }
                        if (colTwoValue.ToLower().StartsWith("resident"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.TTR_ResidentialProperty;
                        }
                        if (colTwoValue.ToLower().StartsWith("commercial_property"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.TTR_CommercialProperty;
                        }
                        if (colTwoValue.ToLower().StartsWith("receivables"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.TTR_Receivables;
                        }
                        if (colTwoValue.ToLower().StartsWith("shares"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.TTR_Shares;
                        }
                        if (colTwoValue.ToLower().StartsWith("vehicle"))
                        {
                            assumptionSheet.Cells[i, colThreeIndex] = input.TTR_Vehicle;
                        }
                    }


                    Worksheet haircutSheet = theWorkbook.Sheets[5];
                    var rowFour = 4;

                    haircutSheet.Cells[rowFour, 2] = input.Haircut.Debenture;
                    haircutSheet.Cells[rowFour, 3] = input.Haircut.Cash;
                    haircutSheet.Cells[rowFour, 4] = input.Haircut.Inventory;
                    haircutSheet.Cells[rowFour, 5] = input.Haircut.Plant_And_Equipment;
                    haircutSheet.Cells[rowFour, 6] = input.Haircut.Residential_Property;
                    haircutSheet.Cells[rowFour, 7] = input.Haircut.Commercial_Property;
                    haircutSheet.Cells[rowFour, 8] = input.Haircut.Receivables;
                    haircutSheet.Cells[rowFour, 9] = input.Haircut.Shares;
                    haircutSheet.Cells[rowFour, 10] = input.Haircut.Vehicle;


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
