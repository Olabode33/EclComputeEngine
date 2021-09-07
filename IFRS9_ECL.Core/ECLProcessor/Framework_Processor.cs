using IFRS9_ECL.Core.Calibration.Input;
using IFRS9_ECL.Core.ECLProcessor.Entities;
using IFRS9_ECL.Core.FrameworkComputation;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models.ECL_Result;
using IFRS9_ECL.Models.Raw;
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
    public class Framework_Processor
    {

        public bool ProcessFramework(FrameworkParameters input, List<Loanbook_Data> batchContracts, Guid eclId, EclType eclType)
        {
            var model = Path.Combine(input.BasePath, $"{AppSettings.new_}{input.ModelFileName}");

            var modelTemp = model.Replace(AppSettings.Drive, AppSettings.ECLServer5);


            if (!(new FileInfo(modelTemp).Directory.Exists))
                Directory.CreateDirectory(new FileInfo(modelTemp).Directory.FullName);

            var inputFile = JsonConvert.SerializeObject(input);
            var inputFilePath = Path.Combine(input.BasePath, AppSettings.ModelInputFileEto);
            File.WriteAllText(inputFilePath, inputFile);
            var inputFilePathTemp = inputFilePath.Replace(AppSettings.Drive, AppSettings.ECLServer5);
            File.WriteAllText(inputFilePathTemp, inputFile);

            model = model.Replace(AppSettings.new_, string.Empty);
            File.Copy(model, modelTemp, true);

            return true;
        }

        public bool ProcessFrameworkOverride(FrameworkParameters input, List<Loanbook_Data> batchContracts, Guid eclId, EclType eclType)
        {
            var overrideModel = Path.Combine(input.BasePath, $"{AppSettings.new_}{AppSettings.override_}{input.ModelFileName}");

            var modelRemote = overrideModel.Replace(AppSettings.Drive, AppSettings.ECLServer5).Replace(AppSettings.new_, AppSettings.complete_).Replace(AppSettings.override_, string.Empty);

            File.Copy(modelRemote, overrideModel, true);

            var overrides = new ProcessECL_LGD(eclId, eclType).GetOverrideData(4);

            WriteOverrideData(overrides, overrideModel);

            var modelOverrideRemote = overrideModel.Replace(AppSettings.Drive, AppSettings.ECLServer5);
            File.Copy(overrideModel, modelOverrideRemote, true);

            return true;
        }



        public bool WriteOverrideData(List<EclOverrides> overrides, string fullName)
        {
            try
            {
                var basePath = new FileInfo(fullName).DirectoryName;
                string txtLocation = fullName;
               
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
                    theWorkbook.Unprotect(AppSettings.SheetPassword);
                    Worksheet overrideSheet = theWorkbook.Sheets[6];
                    //startSheet.Unprotect(AppSettings.SheetPassword);

                    for(int i=1; i<= AppSettings.BatchSize + 20; i++)
                    {
                        if (overrideSheet.Cells[i, 2] == null)
                            continue;

                        try
                        {
                            var contractNo = Convert.ToString(overrideSheet.Cells[i, 2].Value);
                            if (contractNo != null)
                            {
                                var contractObject = overrides.FirstOrDefault(o => o.ContractId == contractNo);
                                if (contractObject != null)
                                {
                                    overrideSheet.Cells[i, 20] = contractObject.Stage;
                                    overrideSheet.Cells[i, 21] = contractObject.TtrYears;
                                    overrideSheet.Cells[i, 22] = contractObject.FSV_Cash;
                                    overrideSheet.Cells[i, 23] = contractObject.FSV_CommercialProperty;
                                    overrideSheet.Cells[i, 24] = contractObject.FSV_Debenture;
                                    overrideSheet.Cells[i, 25] = contractObject.FSV_Inventory;
                                    overrideSheet.Cells[i, 26] = contractObject.FSV_PlantAndEquipment;
                                    overrideSheet.Cells[i, 27] = contractObject.FSV_Receivables;
                                    overrideSheet.Cells[i, 28] = contractObject.FSV_ResidentialProperty;
                                    overrideSheet.Cells[i, 29] = contractObject.FSV_Shares;
                                    overrideSheet.Cells[i, 30] = contractObject.FSV_Vehicle;
                                    overrideSheet.Cells[i, 32] = contractObject.OverlaysPercentage;
                                    overrideSheet.Cells[i, 34] = contractObject.Reason;
                                }
                            }
                        }
                        catch { }
                    }

                    theWorkbook.Close(true);
                    excel.Quit();

                }
                catch (Exception ex)
                {
                    Log4Net.Log.Error(ex);
                    theWorkbook.Close(true);
                    excel.Quit();
                    return false;

                }
                return true;
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                Log4Net.Log.Info(DateTime.Now);

                return false;
            }
        }





        public bool TransferFrameworkInputFiles(string file, string inputType)
        {
            var eadFile = file.Replace(inputType, AppSettings.EAD);
            var lgdFile = file.Replace(inputType, AppSettings.LGD);
            var pdFile = file.Replace(inputType, AppSettings.PD);

            var eadFileFramewotk = eadFile.Replace(AppSettings.ECLServer1, AppSettings.ECLServer5);
            var lgdFileFramewotk = lgdFile.Replace(AppSettings.ECLServer1, AppSettings.ECLServer5);
            var pdFileFramewotk = pdFile.Replace(AppSettings.ECLServer1, AppSettings.ECLServer5);

            if(!File.Exists(eadFileFramewotk))
             File.Copy(eadFile, eadFileFramewotk, true);

            if (!File.Exists(lgdFileFramewotk))
                File.Copy(lgdFile, lgdFileFramewotk, true);

            if (!File.Exists(pdFileFramewotk))
                File.Copy(pdFile, pdFileFramewotk, true);

            File.WriteAllText(Path.Combine(new FileInfo(file.Replace(AppSettings.ECLServer1, AppSettings.ECLServer5)).Directory.FullName, AppSettings.TransferComplete), string.Empty);


            return true;
        }

        public bool ExecuteFrameworkMacro(string fullName)
        {
            try
            {
                var basePath = new FileInfo(fullName).DirectoryName;
                var inputFileText = File.ReadAllText(Path.Combine(basePath, AppSettings.ModelInputFileEto));
                var input = JsonConvert.DeserializeObject<FrameworkParameters>(inputFileText);
                string txtLocation = fullName;
                var csvFileName = fullName.Replace(AppSettings.processing_, AppSettings.complete_).Replace(AppSettings.xlsb, AppSettings.xcsv);

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
                    theWorkbook.Unprotect(AppSettings.SheetPassword);
                    Worksheet startSheet = theWorkbook.Sheets[3];
                    //startSheet.Unprotect(AppSettings.SheetPassword);


                    var reportPath = Path.Combine(basePath, AppSettings.Report);
                    if (!File.Exists(reportPath))
                        Directory.CreateDirectory(reportPath);

                    if(!fullName.ToLower().Contains("override"))
                    {
                        startSheet.Cells[6, 4] = input.ReportDate.ToString("dd MMMM yyyy");

                        startSheet.Cells[9, 4] = reportPath;

                        startSheet.Cells[10, 4] = $"{AppSettings.complete_}{input.PdFileName}";
                        startSheet.Cells[11, 4] = Path.Combine(basePath, $"{AppSettings.complete_}{input.PdFileName}");

                        startSheet.Cells[12, 4] = $"{AppSettings.complete_}{input.LgdFile}";
                        startSheet.Cells[13, 4] = Path.Combine(basePath, $"{AppSettings.complete_}{input.LgdFile}");

                        startSheet.Cells[14, 4] = $"{AppSettings.complete_}{input.EadFileName}";
                        startSheet.Cells[15, 4] = Path.Combine(basePath, $"{AppSettings.complete_}{input.EadFileName}");

                    }

                    if(txtLocation.Contains(AppSettings.override_))
                    {
                        excel.Run("override_formulas");
                        excel.Calculate();
                    }
                    else
                    {
                        excel.Run("calculate_ecl");
                    }
                    

                    theWorkbook.Close(true);
                    excel.Quit();


                }
                catch (Exception ex)
                {
                    Log4Net.Log.Error(ex);
                    theWorkbook.Close(true);
                    excel.Quit();
                    return false;

                }
                excel = new Application();
                excel.DisplayAlerts = false;
                theWorkbook = excel.Workbooks.Open(txtLocation,
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
                    Worksheet worksheet = theWorkbook.Sheets[7];
                    worksheet.Unprotect(AppSettings.SheetPassword);
                    worksheet.Columns.NumberFormat = "0.00";
                    worksheet.SaveAs(csvFileName, XlFileFormat.xlCSV);
                    File.Copy(csvFileName, csvFileName.Replace(AppSettings.xcsv, AppSettings.csv), true);

                    //theWorkbook.Save();
                    // Garbage collecting
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    theWorkbook.Close(true, Missing.Value, Missing.Value);
                    excel.Quit();
                    Marshal.FinalReleaseComObject(theWorkbook);
                    Marshal.FinalReleaseComObject(excel);

                }
                catch (Exception ex)
                {
                    Log4Net.Log.Error(ex);

                    // Garbage collecting
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    theWorkbook.Close(true, Missing.Value, Missing.Value);
                    excel.Quit();
                    Marshal.FinalReleaseComObject(theWorkbook);
                    Marshal.FinalReleaseComObject(excel);

                    return false;

                }
                var csvLines = File.ReadAllLines(csvFileName.Replace(AppSettings.xcsv, AppSettings.csv));

                var frameworkResult = new List<ResultDetailDataMore>();

                for (int i = 9; i <= AppSettings.BatchSize + 20; i++)
                {

                    
                    try
                    {
                        var csvCells = csvLines[i].Split(',');
                        int bc = 0;



                        if (string.IsNullOrEmpty(csvCells[bc + 2]))
                            continue;

                        var c = new ResultDetailDataMore();
                        c.ContractNo = Convert.ToString(csvCells[bc + 2].Replace(".00", string.Empty));
                        c.AccountNo = csvCells[bc + 3] != null ? csvCells[bc + 3].Replace(".00", string.Empty) : "";
                        c.CustomerNo = csvCells[bc + 4] != null ? csvCells[bc + 4].Replace(".00", string.Empty) : "";
                        c.Segment = csvCells[bc + 5] != null ? csvCells[bc + 5].Replace(".00", string.Empty) : "";
                        c.ProductType = csvCells[bc + 6] != null ? csvCells[bc + 6].Replace(".00", string.Empty) : "";
                        c.Sector = csvCells[bc + 7] != null ? csvCells[bc + 7].Replace(".00", string.Empty) : "";
                        c.Stage = csvCells[bc + 8] != null ? Convert.ToInt32(csvCells[bc + 8].Replace(".00", string.Empty)) : 0;
                        c.Outstanding_Balance = csvCells[bc + 9] != null ? Convert.ToDouble(StringHelper.RemoveSpecialCharacters(csvCells[bc + 9])) : 0.0;
                        c.ECL_Best_Estimate = csvCells[bc + 10] != null ? Convert.ToDouble(StringHelper.RemoveSpecialCharacters(csvCells[bc + 10])) : 0.0;
                        c.ECL_Optimistic = csvCells[bc + 11] != null ? Convert.ToDouble(StringHelper.RemoveSpecialCharacters(csvCells[bc + 11])) : 0.0;
                        c.ECL_Downturn = csvCells[bc + 12] != null ? Convert.ToDouble(StringHelper.RemoveSpecialCharacters(csvCells[bc + 12])) : 0.0;
                        c.Impairment_ModelOutput = csvCells[bc + 13] != null ? Convert.ToDouble(StringHelper.RemoveSpecialCharacters(csvCells[bc + 13])) : 0.0;
                        c.Overrides_Stage = csvCells[bc + 14] != null ? Convert.ToInt32(StringHelper.RemoveSpecialCharacters(csvCells[bc + 14].Replace(".00", string.Empty))) : 0;
                        try { c.Overrides_TTR_Years = csvCells[bc + 15] != null ? Convert.ToInt32(StringHelper.RemoveSpecialCharacters(csvCells[bc + 15])) : 0.0; } catch { c.Overrides_TTR_Years = 0.0; }
                        try { c.Overrides_FSV = csvCells[bc + 16] != null ? Convert.ToDouble(StringHelper.RemoveSpecialCharacters(csvCells[bc + 16])) : 0.0; } catch { c.Overrides_FSV = 0.0; }
                        try { c.Overrides_Overlay = csvCells[bc + 17] != null ? Convert.ToDouble(StringHelper.RemoveSpecialCharacters(csvCells[bc + 17])) : 0.0; } catch { c.Overrides_Overlay = 0.0; }
                        c.Overrides_ECL_Best_Estimate = csvCells[bc + 18] != null ? Convert.ToDouble(StringHelper.RemoveSpecialCharacters(csvCells[bc + 18])) : 0.0;
                        c.Overrides_ECL_Optimistic = csvCells[bc + 19] != null ? Convert.ToDouble(StringHelper.RemoveSpecialCharacters(csvCells[bc + 19])) : 0.0;
                        c.Overrides_ECL_Downturn = csvCells[bc + 20] != null ? Convert.ToDouble(StringHelper.RemoveSpecialCharacters(csvCells[bc + 20])) : 0.0;
                        c.Overrides_Impairment_Manual = csvCells[bc + 21] != null ? Convert.ToDouble(StringHelper.RemoveSpecialCharacters(csvCells[bc + 21])) : 0.0;
                        c.OriginalOutstandingBalance = 0.0;


                        frameworkResult.Add(c);
                    }
                    catch (Exception ex)
                    {
                        Log4Net.Log.Error(ex);
                    }

                }

                return FileSystemStorage<ResultDetailDataMore>.WriteCsvData(fullName.Replace(AppSettings.processing_, AppSettings.complete_).Replace(AppSettings.xlsb, AppSettings.csv), frameworkResult);


            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                Log4Net.Log.Info(DateTime.Now);

                return false;
            }
        }

        public bool ProcessFrameworkResult(string filename, FrameworkParameters input)
        {
            try
            {
                
                var frameworkResult = FileSystemStorage<ResultDetailDataMore>.ReadCsvData(filename);

                var c = new ResultDetailDataMore();


                Type myObjOriginalType = c.GetType();
                PropertyInfo[] myProps = myObjOriginalType.GetProperties();

                var dt = new System.Data.DataTable();
                for (int i = 0; i < myProps.Length; i++)
                {
                    dt.Columns.Add(myProps[i].Name, myProps[i].PropertyType);
                }

                dt.Columns.Add($"{input.EclType}EclId", typeof(Guid));


                //var lstContractNoLog = new List<string>();

                foreach (var _d in frameworkResult)
                {
                    //if (lstContractNoLog.Any(o => o == _d.ContractNo))
                    //    continue;

                    //lstContractNoLog.Add(_d.ContractNo);
                    try { c.OriginalOutstandingBalance = 0.0; } catch { }
                    //(double)batchContracts.FirstOrDefault(o => o.ContractNo == c.ContractNo).OutstandingBalanceLCY; } catch { }

                    var Id = Guid.NewGuid();
                    dt.Rows.Add(new object[]
                        {
                            Id, _d.Stage, _d.Outstanding_Balance, _d.ECL_Best_Estimate, _d.ECL_Optimistic, _d.ECL_Downturn, _d.Impairment_ModelOutput,
                            _d.Overrides_Stage, _d.Overrides_TTR_Years, _d.Overrides_FSV, _d.Overrides_Overlay, _d.Overrides_ECL_Best_Estimate, _d.Overrides_ECL_Optimistic, _d.Overrides_ECL_Downturn, _d.Overrides_Impairment_Manual, _d.ContractNo, _d.AccountNo,
                            _d.CustomerNo, _d.Segment, _d.ProductType, _d.Sector, _d.OriginalOutstandingBalance, input.EclId
                        });
                }

                //Save to Report Detail
                var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.EclFramworkReportDetail(input.EclType));

                return true;
            }
            catch(Exception ex)
            {
                Log4Net.Log.Error(ex);
                return false;
            }

        }


    }
}
