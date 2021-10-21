using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Core.ECLProcessor.Entities;
using IFRS9_ECL.Core.FrameworkComputation;
using IFRS9_ECL.Core.PDComputation;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.ECL_Result;
using IFRS9_ECL.Models.Framework;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core
{
    public class AutomationCore
    {
        int serviceId = 0;
        public bool ProcessRunTask(int serviceId)
        {

            this.serviceId = serviceId;

            if (AppSettings.ServiceType==AppSettings.EAD)
            {
                SetProcessPriority();
                Process_EAD_ExcelModel();
            }
            if (AppSettings.ServiceType == AppSettings.LGD)
            {
                SetProcessPriority();
                Process_LGD_ExcelModel();
            }
            if (AppSettings.ServiceType == AppSettings.PD)
            {
                SetProcessPriority();
                Process_PD_ExcelModel();
            }
            if (AppSettings.ServiceType == AppSettings.Framework)
            {
                SetProcessPriority();
                Process_Framework_ExcelModel();
            }
            if(AppSettings.ServiceType == AppSettings.Main)
            {
                ProcessECLRunTask();
            }
            if (AppSettings.ServiceType == AppSettings.ResultUpload)
            {
                ProcessFrameworkResultTask();
                ProcessFrameworkOverrideResultTask();

            }


            return true;
        }

        private void SetProcessPriority()
        {
            var proocesses = Process.GetProcesses().Where(o => o.ProcessName == "EXCEL").ToList();
            foreach (var process in proocesses)
            {
                try
                {
                    Console.WriteLine(process.PriorityClass.ToString());
                    if (process.PriorityClass != ProcessPriorityClass.RealTime)
                    {
                        process.PriorityBoostEnabled = true;
                        process.PriorityClass = ProcessPriorityClass.RealTime;
                        Console.WriteLine($"Optimized - {process.Id}");
                    }
                }
                catch (Exception ex)
                {
                    Log4Net.Log.Error(ex);
                }
            }
        }


        private void Process_Framework_ExcelModel()
        {
            try
            {
                var basePath = AppSettings.ECLServer5;

                var di = new DirectoryInfo(basePath);

                var files = di.GetFiles("*", SearchOption.AllDirectories).OrderBy(n => n.Name).Where(o => o.Name.StartsWith(AppSettings.new_) && o.Name.EndsWith("Framework.xlsb")).ToList();

                Log4Net.Log.Info($"Found {files.Count} EAD file");
                foreach (var file in files)
                {
                    Log4Net.Log.Info($"Processing {file.FullName}");
                    try
                    {

                        if (file == null)
                        {
                            continue;
                        }

                        if (!File.Exists(Path.Combine(file.Directory.FullName, AppSettings.TransferComplete)))
                            continue;

                        //Process Framework
                        var processingFileName = file.FullName.Replace(AppSettings.new_, AppSettings.processing_);
                        try
                        {
                            File.Move(file.FullName, processingFileName);
                        }
                        catch (Exception ex)
                        {
                            Log4Net.Log.Info(file.FullName);
                            Log4Net.Log.Info("File has probably been moved by another service");
                            Log4Net.Log.Error(ex);
                            continue;
                        }

                        var tryCounter = 0;
                        var eadProcessor = false;
                        while (!eadProcessor && tryCounter <= 3)
                        {
                            eadProcessor = new Framework_Processor().ExecuteFrameworkMacro(processingFileName);
                            tryCounter = tryCounter + 1;
                        }
                        if (eadProcessor)
                        {
                            var completedProcessingFileName = processingFileName.Replace(AppSettings.processing_, AppSettings.complete_);
                            File.Move(processingFileName, completedProcessingFileName);

                            completedProcessingFileName = completedProcessingFileName.Replace(AppSettings.xlsb, AppSettings.csv);
                            //transfer file back to master server

                            File.Copy(completedProcessingFileName, completedProcessingFileName.Replace(AppSettings.ECLServer5, AppSettings.ECLServer1), true);
                            File.Copy(completedProcessingFileName.Replace(AppSettings.xlsb, AppSettings.csv), completedProcessingFileName.Replace(AppSettings.ECLServer5, AppSettings.ECLServer1).Replace(AppSettings.xlsb, AppSettings.csv), true);
                            try { File.Delete(processingFileName.Replace(AppSettings.ECLServer5, AppSettings.ECLServer1).Replace(AppSettings.processing_, string.Empty)); } catch { }
                            File.WriteAllText(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer5, AppSettings.ECLServer1)).Directory.FullName, AppSettings.FrameworkComputeComplete), string.Empty);
                        }
                        else
                        {
                            File.Move(processingFileName, processingFileName.Replace(AppSettings.processing_, AppSettings.error_));
                        }
                    }catch(Exception ex)
                    {
                        Log4Net.Log.Error(ex);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
            }
        }

        private void Process_PD_ExcelModel()
        {
            try
            {
                var basePath = AppSettings.ECLServer4;

                var di = new DirectoryInfo(basePath);

                var file = di.GetFiles("*", SearchOption.AllDirectories).OrderBy(n => n.Name).FirstOrDefault(o => o.Name.StartsWith(AppSettings.new_) && o.Name.EndsWith("PD.xlsb"));
                if (file == null)
                {
                    return;
                }
                if (!File.Exists(Path.Combine(file.Directory.FullName, AppSettings.TransferComplete)))
                    return;

                //Process PD
                var processingFileName = file.FullName.Replace(AppSettings.new_, AppSettings.processing_);
                try
                {
                    File.Move(file.FullName, processingFileName);
                }
                catch (Exception ex)
                {
                    Log4Net.Log.Info(file.FullName);
                    Log4Net.Log.Info("File has probably been moved by another service");
                    Log4Net.Log.Error(ex);
                    return;
                }


                var tryCounter = 0;
                var pdProcessor = false;
                while (!pdProcessor && tryCounter <= 3)
                {
                    pdProcessor = new PD_Processor().ExecutePDMacro(processingFileName);
                    tryCounter = tryCounter + 1;
                }
                if (pdProcessor)
                {
                    var completedProcessingFileName = processingFileName.Replace(AppSettings.processing_, AppSettings.complete_);
                    if (!File.Exists(completedProcessingFileName))
                        File.Move(processingFileName, completedProcessingFileName);

                    //transfer file back to master server

                    File.Copy(completedProcessingFileName, completedProcessingFileName.Replace(AppSettings.ECLServer4, AppSettings.ECLServer1), true);
                    try { File.Delete(completedProcessingFileName.Replace(AppSettings.ECLServer4, AppSettings.ECLServer1).Replace(AppSettings.complete_, string.Empty)); } catch { }
                    File.WriteAllText(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer4, AppSettings.ECLServer1)).Directory.FullName, AppSettings.PDComputeComplete), string.Empty);

                    // Move FrameworkFile
                    if (File.Exists(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer4, AppSettings.ECLServer1)).Directory.FullName, AppSettings.EADComputeComplete)) && File.Exists(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer4, AppSettings.ECLServer1)).Directory.FullName, AppSettings.LGDComputeComplete)))
                    {
                        new Framework_Processor().TransferFrameworkInputFiles(completedProcessingFileName.Replace(AppSettings.ECLServer4, AppSettings.ECLServer1), AppSettings.PD);
                    }
                }
                else
                {
                    File.Move(processingFileName, processingFileName.Replace(AppSettings.processing_, AppSettings.error_));
                }
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
            }

        }

        private void Process_LGD_ExcelModel()
        {
            try
            {
                var basePath = AppSettings.ECLServer3;

                var di = new DirectoryInfo(basePath);

                var file = di.GetFiles("*", SearchOption.AllDirectories).OrderBy(n => n.Name).FirstOrDefault(o => o.Name.StartsWith(AppSettings.new_) && o.Name.EndsWith("LGD.xlsb"));

                if (file == null)
                {
                    return;
                }
                if (!File.Exists(Path.Combine(file.Directory.FullName, AppSettings.TransferComplete)))
                    return;

                //Process LGD
                var processingFileName = file.FullName.Replace(AppSettings.new_, AppSettings.processing_);
                try
                {
                    File.Move(file.FullName, processingFileName);
                }
                catch (Exception ex)
                {
                    Log4Net.Log.Info(file.FullName);
                    Log4Net.Log.Info("File has probably been moved by another service");
                    Log4Net.Log.Error(ex);
                    return;
                }


                var tryCounter = 0;
                var lgdProcessor = false;
                while (!lgdProcessor && tryCounter <= 3)
                {
                    lgdProcessor = new LGD_Processor().ExecuteLGDMacro(processingFileName);
                    tryCounter = tryCounter + 1;
                }
                if (lgdProcessor)
                {
                    var completedProcessingFileName = processingFileName.Replace(AppSettings.processing_, AppSettings.complete_);
                    if (!File.Exists(completedProcessingFileName))
                        File.Move(processingFileName, completedProcessingFileName);

                    //transfer file back to master server

                    File.Copy(completedProcessingFileName, completedProcessingFileName.Replace(AppSettings.ECLServer3, AppSettings.ECLServer1), true);
                    try { File.Delete(completedProcessingFileName.Replace(AppSettings.ECLServer3, AppSettings.ECLServer1).Replace(AppSettings.complete_, string.Empty)); } catch { }
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
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
            }
        }

        private void Process_EAD_ExcelModel()
        {
            try
            {
                var basePath = AppSettings.ECLServer2;

                var di = new DirectoryInfo(basePath);

                var file = di.GetFiles("*", SearchOption.AllDirectories).OrderBy(n => n.Name).FirstOrDefault(o => o.Name.StartsWith(AppSettings.new_) && o.Name.EndsWith("EAD.xlsb"));

                if (file == null)
                {
                    return;
                }
                if (!File.Exists(Path.Combine(file.Directory.FullName, AppSettings.TransferComplete)))
                    return;

                //Process EAD
                var processingFileName = file.FullName.Replace(AppSettings.new_, AppSettings.processing_);
                try
                {
                    File.Move(file.FullName, processingFileName);
                }
                catch (Exception ex)
                {
                    Log4Net.Log.Info(file.FullName);
                    Log4Net.Log.Info("File has probably been moved by another service");
                    Log4Net.Log.Error(ex);
                    return;
                }

                var tryCounter = 0;
                var eadProcessor = false;
                while (!eadProcessor && tryCounter <= 3)
                {
                    eadProcessor = new EAD_Processor().ExecuteEADMacro(processingFileName);
                    tryCounter = tryCounter + 1;

                }
                if (eadProcessor)
                {
                    var completedProcessingFileName = processingFileName.Replace(AppSettings.processing_, AppSettings.complete_);
                    if (!File.Exists(completedProcessingFileName))
                        File.Move(processingFileName, completedProcessingFileName);

                    //transfer file back to master server

                    File.Copy(completedProcessingFileName, completedProcessingFileName.Replace(AppSettings.ECLServer2, AppSettings.ECLServer1), true);
                    try { File.Delete(completedProcessingFileName.Replace(AppSettings.ECLServer2, AppSettings.ECLServer1).Replace(AppSettings.complete_, string.Empty)); } catch { }
                    File.WriteAllText(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer2, AppSettings.ECLServer1)).Directory.FullName, AppSettings.EADComputeComplete), string.Empty);

                    // Move FrameworkFile
                    if (File.Exists(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer2, AppSettings.ECLServer1)).Directory.FullName, AppSettings.LGDComputeComplete)) && File.Exists(Path.Combine(new FileInfo(completedProcessingFileName.Replace(AppSettings.ECLServer2, AppSettings.ECLServer1)).Directory.FullName, AppSettings.PDComputeComplete)))
                    {
                        new Framework_Processor().TransferFrameworkInputFiles(completedProcessingFileName.Replace(AppSettings.Drive, AppSettings.ECLServer1), AppSettings.EAD);
                    }
                }
                else
                {
                    File.Move(processingFileName, processingFileName.Replace(AppSettings.processing_, AppSettings.error_));
                }
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
            }
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


                // Check if there is override. if there is. do not execute EAD, LGD, PD. just get override data and apply it to framework
                var overrideExist = false;
                if (eclRegister.Status == 12)
                {
                    overrideExist = CheckOverrideDataExist(eclRegister.Id, eclType);
                    if (!overrideExist)
                    {
                        qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 5, "No Override data found");
                        DataAccess.i.ExecuteQuery(qry);
                        Log4Net.Log.Info("No Override Data Found. Task concluded and exited");
                        return true;
                    }
                    else
                    {
                        qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 7, "Running Overrides");
                        DataAccess.i.ExecuteQuery(qry);
                        qry = Queries.DeleteDataOnWholesaleEclFramworkReportDetail(eclRegister.Id.ToString());
                        DataAccess.i.ExecuteQuery(qry);

                        Log4Net.Log.Info("Running Overrides");
                    }
                }



                LifetimeEadWorkings lifetimeEadWorkings = new LifetimeEadWorkings(eclRegister.Id, eclType);
                var loanbook_data = lifetimeEadWorkings.GetLoanBookDataRaw();
                var payment_Schedules = lifetimeEadWorkings.GetPaymentScheduleRaw();


                var groupedLoanBook = new List<List<Loanbook_Data>>();
                var batchCount = Math.Ceiling(loanbook_data.Count / AppSettings.BatchSizeDouble);


                for (int i = 0; i < batchCount; i++)
                {
                    var sub_items = loanbook_data.Skip(i * AppSettings.BatchSize).Take(AppSettings.BatchSize).ToList();
                    if (sub_items.Count > 0)
                        groupedLoanBook.Add(sub_items);
                }


                var allAccountsGrouped = false;

                try
                {
                    while (!allAccountsGrouped)
                    {
                        allAccountsGrouped = true;
                        for (int i = 1; i < groupedLoanBook.Count; i++)
                        {
                            var lstfromPrev = groupedLoanBook[i - 1].LastOrDefault();
                            var fstfromCurr = groupedLoanBook[i].FirstOrDefault();
                            if (lstfromPrev.CustomerNo == fstfromCurr.CustomerNo)
                            {
                                groupedLoanBook[i - 1].Add(fstfromCurr);
                                groupedLoanBook[i].RemoveAt(0);
                                allAccountsGrouped = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }

                var scenarioLifetimeLGD = new ScenarioLifetimeLGD(eclRegister.Id, eclType);
                var lgd_assumptions = scenarioLifetimeLGD.GetECLLgdAssumptions();

                var scenarioLifetimePD = new ScenarioLifetimePd(eclRegister.Id, eclType);
                var pd_assumptions = scenarioLifetimePD.GetECLPdAssumptions();

                var eadParam = BuildEADParameter(eclRegister.Id, eclRegister.ReportingDate, eclType);
                var lgdParam = BuildLGDParameter(eclRegister.Id, eclRegister.ReportingDate, eclType, lgd_assumptions);
                var pdParam = BuildPDParameter(eclRegister.Id, eclRegister.ReportingDate, eclType, pd_assumptions);
                var frameworkParam = BuildFrameworkParameter(eclRegister.Id, eclRegister.ReportingDate, eclType);

                if (!overrideExist)
                {

                    for (int i = 0; i < batchCount; i++)
                    {
                        GenerateLoanBookFile(i, groupedLoanBook[i], payment_Schedules, eclRegister.OrganizationUnitId, eclRegister.Id);
                    }

                    var counter = 0;
                    var taskList = new List<Task>();
                    var tskStatusLst = new List<TaskStatus> { TaskStatus.RanToCompletion, TaskStatus.Faulted };

                    var hasUpdatedPercent = false;

                    for (int i = 0; i < batchCount; i++)
                    {
                        if (i > (batchCount / 2.0) && !hasUpdatedPercent)
                        {
                            qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 7, "");
                            DataAccess.i.ExecuteQuery(qry);
                            hasUpdatedPercent = true;
                        }


                        var batchContracts = groupedLoanBook[i];
                        RunECL(batchContracts, i, eclRegister.OrganizationUnitId, eclRegister.Id, eclType, eadParam, lgdParam, pdParam, frameworkParam);
                    }
                }
                else
                {

                    var hasUpdatedPercent = false;

                    for (int i = 0; i < batchCount; i++)
                    {
                        if (i > (batchCount / 2.0) && !hasUpdatedPercent)
                        {
                            qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 7, "");
                            DataAccess.i.ExecuteQuery(qry);
                            hasUpdatedPercent = true;
                        }


                        var batchContracts = groupedLoanBook[i];
                        RunECLOverride(batchContracts, i, eclRegister.OrganizationUnitId, eclRegister.Id, eclType, frameworkParam);
                    }
                }


                Log4Net.Log.Info($"Start Time {DateTime.Now}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


            return true;
        }

        private FrameworkParameters BuildFrameworkParameter(Guid id, DateTime reportingDate, EclType eclType)
        {
            return new FrameworkParameters
            {
                EclId = id,
                EclType = eclType,
                BasePath = AppSettings.ECLBasePath,
                ReportDate = reportingDate
            };
        }

        public bool CheckOverrideDataExist(Guid eclId, EclType eclType)
        {
            var qry = Queries.CheckOverrideDataExist(eclId, eclType);
            var cnt = DataAccess.i.getCount(qry);
            return cnt > 0;
        }

        private PDParameters BuildPDParameter(Guid eclId, DateTime reportingDate, EclType eclType, List<EclAssumptions> assumptions)
        {
            var bt_ead = new CalibrationInput_EAD_Behavioural_Terms_Processor();
            var bt_ead_data = bt_ead.GetBehaviouralData(eclId, eclType);

            var pdCali = new CalibrationInput_PD_CR_RD_Processor().GetPDRedefaultFactorCureRate(eclId, eclType);
            double readjustmentFactor = pdCali[0];

            var obj = new PDParameters
            {
                BasePath = AppSettings.ECLBasePath,
                Expired = bt_ead_data.Expired,
                NonExpired = bt_ead_data.NonExpired,
                ReportDate = reportingDate,
                SandPMapping = "Best Fit",
                RedefaultAdjustmentFactor = readjustmentFactor,
                CommCons = new List<Calibration.Input.CalibrationResult_PD_CommsCons_MarginalDefaultRate>(),
                CreditPd = new CreditPdParam(),
                CreditPolicy = new CreditPolicyParam()
            };


            var CommConsResultQuery = Queries.Get_PD_Comm_Cons_Result(eclId);
            var dt = DataAccess.i.GetData(CommConsResultQuery);
            foreach(DataRow dr in dt.Rows)
            {
                obj.CommCons.Add(new Calibration.Input.CalibrationResult_PD_CommsCons_MarginalDefaultRate
                {
                     Comm1= dr["Comm1"]!=DBNull.Value?Convert.ToDouble(dr["Comm1"]):0,
                    Comm2 = dr["Comm2"] != DBNull.Value ? Convert.ToDouble(dr["Comm2"]) : 0,
                    Cons1 = dr["Cons1"] != DBNull.Value ? Convert.ToDouble(dr["Cons1"]) : 0,
                    Cons2 = dr["Cons2"] != DBNull.Value ? Convert.ToDouble(dr["Cons2"]) : 0,
                    Month = dr["Month"] != DBNull.Value ? Convert.ToInt32(dr["Month"]) : 0,
                });
            }

            var pd_Assumptions_CrPD = assumptions.Where(o => o.AssumptionGroup == 1).ToList();
            try { obj.CreditPd.CrPD_CreditPd1 = double.Parse(pd_Assumptions_CrPD.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPd1.ToLower())).Value); } catch { }
            try { obj.CreditPd.CrPD_CreditPd2 = double.Parse(pd_Assumptions_CrPD.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPd2.ToLower())).Value); } catch { }
            try { obj.CreditPd.CrPD_CreditPd3 = double.Parse(pd_Assumptions_CrPD.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPd3.ToLower())).Value); } catch { }
            try { obj.CreditPd.CrPD_CreditPd4 = double.Parse(pd_Assumptions_CrPD.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPd4.ToLower())).Value); } catch { }
            try { obj.CreditPd.CrPD_CreditPd5 = double.Parse(pd_Assumptions_CrPD.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPd5.ToLower())).Value); } catch { }
            try { obj.CreditPd.CrPD_CreditPd6 = double.Parse(pd_Assumptions_CrPD.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPd6.ToLower())).Value); } catch { }
            try { obj.CreditPd.CrPD_CreditPd7 = double.Parse(pd_Assumptions_CrPD.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPd7.ToLower())).Value); } catch { }
            try { obj.CreditPd.CrPD_CreditPd8 = double.Parse(pd_Assumptions_CrPD.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPd8.ToLower())).Value); } catch { }
            try { obj.CreditPd.CrPD_CreditPd9 = double.Parse(pd_Assumptions_CrPD.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPd9.ToLower())).Value); } catch { }
            try { obj.CreditPd.CrPD_CreditPd10 = double.Parse(pd_Assumptions_CrPD.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPd10.ToLower())).Value); } catch { }

            var pd_Assumptions_CreditPolicy = assumptions.Where(o => o.AssumptionGroup == 2).ToList();
            try { obj.CreditPolicy.CrPD_CreditPolicy1 = pd_Assumptions_CreditPolicy.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPdEtiPolicy1.ToLower())).Value; } catch { }
            try { obj.CreditPolicy.CrPD_CreditPolicy2 = pd_Assumptions_CreditPolicy.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPdEtiPolicy2.ToLower())).Value; } catch { }
            try { obj.CreditPolicy.CrPD_CreditPolicy3 = pd_Assumptions_CreditPolicy.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPdEtiPolicy3.ToLower())).Value; } catch { }
            try { obj.CreditPolicy.CrPD_CreditPolicy4 = pd_Assumptions_CreditPolicy.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPdEtiPolicy4.ToLower())).Value; } catch { }
            try { obj.CreditPolicy.CrPD_CreditPolicy5 = pd_Assumptions_CreditPolicy.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPdEtiPolicy5.ToLower())).Value; } catch { }
            try { obj.CreditPolicy.CrPD_CreditPolicy6 = pd_Assumptions_CreditPolicy.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPdEtiPolicy6.ToLower())).Value; } catch { }
            try { obj.CreditPolicy.CrPD_CreditPolicy7 = pd_Assumptions_CreditPolicy.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPdEtiPolicy7.ToLower())).Value; } catch { }
            try { obj.CreditPolicy.CrPD_CreditPolicy8 = pd_Assumptions_CreditPolicy.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPdEtiPolicy8.ToLower())).Value; } catch { }
            try { obj.CreditPolicy.CrPD_CreditPolicy9 = pd_Assumptions_CreditPolicy.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPdEtiPolicy9.ToLower())).Value; } catch { }
            try { obj.CreditPolicy.CrPD_CreditPolicy10 = pd_Assumptions_CreditPolicy.FirstOrDefault(o => o.Key.ToLower().Contains(CreditPd.CreditPdEtiPolicy10.ToLower())).Value; } catch { }


            var EclPdSnPCummulativeDefaultRateQuery = Queries.Get_EclPdSnPCummulativeDefaultRates(eclId);
            var SnPdt = DataAccess.i.GetData(EclPdSnPCummulativeDefaultRateQuery);

            foreach (DataRow dr in SnPdt.Rows)
            {
                obj.CummulativeDefaultRates.Add(new Calibration.Input.CalibrationResult_PD_CummulativeDefaultRate
                {
                    Key = dr["Key"] != DBNull.Value ? Convert.ToString(dr["Key"]) : "",
                    Rating = dr["Rating"] != DBNull.Value ? Convert.ToString(dr["Rating"]) : "",
                    Years = dr["Years"] != DBNull.Value ? Convert.ToInt32(dr["Years"]) : 0,
                    Value = dr["Value"] != DBNull.Value ? Convert.(dr["Value"]) : 0
                });
            }

            return obj;
        }

        private LGDParameters BuildLGDParameter(Guid eclId, DateTime reportingDate, EclType eclType, List<EclAssumptions> assumptions)
        {
            var bt_ead = new CalibrationInput_EAD_Behavioural_Terms_Processor();
            var bt_ead_data = bt_ead.GetBehaviouralData(eclId, eclType);

            var cureRateRedefaultFactor = new CalibrationInput_PD_CR_RD_Processor().GetPDRedefaultFactorCureRate(eclId, eclType);
            var unsecuredRecoveryRate = new CalibrationInput_LGD_RecoveryRate_Processor().GetLGDRecoveryRateData(eclId, eclType);

            
            var lgd_Assumptions_2_first = assumptions.Where(o => o.AssumptionGroup == 4).ToList();
            var lgd_Assumptions_2_last = assumptions.Where(o => o.AssumptionGroup == 3).ToList();

            var lgd_first = new LGD_Assumptions_CollateralType_TTR_Years();

            try { lgd_first.collateral_value = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Collateral)).Value); } catch { lgd_first.collateral_value = 0; }
            try { lgd_first.debenture = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Debenture)).Value); } catch { lgd_first.debenture = 0; }
            try { lgd_first.cash = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Cash)).Value); } catch { lgd_first.cash = 0; }
            try { lgd_first.commercial_property = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.CommercialProperty)).Value); } catch { lgd_first.commercial_property = 0; }
            try { lgd_first.Receivables = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Receivables)).Value); } catch { lgd_first.Receivables = 0; }
            try { lgd_first.inventory = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Inventory)).Value); } catch { lgd_first.inventory = 0; }
            try { lgd_first.plant_and_equipment = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.PlantEquipment)).Value); } catch { lgd_first.plant_and_equipment = 0; }
            try { lgd_first.residential_property = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.ResidentialProperty)).Value); } catch { lgd_first.residential_property = 0; }
            try { lgd_first.shares = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Shares)).Value); } catch { lgd_first.shares = 0; }
            try { lgd_first.vehicle = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Vehicle)).Value); } catch { lgd_first.vehicle = 0; }

            var lgd_last = new LGD_Assumptions_CollateralType_TTR_Years();

            try { lgd_last.collateral_value = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Collateral)).Value); } catch { lgd_first.collateral_value = 0; }
            try { lgd_last.debenture = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Debenture)).Value); } catch { lgd_first.debenture = 0; }
            try { lgd_last.cash = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Cash)).Value); } catch { lgd_first.cash = 0; }
            try { lgd_last.commercial_property = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.CommercialProperty)).Value); } catch { lgd_first.commercial_property = 0; }
            try { lgd_last.Receivables = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Receivables)).Value); } catch { lgd_first.Receivables = 0; }
            try { lgd_last.inventory = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Inventory)).Value); } catch { lgd_first.inventory = 0; }
            try { lgd_last.plant_and_equipment = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.PlantEquipment)).Value); } catch { lgd_first.plant_and_equipment = 0; }
            try { lgd_last.residential_property = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.ResidentialProperty)).Value); } catch { lgd_first.residential_property = 0; }
            try { lgd_last.shares = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Shares)).Value); } catch { lgd_first.shares = 0; }
            try { lgd_last.vehicle = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Vehicle)).Value); } catch { lgd_first.vehicle = 0; }


            var _scenarioLifetimeLGD = new ScenarioLifetimeLGD(eclId, eclType);
            


            var obj = new LGDParameters
            {
                BasePath = AppSettings.ECLBasePath,
                Expired = bt_ead_data.Expired,
                NonExpired = bt_ead_data.NonExpired,
                ReportDate = reportingDate,
                Commercial_CureRate = cureRateRedefaultFactor[2],
                Consumer_CureRate = cureRateRedefaultFactor[3],
                Corporate_CureRate = cureRateRedefaultFactor[1],
                Commercial_RecoveryRate = unsecuredRecoveryRate.Commercial_RecoveryRate,
                Consumer_RecoveryRate = unsecuredRecoveryRate.Consumer_RecoveryRate,
                Corporate_RecoveryRate = unsecuredRecoveryRate.Corporate_RecoveryRate,
                RedefaultFactor = cureRateRedefaultFactor[0],
                lgd_first= lgd_first, 
                lgd_last= lgd_last,

            };

            

            var lgd_Assumptions_collateral = assumptions.Where(o => o.AssumptionGroup == 5).ToList();
            try { obj.LGDCollateralGrowthAssumption_Debenture = double.Parse(lgd_Assumptions_collateral.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Debenture.ToLower())).Value); } catch { }
            try { obj.LGDCollateralGrowthAssumption_Cash = double.Parse(lgd_Assumptions_collateral.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Cash.ToLower())).Value); } catch { }
            try { obj.LGDCollateralGrowthAssumption_Inventory = double.Parse(lgd_Assumptions_collateral.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Inventory.ToLower())).Value); } catch { }
            try { obj.LGDCollateralGrowthAssumption_PlantEquipment = double.Parse(lgd_Assumptions_collateral.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.PlantEquipment.ToLower())).Value); } catch { }
            try { obj.LGDCollateralGrowthAssumption_ResidentialProperty = double.Parse(lgd_Assumptions_collateral.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.ResidentialProperty.ToLower())).Value); } catch { }
            try { obj.LGDCollateralGrowthAssumption_CommercialProperty = double.Parse(lgd_Assumptions_collateral.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.CommercialProperty.ToLower())).Value); } catch { }
            try { obj.LGDCollateralGrowthAssumption_Receivables = double.Parse(lgd_Assumptions_collateral.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Receivables.ToLower())).Value); } catch { }
            try { obj.LGDCollateralGrowthAssumption_Shares = double.Parse(lgd_Assumptions_collateral.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Shares.ToLower())).Value); } catch { }
            try { obj.LGDCollateralGrowthAssumption_Vehicle = double.Parse(lgd_Assumptions_collateral.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Vehicle.ToLower())).Value); } catch { }

            var lgd_Assumptions_ttr = assumptions.Where(o => o.AssumptionGroup == 8).ToList();
            try { obj.TTR_Debenture = double.Parse(lgd_Assumptions_ttr.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Debenture.ToLower())).Value); } catch { }
            try { obj.TTR_Cash = double.Parse(lgd_Assumptions_ttr.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Cash.ToLower())).Value); } catch { }
            try { obj.TTR_Inventory = double.Parse(lgd_Assumptions_ttr.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Inventory.ToLower())).Value); } catch { }
            try { obj.TTR_PlantEquipment = double.Parse(lgd_Assumptions_ttr.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.PlantEquipment.ToLower())).Value); } catch { }
            try { obj.TTR_ResidentialProperty = double.Parse(lgd_Assumptions_ttr.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.ResidentialProperty.ToLower())).Value); } catch { }
            try { obj.TTR_CommercialProperty = double.Parse(lgd_Assumptions_ttr.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.CommercialProperty.ToLower())).Value); } catch { }
            try { obj.TTR_Receivables = double.Parse(lgd_Assumptions_ttr.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Receivables.ToLower())).Value); } catch { }
            try { obj.TTR_Shares = double.Parse(lgd_Assumptions_ttr.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Shares.ToLower())).Value); } catch { }
            try { obj.TTR_Vehicle = double.Parse(lgd_Assumptions_ttr.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Vehicle.ToLower())).Value); } catch { }


            obj.Haircut = new CalibrationInput_LGD_Haricut_Processor().GetLGDHaircutSummaryData(eclId, eclType);



            return obj;
        }

        private EADParameters BuildEADParameter(Guid eclId, DateTime reportingDate, EclType eclType)
        {
            var bt_ead = new CalibrationInput_EAD_Behavioural_Terms_Processor();
            var bt_ead_data = bt_ead.GetBehaviouralData(eclId, eclType);

            var eclTsk = new ECLTasks(eclId, eclType);

            var exchangeRate = eclTsk._eclEadInputAssumption.Where(o => o.Key.StartsWith("ExchangeRate")).ToList();

            var er = new List<ExchangeRate>();
            foreach (var _er in exchangeRate)
            {
                er.Add(new ExchangeRate { Currency = _er.InputName.ToUpper(), Value = Convert.ToDouble(_er.Value) });
            }

            var vir = new List<VariableInterestRate>();
            foreach (var _vir in eclTsk.ViR)
            {
                vir.Add(new VariableInterestRate { VIR_Name = _vir.InputName.ToUpper(), Value = Convert.ToDouble(_vir.Value) });
            }

            var CCF_OBE = 1.0;
            try { CCF_OBE = Convert.ToDouble(eclTsk._eclEadInputAssumption.FirstOrDefault(o => o.Key == "ConversionFactorOBE").Value); } catch { }


            var PrePaymentFactor = 0.0;
            try { PrePaymentFactor = Convert.ToDouble(eclTsk._eclEadInputAssumption.FirstOrDefault(o => o.Key == "PrePaymentFactor)").Value); } catch { }

            var ccfData = new CalibrationInput_EAD_CCF_Summary_Processor().GetCCFData(eclId, eclType);

            var ccfOverall = ccfData.Overall_CCF ?? 0.0;

            var obj = new EADParameters
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


            return obj;


        }

        private bool ExtractAndSaveResult(List<Loanbook_Data> batchContracts, string filePath, Guid eclId, EclType eclType)
        {
            try
            {
                var frameworkResult = new List<ResultDetailDataMore>();
                var c = new ResultDetailDataMore();


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


                    for (int i = 10; i <= AppSettings.BatchSize + 20; i++)
                    {
                        int bc = 1;

                        if (worksheet.Cells[i, bc + 2].Value == null)
                            continue;

                        try
                        {
                            c = new ResultDetailDataMore();
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

                            try { c.OriginalOutstandingBalance = (double)batchContracts.FirstOrDefault(o => o.ContractNo == c.ContractNo).OutstandingBalanceLCY; } catch { }


                            frameworkResult.Add(c);
                        }
                        catch (Exception ex)
                        {
                            Log4Net.Log.Error(ex);
                        }

                    }

                    theWorkbook.Save();

                    theWorkbook.Close(true);

                }
                catch (Exception ex)
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

            } catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                return false;
            }

            return true;

        }

        private void RunECL(List<Loanbook_Data> batchContracts, int batchId, long affiliateId, Guid eclId, EclType eclType, EADParameters eadParam, LGDParameters lgdParam, PDParameters pdParam, FrameworkParameters frameworkParam)
        {
            var affiliatePath = Path.Combine(AppSettings.ECLBasePath, affiliateId.ToString());
            var eclPath = Path.Combine(affiliatePath, eclId.ToString());
            var batchPath = Path.Combine(eclPath, batchId.ToString());

            var eadTemplate = Path.Combine(affiliatePath, "EADTemplate.xlsb");
            var lgdTemplate = Path.Combine(affiliatePath, "LGDTemplate.xlsb");
            var pdTemplate = Path.Combine(affiliatePath, "PDTemplate.xlsb");
            var frameworkTemplate = Path.Combine(affiliatePath, "FrameworkTemplate.xlsb");

            var eadFile = Path.Combine(batchPath, $"{batchId}_{eclId}_EAD.xlsb");
            var lgdFile = Path.Combine(batchPath, $"{batchId}_{eclId}_LGD.xlsb");
            var pdFile = Path.Combine(batchPath, $"{batchId}_{eclId}_PD.xlsb");
            var frameworkFile = Path.Combine(batchPath, $"{batchId}_{eclId}_Framework.xlsb");

            var eadFileName = Path.Combine($"{batchId}_{eclId}_EAD.xlsb");
            var lgdFileName = Path.Combine($"{batchId}_{eclId}_LGD.xlsb");
            var pdFileName = Path.Combine($"{batchId}_{eclId}_PD.xlsb");
            var fraemworkFileName = Path.Combine($"{batchId}_{eclId}_Framework.xlsb");

            File.Copy(eadTemplate, eadFile);
            File.Copy(lgdTemplate, lgdFile);
            File.Copy(pdTemplate, pdFile);
            File.Copy(frameworkTemplate, frameworkFile);

            eadParam.ModelFileName = eadFileName;
            eadParam.BasePath = batchPath;
            eadParam.LoanBookFileName = $"{batchId}_{eclId}_EAD_LoanBook.xlsx";
            eadParam.PaymentScheduleFileName = $"{batchId}_{eclId}_PaymentSchedule.xlsx";

            lgdParam.ModelFileName = lgdFileName;
            lgdParam.BasePath = batchPath;
            lgdParam.LoanBookFileName = $"{batchId}_{eclId}_LGD_LoanBook.xlsx";

            pdParam.ModelFileName = pdFileName;
            pdParam.BasePath = batchPath;
            pdParam.LoanBookFileName = $"{batchId}_{eclId}_PD_LoanBook.xlsx";

            frameworkParam.ModelFileName = fraemworkFileName;
            frameworkParam.BasePath = batchPath;
            frameworkParam.EadFileName = $"{batchId}_{eclId}_EAD.xlsb";
            frameworkParam.LgdFile = $"{batchId}_{eclId}_LGD.xlsb";
            frameworkParam.PdFileName = $"{batchId}_{eclId}_PD.xlsb";
            var reportPath = Path.Combine(batchPath, "Report");

            if (!Directory.Exists(reportPath))
            {
                Directory.CreateDirectory(reportPath);
            }
            frameworkParam.ReportFolderName = reportPath;


            var taskList = new List<Task>();
            var tskStatusLst = new List<TaskStatus> { TaskStatus.RanToCompletion, TaskStatus.Faulted };

            //new EAD_Processor().ProcessEAD(eadParam);
            //new PD_Processor().ProcessPD(pdParam);
            // return;

            var tryCounter = 0;
            var task1 = Task.Run(() =>
            {


                var eadProcessor = false;
                while (!eadProcessor && tryCounter <= 3)
                {
                    Log4Net.Log.Info($"{batchId} - Started EAD");
                    tryCounter = tryCounter + 1;
                    eadProcessor = new EAD_Processor().ProcessEAD(eadParam);
                }
                tryCounter = 0;

                Log4Net.Log.Info("Completed EAD Files transfer");
            });
            taskList.Add(task1);

            var task2 = Task.Run(() =>
            {

                var lgdProcessor = false;
                while (!lgdProcessor && tryCounter <= 3)
                {
                    Log4Net.Log.Info($"{batchId} - Started LGD");
                    tryCounter = tryCounter + 1;
                    lgdProcessor = new LGD_Processor().ProcessLGD(lgdParam, pdParam);
                }
                tryCounter = 0;
                Log4Net.Log.Info("Completed LGD Files transfer");
            });
            taskList.Add(task2);

            var task3 = Task.Run(() =>
            {

                var pdProcessor = false;
                while (!pdProcessor && tryCounter <= 3)
                {
                    Log4Net.Log.Info($"{batchId} - Started PD");
                    tryCounter = tryCounter + 1;
                    pdProcessor = new PD_Processor().ProcessPD(pdParam);
                }
                Log4Net.Log.Info("Completed PD Files transfer");
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

            tryCounter = 0;

            var fwProcessor = false;
            while (!fwProcessor && tryCounter <= 3)
            {
                Log4Net.Log.Info($"{batchId} - Started Framework");
                tryCounter = tryCounter + 1;
                fwProcessor = new Framework_Processor().ProcessFramework(frameworkParam, batchContracts, eclId, eclType);
            }
            Log4Net.Log.Info("Completed Framework");

            //var fraemworkResultFile = Path.Combine(batchPath, fraemworkFile);

            //tryCounter = 0;

            //var exProcessor = false;
            //while (!exProcessor && tryCounter <= 3)
            //{
            //    Log4Net.Log.Info($"{batchId} - Started Extraction");
            //    tryCounter = tryCounter + 1;
            //    exProcessor=ExtractAndSaveResult(batchContracts, fraemworkResultFile, eclId, eclType);
            //}
            Log4Net.Log.Info("Completed Extraction");

        }

        private void RunECLOverride(List<Loanbook_Data> batchContracts, int batchId, long affiliateId, Guid eclId, EclType eclType, FrameworkParameters frameworkParam)
        {
            var affiliatePath = Path.Combine(AppSettings.ECLBasePath, affiliateId.ToString());
            var eclPath = Path.Combine(affiliatePath, eclId.ToString());
            var batchPath = Path.Combine(eclPath, batchId.ToString());

            var frameworkFileName = Path.Combine($"{batchId}_{eclId}_Framework.xlsb");
            frameworkParam.ModelFileName = frameworkFileName;
            frameworkParam.BasePath = batchPath;

            var tryCounter = 0;
            
            var fwProcessor = false;
            while (!fwProcessor && tryCounter <= 3)
            {
                Log4Net.Log.Info($"{batchId} - Started Framework");
                tryCounter = tryCounter + 1;
                fwProcessor = new Framework_Processor().ProcessFrameworkOverride(frameworkParam, batchContracts, eclId, eclType);
            }
            Log4Net.Log.Info("Completed Framework");

            //var fraemworkResultFile = Path.Combine(batchPath, fraemworkFile);

            //tryCounter = 0;

            //var exProcessor = false;
            //while (!exProcessor && tryCounter <= 3)
            //{
            //    Log4Net.Log.Info($"{batchId} - Started Extraction");
            //    tryCounter = tryCounter + 1;
            //    exProcessor=ExtractAndSaveResult(batchContracts, fraemworkResultFile, eclId, eclType);
            //}
            Log4Net.Log.Info("Completed Extraction");

        }

        private void GenerateLoanBookFile(int batchId, List<Loanbook_Data> loanbook, List<TempPaymentSchedule> payment_Schedules, long affiliateId, Guid eclId)
        {
            var contractNos = loanbook.Select(o => o.ContractNo).ToList();
            //payment_Schedules = payment_Schedules.Where(o => contractNos.Contains(o.ContractRefNo)).ToList();

            var includeDummyLoanBook = payment_Schedules.Count > 0;

            if (payment_Schedules.Count == 0)
            {
                payment_Schedules.Add(new TempPaymentSchedule { Amount = 0, ContractRefNo = "DummyContract", Component = "amortise", Frequency = "M", NoOfSchedules = 1, StartDate = DateTime.Now });
            }

            var affiliatePath = Path.Combine(AppSettings.ECLBasePath, affiliateId.ToString());
            var eclPath = Path.Combine(affiliatePath, eclId.ToString());
            var batchPath = Path.Combine(eclPath, batchId.ToString());

            if (Directory.Exists(batchPath))
            {
                Directory.Delete(batchPath, true);
            }


            Directory.CreateDirectory(batchPath);


            var loanBookTemplatePath = Path.Combine(AppSettings.ECLBasePath, "LoanBookTemplate.xlsx");
            var paymentScheduleTemplatePath = Path.Combine(AppSettings.ECLBasePath, "PaymentScheduleTemplate.xlsx");

            //var includeDummyLoanBook = false;

            var eadloanbookPath = Path.Combine(batchPath, $"{batchId}_{eclId}_EAD_LoanBook.xlsx");
            File.Copy(loanBookTemplatePath, eadloanbookPath);
            WriteLoanBook(loanbook, eadloanbookPath);

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
                    worksheet.Cells[i + 3, 1 + 1].Value = p.ContractRefNo;
                    worksheet.Cells[i + 3, 1 + 2].Value = p.StartDate;
                    worksheet.Cells[i + 3, 1 + 3].Value = p.Component;
                    worksheet.Cells[i + 3, 1 + 4].Value = p.NoOfSchedules;
                    worksheet.Cells[i + 3, 1 + 5].Value = p.Frequency;
                    worksheet.Cells[i + 3, 1 + 6].Value = p.Amount;
                }

                package.Save();
            }

        }

        private void WriteLoanBook(List<Loanbook_Data> loanbook, string loanbookPath)
        {

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

                    if (p.SpecialisedLending == "#N/A")
                    {
                        p.SpecialisedLending = "";
                    }
                    p.OriginalRating = p.OriginalRating.Replace("+", "").Replace(",", "");
                    p.RatingModel = p.RatingModel.Replace("+", "").Replace(",", "");
                    p.CurrentRating = p.CurrentRating.Replace("+", "").Replace(",", "");

                    p.CustomerNo= p.CustomerNo ?? "";
                    p.AccountNo = p.AccountNo ?? "";
                    p.ContractNo = p.ContractNo ?? "";
                    p.CustomerName = p.CustomerName ?? "";
                    p.Segment = p.Segment ?? "";
                    p.Sector = p.Sector ?? "";
                    p.ProductType = p.ProductType ?? "";



                    worksheet.Cells[i + 4, 1 + 1].Value = p.CustomerNo.Replace(",", "");
                    worksheet.Cells[i + 4, 1 + 2].Value = p.AccountNo.Replace(",", "");
                    worksheet.Cells[i + 4, 1 + 3].Value = p.ContractNo.Replace(",", "");
                    worksheet.Cells[i + 4, 1 + 4].Value = p.CustomerName.Replace(",", "");
                    worksheet.Cells[i + 4, 1 + 5].Value = p.SnapshotDate;
                    worksheet.Cells[i + 4, 1 + 6].Value = p.Segment.Replace(",", "");
                    worksheet.Cells[i + 4, 1 + 7].Value = p.Sector.Replace(",", "");
                    worksheet.Cells[i + 4, 1 + 8].Value = p.Currency ?? "";
                    worksheet.Cells[i + 4, 1 + 9].Value = p.ProductType.Replace(",", "");
                    worksheet.Cells[i + 4, 1 + 10].Value = p.ProductMapping ?? "";
                    worksheet.Cells[i + 4, 1 + 11].Value = "";// p.SpecialisedLending??"";
                    worksheet.Cells[i + 4, 1 + 12].Value = p.RatingModel ?? "";
                    worksheet.Cells[i + 4, 1 + 13].Value = p.OriginalRating ?? "";
                    worksheet.Cells[i + 4, 1 + 14].Value = p.CurrentRating ?? "";
                    worksheet.Cells[i + 4, 1 + 15].Value = p.LifetimePD;
                    worksheet.Cells[i + 4, 1 + 16].Value = p.Month12PD;
                    worksheet.Cells[i + 4, 1 + 17].Value = p.DaysPastDue;
                    worksheet.Cells[i + 4, 1 + 18].Value = p.WatchlistIndicator ? "1" : "";
                    worksheet.Cells[i + 4, 1 + 19].Value = p.Classification ?? "";
                    worksheet.Cells[i + 4, 1 + 20].Value = p.ImpairedDate;
                    worksheet.Cells[i + 4, 1 + 21].Value = p.DefaultDate;
                    worksheet.Cells[i + 4, 1 + 22].Value = p.CreditLimit;
                    worksheet.Cells[i + 4, 1 + 23].Value = p.OriginalBalanceLCY;
                    worksheet.Cells[i + 4, 1 + 24].Value = p.OutstandingBalanceLCY;
                    worksheet.Cells[i + 4, 1 + 25].Value = p.OutstandingBalanceACY;
                    worksheet.Cells[i + 4, 1 + 26].Value = p.ContractStartDate;
                    worksheet.Cells[i + 4, 1 + 27].Value = p.ContractEndDate;
                    worksheet.Cells[i + 4, 1 + 28].Value = p.RestructureIndicator ? "1" : "";
                    worksheet.Cells[i + 4, 1 + 29].Value = p.RestructureRisk;
                    worksheet.Cells[i + 4, 1 + 30].Value = p.RestructureType;
                    worksheet.Cells[i + 4, 1 + 31].Value = p.RestructureStartDate;
                    worksheet.Cells[i + 4, 1 + 32].Value = p.RestructureEndDate;
                    worksheet.Cells[i + 4, 1 + 33].Value = p.PrincipalPaymentTermsOrigination;
                    worksheet.Cells[i + 4, 1 + 34].Value = p.PPTOPeriod;
                    worksheet.Cells[i + 4, 1 + 35].Value = p.InterestPaymentTermsOrigination;
                    worksheet.Cells[i + 4, 1 + 36].Value = p.IPTOPeriod;
                    worksheet.Cells[i + 4, 1 + 37].Value = p.PrincipalPaymentStructure;
                    worksheet.Cells[i + 4, 1 + 38].Value = p.InterestPaymentStructure;
                    worksheet.Cells[i + 4, 1 + 39].Value = p.InterestRateType;
                    worksheet.Cells[i + 4, 1 + 40].Value = p.BaseRate ?? "";
                    worksheet.Cells[i + 4, 1 + 41].Value = p.OriginationContractualInterestRate ?? "";
                    worksheet.Cells[i + 4, 1 + 42].Value = p.IntroductoryPeriod;
                    worksheet.Cells[i + 4, 1 + 43].Value = p.PostIPContractualInterestRate;
                    worksheet.Cells[i + 4, 1 + 44].Value = p.CurrentContractualInterestRate;
                    worksheet.Cells[i + 4, 1 + 45].Value = p.EIR;

                    worksheet.Cells[i + 4, 1 + 46].Value = p.DebentureOMV;
                    worksheet.Cells[i + 4, 1 + 47].Value = p.DebentureFSV;

                    worksheet.Cells[i + 4, 1 + 48].Value = p.CashOMV;
                    worksheet.Cells[i + 4, 1 + 49].Value = p.CashFSV;

                    worksheet.Cells[i + 4, 1 + 50].Value = p.InventoryOMV;
                    worksheet.Cells[i + 4, 1 + 51].Value = p.InventoryFSV;

                    worksheet.Cells[i + 4, 1 + 52].Value = p.PlantEquipmentOMV;
                    worksheet.Cells[i + 4, 1 + 53].Value = p.PlantEquipmentFSV;

                    worksheet.Cells[i + 4, 1 + 54].Value = p.ResidentialPropertyOMV;
                    worksheet.Cells[i + 4, 1 + 55].Value = p.ResidentialPropertyFSV;

                    worksheet.Cells[i + 4, 1 + 56].Value = p.CommercialPropertyOMV;
                    worksheet.Cells[i + 4, 1 + 57].Value = p.CommercialProperty;

                    worksheet.Cells[i + 4, 1 + 58].Value = p.ReceivablesOMV;
                    worksheet.Cells[i + 4, 1 + 59].Value = p.ReceivablesFSV;

                    worksheet.Cells[i + 4, 1 + 60].Value = p.SharesOMV;
                    worksheet.Cells[i + 4, 1 + 61].Value = p.SharesFSV;

                    worksheet.Cells[i + 4, 1 + 62].Value = p.VehicleOMV;
                    worksheet.Cells[i + 4, 1 + 63].Value = p.VehicleFSV;

                    worksheet.Cells[i + 4, 1 + 64].Value = p.CureRate;
                    worksheet.Cells[i + 4, 1 + 65].Value = p.GuaranteeIndicator ? "1" : "";
                    worksheet.Cells[i + 4, 1 + 66].Value = p.GuarantorPD;
                    worksheet.Cells[i + 4, 1 + 67].Value = p.GuarantorLGD;
                    worksheet.Cells[i + 4, 1 + 68].Value = p.GuaranteeValue;

                    if (p.GuaranteeLevel != null && p.GuaranteeLevel > 1)
                    {
                        p.GuaranteeLevel = 1;
                    }
                    worksheet.Cells[i + 4, 1 + 69].Value = p.GuaranteeLevel;

                }

                package.Save();
            }
        }


        private void ProcessFrameworkOverrideResultTask()
        {
            var eclServer1Path = Path.Combine(AppSettings.ECLServer1, AppSettings.ECLAutomation);

            var di = new DirectoryInfo(eclServer1Path);


            var files = new List<FileInfo>();

            files = di.GetFiles("*", SearchOption.AllDirectories).Where(o => o.Name.StartsWith(AppSettings.complete_) && o.Name.Contains(AppSettings.override_) && o.Name.EndsWith($"{AppSettings.Framework}.{AppSettings.csv}")).ToList();

            if (files.Count == 0)
            {
                Log4Net.Log.Info("Found no Framework to process");
                return;
            }


            foreach (var file in files)
            {
                Log4Net.Log.Info(file.FullName);
                if (!File.Exists(Path.Combine(file.Directory.FullName, AppSettings.FrameworkComputeComplete)))
                {
                    continue;
                }
                var uploadingSourceCsvResult = Path.Combine(file.Directory.FullName, $"{file.Name.Replace(AppSettings.complete_, AppSettings.LoadingDB_)}");
                if (!File.Exists(file.FullName))
                {
                    Log4Net.Log.Info("File has probably been moved.");
                    continue;
                }
                try
                {
                    if (File.Exists(uploadingSourceCsvResult))
                    {
                        Log4Net.Log.Info("File has probably been moved.");
                        continue;
                    }
                    File.Move(file.FullName, uploadingSourceCsvResult);
                }
                catch (Exception ex)
                {
                    Log4Net.Log.Info("File has probably been moved.");
                    Log4Net.Log.Error(ex);
                    continue;
                }

                var basePath = new FileInfo(uploadingSourceCsvResult).DirectoryName;
                var inputFileText = File.ReadAllText(Path.Combine(basePath, AppSettings.ModelInputFileEto));
                var input = JsonConvert.DeserializeObject<FrameworkParameters>(inputFileText);


                var uploadStatus = new Framework_Processor().
                    ProcessFrameworkResult(uploadingSourceCsvResult, input);

                if (uploadStatus)
                {
                    Log4Net.Log.Info("Concluding on DB upload completed file:");
                    Log4Net.Log.Info(uploadingSourceCsvResult);
                    File.Move(uploadingSourceCsvResult, uploadingSourceCsvResult.Replace(AppSettings.LoadingDB_, AppSettings.LoadingDBComplete_));

                    var eclDirectories = new FileInfo(uploadingSourceCsvResult).Directory.Parent.GetDirectories();

                    var eclDataUploadIsCompleted = true;
                    foreach (var dir in eclDirectories)
                    {
                        try
                        {
                            if (int.Parse(dir.Name) >= 0)
                            {
                                var loadingDBComplete = dir.GetFiles().Any(o => o.Name.StartsWith(AppSettings.LoadingDBComplete_) && o.Name.Contains(AppSettings.override_));
                                if (!loadingDBComplete)
                                {
                                    eclDataUploadIsCompleted = false;
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log4Net.Log.Error(ex);
                            continue;
                        }
                    }

                    if (eclDataUploadIsCompleted)
                    {
                        var qry = Queries.EclOverrideIsRunning(input.EclId);
                        var dt = DataAccess.i.GetData(qry);
                        if (dt.Rows[0][0].ToString() != "Running Overrides")
                        {
                            qry = Queries.UpdateEclStatus(input.EclType.ToString(), input.EclId.ToString(), 4, "");
                            DataAccess.i.ExecuteQuery(qry);
                        }
                        else
                        {
                            qry = Queries.UpdateEclStatus(input.EclType.ToString(), input.EclId.ToString(), 5, "");
                            DataAccess.i.ExecuteQuery(qry);
                        }

                    }
                }
                else
                {
                    // reset files
                    Log4Net.Log.Info("Reverting file from loading to completed");
                    Log4Net.Log.Info(uploadingSourceCsvResult);
                    Log4Net.Log.Info(file.FullName);

                    File.Move(uploadingSourceCsvResult, file.FullName);
                }
            }

        }
        private void ProcessFrameworkResultTask()
        {
            var eclServer1Path = Path.Combine(AppSettings.ECLServer1,AppSettings.ECLAutomation);

            var di = new DirectoryInfo(eclServer1Path);


            var files = new List<FileInfo>();
            
            files=di.GetFiles("*", SearchOption.AllDirectories).Where(o => o.Name.StartsWith(AppSettings.complete_) && !o.Name.Contains(AppSettings.override_) && o.Name.EndsWith($"{AppSettings.Framework}.{AppSettings.csv}")).ToList();

            if (files.Count == 0)
            {
                Log4Net.Log.Info("Found no Framework to process");
                return;
            }
                

            foreach (var file in files)
            {
                Log4Net.Log.Info(file.FullName);
                if (!File.Exists(Path.Combine(file.Directory.FullName, AppSettings.FrameworkComputeComplete)))
                {
                    continue;
                }
                var uploadingSourceCsvResult = Path.Combine(file.Directory.FullName, $"{file.Name.Replace(AppSettings.complete_, AppSettings.LoadingDB_)}");
                if (!File.Exists(file.FullName))
                {
                    Log4Net.Log.Info("File has probably been moved.");
                    continue;
                }
                try
                {
                    if (File.Exists(uploadingSourceCsvResult))
                    {
                        Log4Net.Log.Info("File has probably been moved.");
                        continue;
                    }
                    File.Move(file.FullName, uploadingSourceCsvResult);
                }
                catch(Exception ex)
                {
                    Log4Net.Log.Info("File has probably been moved.");
                    Log4Net.Log.Error(ex);
                    continue;
                }

                var basePath = new FileInfo(uploadingSourceCsvResult).DirectoryName;
                var inputFileText = File.ReadAllText(Path.Combine(basePath, AppSettings.ModelInputFileEto));
                var input = JsonConvert.DeserializeObject<FrameworkParameters>(inputFileText);


                var uploadStatus =new Framework_Processor().
                    ProcessFrameworkResult(uploadingSourceCsvResult, input);

                if(uploadStatus)
                {
                    Log4Net.Log.Info("Concluding on DB upload completed file:");
                    Log4Net.Log.Info(uploadingSourceCsvResult);
                    File.Move(uploadingSourceCsvResult, uploadingSourceCsvResult.Replace(AppSettings.LoadingDB_, AppSettings.LoadingDBComplete_));

                    var eclDirectories = new FileInfo(uploadingSourceCsvResult).Directory.Parent.GetDirectories();

                    var eclDataUploadIsCompleted = true;
                    foreach (var dir in eclDirectories)
                    {
                        try
                        {
                            if (int.Parse(dir.Name) >= 0)
                            {
                                var loadingDBComplete=dir.GetFiles().Any(o=>o.Name.StartsWith(AppSettings.LoadingDBComplete_) && !o.Name.Contains(AppSettings.override_));
                                if(!loadingDBComplete)
                                {
                                    eclDataUploadIsCompleted = false;
                                    break;
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            Log4Net.Log.Error(ex);
                            continue;
                        }
                    }

                    if(eclDataUploadIsCompleted)
                    {
                        var qry = Queries.EclOverrideIsRunning(input.EclId);
                        var dt=DataAccess.i.GetData(qry);

                        //dt.Rows.Count > 0 && 
                        if (dt.Rows[0][0].ToString() == "Running Overrides")
                        {
                            qry = Queries.UpdateEclStatus(input.EclType.ToString(), input.EclId.ToString(), 4, "");
                            DataAccess.i.ExecuteQuery(qry);
                        }
                        else
                        {
                            qry = Queries.UpdateEclStatus(input.EclType.ToString(), input.EclId.ToString(), 5, "");
                            DataAccess.i.ExecuteQuery(qry);
                        }

                    }
                }
                else
                {
                    // reset files
                    Log4Net.Log.Info("Reverting file from loading to completed");
                    Log4Net.Log.Info(uploadingSourceCsvResult);
                    Log4Net.Log.Info(file.FullName);

                    File.Move(uploadingSourceCsvResult,file.FullName);
                }
            }

        }
    }
}


























