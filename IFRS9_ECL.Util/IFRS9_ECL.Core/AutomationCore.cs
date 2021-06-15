using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Core.ECLProcessor.Entities;
using IFRS9_ECL.Core.FrameworkComputation;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.ECL_Result;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core
{
    public class AutomationCore
    {
        int serviceId = 0;
        public bool ProcessRunTask(int serviceId)
        {
            this.serviceId = serviceId;
            ProcessECLRunTask();

            return true;
        }

        private bool ProcessECLRunTask()
        {
            var eclRegister = new EclRegister { EclType = -1 };
            try
            {
                //return true;
                var retailEcls = Queries.EclsRegister(EclType.Retail.ToString());
                var wholesaleEcls = Queries.EclsRegister(EclType.Wholesale.ToString());
                var obeEcls = Queries.EclsRegister(EclType.Obe.ToString());

                var dtR = DataAccess.i.GetData(retailEcls);

                if (dtR.Rows.Count > 0)
                {
                    var itm = DataAccess.i.ParseDataToObject(new EclRegister(), dtR.Rows[0]);
                    itm.EclType = 0;
                    itm.eclType = EclType.Retail;
                    eclRegister = itm;

                }

                if (eclRegister.EclType == -1)
                {
                    var dtW = DataAccess.i.GetData(wholesaleEcls);

                    if (dtW.Rows.Count > 0)
                    {
                        var itm = DataAccess.i.ParseDataToObject(new EclRegister(), dtW.Rows[0]);
                        itm.EclType = 1;
                        itm.eclType = EclType.Wholesale;
                        eclRegister = itm;
                    }
                }
                if (eclRegister.EclType == -1)
                {
                    var dtO = DataAccess.i.GetData(obeEcls);
                    if (dtO.Rows.Count > 0)
                    {
                        var itm = DataAccess.i.ParseDataToObject(new EclRegister(), dtO.Rows[0]);
                        itm.EclType = 2;
                        itm.eclType = EclType.Obe;
                        eclRegister = itm;
                    }

                }

                if (eclRegister.EclType == -1)
                {
                    Log4Net.Log.Info("Found No Pending ECL");
                    return true;
                }

                var qry = Queries.UpdateGuidTableServiceId($"{eclRegister.eclType.ToString()}Ecls", this.serviceId, eclRegister.Id);
                var resp = DataAccess.i.ExecuteQuery(qry);

                if (resp == 0)
                {
                    Log4Net.Log.Info($"Another service has picked ECL with ID {eclRegister.Id} of Type [{eclRegister.eclType.ToString()}].");
                    return true;
                }

                qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 6, "");
                DataAccess.i.ExecuteQuery(qry);
                var eclType = eclRegister.eclType;
                Log4Net.Log.Info($"Found ECL with ID {eclRegister.Id} of Type [{eclType.ToString()}]. Running will commence if it has not been picked by another Job");

                LifetimeEadWorkings lifetimeEadWorkings = new LifetimeEadWorkings(eclRegister.Id, eclType);
                var loanbook_data = lifetimeEadWorkings.GetLoanBookDataRaw();
                var payment_Schedules = lifetimeEadWorkings.GetPaymentScheduleRaw();

                var batchCount = Math.Ceiling(Convert.ToDecimal(loanbook_data.Count) / 1000.0M);

                for (int i = 0; i < batchCount; i++)
                {
                    GenerateLoanBookFile(i, loanbook_data.Skip(i * 1000).Take(1000).ToList(), payment_Schedules, eclRegister.OrganizationUnitId, eclRegister.Id);
                }


                var eadParam = BuildEADParameter(eclRegister.Id, eclRegister.ReportingDate, eclType);
                var lgdParam = BuildLGDParameter(eclRegister.Id, eclRegister.ReportingDate, eclType);
                var pdParam = BuildPDParameter(eclRegister.Id, eclRegister.ReportingDate, eclType);
                var frameworkParam = BuildFrameworkParameter(eclRegister.Id, eclRegister.ReportingDate, eclType);


                var counter = 0;
                var taskList = new List<Task>();
                var tskStatusLst = new List<TaskStatus> { TaskStatus.RanToCompletion, TaskStatus.Faulted };

                //while (counter < batchCount)
                //{

                //    var batchContracts = loanbook_data.Skip(counter * 1000).Take(1000).ToList();
                //    var task1 = Task.Run(() =>
                //    {
                //        RunECL(batchContracts, counter, eclRegister.OrganizationUnitId, eclRegister.Id, eclType, eadParam, lgdParam, pdParam, frameworkParam);
                //    });
                //    taskList.Add(task1);

                //    while (taskList.Where(o => !tskStatusLst.Contains(o.Status)).Count() >= 5)
                //    {
                //        //do nothing
                //    }
                //    counter = counter + 1;
                //}

                for (int i = 0; i < batchCount; i++)
                {
                    var batchContracts = loanbook_data.Skip(i * 1000).Take(1000).ToList();
                    RunECL(batchContracts, i, eclRegister.OrganizationUnitId, eclRegister.Id, eclType, eadParam, lgdParam, pdParam, frameworkParam);
                }

                qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 5, "");
                DataAccess.i.ExecuteQuery(qry);


                Log4Net.Log.Info($"Start Time {DateTime.Now}");

            }
            catch (Exception ex)
            {

            }


            return true;
        }

        private FrameworkParameters BuildFrameworkParameter(Guid id, DateTime reportingDate, EclType eclType)
        {
            return new FrameworkParameters
            {  
                BasePath= AppSettings.ECLBasePath,
                 ReportDate= reportingDate
            };
        }

        private PDParameters BuildPDParameter(Guid eclId, DateTime reportingDate, EclType eclType)
        {
            var bt_ead = new CalibrationInput_EAD_Behavioural_Terms_Processor();
            var bt_ead_data = bt_ead.GetBehaviouralData(eclId, eclType);

            var pdCali = new CalibrationInput_PD_CR_RD_Processor().GetPDRedefaultFactorCureRate(eclId, eclType);
            double readjustmentFactor = pdCali[0];

            var obj= new PDParameters
            {
                 BasePath= AppSettings.ECLBasePath,
                Expired = bt_ead_data.Expired,
                NonExpired = bt_ead_data.NonExpired,
                ReportDate = reportingDate,
                 SandPMapping= "Best Fit",
                 RedefaultAdjustmentFactor= readjustmentFactor
            };


            obj.NonExpired = 19;
            obj.Expired = 26;
            obj.RedefaultAdjustmentFactor = 1;
            return obj;
        }

        private LGDParameters BuildLGDParameter(Guid eclId, DateTime reportingDate, EclType eclType)
        {
            var bt_ead = new CalibrationInput_EAD_Behavioural_Terms_Processor();
            var bt_ead_data = bt_ead.GetBehaviouralData(eclId, eclType);
            var obj= new LGDParameters
            {
                 BasePath= AppSettings.ECLBasePath,
                 Expired= bt_ead_data.Expired,
                 NonExpired= bt_ead_data.NonExpired,
                 ReportDate=reportingDate
            };


            obj.NonExpired = 19;
            obj.Expired = 26;

            return obj;
        }

        private EADParameters BuildEADParameter(Guid eclId, DateTime reportingDate, EclType eclType)
        {
            var bt_ead = new CalibrationInput_EAD_Behavioural_Terms_Processor();
            var bt_ead_data = bt_ead.GetBehaviouralData(eclId, eclType);

            var eclTsk = new ECLTasks(eclId, eclType);

            var exchangeRate = eclTsk._eclEadInputAssumption.Where(o => o.Key.StartsWith("ExchangeRate")).ToList();

            var er=new List<ExchangeRate>();
            foreach (var _er in exchangeRate)
            {
                er.Add(new ExchangeRate { Currency=_er.InputName.ToUpper(), Value= Convert.ToDouble(_er.Value) });
            }

            var vir = new List<VariableInterestRate>();
            foreach (var _vir in eclTsk.ViR)
            {
                vir.Add(new VariableInterestRate {  VIR_Name = _vir.InputName.ToUpper(), Value = Convert.ToDouble(_vir.Value) });
            }

            var CCF_OBE = 1.0;
            try { CCF_OBE = Convert.ToDouble(eclTsk._eclEadInputAssumption.FirstOrDefault(o => o.Key == "ConversionFactorOBE").Value); } catch { }


            var PrePaymentFactor = 0.0;
            try { PrePaymentFactor = Convert.ToDouble(eclTsk._eclEadInputAssumption.FirstOrDefault(o => o.Key == "PrePaymentFactor)").Value); } catch { }

            var ccfData = new CalibrationInput_EAD_CCF_Summary_Processor().GetCCFData(eclId, eclType);

            var ccfOverall = ccfData.Overall_CCF ?? 0.0;

            var obj= new EADParameters
            {
                ExchangeRates = er,
                VariableInterestRates = vir,
                Expired = bt_ead_data.Expired,
                NonExpired = bt_ead_data.NonExpired,
                ReportDate = reportingDate,
                ConversionFactorObe = CCF_OBE,
                PrePaymentFactor = PrePaymentFactor,
                CCF_Commercial = ccfOverall,
                CCF_Consumer = ccfOverall,
                CCF_Corporate = ccfOverall,
                CCF_OBE = CCF_OBE,
                BasePath = AppSettings.ECLBasePath
            };

            obj.NonExpired = 19;
            obj.Expired = 26;
            obj.ConversionFactorObe = 0.29;
            obj.PrePaymentFactor = 0;
            obj.ExchangeRates = new List<ExchangeRate>();
            obj.ExchangeRates.Add(new ExchangeRate { Currency= "EUR", Value= 6.3613 });
            obj.ExchangeRates.Add(new ExchangeRate { Currency = "GBP", Value = 7.00375 });
            obj.ExchangeRates.Add(new ExchangeRate { Currency = "GHS", Value = 1 });
            obj.ExchangeRates.Add(new ExchangeRate { Currency = "USD", Value = 5.6674 });
            obj.ExchangeRates.Add(new ExchangeRate { Currency = "XOF", Value = 0.00969773921721742 });
            obj.ExchangeRates.Add(new ExchangeRate { Currency = "ZAR", Value = 0.326 });

            obj.VariableInterestRates = new List<VariableInterestRate>();
            obj.VariableInterestRates.Add(new VariableInterestRate {  VIR_Name = "EGH GHS BASE RATE", Value = 0.2595 });
            obj.VariableInterestRates.Add(new VariableInterestRate { VIR_Name = "EGH USD BASE RATE", Value = 0.326 });
            obj.VariableInterestRates.Add(new VariableInterestRate { VIR_Name = "GHANA REFERENCE RATE", Value = 0.1475 });

            obj.CCF_Commercial = 0.9997;
            obj.CCF_Consumer = 0.9997;
            obj.CCF_Corporate = 0.9997;
            obj.CCF_OBE = 0.29;

            return obj;


        }

        private void ExtractAndSaveResult(List<Loanbook_Data> batchContracts, string filePath, Guid eclId, EclType eclType)
        {
            var frameworkResult = new List<ResultDetailDataMore>();
            var c = new ResultDetailDataMore();

            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            //using (var package = new ExcelPackage(new FileInfo(filePath)))
            //{
            //    ExcelWorksheet worksheet = package.Workbook.Worksheets[6];//.FirstOrDefault();

            //    Log4Net.Log.Info("Read Result File");
            //    // get number of rows in the sheet
            //    int rows = worksheet.Dimension.Rows; 

            //    package.Workbook.CalcMode = ExcelCalcMode.Automatic;

            //    for (int i = 10; i <= rows; i++)
            //    {
            //        if (worksheet.Cells[i, 3].Value == null)
            //            continue;

            //        try
            //        {
            //            c.ContractNo = Convert.ToString(worksheet.Cells[i, 3].Value);
            //            c.AccountNo = worksheet.Cells[i, 4].Value != null ? Convert.ToString(worksheet.Cells[i, 4].Value) : "";
            //            c.CustomerNo = worksheet.Cells[i, 5].Value != null ? Convert.ToString(worksheet.Cells[i, 5].Value) : "";
            //            c.Segment = worksheet.Cells[i, 6].Value != null ? Convert.ToString(worksheet.Cells[i, 6].Value) : "";
            //            c.ProductType = worksheet.Cells[i, 7].Value != null ? Convert.ToString(worksheet.Cells[i, 7].Value) : "";
            //            c.Sector = worksheet.Cells[i, 8].Value != null ? Convert.ToString(worksheet.Cells[i, 8].Value) : "";
            //            c.Stage = worksheet.Cells[i, 9].Value != null ? (int)worksheet.Cells[i, 9].Value : 0;
            //            c.Outstanding_Balance = worksheet.Cells[i, 10].Value != null ? (double)worksheet.Cells[i, 10].Value : 0.0;
            //            c.ECL_Best_Estimate = worksheet.Cells[i, 11].Value != null ? (double)worksheet.Cells[i, 11].Value : 0.0;
            //            c.ECL_Optimistic = worksheet.Cells[i, 12].Value != null ? (double)worksheet.Cells[i, 12].Value : 0.0;
            //            c.ECL_Downturn = worksheet.Cells[i, 12].Value != null ? (double)worksheet.Cells[i, 13].Value : 0.0;
            //            c.Impairment_ModelOutput = worksheet.Cells[i, 14].Value != null ? (double)worksheet.Cells[i, 14].Value : 0.0;
            //            c.Overrides_Stage = worksheet.Cells[i, 15].Value != null ? (int)worksheet.Cells[i, 15].Value : 0;
            //            c.Overrides_TTR_Years = worksheet.Cells[i, 16].Value != null ? (double)worksheet.Cells[i, 16].Value : 0.0;
            //            c.Overrides_FSV = worksheet.Cells[i, 17].Value != null ? (double)worksheet.Cells[i, 16].Value : 0.0;
            //            c.Overrides_Overlay = worksheet.Cells[i, 18].Value != null ? (double)worksheet.Cells[i, 18].Value : 0.0;
            //            c.Overrides_ECL_Best_Estimate = worksheet.Cells[i, 19].Value != null ? (double)worksheet.Cells[i, 18].Value : 0.0;
            //            c.Overrides_ECL_Optimistic = worksheet.Cells[i, 20].Value != null ? (double)worksheet.Cells[i, 20].Value : 0.0;
            //            c.Overrides_ECL_Downturn = worksheet.Cells[i, 21].Value != null ? (double)worksheet.Cells[i, 21].Value : 0.0;
            //            c.Overrides_Impairment_Manual = worksheet.Cells[i, 22].Value != null ? (double)worksheet.Cells[i, 22].Value : 0.0;

            //            try { c.OriginalOutstandingBalance = (double)batchContracts.FirstOrDefault(o => o.ContractNo == c.ContractNo).OutstandingBalanceLCY; } catch { }

            //            frameworkResult.Add(c);
            //        }
            //        catch(Exception ex)
            //        {

            //        }

            //    }
            //}

            
            string txtLocation = Path.GetFullPath(filePath);

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
                Worksheet worksheet = theWorkbook.Sheets[7];
                worksheet.Unprotect(AppSettings.SheetPassword);

                var rows = worksheet.Rows;

                
                for (int i = 10; i <= 1020; i++)
                {
                    int bc = 1;

                    if (worksheet.Cells[i, bc + 2].Value == null)
                        continue;

                    try
                    {
                        c = new ResultDetailDataMore();
                        c.ContractNo = Convert.ToString(worksheet.Cells[i, bc+2].Value);
                        c.AccountNo = worksheet.Cells[i, bc+ 3].Value != null ? Convert.ToString(worksheet.Cells[i, bc+ 3].Value) : "";
                        c.CustomerNo = worksheet.Cells[i, bc+ 4].Value != null ? Convert.ToString(worksheet.Cells[i, bc+ 4].Value) : "";
                        c.Segment = worksheet.Cells[i, bc+ 5].Value != null ? Convert.ToString(worksheet.Cells[i, bc+ 5].Value) : "";
                        c.ProductType = worksheet.Cells[i, bc+ 6].Value != null ? Convert.ToString(worksheet.Cells[i, bc+ 6].Value) : "";
                        c.Sector = worksheet.Cells[i, bc+ 7].Value != null ? Convert.ToString(worksheet.Cells[i, bc+ 7].Value) : "";
                        c.Stage = worksheet.Cells[i, bc+ 8].Value != null ? Convert.ToInt32(worksheet.Cells[i, bc+ 8].Value) : 0;
                        c.Outstanding_Balance = worksheet.Cells[i, bc+ 9].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc+ 9].Value) : 0.0;
                        c.ECL_Best_Estimate = worksheet.Cells[i, bc+ 10].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc+ 10].Value) : 0.0;
                        c.ECL_Optimistic = worksheet.Cells[i, bc+ 11].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc+ 11].Value) : 0.0;
                        c.ECL_Downturn = worksheet.Cells[i, bc+ 12].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc+ 12].Value) : 0.0;
                        c.Impairment_ModelOutput = worksheet.Cells[i, bc+ 13].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc+ 13].Value) : 0.0;
                        c.Overrides_Stage = worksheet.Cells[i, bc+ 14].Value != null ? Convert.ToInt32(worksheet.Cells[i, bc+ 14].Value) : 0;
                        try { c.Overrides_TTR_Years = worksheet.Cells[i, bc + 15].Value != null ? Convert.ToInt32(worksheet.Cells[i, bc + 15].Value) : 0.0; } catch { c.Overrides_TTR_Years = 0.0; }
                        try { c.Overrides_FSV = worksheet.Cells[i, bc + 16].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 16].Value) : 0.0; } catch { c.Overrides_FSV = 0.0; }
                        try { c.Overrides_Overlay = worksheet.Cells[i, bc + 17].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc + 17].Value) : 0.0; } catch { c.Overrides_Overlay = 0.0; }
                        c.Overrides_ECL_Best_Estimate = worksheet.Cells[i, bc+ 18].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc+ 18].Value) : 0.0;
                        c.Overrides_ECL_Optimistic = worksheet.Cells[i, bc+ 19].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc+ 19].Value) : 0.0;
                        c.Overrides_ECL_Downturn = worksheet.Cells[i, bc+ 20].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc+ 20].Value) : 0.0;
                        c.Overrides_Impairment_Manual = worksheet.Cells[i, bc+ 21].Value != null ? Convert.ToDouble(worksheet.Cells[i, bc+ 21].Value) : 0.0;

                        try { c.OriginalOutstandingBalance = (double)batchContracts.FirstOrDefault(o => o.ContractNo == c.ContractNo).OutstandingBalanceLCY; } catch { }

                        if(!c.ContractNo.Contains(AppSettings.DumbContract))
                            frameworkResult.Add(c);
                    }
                    catch (Exception ex)
                    {

                    }

                }

                theWorkbook.Save();

                theWorkbook.Close(true);

            }
            catch (Exception ex)
            {
                theWorkbook.Close(true);
                excel.Quit();
                Console.WriteLine(ex);
            }
            finally
            {
                excel.Quit();
            }

            //return true;



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

        }

        private void RunECL(List<Loanbook_Data> batchContracts, int batchId, long affiliateId, Guid eclId, EclType eclType, EADParameters eadParam, LGDParameters lgdParam, PDParameters pdParam, FrameworkParameters frameworkParam)
        {
            var affiliatePath = Path.Combine(AppSettings.ECLBasePath, affiliateId.ToString());
            var eclPath = Path.Combine(affiliatePath, eclId.ToString());
            var batchPath = Path.Combine(eclPath, batchId.ToString());

            var eadTemplate = Path.Combine(affiliatePath, "EADTemplate.xlsb");
            var lgdTemplate = Path.Combine(affiliatePath, "LGDTemplate.xlsb");
            var pdTemplate = Path.Combine(affiliatePath, "PDTemplate.xlsb");
            var fraemworkTemplate = Path.Combine(affiliatePath, "FrameworkTemplate.xlsb");

            var eadFile = Path.Combine(batchPath, "EAD.xlsb");
            var lgdFile = Path.Combine(batchPath, "LGD.xlsb");
            var pdFile = Path.Combine(batchPath, "PD.xlsb");
            var fraemworkFile = Path.Combine(batchPath, "Framework.xlsb");
            

            File.Copy(eadTemplate, eadFile);
            File.Copy(lgdTemplate, lgdFile);
            File.Copy(pdTemplate, pdFile);
            File.Copy(fraemworkTemplate, fraemworkFile);

            eadParam.ModelFileName = eadFile;
            eadParam.BasePath = batchPath;
            eadParam.LoanBookFileName = $"{batchId}_{eclId}_EAD_LoanBook.xlsx";
            eadParam.PaymentScheduleFileName = $"{batchId}_{eclId}_PaymentSchedule.xlsx";

            lgdParam.ModelFileName = lgdFile;
            lgdParam.BasePath = batchPath;
            lgdParam.LoanBookFileName = $"{batchId}_{eclId}_LGD_LoanBook.xlsx";

            pdParam.ModelFileName = pdFile;
            pdParam.BasePath = batchPath;
            pdParam.LoanBookFileName = $"{batchId}_{eclId}_PD_LoanBook.xlsx";

            frameworkParam.ModelFileName = fraemworkFile;
            frameworkParam.BasePath = batchPath;
            frameworkParam.EadFileName = "EAD.xlsb";
            frameworkParam.LgdFile = "LGD.xlsb";
            frameworkParam.PdFileName = "PD.xlsb";
            var reportPath = Path.Combine(batchPath, "Report");
            
            if(!Directory.Exists(reportPath))
            {
                Directory.CreateDirectory(reportPath);
            }
            frameworkParam.ReportFolderName= reportPath;
            

            var taskList = new List<Task>();
            var tskStatusLst = new List<TaskStatus> { TaskStatus.RanToCompletion, TaskStatus.Faulted };

            var task1 = Task.Run(() =>
            {
                var eadProcessor = new EAD_Processor().ProcessEAD(eadParam);
            });
            taskList.Add(task1);

            var task2 = Task.Run(() =>
            {
                var lgdProcessor = new LGD_Processor().ProcessLGD(lgdParam);
            });
            taskList.Add(task2);

            var task3 = Task.Run(() =>
            {
                var pdProcessor = new PD_Processor().ProcessPD(pdParam);
            });
            taskList.Add(task3);

            while (0 < 1)
            {
                if (taskList.All(o => tskStatusLst.Contains(o.Status)))
                {
                    foreach (var itm in taskList)
                    {
                        if (itm.Status != TaskStatus.RanToCompletion)
                        {
                            Log4Net.Log.Info("Did not run to Completion");
                            Log4Net.Log.Error(itm.Exception);
                        }
                        else
                        {
                            Log4Net.Log.Info("Ran to Completion");
                        }
                    }
                    break;
                }
                //Do Nothing
            }

            var fwProcessor = new Framework_Processor().ProcessFramework(frameworkParam);

            var fraemworkResultFile = Path.Combine(batchPath, "Framework.xlsb");
            ExtractAndSaveResult(batchContracts, fraemworkResultFile, eclId,eclType);

        }

        private void GenerateLoanBookFile(int batchId, List<Loanbook_Data> loanbook, List<TempPaymentSchedule> payment_Schedules, long affiliateId, Guid eclId)
        {
            var contractNos = loanbook.Select(o => o.ContractNo).ToList();
            payment_Schedules = payment_Schedules.Where(o => contractNos.Contains(o.ContractRefNo)).ToList();

            var includeDummyLoanBook = payment_Schedules.Count>0;

            if(payment_Schedules.Count==0)
            {
                payment_Schedules.Add(new TempPaymentSchedule { Amount=0, ContractRefNo="DummyContract", Component="amortise", Frequency="M", NoOfSchedules=1, StartDate=DateTime.Now });
            }

            var affiliatePath = Path.Combine(AppSettings.ECLBasePath, affiliateId.ToString());
            var eclPath = Path.Combine(affiliatePath, eclId.ToString());
            var batchPath = Path.Combine(eclPath, batchId.ToString());

            if(Directory.Exists(batchPath))
            {
                Directory.Delete(batchPath, true);
            }
                
            
                Directory.CreateDirectory(batchPath);
            

            var loanBookTemplatePath = Path.Combine(AppSettings.ECLBasePath, "LoanBookTemplate.xlsx");
            var paymentScheduleTemplatePath = Path.Combine(AppSettings.ECLBasePath, "PaymentScheduleTemplate.xlsx");



            var eadloanbookPath = Path.Combine(batchPath, $"{batchId}_{eclId}_EAD_LoanBook.xlsx");
            File.Copy(loanBookTemplatePath, eadloanbookPath);
            WriteLoanBook(loanbook, loanbook.FirstOrDefault(), eadloanbookPath, includeDummyLoanBook);

            var lgdloanbookPath = Path.Combine(batchPath, $"{batchId}_{eclId}_LGD_LoanBook.xlsx");
            File.Copy(eadloanbookPath, lgdloanbookPath);
            

            var pdloanbookPath = Path.Combine(batchPath, $"{batchId}_{eclId}_PD_LoanBook.xlsx");
            File.Copy(eadloanbookPath, pdloanbookPath);
            

            var paymentSchedulePath = Path.Combine(batchPath, $"{batchId}_{eclId}_PaymentSchedule.xlsx");
            File.Copy(paymentScheduleTemplatePath, paymentSchedulePath);
            WritePaymentSchedule(payment_Schedules, paymentSchedulePath);

        }

        private void WritePaymentSchedule(List<TempPaymentSchedule> payment_Schedules, string paymentSchedulePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(paymentSchedulePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                // get number of rows in the sheet
                int rows = worksheet.Dimension.Rows; // 10

                package.Workbook.CalcMode = ExcelCalcMode.Automatic;

                for (int i = 0; i < payment_Schedules.Count; i++)
                {
                    var p = payment_Schedules[i];
                    worksheet.Cells[i+3, 1+ 1].Value = p.ContractRefNo;
                    worksheet.Cells[i+3, 1+ 2].Value = p.StartDate;
                    worksheet.Cells[i+3, 1+ 3].Value = p.Component;
                    worksheet.Cells[i+3, 1+ 4].Value = p.NoOfSchedules;
                    worksheet.Cells[i+3, 1+ 5].Value = p.Frequency;
                    worksheet.Cells[i+3, 1+ 6].Value = p.Amount;
                }

                package.Save();
            }

        }

        private void WriteLoanBook(List<Loanbook_Data> loanbook, Loanbook_Data loanbook_Data, string loanbookPath, bool includeDummyLoanBook)
        {
            if(includeDummyLoanBook)
            {
                var maxContractDate = loanbook.Max(o => o.ContractEndDate);

                loanbook_Data.ContractEndDate = maxContractDate.Value.AddYears(1);
                loanbook_Data.ContractNo = AppSettings.DumbContract;
                loanbook_Data.OutstandingBalanceLCY = 0;

                loanbook_Data.DebentureOMV = 0.0;
                loanbook_Data.DebentureFSV = 0.0;
                loanbook_Data.CashOMV = 0.0;
                loanbook_Data.CashFSV = 0.0;
                loanbook_Data.InventoryOMV = 0.0;
                loanbook_Data.InventoryFSV = 0.0;
                loanbook_Data.PlantEquipmentOMV = 0.0;
                loanbook_Data.PlantEquipmentFSV = 0.0;
                loanbook_Data.ResidentialPropertyOMV = 0.0;
                loanbook_Data.ResidentialPropertyFSV = 0.0;
                loanbook_Data.CommercialPropertyOMV = 0.0;
                loanbook_Data.CommercialProperty = 0.0;
                loanbook_Data.ReceivablesOMV = 0.0;
                loanbook_Data.ReceivablesFSV = 0.0;
                loanbook_Data.SharesOMV = 0.0;
                loanbook_Data.SharesFSV = 0.0;
                loanbook_Data.VehicleOMV = 0.0;
                loanbook_Data.VehicleFSV = 0.0;
                loanbook_Data.CureRate = 0.0;
                loanbook_Data.GuaranteeIndicator = false;
                loanbook_Data.GuaranteeValue = 0.0;
                loanbook_Data.GuaranteeLevel = 0.0;



                loanbook.Add(loanbook_Data);
            }
            

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(loanbookPath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                // get number of rows in the sheet
                int rows = worksheet.Dimension.Rows; // 10

                package.Workbook.CalcMode = ExcelCalcMode.Automatic;

                for (int i = 0; i < loanbook.Count; i++)
                {
                    var p = loanbook[i];
                    worksheet.Cells[i+4, 1+ 1].Value = p.CustomerNo;
                    worksheet.Cells[i+4, 1+ 2].Value = p.AccountNo;
                    worksheet.Cells[i+4, 1+ 3].Value = p.ContractNo;
                    worksheet.Cells[i+4, 1+ 4].Value = p.CustomerName;
                    worksheet.Cells[i+4, 1+ 5].Value = p.SnapshotDate;
                    worksheet.Cells[i+4, 1+ 6].Value = p.Segment;
                    worksheet.Cells[i+4, 1+ 7].Value = p.Sector;
                    worksheet.Cells[i+4, 1+ 8].Value = p.Currency;
                    worksheet.Cells[i+4, 1+ 9].Value = p.ProductType;
                    worksheet.Cells[i+4, 1+ 10].Value = p.ProductMapping;
                    worksheet.Cells[i+4, 1+ 11].Value = p.SpecialisedLending;
                    worksheet.Cells[i+4, 1+ 12].Value = p.RatingModel;
                    worksheet.Cells[i+4, 1+ 13].Value = p.OriginalRating;
                    worksheet.Cells[i+4, 1+ 14].Value = p.CurrentRating;
                    worksheet.Cells[i+4, 1+ 15].Value = p.LifetimePD;
                    worksheet.Cells[i+4, 1+ 16].Value = p.Month12PD;
                    worksheet.Cells[i+4, 1+ 17].Value = p.DaysPastDue;
                    worksheet.Cells[i+4, 1+ 18].Value = p.WatchlistIndicator ? "1" : "";
                    worksheet.Cells[i+4, 1+ 19].Value = p.Classification;
                    worksheet.Cells[i+4, 1+ 20].Value = p.ImpairedDate;
                    worksheet.Cells[i+4, 1+ 21].Value = p.DefaultDate;
                    worksheet.Cells[i+4, 1+ 22].Value = p.CreditLimit;
                    worksheet.Cells[i+4, 1+ 23].Value = p.OriginalBalanceLCY;
                    worksheet.Cells[i+4, 1+ 24].Value = p.OutstandingBalanceLCY;
                    worksheet.Cells[i+4, 1+ 25].Value = p.OutstandingBalanceACY;
                    worksheet.Cells[i+4, 1+ 26].Value = p.ContractStartDate;
                    worksheet.Cells[i+4, 1+ 27].Value = p.ContractEndDate;
                    worksheet.Cells[i+4, 1+ 28].Value = p.RestructureIndicator ? "1" : "";
                    worksheet.Cells[i+4, 1+ 29].Value = p.RestructureRisk;
                    worksheet.Cells[i+4, 1+ 30].Value = p.RestructureType;
                    worksheet.Cells[i+4, 1+ 31].Value = p.RestructureStartDate;
                    worksheet.Cells[i+4, 1+ 32].Value = p.RestructureEndDate;
                    worksheet.Cells[i+4, 1+ 33].Value = p.PrincipalPaymentTermsOrigination;
                    worksheet.Cells[i+4, 1+ 34].Value = p.PPTOPeriod;
                    worksheet.Cells[i+4, 1+ 35].Value = p.InterestPaymentTermsOrigination;
                    worksheet.Cells[i+4, 1+ 36].Value = p.IPTOPeriod;
                    worksheet.Cells[i+4, 1+ 37].Value = p.PrincipalPaymentStructure;
                    worksheet.Cells[i+4, 1+ 38].Value = p.InterestPaymentStructure;
                    worksheet.Cells[i+4, 1+ 39].Value = p.InterestRateType;
                    worksheet.Cells[i+4, 1+ 40].Value = p.BaseRate;
                    worksheet.Cells[i+4, 1+ 41].Value = p.OriginationContractualInterestRate;
                    worksheet.Cells[i+4, 1+ 42].Value = p.IntroductoryPeriod;
                    worksheet.Cells[i+4, 1+ 43].Value = p.PostIPContractualInterestRate;
                    worksheet.Cells[i+4, 1+ 44].Value = p.CurrentContractualInterestRate;
                    worksheet.Cells[i+4, 1+ 45].Value = p.EIR;

                    worksheet.Cells[i+4, 1+ 46].Value = p.DebentureOMV;
                    worksheet.Cells[i+4, 1+ 47].Value = p.DebentureFSV;

                    worksheet.Cells[i+4, 1+ 48].Value = p.CashOMV;
                    worksheet.Cells[i+4, 1+ 49].Value = p.CashFSV;

                    worksheet.Cells[i+4, 1+ 50].Value = p.InventoryOMV;
                    worksheet.Cells[i+4, 1+ 51].Value = p.InventoryFSV;

                    worksheet.Cells[i+4, 1+ 52].Value = p.PlantEquipmentOMV;
                    worksheet.Cells[i+4, 1+ 53].Value = p.PlantEquipmentFSV;

                    worksheet.Cells[i+4, 1+ 54].Value = p.ResidentialPropertyOMV;
                    worksheet.Cells[i+4, 1+ 55].Value = p.ResidentialPropertyFSV;

                    worksheet.Cells[i+4, 1+ 56].Value = p.CommercialPropertyOMV;
                    worksheet.Cells[i+4, 1+ 57].Value = p.CommercialProperty;

                    worksheet.Cells[i+4, 1+ 58].Value = p.ReceivablesOMV;
                    worksheet.Cells[i+4, 1+ 59].Value = p.ReceivablesFSV;

                    worksheet.Cells[i+4, 1+ 60].Value = p.SharesOMV;
                    worksheet.Cells[i+4, 1+ 61].Value = p.SharesFSV;

                    worksheet.Cells[i+4, 1+ 62].Value = p.VehicleOMV;
                    worksheet.Cells[i+4, 1+ 63].Value = p.VehicleFSV;

                    worksheet.Cells[i+4, 1+ 64].Value = p.CureRate;
                    worksheet.Cells[i+4, 1+ 65].Value = p.GuaranteeIndicator?"1":"";
                    worksheet.Cells[i+4, 1+ 66].Value = p.GuarantorPD;
                    worksheet.Cells[i+4, 1+ 67].Value = p.GuarantorLGD;
                    worksheet.Cells[i+4, 1+ 68].Value = p.GuaranteeValue;
                    worksheet.Cells[i+4, 1+ 69].Value = p.GuaranteeLevel;

                }

                package.Save();
            }
        }
    }
}


























