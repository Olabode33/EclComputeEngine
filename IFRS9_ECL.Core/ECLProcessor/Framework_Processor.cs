using IFRS9_ECL.Core.Calibration.Input;
using IFRS9_ECL.Core.ECLProcessor.Entities;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration
{
    public class Framework_Processor
    {

        public bool ProcessFramework(FrameworkParameters input, List<Loanbook_Data> batchContracts, Guid eclId, EclType eclType)
        {


            var ead = Path.Combine(input.BasePath, input.EadFileName);
            var lgd = Path.Combine(input.BasePath, input.LgdFile);
            var pd = Path.Combine(input.BasePath, input.PdFileName);
            var model = Path.Combine(input.BasePath, $"{AppSettings.new_}{input.ModelFileName}");

            var eadTemp = ead.Replace(AppSettings.Drive, AppSettings.ECLServer5);
            var lgdTemp = lgd.Replace(AppSettings.Drive, AppSettings.ECLServer5);
            var pdTemp = pd.Replace(AppSettings.Drive, AppSettings.ECLServer5);
            var modelTemp = model.Replace(AppSettings.Drive, AppSettings.ECLServer5);



            File.Copy(model, modelTemp, true);
            File.Copy(model, modelTemp, true);
            File.Copy(model, modelTemp, true);

            model = model.Replace(AppSettings.new_, string.Empty);
            File.Copy(model, modelTemp, true);

            var inputFile = JsonConvert.SerializeObject(input);
            var inputFilePath = Path.Combine(input.BasePath, AppSettings.ModelInputFileEto);
            var inputFilePathTemp = inputFilePath.Replace(AppSettings.Drive, AppSettings.ECLServer5);

            File.WriteAllText(inputFilePathTemp, inputFile);

            while (!File.Exists(modelTemp.Replace(AppSettings.new_, AppSettings.complete_)) || !File.Exists(modelTemp.Replace(AppSettings.new_, AppSettings.complete_)))
            {
                Thread.Sleep(AppSettings.ServerCallWaitTime);
            }

            if (!File.Exists(modelTemp.Replace(AppSettings.new_, AppSettings.complete_)))
            {
                var resultFileCSV = model.Replace(AppSettings.xlsb, AppSettings.csv);
                File.Copy(modelTemp.Replace(AppSettings.new_, AppSettings.complete_).Replace(AppSettings.xlsb, AppSettings.csv), resultFileCSV, true);

                var frameworkResult= FileSystemStorage<ResultDetailDataMore>.ReadCsvData(resultFileCSV);

                var c = new ResultDetailDataMore();


                Type myObjOriginalType = c.GetType();
                PropertyInfo[] myProps = myObjOriginalType.GetProperties();

                var dt = new System.Data.DataTable();
                for (int i = 0; i < myProps.Length; i++)
                {
                    dt.Columns.Add(myProps[i].Name, myProps[i].PropertyType);
                }

                dt.Columns.Add($"{eclType}EclId", typeof(Guid));


                //var lstContractNoLog = new List<string>();

                foreach (var _d in frameworkResult)
                {
                    //if (lstContractNoLog.Any(o => o == _d.ContractNo))
                    //    continue;

                    //lstContractNoLog.Add(_d.ContractNo);
                    try { c.OriginalOutstandingBalance = (double)batchContracts.FirstOrDefault(o => o.ContractNo == c.ContractNo).OutstandingBalanceLCY; } catch { }

                    var Id = Guid.NewGuid();
                    dt.Rows.Add(new object[]
                        {
                            Id, _d.Stage, _d.Outstanding_Balance, _d.ECL_Best_Estimate, _d.ECL_Optimistic, _d.ECL_Downturn, _d.Impairment_ModelOutput,
                            _d.Overrides_Stage, _d.Overrides_TTR_Years, _d.Overrides_FSV, _d.Overrides_Overlay, _d.Overrides_ECL_Best_Estimate, _d.Overrides_ECL_Optimistic, _d.Overrides_ECL_Downturn, _d.Overrides_Impairment_Manual, _d.ContractNo, _d.AccountNo,
                            _d.CustomerNo, _d.Segment, _d.ProductType, _d.Sector, _d.OriginalOutstandingBalance, eclId
                        });
                }

                //Save to Report Detail
                var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.EclFramworkReportDetail(eclType));


                return true;

            }
            if (!File.Exists(modelTemp.Replace(AppSettings.new_, AppSettings.error_)))
            {
                File.Copy(modelTemp.Replace(AppSettings.new_, AppSettings.error_), model, true);
                //Update DB with failed
            }

            return true;
        }

        public bool ExecuteFrameworkMacro(string fullName)
        {
            var basePath = new FileInfo(fullName).DirectoryName;
            var inputFileText = File.ReadAllText(Path.Combine(basePath, AppSettings.ModelInputFileEto));
            var input = JsonConvert.DeserializeObject<FrameworkParameters>(inputFileText);
            string txtLocation = fullName;

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
                Worksheet startSheet = theWorkbook.Sheets[3];
                startSheet.Unprotect(AppSettings.SheetPassword);

                Directory.CreateDirectory(Path.Combine(input.BasePath, input.ReportFolderName));

                startSheet.Cells[6, 4] = input.ReportDate.ToString("dd MMMM yyyy");

                startSheet.Cells[9, 4] = Path.Combine(input.BasePath, input.ReportFolderName);

                startSheet.Cells[10, 4] = input.PdFileName;
                startSheet.Cells[11, 4] = Path.Combine(input.BasePath, input.PdFileName);

                startSheet.Cells[12, 4] = input.LgdFile;
                startSheet.Cells[13, 4] = Path.Combine(input.BasePath, input.LgdFile);

                startSheet.Cells[14, 4] = input.EadFileName;
                startSheet.Cells[15, 4] = Path.Combine(input.BasePath, input.EadFileName);
                
                excel.Run("calculate_ecl");


                Worksheet worksheet = theWorkbook.Sheets[7];
                worksheet.Unprotect(AppSettings.SheetPassword);

                var rows = worksheet.Rows;
                
                var frameworkResult = new List<ResultDetailDataMore>();

                for (int i = 10; i <= AppSettings.BatchSize + 20; i++)
                {
                    int bc = 1;

                    if (worksheet.Cells[i, bc + 2].Value == null)
                        continue;

                    try
                    {
                        var c = new ResultDetailDataMore();
                        c.ContractNo = Convert.ToString(worksheet.Cells[i, bc + 2].Value);
                        c.AccountNo = worksheet.Cells[i, bc + 3].Value != null ? Convert.ToString(worksheet.Cells[i, bc + 3].Value) : "";
                        c.CustomerNo = worksheet.Cells[i, bc + 4].Value != null ? Convert.ToString(worksheet.Cells[i, bc + 4].Value) : "";
                        c.Segment = worksheet.Cells[i, bc + 5].Value != null ? Convert.ToString(worksheet.Cells[i, bc + 5].Value) : "";
                        c.ProductType = worksheet.Cells[i, bc + 6].Value != null ? Convert.ToString(worksheet.Cells[i, bc + 6].Value) : "";
                        c.Sector = worksheet.Cells[i, bc + 7].Value != null ? Convert.ToString(worksheet.Cells[i, bc + 7].Value) : "";
                        c.Stage = worksheet.Cells[i, bc + 8].Value != null ? Convert.ToInt32(worksheet.Cells[i, bc + 8].Value) : 0;
                        c.Outstanding_Balance = worksheet.Cells[i, bc + 9].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 9].Value) : 0.0;
                        c.ECL_Best_Estimate = worksheet.Cells[i, bc + 10].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 10].Value) : 0.0;
                        c.ECL_Optimistic = worksheet.Cells[i, bc + 11].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 11].Value) : 0.0;
                        c.ECL_Downturn = worksheet.Cells[i, bc + 12].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 12].Value) : 0.0;
                        c.Impairment_ModelOutput = worksheet.Cells[i, bc + 13].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 13].Value) : 0.0;
                        c.Overrides_Stage = worksheet.Cells[i, bc + 14].Value != null ? Convert.ToInt32(worksheet.Cells[i, bc + 14].Value) : 0;
                        try { c.Overrides_TTR_Years = worksheet.Cells[i, bc + 15].Value != null ? Convert.ToInt32(worksheet.Cells[i, bc + 15].Value) : 0.0; } catch { c.Overrides_TTR_Years = 0.0; }
                        try { c.Overrides_FSV = worksheet.Cells[i, bc + 16].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 16].Value) : 0.0; } catch { c.Overrides_FSV = 0.0; }
                        try { c.Overrides_Overlay = worksheet.Cells[i, bc + 17].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 17].Value) : 0.0; } catch { c.Overrides_Overlay = 0.0; }
                        c.Overrides_ECL_Best_Estimate = worksheet.Cells[i, bc + 18].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 18].Value) : 0.0;
                        c.Overrides_ECL_Optimistic = worksheet.Cells[i, bc + 19].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 19].Value) : 0.0;
                        c.Overrides_ECL_Downturn = worksheet.Cells[i, bc + 20].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 20].Value) : 0.0;
                        c.Overrides_Impairment_Manual = worksheet.Cells[i, bc + 21].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 21].Value) : 0.0;
                        c.OriginalOutstandingBalance = 0.0;


                        frameworkResult.Add(c);
                    }
                    catch (Exception ex)
                    {
                        Log4Net.Log.Error(ex);
                    }

                }




                theWorkbook.Save();

                theWorkbook.Close(true);
                excel.Quit();

                return FileSystemStorage<ResultDetailDataMore>.WriteCsvData(fullName.Replace(AppSettings.processing_, AppSettings.complete_).Replace(AppSettings.xlsb, AppSettings.csv),frameworkResult);

            }
            catch(Exception ex)
            {
                Log4Net.Log.Error(ex);
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
