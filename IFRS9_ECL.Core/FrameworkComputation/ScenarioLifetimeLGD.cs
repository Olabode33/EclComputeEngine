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

        public ScenarioLifetimeLGD(Guid eclId, ECL_Scenario scenario, EclType eclType)
        {
            this._eclId = eclId;
            this._scenario = scenario;
            this._eclType = eclType;
            _sicrInputs = new SicrInputWorkings(this._eclId, _eclType);
            _sicrWorkings = new SicrWorkings(this._eclId, _eclType);
            _lifetimeEadWorkings = new LifetimeEadWorkings(this._eclId, _eclType);
            _scenarioLifetimeCollateral = new ScenarioLifetimeCollateral(this._scenario, this._eclId, _eclType);
            _pdMapping = new PDMapping(this._eclId, _eclType);
            _creditIndex = new CreditIndex(this._eclId, _eclType);

        }

        public ScenarioLifetimeLGD(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
        }

        protected ECL_Scenario _scenario;
        protected SicrWorkings _sicrWorkings;
        protected LifetimeEadWorkings _lifetimeEadWorkings;
        protected ScenarioLifetimeCollateral _scenarioLifetimeCollateral;
        protected PDMapping _pdMapping;
        protected SicrInputWorkings _sicrInputs;
        protected CreditIndex _creditIndex;

        public void Run()
        {
            //var dataTable = ComputeLifetimeLGD();
            string stop = "Ma te";
        }

        public List<LifetimeLgd> ComputeLifetimeLGD(List<Loanbook_Data> loanbook)
        {
            var lifetimeLGD = new List<LifetimeLgd>();

            var contractData = new ProcessECL_LGD(this._eclId, this._eclType).GetLgdContractData(loanbook);
            var pdMapping = GetPdIndexMappingResult();
            var lgdAssumptions = GetLgdAssumptionsData();
            var sicrInput = GetSicrResult();
            var stageClassification = GetStagingClassificationResult(loanbook);
            var impairmentAssumptions = GetECLFrameworkAssumptions();
            var lifetimePd = GetScenarioLifetimePdResult();
            var redefaultPd = GetScenarioRedfaultLifetimePdResult();
            var lifetimeEAD = GetLifetimeEadResult(loanbook);
            var creditIndex = GetCreditRiskResult();
            var lifetimeCollateral = GetScenarioLifetimeCollateralResult(loanbook);

            //xxxxxxxxxxxxx
            //try { Convert.ToInt32(impairmentAssumptions.FirstOrDefault(x => x.Key == ImpairmentRowKeys.ForwardTransitionStage2to3).Value); } catch { }
            int stage2to3Forward = 0;
            stage2to3Forward=Convert.ToInt32(impairmentAssumptions.FirstOrDefault(x => x.Key == ImpairmentRowKeys.ForwardTransitionStage2to3).Value);
            //double creditIndexHurdle = Convert.ToDouble(GetImpairmentAssumptionValue(impairmentAssumptions, ImpairmentRowKeys.CreditIndexThreshold));

            foreach (var row in contractData)
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
                var best_downTurn_Assumption = lgdAssumptions.FirstOrDefault(o => o.Segment_Product_Type == $"{segment}_{productType}");
                //if (best_downTurn_Assumption == null)
                //{
                //    best_downTurn_Assumption = lgdAssumptions.FirstOrDefault();
                //}
                double cureRates = best_downTurn_Assumption.Cure_Rate;

                long lgdAssumptionColumn = Math.Max(daysPastDue - stage2to3Forward, 0);

                double unsecuredRecoveriesBest = 0;
                double unsecuredRecoveriesDownturn = 0;

                if (lgdAssumptionColumn==0)
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
                    double monthCreditIndex = GetCreditIndexPerMonth(creditIndex, month);           // Excel $O$3
                    double sumLifetimePds = ComputeLifetimeRedefaultPdValuePerMonth(lifetimePd, pdGroup, month);   //Excel Sum(OFFSET(PD_BE, $C8-1, 1, 1, O$7))
                    double sumRedefaultPds = ComputeLifetimeRedefaultPdValuePerMonth(redefaultPd, pdGroup, month); //Excel SUM(OFFSET(RD_PD_BE, $C8-1, 1, 1, O$7)))
                    double lifetimeCollateralValue = ComputeLifetimeCollateralValuePerMonth(lifetimeCollateral, contractId, month);  // Excel 'Lifetime Collateral (BE)'!E4

                    var newRow = new LifetimeLgd();

                    double month1pdValue = ComputeLifetimeRedefaultPdValuePerMonth(lifetimePd, pdGroup, 1);  // Excel INDEX(PD_BE,$C8, 2)
                    double resultUsingMonth1pdValue = month1pdValue == 1.0 ? 1.0 : 0.0; // IF(INDEX(PD_BE,$C8, 2) = 1,1,0),
                    double redefaultCalculation = (redefaultLifetimePd - sumRedefaultPds) / (1 - sumLifetimePds);
                    double maxRedefaultPdValue = Math.Max(redefaultCalculation, 0.0);
                    double ifSumLifetimePd = sumLifetimePds == 1.0 ? resultUsingMonth1pdValue : maxRedefaultPdValue;
                    double checkForMonth0 = month == 0.0 ? redefaultLifetimePd : ifSumLifetimePd;
                    double checkForStage1 = loanStage != 1.0 ? cureRates * checkForMonth0 : 0.0;
                    double maxCurerateResult = Math.Max((1.0 - cureRates) + checkForStage1, 0.0);  //result not in double
                    double minMaxCureRateResult = Math.Min(maxCurerateResult, 1.0);
                    ///
                    double lifetimeCollateralForMonthCor = lifetimeCollateralValue * (1 - costOfRecovery);
                    double min_gvalue_glevel = Math.Min(guaranteeValue, guaranteeLevel * monthLifetimeEAD);
                    double gLgd_gPd = (1 - guarantorLgd * guarantorPd);
                    double multiplerMinColl = (gLgd_gPd * min_gvalue_glevel) + lifetimeCollateralForMonthCor;
                    ///

                    //xxxxxxxxxxxxxxxxxxxxxxxx
                    double  creditIndexHurdle= 0;
                    creditIndexHurdle = Convert.ToDouble(impairmentAssumptions.FirstOrDefault(x => x.Key == ImpairmentRowKeys.CreditIndexThreshold).Value);
                    //try { creditIndexHurdle = Convert.ToDouble(impairmentAssumptions.FirstOrDefault(x => x.Key == ImpairmentRowKeys.CreditIndexThreshold).Value); } catch { };

                    double ifCreditIndexHurdle;
                    if (monthCreditIndex > creditIndexHurdle)
                    {
                        ifCreditIndexHurdle = ((1 - unsecuredRecoveriesDownturn) * multiplerMinColl) + (unsecuredRecoveriesDownturn * monthLifetimeEAD);
                    }
                    else
                    {
                        ifCreditIndexHurdle = ((1 - unsecuredRecoveriesBest) * multiplerMinColl) + (unsecuredRecoveriesBest * monthLifetimeEAD);
                    }

                    double maxCreditIndexHurdle = Math.Max(1 - (ifCreditIndexHurdle) / monthLifetimeEAD, 0);
                    double minMaxCreditIndexHurdle = Math.Min(maxCreditIndexHurdle, 1);
                    double lifetimeLgdValue = monthLifetimeEAD == 0 ? 0 : minMaxCureRateResult * minMaxCreditIndexHurdle;


                    newRow.ContractId = contractId;
                    newRow.PdIndex = pdGroup;
                    newRow.LgdIndex = segment + "_" + productType;
                    newRow.RedefaultLifetimePD = redefaultLifetimePd;
                    newRow.CureRate = cureRates;
                    newRow.UrBest = unsecuredRecoveriesBest;
                    newRow.URDownturn = unsecuredRecoveriesDownturn;
                    newRow.Cor = costOfRecovery;
                    newRow.GPd = guarantorPd;
                    newRow.GuarantorLgd = guarantorLgd;
                    newRow.GuaranteeValue = guaranteeValue;
                    newRow.GuaranteeLevel = guaranteeLevel;
                    newRow.Stage = loanStage;
                    newRow.Month = month;
                    newRow.Value = lifetimeLgdValue;
                    lifetimeLGD.Add(newRow);
                    //xxxxxxxxxxxaaaaaaaaaa
                }
            }


            return lifetimeLGD;
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

        private double GetCreditIndexPerMonth(List<CreditIndex_Output> creditIndex, int month)
        {

            var _creditIndx = creditIndex.FirstOrDefault(x => x.ProjectionMonth == (month > 60 ? 60 : month));

            if (this._scenario == ECL_Scenario.Best)
                return _creditIndx.BestEstimate;

            if (this._scenario == ECL_Scenario.Downturn)
                return _creditIndx.Downturn;

            if (this._scenario == ECL_Scenario.Optimistic)
                return _creditIndx.Optimistic;

            return 0;
        }

        private double GetLifetimeEADPerMonth(List<LifetimeEad> lifetimeEAD, string contractId, int month)
        {
            //return lifetimeEAD.FirstOrDefault(x => x.ContractId == contractId && x.ProjectionMonth == month).ProjectionValue;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            try { return lifetimeEAD.FirstOrDefault(x => x.ContractId == contractId && x.ProjectionMonth == month).ProjectionValue; }
            catch { return lifetimeEAD.FirstOrDefault().ProjectionValue; }

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
            return _lifetimeEadWorkings.GetLoanBookData();
        }
        protected List<PdMappings> GetPdIndexMappingResult()
        {
            return _pdMapping.GetPdMapping();
        }
        protected List<LgdInputAssumptions_UnsecuredRecovery> GetLgdAssumptionsData()
        {
            var qry = Queries.LGD_InputAssumptions_UnsecuredRecovery(this._eclId, this._eclType);
            var dt = DataAccess.i.GetData(qry);
            var ldg_inputassumption = new List<LgdInputAssumptions_UnsecuredRecovery>();

            foreach (DataRow dr in dt.Rows)
            {
                var _lgdAssumption = DataAccess.i.ParseDataToObject(new LgdInputAssumptions_UnsecuredRecovery(), dr);
                _lgdAssumption.Days_90 = _lgdAssumption.Days_0 - (_lgdAssumption.Days_0 / 4);
                _lgdAssumption.Days_180 = _lgdAssumption.Days_90 - (_lgdAssumption.Days_0 / 4);
                _lgdAssumption.Days_270 = _lgdAssumption.Days_180 - (_lgdAssumption.Days_0 / 4);
                _lgdAssumption.Days_360 = _lgdAssumption.Days_270 - (_lgdAssumption.Days_0 / 4);

                _lgdAssumption.Downturn_Days_0 = 1 - ((1 - _lgdAssumption.Days_0) * 0.92 + 0.08);
                _lgdAssumption.Downturn_Days_90 = 1 - ((1 - _lgdAssumption.Days_90) * 0.92 + 0.08);
                _lgdAssumption.Downturn_Days_180 = 1 - ((1 - _lgdAssumption.Days_180) * 0.92 + 0.08);
                _lgdAssumption.Downturn_Days_270 = 1 - ((1 - _lgdAssumption.Days_270) * 0.92 + 0.08);
                _lgdAssumption.Downturn_Days_360 = 1 - ((1 - _lgdAssumption.Days_360) * 0.92 + 0.08);

                ldg_inputassumption.Add(_lgdAssumption);
            }

            return ldg_inputassumption;
        }

        protected List<SicrInputs> GetSicrResult()
        {
            return _sicrInputs.GetSircInputResult();
        }

        protected List<StageClassification> GetStagingClassificationResult(List<Loanbook_Data> loanbook)
        {
            return _sicrWorkings.ComputeStageClassification(loanbook);
        }
        protected List<LifetimeEad> GetLifetimeEadResult(List<Loanbook_Data> loanbook)
        {
            return _lifetimeEadWorkings.ComputeLifetimeEad(loanbook);
        }
        protected List<LifeTimeObject> GetScenarioLifetimePdResult()
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
            Console.WriteLine("Completed pass data to object");

            return lifetimePd;
        }
        protected List<LifeTimeObject> GetScenarioRedfaultLifetimePdResult()
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
            Console.WriteLine("Completed pass data to object");

            return lifetimePd;
        }
        protected List<CreditIndex_Output> GetCreditRiskResult()
        {
            return _creditIndex.GetCreditIndexResult();
        }
        protected List<LifetimeCollateral> GetScenarioLifetimeCollateralResult(List<Loanbook_Data> loanbook)
        {
            return _scenarioLifetimeCollateral.ComputeLifetimeCollateral(loanbook);
        }
    }
}
