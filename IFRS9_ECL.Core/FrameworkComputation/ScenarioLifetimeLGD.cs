using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Core.PDComputation;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Framework;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.FrameworkComputation
{
    public class ScenarioLifetimeLGD
    {
        private Guid _eclId;
        EclType _eclType;

        public ScenarioLifetimeLGD(Guid eclId, EclType eclType, ECL_Scenario _scenario)
        {
            this._eclId = eclId;
           // this._scenario = scenario;
            this._eclType = eclType;
            _sicrInputs = new SicrInputWorkings(this._eclId, _eclType);
            _sicrWorkings = new SicrWorkings(this._eclId, _eclType);
            _lifetimeEadWorkings = new LifetimeEadWorkings(this._eclId, _eclType);
            _scenarioLifetimeCollateral = new ScenarioLifetimeCollateral( ECL_Scenario.Best,this._eclId, _eclType);
            _pdMapping = new PDMapping(this._eclId, _eclType);
            _creditIndex = new CreditIndex(this._eclId, _eclType);

        }

        public ScenarioLifetimeLGD(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
        }


        //protected ECL_Scenario _scenario;
        protected SicrWorkings _sicrWorkings;
        protected LifetimeEadWorkings _lifetimeEadWorkings;
        protected ScenarioLifetimeCollateral _scenarioLifetimeCollateral;
        protected PDMapping _pdMapping;
        protected SicrInputWorkings _sicrInputs;
        protected CreditIndex _creditIndex;

     
        List<LGDAccountData> contractData;
        List<PdMappings> pdMapping;
        List<LgdInputAssumptions_UnsecuredRecovery> lgdAssumptions;
        List<SicrInputs> sicrInput;
        List<StageClassification> stageClassification;
        List<EclAssumptions> impairmentAssumptions;

        List<LifeTimeObject> lifetimePdBest;
        List<LifeTimeObject> lifetimePdOptimistic;
        List<LifeTimeObject> lifetimePdDownturn;

        List<LifeTimeObject> redefaultPdBest;
        List<LifeTimeObject> redefaultPdOptimistic;
        List<LifeTimeObject> redefaultPdDownturn;

        List<LifetimeEad> lifetimeEAD;
        List<CreditIndex_Output> creditIndex;
        List<LifetimeCollateral> lifetimeCollateralBest;
        List<LifetimeCollateral> lifetimeCollateralOptimistic;
        List<LifetimeCollateral> lifetimeCollateralDownturn;

        List<LifetimeLgd> lifetimeLGD = new List<LifetimeLgd>();
        int stage2to3Forward = 0;
        public List<LifetimeLgd> ComputeLifetimeLGD(List<Loanbook_Data> loanbook)
        {
            

            contractData = new ProcessECL_LGD(this._eclId, this._eclType).GetLgdContractData(loanbook);
            pdMapping = GetPdIndexMappingResult();
            lgdAssumptions = GetLgdAssumptionsData();
            sicrInput = GetSicrResult();
            stageClassification = GetStagingClassificationResult(loanbook);
            impairmentAssumptions = GetECLFrameworkAssumptions();

            lifetimePdBest = GetScenarioLifetimePdResult(ECL_Scenario.Best);
            lifetimePdOptimistic = GetScenarioLifetimePdResult(ECL_Scenario.Optimistic);
            lifetimePdDownturn = GetScenarioLifetimePdResult(ECL_Scenario.Downturn);

            redefaultPdBest = GetScenarioRedfaultLifetimePdResult(ECL_Scenario.Best);
            redefaultPdOptimistic = GetScenarioRedfaultLifetimePdResult(ECL_Scenario.Optimistic);
            redefaultPdDownturn = GetScenarioRedfaultLifetimePdResult(ECL_Scenario.Downturn);

            lifetimeEAD = GetLifetimeEadResult(loanbook);
            creditIndex = GetCreditRiskResult();

            lifetimeCollateralBest = GetScenarioLifetimeCollateralResult(loanbook, ECL_Scenario.Best);
            lifetimeCollateralOptimistic = GetScenarioLifetimeCollateralResult(loanbook, ECL_Scenario.Optimistic);
            lifetimeCollateralDownturn = GetScenarioLifetimeCollateralResult(loanbook, ECL_Scenario.Downturn);

            //xxxxxxxxxxxxx
            //try { Convert.ToInt32(impairmentAssumptions.FirstOrDefault(x => x.Key == ImpairmentRowKeys.ForwardTransitionStage2to3).Value); } catch { }

            try { stage2to3Forward = Convert.ToInt32(impairmentAssumptions.FirstOrDefault(x => x.Key == ImpairmentRowKeys.ForwardTransitionStage2to3).Value); } catch { }
            //double creditIndexHurdle = Convert.ToDouble(GetImpairmentAssumptionValue(impairmentAssumptions, ImpairmentRowKeys.CreditIndexThreshold));



            var threads = contractData.Count / 500;
            threads = threads + 1;

            var taskLst = new List<Task>();
            for (int i = 0; i < threads; i++)
            {
                var subcontract = contractData.Skip(i * 500).Take(500).ToList();

                var task = Task.Run(() =>
                {
                    RunLGDJob(subcontract);
                });
                taskLst.Add(task);
            }
            Log4Net.Log.Info($"Total Task : {taskLst.Count()}");

            var completedTask = taskLst.Where(o => o.IsCompleted).Count();
            Log4Net.Log.Info($"Task Completed: {completedTask}");

            var tskStatusLst = new List<TaskStatus> {TaskStatus.RanToCompletion, TaskStatus.Faulted };
            while (!taskLst.Any(o => tskStatusLst.Contains(o.Status)))
            {
                var newCount = taskLst.Where(o => o.IsCompleted).Count();
                if (completedTask != newCount)
                {
                    Log4Net.Log.Info($"Task Completed: {completedTask}");
                }
                //Do Nothing
            }



            return lifetimeLGD;
        }

        private void RunLGDJob(List<LGDAccountData> subcontract)
        {

            var _lifetimeLGD = new List<LifetimeLgd>();
            foreach (var row in subcontract)
            {
                string contractId = row.CONTRACT_NO;
                double costOfRecovery = row.COST_OF_RECOVERY;
                double guarantorPd = row.GUARANTOR_PD;
                double guarantorLgd = row.GUARANTOR_LGD;
                double guaranteeValue = row.GUARANTEE_VALUE;
                double guaranteeLevel = row.GUARANTEE_LEVEL;

                //xxxxxxxxxxxxxxxxxxxxxxxxxxx
                //try { loanStage= stageClassification.FirstOrDefault(x => x.ContractId == contractId).Stage; } catch { };
                int loanStage = 0;
                loanStage = stageClassification.FirstOrDefault(x => x.ContractId == contractId).Stage;


                var pdMappingRow = pdMapping.FirstOrDefault(x => x.ContractId == contractId);

                //xxxxxxxxxxxxxxxxxxxxxxxxxxxx
                //if (pdMappingRow==null)
                //{
                //    pdMappingRow = pdMapping.FirstOrDefault();
                //}
                string pdGroup = pdMappingRow.PdGroup;
                string segment = pdMappingRow.Segment;
                string productType = pdMappingRow.ProductType;

                //xxxxxxxxxxxxxxxxxxxxxxxxxxxx
                var sicrInputRow = sicrInput.FirstOrDefault(x => x.ContractId == contractId);
                //if (sicrInputRow == null)
                //{
                //    sicrInputRow = sicrInput.FirstOrDefault();
                //}
                double redefaultLifetimePd = sicrInputRow.RedefaultLifetimePd;
                long daysPastDue = sicrInputRow.DaysPastDue;

                //XXXXXXXXXXXXXXXX
                //var best_downTurn_Assumption = lgdAssumptions.FirstOrDefault(o => o.Segment_Product_Type.ToLower().Contains($"{segment.ToLower()}{productType.ToLower()}".Replace(" ", "")));
                var best_downTurn_Assumption = lgdAssumptions.FirstOrDefault();// o => o.Segment_Product_Type.ToLower().Contains($"{segment.ToLower()}{productType.ToLower()}".Replace(" ","")));
                //if (best_downTurn_Assumption == null)
                //{
                //    best_downTurn_Assumption = lgdAssumptions.FirstOrDefault();
                //}
                double cureRates = best_downTurn_Assumption.Cure_Rate;

                long lgdAssumptionColumn = Math.Max(daysPastDue - stage2to3Forward, 0);

                double unsecuredRecoveriesBest = 0;
                double unsecuredRecoveriesDownturn = 0;

                if (lgdAssumptionColumn == 0)
                {
                    unsecuredRecoveriesBest = best_downTurn_Assumption.Days_0;
                    unsecuredRecoveriesDownturn = best_downTurn_Assumption.Downturn_Days_0;
                }
                if (lgdAssumptionColumn == 90)
                {
                    unsecuredRecoveriesBest = best_downTurn_Assumption.Days_90;
                    unsecuredRecoveriesDownturn = best_downTurn_Assumption.Downturn_Days_90;
                }
                if (lgdAssumptionColumn == 180)
                {
                    unsecuredRecoveriesBest = best_downTurn_Assumption.Days_180;
                    unsecuredRecoveriesDownturn = best_downTurn_Assumption.Downturn_Days_180;
                }
                if (lgdAssumptionColumn == 270)
                {
                    unsecuredRecoveriesBest = best_downTurn_Assumption.Days_270;
                    unsecuredRecoveriesDownturn = best_downTurn_Assumption.Downturn_Days_270;
                }
                if (lgdAssumptionColumn == 360)
                {
                    unsecuredRecoveriesBest = best_downTurn_Assumption.Days_360;
                    unsecuredRecoveriesDownturn = best_downTurn_Assumption.Downturn_Days_360;
                }



                for (int month = 0; month < FrameworkConstants.TempExcelVariable_LIM_MONTH; month++)
                {
                    
                    double monthLifetimeEAD = GetLifetimeEADPerMonth(lifetimeEAD, contractId, month);  //Excel lifetimeEAD!F

                    double monthCreditIndexBest = GetCreditIndexPerMonth(creditIndex, month, ECL_Scenario.Best);           // Excel $O$3
                    double monthCreditIndexOptimistic = GetCreditIndexPerMonth(creditIndex, month, ECL_Scenario.Optimistic);
                    double monthCreditIndexDownturn = GetCreditIndexPerMonth(creditIndex, month, ECL_Scenario.Downturn);

                    double sumLifetimePdsBest = ComputeLifetimeRedefaultPdValuePerMonth(lifetimePdBest, pdGroup, month);   //Excel Sum(OFFSET(PD_BE, $C8-1, 1, 1, O$7))
                    double sumLifetimePdsOptimistic = ComputeLifetimeRedefaultPdValuePerMonth(lifetimePdOptimistic, pdGroup, month);   //Excel Sum(OFFSET(PD_BE, $C8-1, 1, 1, O$7))
                    double sumLifetimePdsDownturn = ComputeLifetimeRedefaultPdValuePerMonth(lifetimePdDownturn, pdGroup, month);   //Excel Sum(OFFSET(PD_BE, $C8-1, 1, 1, O$7))

                    double sumRedefaultPdsBest = ComputeLifetimeRedefaultPdValuePerMonth(redefaultPdBest, pdGroup, month); //Excel SUM(OFFSET(RD_PD_BE, $C8-1, 1, 1, O$7)))
                    double sumRedefaultPdsOptimistic = ComputeLifetimeRedefaultPdValuePerMonth(redefaultPdOptimistic, pdGroup, month);
                    double sumRedefaultPdsDownturn = ComputeLifetimeRedefaultPdValuePerMonth(redefaultPdDownturn, pdGroup, month);

                    double lifetimeCollateralValueBest = ComputeLifetimeCollateralValuePerMonth(lifetimeCollateralBest, contractId, month);  // Excel 'Lifetime Collateral (BE)'!E4
                    double lifetimeCollateralValueOptimistic = ComputeLifetimeCollateralValuePerMonth(lifetimeCollateralOptimistic, contractId, month);  // Excel 'Lifetime Collateral (BE)'!E4
                    double lifetimeCollateralValueDownturn = ComputeLifetimeCollateralValuePerMonth(lifetimeCollateralDownturn, contractId, month);  // Excel 'Lifetime Collateral (BE)'!E4


                    double month1pdValueBest = ComputeLifetimeRedefaultPdValuePerMonth(lifetimePdBest, pdGroup, 1);  // Excel INDEX(PD_BE,$C8, 2)
                    double month1pdValueOptimistic = ComputeLifetimeRedefaultPdValuePerMonth(lifetimePdBest, pdGroup, 1);
                    double month1pdValueDownturn = ComputeLifetimeRedefaultPdValuePerMonth(lifetimePdBest, pdGroup, 1);

                    double resultUsingMonth1pdValueBest = month1pdValueBest == 1.0 ? 1.0 : 0.0; // IF(INDEX(PD_BE,$C8, 2) = 1,1,0),
                    double resultUsingMonth1pdValueOptimistic = month1pdValueOptimistic == 1.0 ? 1.0 : 0.0;
                    double resultUsingMonth1pdValueDownturn = month1pdValueDownturn == 1.0 ? 1.0 : 0.0;

                    double redefaultCalculationBest = (redefaultLifetimePd - sumRedefaultPdsBest) / (1 - sumLifetimePdsBest);
                    double redefaultCalculationOptimistic = (redefaultLifetimePd - sumRedefaultPdsOptimistic) / (1 - sumLifetimePdsOptimistic);
                    double redefaultCalculationDownturn = (redefaultLifetimePd - sumRedefaultPdsDownturn) / (1 - sumLifetimePdsDownturn);

                    double maxRedefaultPdValueBest = Math.Max(redefaultCalculationBest, 0.0);
                    double maxRedefaultPdValueOptimistic = Math.Max(redefaultCalculationOptimistic, 0.0);
                    double maxRedefaultPdValueDownturn = Math.Max(redefaultCalculationDownturn, 0.0);

                    double ifSumLifetimePdBest = sumLifetimePdsBest == 1.0 ? resultUsingMonth1pdValueBest : maxRedefaultPdValueBest;
                    double ifSumLifetimePdOptimistic = sumLifetimePdsOptimistic == 1.0 ? resultUsingMonth1pdValueOptimistic : maxRedefaultPdValueOptimistic;
                    double ifSumLifetimePdDownturn = sumLifetimePdsDownturn == 1.0 ? resultUsingMonth1pdValueDownturn : maxRedefaultPdValueDownturn;

                    double checkForMonth0Best = month == 0.0 ? redefaultLifetimePd : ifSumLifetimePdBest;
                    double checkForMonth0Optimistic = month == 0.0 ? redefaultLifetimePd : ifSumLifetimePdOptimistic;
                    double checkForMonth0Downturn = month == 0.0 ? redefaultLifetimePd : ifSumLifetimePdDownturn;

                    double checkForStage1Best = loanStage != 1.0 ? cureRates * checkForMonth0Best : 0.0;
                    double checkForStage1Optimistic = loanStage != 1.0 ? cureRates * checkForMonth0Optimistic : 0.0;
                    double checkForStage1Downturn = loanStage != 1.0 ? cureRates * checkForMonth0Downturn : 0.0;

                    double maxCurerateResultBest = Math.Max((1.0 - cureRates) + checkForStage1Best, 0.0);
                    double maxCurerateResultOptimistic = Math.Max((1.0 - cureRates) + checkForStage1Optimistic, 0.0);
                    double maxCurerateResultDownturn = Math.Max((1.0 - cureRates) + checkForStage1Downturn, 0.0);

                    double minMaxCureRateResultBest = Math.Min(maxCurerateResultBest, 1.0);
                    double minMaxCureRateResultOptimistic = Math.Min(maxCurerateResultOptimistic, 1.0);
                    double minMaxCureRateResultDownturn = Math.Min(maxCurerateResultDownturn, 1.0);
                    ///
                    double lifetimeCollateralForMonthCorBest = lifetimeCollateralValueBest * (1 - costOfRecovery);
                    double lifetimeCollateralForMonthCorOptimistic = lifetimeCollateralValueOptimistic * (1 - costOfRecovery);
                    double lifetimeCollateralForMonthCorDownturn = lifetimeCollateralValueDownturn * (1 - costOfRecovery);

                    double min_gvalue_glevel = Math.Min(guaranteeValue, guaranteeLevel * monthLifetimeEAD);
                    double gLgd_gPd = (1 - guarantorLgd * guarantorPd);

                    double multiplerMinCollBest = (gLgd_gPd * min_gvalue_glevel) + lifetimeCollateralForMonthCorBest;
                    double multiplerMinCollOptimistic = (gLgd_gPd * min_gvalue_glevel) + lifetimeCollateralForMonthCorOptimistic;
                    double multiplerMinCollDownturn = (gLgd_gPd * min_gvalue_glevel) + lifetimeCollateralForMonthCorDownturn;
                    ///

                    //xxxxxxxxxxxxxxxxxxxxxxxx
                    double creditIndexHurdle = 0;
                    try { creditIndexHurdle = Convert.ToDouble(impairmentAssumptions.FirstOrDefault(x => x.Key == ImpairmentRowKeys.CreditIndexThreshold).Value); } catch { }
                    //try { creditIndexHurdle = Convert.ToDouble(impairmentAssumptions.FirstOrDefault(x => x.Key == ImpairmentRowKeys.CreditIndexThreshold).Value); } catch { };

                    double ifCreditIndexHurdleBest = 0;
                    if (monthCreditIndexBest > creditIndexHurdle)
                    {
                        ifCreditIndexHurdleBest = ((1 - unsecuredRecoveriesDownturn) * multiplerMinCollBest) + (unsecuredRecoveriesDownturn * monthLifetimeEAD);
                    }
                    else
                    {
                        ifCreditIndexHurdleBest = ((1 - unsecuredRecoveriesBest) * multiplerMinCollBest) + (unsecuredRecoveriesBest * monthLifetimeEAD);
                    }

                    double ifCreditIndexHurdleOptimistic=0;
                    if (monthCreditIndexOptimistic > creditIndexHurdle)
                    {
                        ifCreditIndexHurdleOptimistic = ((1 - unsecuredRecoveriesDownturn) * multiplerMinCollOptimistic) + (unsecuredRecoveriesDownturn * monthLifetimeEAD);
                    }
                    else
                    {
                        ifCreditIndexHurdleOptimistic = ((1 - unsecuredRecoveriesBest) * multiplerMinCollOptimistic) + (unsecuredRecoveriesBest * monthLifetimeEAD);
                    }

                    double ifCreditIndexHurdleDownturn=0;
                    if (monthCreditIndexDownturn > creditIndexHurdle)
                    {
                        ifCreditIndexHurdleDownturn = ((1 - unsecuredRecoveriesDownturn) * multiplerMinCollDownturn) + (unsecuredRecoveriesDownturn * monthLifetimeEAD);
                    }
                    else
                    {
                        ifCreditIndexHurdleDownturn = ((1 - unsecuredRecoveriesBest) * multiplerMinCollDownturn) + (unsecuredRecoveriesBest * monthLifetimeEAD);
                    }


                    double maxCreditIndexHurdleBest = Math.Max(1 - (ifCreditIndexHurdleBest) / monthLifetimeEAD, 0);
                    double maxCreditIndexHurdleOptimistic = Math.Max(1 - (ifCreditIndexHurdleOptimistic) / monthLifetimeEAD, 0);
                    double maxCreditIndexHurdleDownturn = Math.Max(1 - (ifCreditIndexHurdleDownturn) / monthLifetimeEAD, 0);

                    double minMaxCreditIndexHurdleBest = Math.Min(maxCreditIndexHurdleBest, 1);
                    double minMaxCreditIndexHurdleOptimistic = Math.Min(maxCreditIndexHurdleOptimistic, 1);
                    double minMaxCreditIndexHurdleDownturn = Math.Min(maxCreditIndexHurdleDownturn, 1);

                    double lifetimeLgdValueBest = monthLifetimeEAD == 0 ? 0 : minMaxCureRateResultBest * minMaxCreditIndexHurdleBest;
                    double lifetimeLgdValueOptimistic = monthLifetimeEAD == 0 ? 0 : minMaxCureRateResultOptimistic * minMaxCreditIndexHurdleOptimistic;
                    double lifetimeLgdValueDownturn = monthLifetimeEAD == 0 ? 0 : minMaxCureRateResultDownturn * minMaxCreditIndexHurdleDownturn;


                    var newRowBest = new LifetimeLgd();
                    
                    

                    newRowBest.ContractId = contractId;
                    newRowBest.PdIndex = pdGroup;
                    newRowBest.LgdIndex = segment + "_" + productType;
                    newRowBest.RedefaultLifetimePD = redefaultLifetimePd;
                    newRowBest.CureRate = cureRates;
                    newRowBest.UrBest = unsecuredRecoveriesBest;
                    newRowBest.URDownturn = unsecuredRecoveriesDownturn;
                    newRowBest.Cor = costOfRecovery;
                    newRowBest.GPd = guarantorPd;
                    newRowBest.GuarantorLgd = guarantorLgd;
                    newRowBest.GuaranteeValue = guaranteeValue;
                    newRowBest.GuaranteeLevel = guaranteeLevel;
                    newRowBest.Stage = loanStage;
                    newRowBest.Month = month;

                    newRowBest.Ecl_Scenerio = ECL_Scenario.Best;
                    newRowBest.Value = lifetimeLgdValueBest;
                    _lifetimeLGD.Add(newRowBest);

                    var newRowOptimistic = new LifetimeLgd();
                    newRowOptimistic.ContractId = contractId;
                    newRowOptimistic.PdIndex = pdGroup;
                    newRowOptimistic.LgdIndex = segment + "_" + productType;
                    newRowOptimistic.RedefaultLifetimePD = redefaultLifetimePd;
                    newRowOptimistic.CureRate = cureRates;
                    newRowOptimistic.UrBest = unsecuredRecoveriesBest;
                    newRowOptimistic.URDownturn = unsecuredRecoveriesDownturn;
                    newRowOptimistic.Cor = costOfRecovery;
                    newRowOptimistic.GPd = guarantorPd;
                    newRowOptimistic.GuarantorLgd = guarantorLgd;
                    newRowOptimistic.GuaranteeValue = guaranteeValue;
                    newRowOptimistic.GuaranteeLevel = guaranteeLevel;
                    newRowOptimistic.Stage = loanStage;
                    newRowOptimistic.Month = month;
                    newRowOptimistic.Ecl_Scenerio = ECL_Scenario.Optimistic;
                    newRowOptimistic.Value = lifetimeLgdValueOptimistic;
                    _lifetimeLGD.Add(newRowOptimistic);


                    var newRowDownturn = new LifetimeLgd();

                    newRowDownturn.ContractId = contractId;
                    newRowDownturn.PdIndex = pdGroup;
                    newRowDownturn.LgdIndex = segment + "_" + productType;
                    newRowDownturn.RedefaultLifetimePD = redefaultLifetimePd;
                    newRowDownturn.CureRate = cureRates;
                    newRowDownturn.UrBest = unsecuredRecoveriesBest;
                    newRowDownturn.URDownturn = unsecuredRecoveriesDownturn;
                    newRowDownturn.Cor = costOfRecovery;
                    newRowDownturn.GPd = guarantorPd;
                    newRowDownturn.GuarantorLgd = guarantorLgd;
                    newRowDownturn.GuaranteeValue = guaranteeValue;
                    newRowDownturn.GuaranteeLevel = guaranteeLevel;
                    newRowDownturn.Stage = loanStage;
                    newRowDownturn.Month = month;

                    newRowDownturn.Ecl_Scenerio = ECL_Scenario.Downturn;
                    newRowDownturn.Value = lifetimeLgdValueDownturn;
                    _lifetimeLGD.Add(newRowDownturn);

                    //xxxxxxxxxxxaaaaaaaaaa
                }
            }
            lifetimeLGD.AddRange(_lifetimeLGD);
        }

        private double ComputeLifetimeCollateralValuePerMonth(List<LifetimeCollateral> lifetimeCollateral, string contractId, int month)
        {

            var lifetimeCollateralValue = lifetimeCollateral.FirstOrDefault(x => x.ContractId == contractId);

            //xxxxxxxxxxxxxxx && x.ProjectionMonth == month);
            if (lifetimeCollateralValue == null)
            {
                return lifetimeCollateral[0].ProjectionValue;
            }
            return lifetimeCollateralValue.ProjectionValue;
        }

        private double ComputeLifetimeRedefaultPdValuePerMonth(List<LifeTimeObject> redefaultPd, string pdGroup, int month)
        {
            double[] pds = redefaultPd.Where(x => x.PdGroup == pdGroup
                                                              && x.Month >= 1
                                                              && x.Month <= month)
                                                       .Select(x =>
                                                       {
                                                           return x.Value;
                                                       }).ToArray();
            return pds.Aggregate(0.0, (acc, x) => acc + x);
        }

        private double GetCreditIndexPerMonth(List<CreditIndex_Output> creditIndex, int month, ECL_Scenario _scenario)
        {

            var _creditIndx = creditIndex.FirstOrDefault(x => x.ProjectionMonth == (month > 60 ? 60 : month));

            if (_scenario == ECL_Scenario.Best)
                return _creditIndx.BestEstimate;

            if (_scenario == ECL_Scenario.Downturn)
                return _creditIndx.Downturn;

            if (_scenario == ECL_Scenario.Optimistic)
                return _creditIndx.Optimistic;

            return 0;
        }

        private double GetLifetimeEADPerMonth(List<LifetimeEad> lifetimeEAD, string contractId, int month)
        {
            //return lifetimeEAD.FirstOrDefault(x => x.ContractId == contractId && x.ProjectionMonth == month).ProjectionValue;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            try { return lifetimeEAD.FirstOrDefault(x => x.ContractId == contractId && x.ProjectionMonth == month).ProjectionValue; }
            catch { try { return lifetimeEAD.FirstOrDefault().ProjectionValue; } catch { return 0; } }

        }


        public List<EclAssumptions> GetECLLgdAssumptions()
        {
            var qry = Queries.eclLGDAssumptions(this._eclId, this._eclType);
            var dt = DataAccess.i.GetData(qry);
            var eclAssumptions = new List<EclAssumptions>();

            foreach (DataRow dr in dt.Rows)
            {
                eclAssumptions.Add(DataAccess.i.ParseDataToObject(new EclAssumptions(), dr));
            }

            return eclAssumptions;
        }
        
        public List<EclAssumptions> GetECLFrameworkAssumptions()
        {
            var qry = Queries.eclFrameworkAssumptions(this._eclId, this._eclType);
            var dt = DataAccess.i.GetData(qry);
            var eclAssumptions = new List<EclAssumptions>();

            foreach (DataRow dr in dt.Rows)
            {
                eclAssumptions.Add(DataAccess.i.ParseDataToObject(new EclAssumptions(), dr));
            }

            return eclAssumptions;
        }
        protected List<Loanbook_Data> GetContractData()
        {
            _lifetimeEadWorkings= new LifetimeEadWorkings(this._eclId, this._eclType);
            return _lifetimeEadWorkings.GetLoanBookData();
        }
        protected List<PdMappings> GetPdIndexMappingResult()
        {
            _pdMapping = new PDMapping(this._eclId, this._eclType);
            return _pdMapping.GetPdMapping();
        }
        protected List<LgdInputAssumptions_UnsecuredRecovery> GetLgdAssumptionsData()
        {
            //var qry = Queries.LGD_InputAssumptions_UnsecuredRecovery(this._eclId, this._eclType);
            //var dt = DataAccess.i.GetData(qry);
            var ldg_inputassumption = new List<LgdInputAssumptions_UnsecuredRecovery>();

            var pdCali = new CalibrationInput_PD_CR_RD_Processor().GetPDRedefaultFactorCureRate(this._eclId, this._eclType);
            var rcvCaliRate = new CalibrationInput_LGD_RecoveryRate_Processor().GetLGDRecoveryRateData(this._eclId, this._eclType);

            //foreach (DataRow dr in dt.Rows)
            for(int i=0; i<1; i++)
            {
                var _lgdAssumption = new LgdInputAssumptions_UnsecuredRecovery();// DataAccess.i.ParseDataToObject(new LgdInputAssumptions_UnsecuredRecovery(), dr);
                _lgdAssumption.Days_90 = rcvCaliRate - (rcvCaliRate / 4);
                _lgdAssumption.Days_180 = _lgdAssumption.Days_90 - (rcvCaliRate / 4);
                _lgdAssumption.Days_270 = _lgdAssumption.Days_180 - (rcvCaliRate / 4);
                _lgdAssumption.Days_360 = _lgdAssumption.Days_270 - (rcvCaliRate / 4);

                _lgdAssumption.Downturn_Days_0 = 1 - ((1 - rcvCaliRate) * 0.92 + 0.08);
                _lgdAssumption.Downturn_Days_90 = 1 - ((1 - _lgdAssumption.Days_90) * 0.92 + 0.08);
                _lgdAssumption.Downturn_Days_180 = 1 - ((1 - _lgdAssumption.Days_180) * 0.92 + 0.08);
                _lgdAssumption.Downturn_Days_270 = 1 - ((1 - _lgdAssumption.Days_270) * 0.92 + 0.08);
                _lgdAssumption.Downturn_Days_360 = 1 - ((1 - _lgdAssumption.Days_360) * 0.92 + 0.08);

                _lgdAssumption.Cure_Rate = pdCali[1];
                ldg_inputassumption.Add(_lgdAssumption);
            }

            var _ldg_inputassumption = new List<LgdInputAssumptions_UnsecuredRecovery>();
            foreach (var itm in ldg_inputassumption)
            {
                itm.Segment_Product_Type = itm.Segment_Product_Type ?? "";
                if (itm.Segment_Product_Type.ToLower().EndsWith("curerate"))
                {
                    itm.Days_0 = 0;
                    var sub_itm=ldg_inputassumption.FirstOrDefault(o => o.Segment_Product_Type.ToLower().Contains(itm.Segment_Product_Type.ToLower().Replace("curerate", "timeIndefault")));
                    if(sub_itm!=null)
                    {
                        itm.Days_0 = sub_itm.Days_0;
                    }
                    _ldg_inputassumption.Add(itm);
                }
            }
            return ldg_inputassumption;
        }

        protected List<SicrInputs> GetSicrResult()
        {
            _sicrInputs = new SicrInputWorkings(this._eclId, this._eclType);
            return _sicrInputs.GetSircInputResult();
        }

        protected List<StageClassification> GetStagingClassificationResult(List<Loanbook_Data> loanbook)
        {
            _sicrWorkings= new SicrWorkings(this._eclId, this._eclType);
            return _sicrWorkings.ComputeStageClassification(loanbook);
        }
        protected List<LifetimeEad> GetLifetimeEadResult(List<Loanbook_Data> loanbook)
        {
            _lifetimeEadWorkings= new LifetimeEadWorkings(this._eclId, this._eclType);
            return _lifetimeEadWorkings.ComputeLifetimeEad(loanbook);
        }
        protected List<LifeTimeObject> GetScenarioLifetimePdResult(ECL_Scenario _scenario)
        {
            
            var qry = "";
            switch (_scenario)
            {
                case ECL_Scenario.Best:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.PdLifetimeBests_Table(this._eclType), this._eclId, this._eclType);
                    break;
                case ECL_Scenario.Optimistic:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.PdLifetimeOptimistics_Table(this._eclType), this._eclId, this._eclType);
                    break;
                case ECL_Scenario.Downturn:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.PdLifetimeDownturns_Table(this._eclType), this._eclId, this._eclType);
                    break;
                default:
                    return null;
            }

            var _lstRaw = DataAccess.i.GetData(qry);

            var lifetimePd = new List<LifeTimeObject>();
            foreach (DataRow dr in _lstRaw.Rows)
            {
                lifetimePd.Add(DataAccess.i.ParseDataToObject(new LifeTimeObject(), dr));
            }
            Log4Net.Log.Info("Completed pass data to object");

            return lifetimePd;
        }
        protected List<LifeTimeObject> GetScenarioRedfaultLifetimePdResult(ECL_Scenario _scenario)
        {
            var qry = "";
            switch (_scenario)
            {
                case ECL_Scenario.Best:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.PdRedefaultLifetimeBests_Table(this._eclType), this._eclId, this._eclType);
                    break;
                case ECL_Scenario.Optimistic:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.PdRedefaultLifetimeOptimistics_Table(this._eclType), this._eclId, this._eclType);
                    break;
                case ECL_Scenario.Downturn:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.PdRedefaultLifetimeDownturns_Table(this._eclType), this._eclId, this._eclType);
                    break;
                default:
                    return null;
            }

            var _lstRaw = DataAccess.i.GetData(qry);

            var lifetimePd = new List<LifeTimeObject>();
            foreach (DataRow dr in _lstRaw.Rows)
            {
                lifetimePd.Add(DataAccess.i.ParseDataToObject(new LifeTimeObject(), dr));
            }
            Log4Net.Log.Info("Completed pass data to object");

            return lifetimePd;
        }
        protected List<CreditIndex_Output> GetCreditRiskResult()
        {
            _creditIndex = new CreditIndex(this._eclId, this._eclType);
            return _creditIndex.GetCreditIndexResult();
        }
        protected List<LifetimeCollateral> GetScenarioLifetimeCollateralResult(List<Loanbook_Data> loanbook, ECL_Scenario _scenario)
        {
            _scenarioLifetimeCollateral = new ScenarioLifetimeCollateral(_scenario, this._eclId, this._eclType);
            return _scenarioLifetimeCollateral.ComputeLifetimeCollateral(loanbook);
        }
    }
}
