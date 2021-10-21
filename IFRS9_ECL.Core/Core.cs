using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Core.FrameworkComputation;
using IFRS9_ECL.Core.Report;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Framework;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core
{
    public class Core
    {
        int serviceId = 0;
        public bool ProcessRunTask(int serviceId)
        {
            this.serviceId = serviceId;
            //ProcessECLRunTask();
            var autoCore = new AutomationCore();
            autoCore.ProcessRunTask(serviceId);

            if(AppSettings.ServiceType == AppSettings.Main)
            {
                ProcessCalibrationRunTask();
                ProcessMacroRunTask();
                ProcessIVModelsRunTask();
            }

            return true;
        }

        private bool ProcessIVModelsRunTask()
        {

            try
            {


                var cali = Queries.CalibrationBehavioural();
                var dt = DataAccess.i.GetData(cali);
                if (dt.Rows.Count > 0)
                {
                    var qry = "";
                    var caliId = Guid.NewGuid();
                    try
                    {
                        var affId = (long)dt.Rows[0]["AffiliateId"];
                        caliId = (Guid)dt.Rows[0]["Id"];


                        qry = Queries.UpdateGuidTableServiceId("CalibrationRunEadBehaviouralTerms", this.serviceId, caliId);
                        var resp = DataAccess.i.ExecuteQuery(qry);

                        if (resp == 0)
                        {
                            Log4Net.Log.Info($"Another service has picked Behavioural Calibration with ID {caliId.ToString()}.");
                            return true;
                        }

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "Processing", "CalibrationRunEadBehaviouralTerms");
                        DataAccess.i.ExecuteQuery(qry);


                        var ead_bahavioural = new CalibrationInput_EAD_Behavioural_Terms_Processor();
                        ead_bahavioural.ProcessCalibration(caliId, affId);


                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "Completed", "CalibrationRunEadBehaviouralTerms");
                        DataAccess.i.ExecuteQuery(qry);

                    }
                    catch (Exception ex)
                    {

                        Log4Net.Log.Info("At Calibration");
                        Log4Net.Log.Error(ex.ToString());
                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 10, ex.ToString(), "CalibrationRunEadBehaviouralTerms");
                        DataAccess.i.ExecuteQuery(qry);
                    }
                }
                else
                {
                    Log4Net.Log.Info("No new Calibration found!");
                }


                cali = Queries.Calibration_ReceivablesRegisters();
                dt = DataAccess.i.GetData(cali);
                if (dt.Rows.Count > 0)
                {
                    var qry = "";
                    var caliId = Guid.NewGuid();
                    try
                    {
                        caliId = (Guid)dt.Rows[0]["Id"];

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "ReceivablesRegisters");
                        DataAccess.i.ExecuteQuery(qry);

                        var processor = new ETIReceivables_Processor();
                        processor.ProcessCalibration(caliId);

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "ReceivablesRegisters");
                        DataAccess.i.ExecuteQuery(qry);

                    }
                    catch (Exception ex)
                    {

                        Log4Net.Log.Info("At IV Receivables");
                        Log4Net.Log.Error(ex);
                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 10, "ReceivablesRegisters");
                        DataAccess.i.ExecuteQuery(qry);
                    }
                }

                var holdingCo = Queries.Calibration_HoldingCo_Registers();
                dt = DataAccess.i.GetData(holdingCo);
                if (dt.Rows.Count > 0)
                {
                    var qry = "";
                    var caliId = Guid.NewGuid();
                    try
                    {
                        caliId = (Guid)dt.Rows[0]["Id"];

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "HoldCoRegisters");
                        DataAccess.i.ExecuteQuery(qry);

                        var processor = new HoldingCo_Processor();
                        processor.ProcessCalibration(caliId);

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "HoldCoRegisters");
                        DataAccess.i.ExecuteQuery(qry);

                    }
                    catch (Exception ex)
                    {

                        Log4Net.Log.Info("At IV Holding Co");
                        Log4Net.Log.Error(ex);
                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 10, "HoldCoRegisters");
                        DataAccess.i.ExecuteQuery(qry);
                    }
                }


                var rvImpairment = Queries.Calibration_RvImpairment_Registers();
                dt = DataAccess.i.GetData(rvImpairment);
                if (dt.Rows.Count > 0)
                {
                    var qry = "";
                    var caliId = Guid.NewGuid();
                    try
                    {
                        caliId = (Guid)dt.Rows[0]["Id"];

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "LoanImpairmentRegisters");
                        DataAccess.i.ExecuteQuery(qry);

                        var processor = new RV_Impairment_Processor();
                        processor.ProcessCalibration(caliId);

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "LoanImpairmentRegisters");
                        DataAccess.i.ExecuteQuery(qry);

                    }
                    catch (Exception ex)
                    {

                        Log4Net.Log.Info("At RV Impairment Model");
                        Log4Net.Log.Error(ex);
                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 10, "LoanImpairmentRegisters");
                        DataAccess.i.ExecuteQuery(qry);
                    }
                }


            }
            catch (Exception ex)
            {
                Log4Net.Log.Info("At IV Model");
                Log4Net.Log.Error(ex.ToString());
                var x = 0;

            }

            return true;
        }

        public bool ProcessCaliMacroTaskOnly(int serviceId)
        {
            this.serviceId = serviceId;
            ProcessCalibrationRunTask();
            ProcessMacroRunTask();
            //ProcessECLRunTask();
            var autoCore = new AutomationCore();
            autoCore.ProcessRunTask(serviceId);
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

                var qry = Queries.UpdateGuidTableServiceId($"{eclRegister.eclType.ToString()}Ecls",this.serviceId, eclRegister.Id);
                var resp=DataAccess.i.ExecuteQuery(qry);

                if(resp==0)
                {
                    Log4Net.Log.Info($"Another service has picked ECL with ID {eclRegister.Id} of Type [{eclRegister.eclType.ToString()}].");
                    return true;
                }

                qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 6, "");
                DataAccess.i.ExecuteQuery(qry);
                var eclType = eclRegister.eclType;
                Log4Net.Log.Info($"Found ECL with ID {eclRegister.Id} of Type [{eclType.ToString()}]. Running will commence if it has not been picked by another Job");



                var masterGuid = eclRegister.Id;//Guid.NewGuid();
                                                
                LifetimeEadWorkings lifetimeEadWorkings = new LifetimeEadWorkings(masterGuid, eclType);
                var loanbook_data = lifetimeEadWorkings.GetLoanBookData();

                //masterGuid = Guid.Parse("23f61e5f-46aa-4f33-aab0-08d844eaa419");
                Log4Net.Log.Info($"Start Time {DateTime.Now}");






                //new ProcessECL_Framework(masterGuid, eclType).ProcessResultDetails(loanbook_data);



               // return true; ;


                var overrideExist = false;
                if (eclRegister.Status==12)
                {
                    overrideExist = CheckOverrideDataExist(masterGuid, eclType);
                    if(!overrideExist)
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
                        Log4Net.Log.Info("Running Overrides");
                    }
                }




                var new_loanbook_data = loanbook_data;// new List<Loanbook_Data>();
                //var distinctContracts = loanbook_data.Select(o => o.ContractId).Distinct().ToList();
                //foreach (var contract in distinctContracts)
                //{
                //    var new_contract = loanbook_data.LastOrDefault(o => o.ContractId == contract);
                //    new_contract.OutstandingBalanceLCY = loanbook_data.Where(o => o.ContractId == contract).Sum(o => o.OutstandingBalanceLCY);
                //    loanbook_data.Add(new_contract);
                //}

                if (!overrideExist) //1!=1)//
                {

                    // Process EAD
                    new ProcessECL_EAD(masterGuid, eclType).ProcessTask(new_loanbook_data);
                    qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 6, "");
                    DataAccess.i.ExecuteQuery(qry);

                    //Exporting Reports
                    //var _rpt = new ExcelReport().GenerateResult(masterGuid.ToString(), new List<LifetimeEad>(), new List<LifetimeLgd>());
                    //return true;

                    //Process LGD
                    new ProcessECL_LGD(masterGuid, eclType).ProcessTask(new_loanbook_data);
                    qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 7, "");
                    DataAccess.i.ExecuteQuery(qry);

                    //Process PD
                    new ProcessECL_PD(masterGuid, eclType).ProcessTask(new_loanbook_data);
                    qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 7, ""); // should change to framekwork
                    DataAccess.i.ExecuteQuery(qry);

                }

                //Process Framework

                var taskLst = new List<Task>();

                var cummulativeDiscountFactor = new IrFactorWorkings(masterGuid, eclType).ComputeCummulativeDiscountFactor();

                var eadInput = new LifetimeEadWorkings(masterGuid, eclType).GetTempEadInputData(new_loanbook_data);
                
                var lifetimeEad = new LifetimeEadWorkings(masterGuid, eclType).ComputeLifetimeEad(new_loanbook_data, eadInput);


                var stageClassification = GetStagingClassificationResult(new_loanbook_data, masterGuid, eclType);

                var lifetimeLGD = new ScenarioLifetimeLGD(masterGuid, eclType, ECL_Scenario.Best).ComputeLifetimeLGD(new_loanbook_data, lifetimeEad, eadInput, stageClassification);

                var task1 = Task.Run(() =>
                {
                    var _lifetimeLGD = lifetimeLGD.Where(o => o.Ecl_Scenerio == ECL_Scenario.Best).ToList();
                    Log4Net.Log.Info("************Processing Final Best");
                    new ProcessECL_Framework(masterGuid, ECL_Scenario.Best, eclType).ProcessTask(new_loanbook_data, lifetimeEad, _lifetimeLGD, cummulativeDiscountFactor, eadInput, stageClassification, overrideExist);
                });
                taskLst.Add(task1);
                var task2 = Task.Run(() =>
                {
                    var _lifetimeLGD = lifetimeLGD.Where(o => o.Ecl_Scenerio == ECL_Scenario.Optimistic).ToList();
                    Log4Net.Log.Info("*************Processing Final Optimistic");
                    new ProcessECL_Framework(masterGuid, ECL_Scenario.Optimistic, eclType).ProcessTask(new_loanbook_data, lifetimeEad, _lifetimeLGD, cummulativeDiscountFactor, eadInput, stageClassification, overrideExist);
                });
                taskLst.Add(task2);
                var task3 = Task.Run(() =>
                {
                    var _lifetimeLGD = lifetimeLGD.Where(o => o.Ecl_Scenerio == ECL_Scenario.Downturn).ToList();
                    Log4Net.Log.Info("*************Processing Final Down turn");
                    new ProcessECL_Framework(masterGuid, ECL_Scenario.Downturn, eclType).ProcessTask(new_loanbook_data, lifetimeEad, _lifetimeLGD, cummulativeDiscountFactor, eadInput, stageClassification, overrideExist);
                });
                taskLst.Add(task3);

                
                var tskStatusLst = new List<TaskStatus> { TaskStatus.RanToCompletion, TaskStatus.Faulted };
                while (0 < 1)
                {
                    if (taskLst.All(o => tskStatusLst.Contains(o.Status)))
                    {
                        foreach(var itm in taskLst)
                        {
                            if(itm.Status!=TaskStatus.RanToCompletion)
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

                //Task t = Task.WhenAll(taskLst);

                //try
                //{
                //    t.Wait();
                //}
                //catch (Exception ex)
                //{
                //    Log4Net.Log.Error(ex);
                //}
                //Log4Net.Log.Info($"All Task status: {t.Status}");

                //if (t.Status == TaskStatus.RanToCompletion)
                //{
                //    Log4Net.Log.Info($"All Task ran to completion");
                //}
                //if (t.Status == TaskStatus.Faulted)
                //{
                //    Log4Net.Log.Info($"All Task ran to fault");
                //}
                

                new ProcessECL_Framework(masterGuid, eclType).ProcessResultDetails(new_loanbook_data, overrideExist);


                if (!overrideExist)
                {
                    qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 4, "");
                    DataAccess.i.ExecuteQuery(qry);
                }
                else
                {
                    qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 5, "");
                    DataAccess.i.ExecuteQuery(qry);
                }
                Log4Net.Log.Info($"Main Task Completed. Report output to start {DateTime.Now}");
                //Exporting Reports
                var rpt = new ExcelReport().GenerateResult(masterGuid.ToString(), lifetimeEad, lifetimeLGD);

                //Delete Logs in table
                    //qry = Queries.ClearAllEclLogs(eclRegister.eclType.ToString(), eclRegister.Id.ToString());
                    //DataAccess.i.ExecuteQuery(qry);

                Log4Net.Log.Info($"End Time {DateTime.Now}");
                return true;
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                var qry = Queries.UpdateEclStatus(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 13, ex.ToString());
                DataAccess.i.ExecuteQuery(qry);
            }
            return true;
        }

        public bool CheckOverrideDataExist(Guid eclId, EclType eclType)
        {
            var qry = Queries.CheckOverrideDataExist(eclId, eclType);
            var cnt = DataAccess.i.getCount(qry);
            return cnt > 0;
        }

        protected List<StageClassification> GetStagingClassificationResult(List<Loanbook_Data> loanbook,Guid eclId, EclType eclType)
        {
            SicrWorkings _sicrWorkings = new SicrWorkings(eclId, eclType);
            return _sicrWorkings.ComputeStageClassification(loanbook);
        }

        public bool ProcessCalibrationRunTask()
        {


            try
            {

            
                var cali = Queries.CalibrationBehavioural();
                var dt = DataAccess.i.GetData(cali);
                if (dt.Rows.Count > 0)
                {
                    var qry = "";
                    var caliId = Guid.NewGuid();
                    try
                    {
                        var affId = (long)dt.Rows[0]["AffiliateId"];
                        caliId = (Guid)dt.Rows[0]["Id"];


                        //qry = Queries.UpdateGuidTableServiceId("CalibrationRunEadBehaviouralTerms", this.serviceId, caliId);
                        //var resp = DataAccess.i.ExecuteQuery(qry);

                        //if (resp == 0)
                        //{
                        //    Log4Net.Log.Info($"Another service has picked Behavioural Calibration with ID {caliId.ToString()}.");
                        //    return true;
                        //}

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "Processing", "CalibrationRunEadBehaviouralTerms");
                        DataAccess.i.ExecuteQuery(qry);


                        var ead_bahavioural = new CalibrationInput_EAD_Behavioural_Terms_Processor();
                        ead_bahavioural.ProcessCalibration(caliId, affId);


                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "Completed", "CalibrationRunEadBehaviouralTerms");
                        DataAccess.i.ExecuteQuery(qry);

                    }
                    catch (Exception ex)
                    {

                        Log4Net.Log.Info("At Calibration");
                        Log4Net.Log.Error(ex.ToString());
                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 10, ex.ToString(), "CalibrationRunEadBehaviouralTerms");
                        DataAccess.i.ExecuteQuery(qry);
                    }
                }
                else
                {
                    Log4Net.Log.Info("No new Calibration found!");
                }


                cali = Queries.CalibrationCCF();
                dt = DataAccess.i.GetData(cali);
                if (dt.Rows.Count > 0)
                {
                    var qry = "";
                    var caliId = Guid.NewGuid();
                    try
                    {
                        var affId = (long)dt.Rows[0]["AffiliateId"];
                        caliId = (Guid)dt.Rows[0]["Id"];

                        qry = Queries.UpdateGuidTableServiceId("CalibrationRunEadCcfSummary", this.serviceId, caliId);
                        var resp=DataAccess.i.ExecuteQuery(qry);
                        if (resp == 0)
                        {
                            Log4Net.Log.Info($"Another service has picked CCF Calibration with ID {caliId.ToString()}.");
                            return true;
                        }

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "Processing", "CalibrationRunEadCcfSummary");
                        DataAccess.i.ExecuteQuery(qry);

                        var ead_ccf = new CalibrationInput_EAD_CCF_Summary_Processor();
                        ead_ccf.ProcessCalibration(caliId, affId);

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "Completed", "CalibrationRunEadCcfSummary");
                        DataAccess.i.ExecuteQuery(qry);

                    }
                    catch (Exception ex)
                    {
                        
                        Log4Net.Log.Info("At Calibration");
                        Log4Net.Log.Error(ex.ToString());
                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 10, ex.ToString(), "CalibrationRunEadCcfSummary");
                        DataAccess.i.ExecuteQuery(qry);
                    }
                }


                cali = Queries.CalibrationHaircut();
                dt = DataAccess.i.GetData(cali);
                if (dt.Rows.Count > 0)
                {
                    var qry = "";
                    var caliId = Guid.NewGuid();
                    try
                    {
                        var affId = (long)dt.Rows[0]["AffiliateId"];
                        caliId = (Guid)dt.Rows[0]["Id"];


                        qry = Queries.UpdateGuidTableServiceId("CalibrationRunLgdHairCut", this.serviceId, caliId);
                        var resp = DataAccess.i.ExecuteQuery(qry);
                        if (resp == 0)
                        {
                            Log4Net.Log.Info($"Another service has picked Haircut Calibration with ID {caliId.ToString()}.");
                            return true;
                        }

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "Processing", "CalibrationRunLgdHairCut");
                        DataAccess.i.ExecuteQuery(qry);


                        var lgd_haircut = new CalibrationInput_LGD_Haricut_Processor();
                        lgd_haircut.ProcessCalibration(caliId, affId);


                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "Completed", "CalibrationRunLgdHairCut");
                        DataAccess.i.ExecuteQuery(qry);

                    }
                    catch (Exception ex)
                    {
                        Log4Net.Log.Error(ex.ToString());
                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 10, ex.ToString(), "CalibrationRunLgdHairCut");
                        DataAccess.i.ExecuteQuery(qry);
                    }
                }


                cali = Queries.CalibrationRecovery();
                dt = DataAccess.i.GetData(cali);
                if (dt.Rows.Count > 0)
                {
                    var qry = "";
                    var caliId = Guid.NewGuid();
                    try
                    {
                        var affId = (long)dt.Rows[0]["AffiliateId"];
                        caliId = (Guid)dt.Rows[0]["Id"];

                        qry = Queries.UpdateGuidTableServiceId("CalibrationRunLgdRecoveryRate", this.serviceId, caliId);
                        var resp = DataAccess.i.ExecuteQuery(qry);
                        if (resp == 0)
                        {
                            Log4Net.Log.Info($"Another service has picked Recovery Rate Calibration with ID {caliId.ToString()}.");
                            return true;
                        }

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "Processing", "CalibrationRunLgdRecoveryRate");
                        DataAccess.i.ExecuteQuery(qry);

                        var lgd_recoveryR = new CalibrationInput_LGD_RecoveryRate_Processor();
                        lgd_recoveryR.ProcessCalibration(caliId, affId);


                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "Completed", "CalibrationRunLgdRecoveryRate");
                        DataAccess.i.ExecuteQuery(qry);

                    }
                    catch (Exception ex)
                    {
                        Log4Net.Log.Error(ex.ToString());
                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 10, ex.ToString(), "CalibrationRunLgdRecoveryRate");
                        DataAccess.i.ExecuteQuery(qry);
                    }
                }


                cali = Queries.CalibrationPD();
                dt = DataAccess.i.GetData(cali);
                if (dt.Rows.Count > 0)
                {
                    var qry = "";
                    var caliId = Guid.NewGuid();
                    try
                    {
                        var affId = (long)dt.Rows[0]["AffiliateId"];
                        caliId = (Guid)dt.Rows[0]["Id"];

                        qry = Queries.UpdateGuidTableServiceId("CalibrationRunPdCrDrs", this.serviceId, caliId);
                        var resp = DataAccess.i.ExecuteQuery(qry);
                        if (resp == 0)
                        {
                            Log4Net.Log.Info($"Another service has picked PD CR DR Calibration with ID {caliId.ToString()}.");
                            return true;
                        }

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "Processing", "CalibrationRunPdCrDrs");
                        DataAccess.i.ExecuteQuery(qry);

                        var pd_cr_dr = new CalibrationInput_PD_CR_RD_Processor();
                        pd_cr_dr.ProcessCalibration(caliId, affId);


                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "Completed", "CalibrationRunPdCrDrs");
                        DataAccess.i.ExecuteQuery(qry);

                    }catch(Exception ex)
                    {
                        Log4Net.Log.Error(ex);
                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 10, ex.ToString(), "CalibrationRunPdCrDrs");
                        DataAccess.i.ExecuteQuery(qry);
                    }
                }



            }catch(Exception ex)
            {
                Log4Net.Log.Info("At Calibration");
                Log4Net.Log.Error(ex.ToString());
                var x = 0;

            }

            return true;
        }



        public bool ProcessMacroRunTask()
        {
            var macroId = 0;
            try
            {
                var macro = Queries.MacroRegister();
                var dt = DataAccess.i.GetData(macro);

                if (dt.Rows.Count == 0)
                {
                    Log4Net.Log.Info($"No new pending Macro");
                    return true;
                }
                else
                {
                    Log4Net.Log.Info($"Found Macro to RUN");
                }

                var affId = (long)dt.Rows[0]["AffiliateId"];
                macroId = (int)dt.Rows[0]["Id"];

                //var qry = Queries.UpdateIntTableServiceId("CalibrationRunMacroAnalysis", this.serviceId, macroId);
                //DataAccess.i.ExecuteQuery(qry);

                var qry = Queries.MacroRegisterUpdate(macroId, 4, "Processing");
                DataAccess.i.ExecuteQuery(qry);

                try
                {
                    var macroP = new Macro_Processor();
                    macroP.ProcessMacro(macroId, affId);

                    qry = Queries.MacroRegisterUpdate(macroId, 5, "Completed");
                    DataAccess.i.ExecuteQuery(qry);

                }
                catch (Exception ex)
                {
                    Log4Net.Log.Info("At Macro");
                    Log4Net.Log.Error(ex);
                    Log4Net.Log.Error(ex.ToString());
                    Log4Net.Log.Error(ex.StackTrace);
                    qry = Queries.MacroRegisterUpdate(macroId, 10, ex.ToString());
                    DataAccess.i.ExecuteQuery(qry);
                }
            }
            catch (Exception ex)
            {
                Log4Net.Log.Info("At Macro");
                Log4Net.Log.Error(ex);
                var qry = Queries.MacroRegisterUpdate(macroId, 10, ex.ToString());
                DataAccess.i.ExecuteQuery(qry);
            }
            return true;
        }


    }
}
