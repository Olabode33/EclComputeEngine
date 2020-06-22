using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Core.FrameworkComputation;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core
{
    public class Core
    {
        public bool ProcessRunTask()
        {
            ProcessECLRunTask();
            //ProcessCalibrationRunTask();
            //ProcessMacroRunTask();
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

                var qry = Queries.EclsRegisterUpdate(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 3,"");
                DataAccess.i.ExecuteQuery(qry);
                var eclType = eclRegister.eclType;
                Log4Net.Log.Info($"Found ECL with ID {eclRegister.Id} of Type [{eclType.ToString()}]. Running will commence if it has not been picked by another Job");



                var masterGuid = eclRegister.Id;//Guid.NewGuid();
                                                //masterGuid = Guid.Parse("4140a69e-a729-4269-a078-91a01b5e0cd0");

                LifetimeEadWorkings lifetimeEadWorkings = new LifetimeEadWorkings(masterGuid, eclType);
                var loanbook_data = lifetimeEadWorkings.GetLoanBookData();

                Console.WriteLine($"Start Time {DateTime.Now}");

                // Process EAD
                new ProcessECL_EAD(masterGuid, eclType).ProcessTask(loanbook_data);

                //Process LGD
                new ProcessECL_LGD(masterGuid, eclType).ProcessTask(loanbook_data);

                //Process PD
                new ProcessECL_PD(masterGuid, eclType).ProcessTask(loanbook_data);

                //Process PD
                new ProcessECL_Framework(masterGuid, ECL_Scenario.Best, eclType).ProcessTask(loanbook_data);
                new ProcessECL_Framework(masterGuid, ECL_Scenario.Optimistic, eclType).ProcessTask(loanbook_data);
                new ProcessECL_Framework(masterGuid, ECL_Scenario.Downturn, eclType).ProcessTask(loanbook_data);


                qry = Queries.EclsRegisterUpdate(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 4,"");
                DataAccess.i.ExecuteQuery(qry);

                Console.WriteLine($"End Time {DateTime.Now}");
                return true;
            }catch(Exception ex)
            {
                var qry = Queries.EclsRegisterUpdate(eclRegister.eclType.ToString(), eclRegister.Id.ToString(), 10, ex.ToString());
                DataAccess.i.ExecuteQuery(qry);
            }
            return true;
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

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "Processing", "CalibrationRunEadBehaviouralTerms");
                        DataAccess.i.ExecuteQuery(qry);


                        var ead_bahavioural = new CalibrationInput_EAD_Behavioural_Terms_Processor();
                        ead_bahavioural.ProcessCalibration(caliId, affId);


                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "Completed", "CalibrationRunEadBehaviouralTerms");
                        DataAccess.i.ExecuteQuery(qry);

                    }
                    catch (Exception ex)
                    {
                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 10, ex.ToString(), "CalibrationRunEadBehaviouralTerms");
                        DataAccess.i.ExecuteQuery(qry);
                    }
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

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "Processing", "CalibrationRunEadCcfSummary");
                        DataAccess.i.ExecuteQuery(qry);


                        var ead_ccf = new CalibrationInput_EAD_CCF_Summary_Processor();
                        ead_ccf.ProcessCalibration(caliId, affId);


                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "Completed", "CalibrationRunEadCcfSummary");
                        DataAccess.i.ExecuteQuery(qry);

                    }
                    catch (Exception ex)
                    {
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

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "Processing", "CalibrationRunLgdHairCut");
                        DataAccess.i.ExecuteQuery(qry);


                        var lgd_haircut = new CalibrationInput_LGD_Haricut_Processor();
                        lgd_haircut.ProcessCalibration(caliId, affId);


                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "Completed", "CalibrationRunLgdHairCut");
                        DataAccess.i.ExecuteQuery(qry);

                    }
                    catch (Exception ex)
                    {
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

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "Processing", "CalibrationRunLgdRecoveryRate");
                        DataAccess.i.ExecuteQuery(qry);

                        var lgd_recoveryR = new CalibrationInput_LGD_RecoveryRate_Processor();
                        lgd_recoveryR.ProcessCalibration(caliId, affId);


                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "Completed", "CalibrationRunLgdRecoveryRate");
                        DataAccess.i.ExecuteQuery(qry);

                    }
                    catch (Exception ex)
                    {
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

                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 4, "Processing", "CalibrationRunPdCrDrs");
                        DataAccess.i.ExecuteQuery(qry);

                        var pd_cr_dr = new CalibrationInput_PD_CR_RD_Processor();
                        pd_cr_dr.ProcessCalibration(caliId, affId);


                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 5, "Completed", "CalibrationRunPdCrDrs");
                        DataAccess.i.ExecuteQuery(qry);

                    }catch(Exception ex)
                    {
                        qry = Queries.CalibrationRegisterUpdate(caliId.ToString(), 10, ex.ToString(), "CalibrationRunPdCrDrs");
                        DataAccess.i.ExecuteQuery(qry);
                    }
                }



            }catch(Exception ex)
            {


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
                    Console.WriteLine($"No new pending Macro");
                }

                var affId = (long)dt.Rows[0]["AffiliateId"];
                macroId = (int)dt.Rows[0]["Id"];

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
                    qry = Queries.MacroRegisterUpdate(macroId, 10, ex.ToString());
                    DataAccess.i.ExecuteQuery(qry);
                }
            }
            catch (Exception ex)
            {
                var qry = Queries.MacroRegisterUpdate(macroId, 10, ex.ToString());
                DataAccess.i.ExecuteQuery(qry);
            }
            return true;
        }

    }
}
