using IFRS9_ECL.Core.PDComputation;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Framework;
using IFRS9_ECL.Models.PD;
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

        public ScenarioLifetimeLGD(Guid eclId, ECL_Scenario scenario)
        {
            this._eclId = eclId;
            this._scenario = scenario;
            _sicrInputs = new SicrInputWorkings(this._eclId);
            _sicrWorkings = new SicrWorkings(this._eclId);
            _lifetimeEadWorkings = new LifetimeEadWorkings(this._eclId);
            _scenarioLifetimeCollateral = new ScenarioLifetimeCollateral(this._scenario, this._eclId);
            _pdMapping = new PDMapping(this._eclId);
            _creditIndex = new CreditIndex(this._eclId);

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
            var dataTable = ComputeLifetimeLGD();
            string stop = "Ma te";
        }

        public List<LifetimeLgd> ComputeLifetimeLGD()
        {
            var lifetimeLGD = new List<LifetimeLgd>();

            var contractData = GetContractData();
            var pdMapping = GetPdIndexMappingResult();
            var lgdAssumptions = GetLgdAssumptionsData();
            var sicrInput = GetSicrResult();
            var stageClassification = GetStagingClassificationResult();
            var impairmentAssumptions = GetImpairmentAssumptions();
            var lifetimePd = GetScenarioLifetimePdResult();
            var redefaultPd = GetScenarioRedfaultLifetimePdResult();
            var lifetimeEAD = GetLifetimeEadResult();
            var creditIndex = GetCreditRiskResult();
            var lifetimeCollateral = GetScenarioLifetimeCollateralResult();


            int stage2to3Forward = Convert.ToInt32(impairmentAssumptions.FirstOrDefault(x => x.Key == ImpairmentRowKeys.ForwardTransitionStage2to3).Value);
            //double creditIndexHurdle = Convert.ToDouble(GetImpairmentAssumptionValue(impairmentAssumptions, ImpairmentRowKeys.CreditIndexThreshold));

            foreach (var row in contractData)
            {
                string contractId = row.contract_no;
                double costOfRecovery = row.CostOfRecovery;
                double guarantorPd = row.GuaranteePd;
                double guarantorLgd = row.Field<double>(TempLgdContractDataColumns.GuaranteeLgd);
                double guaranteeValue = row.Field<double>(TempLgdContractDataColumns.GuaranteeValue);
                double guaranteeLevel = row.Field<double>(TempLgdContractDataColumns.GuaranteeLevel);

                int loanStage = stageClassification.AsEnumerable()
                                                   .FirstOrDefault(x => x.Field<string>(StageClassificationColumns.ContractId) == contractId)
                                                   .Field<int>(StageClassificationColumns.Stage);

                DataRow pdMappingRow = pdMapping.AsEnumerable().FirstOrDefault(x => x.Field<string>(LoanBookColumns.ContractID) == contractId);
                string pdGroup = pdMappingRow.Field<string>(PdMappingColumns.PdGroup);
                string segment = pdMappingRow.Field<string>(LoanBookColumns.Segment);
                string productType = pdMappingRow.Field<string>(LoanBookColumns.ProductType);

                DataRow sicrInputRow = sicrInput.AsEnumerable().FirstOrDefault(x => x.Field<string>(SicrInputsColumns.ContractId) == contractId);
                double redefaultLifetimePd = sicrInputRow.Field<double>(SicrInputsColumns.RedefaultLifetimePd);
                long daysPastDue = sicrInputRow.Field<long>(SicrInputsColumns.DaysPastDue);

                DataRow bestAssumption = FindDataRowInTable(lgdAssumptions, LgdInputAssumptionColumns.SegementProductType, segment + "_" + productType,
                                                                            LgdInputAssumptionColumns.Scenario, TempEclData.ScenarioBest);
                DataRow downturnAssumption = FindDataRowInTable(lgdAssumptions, LgdInputAssumptionColumns.SegementProductType, segment + "_" + productType,
                                                                                LgdInputAssumptionColumns.Scenario, TempEclData.ScenarioDownturn);

                double cureRates = bestAssumption.Field<double>(LgdInputAssumptionColumns.CureRate);

                long lgdAssumptionColumn = Math.Max(daysPastDue - stage2to3Forward, 0);
                double unsecuredRecoveriesBest = bestAssumption.Field<double>(lgdAssumptionColumn.ToString());
                double unsecuredRecoveriesDownturn = downturnAssumption.Field<double>(lgdAssumptionColumn.ToString());


                for (int month = 0; month < TempEclData.TempExcelVariable_LIM_MONTH; month++)
                {

                    if (month == 10)
                    {
                        string stop = "stop";
                    }

                    double monthLifetimeEAD = GetLifetimeEADPerMonth(lifetimeEAD, contractId, month);  //Excel lifetimeEAD!F
                    double monthCreditIndex = GetCreditIndexPerMonth(creditIndex, month);           // Excel $O$3
                    double sumLifetimePds = ComputeLifetimeRedefaultPdValuePerMonth(lifetimePd, pdGroup, month);   //Excel Sum(OFFSET(PD_BE, $C8-1, 1, 1, O$7))
                    double sumRedefaultPds = ComputeLifetimeRedefaultPdValuePerMonth(redefaultPd, pdGroup, month); //Excel SUM(OFFSET(RD_PD_BE, $C8-1, 1, 1, O$7)))
                    double lifetimeCollateralValue = ComputeLifetimeCollateralValuePerMonth(lifetimeCollateral, contractId, month);  // Excel 'Lifetime Collateral (BE)'!E4



                    DataRow newRow = lifetimeLGD.NewRow();

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

                    double creditIndexHurdle = Convert.ToDouble(GetImpairmentAssumptionValue(impairmentAssumptions, ImpairmentRowKeys.CreditIndexThreshold));

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


                    newRow[LifetimeLgdColumns.ContractId] = contractId;
                    newRow[LifetimeLgdColumns.PdIndex] = pdGroup;
                    newRow[LifetimeLgdColumns.LgdIndex] = segment + "_" + productType;
                    newRow[LifetimeLgdColumns.RedefaultLifetimePD] = redefaultLifetimePd;
                    newRow[LifetimeLgdColumns.CureRate] = cureRates;
                    newRow[LifetimeLgdColumns.UrBest] = unsecuredRecoveriesBest;
                    newRow[LifetimeLgdColumns.URDownturn] = unsecuredRecoveriesDownturn;
                    newRow[LifetimeLgdColumns.Cor] = costOfRecovery;
                    newRow[LifetimeLgdColumns.GPd] = guarantorPd;
                    newRow[LifetimeLgdColumns.GuarantorLgd] = guarantorLgd;
                    newRow[LifetimeLgdColumns.GuaranteeValue] = guaranteeValue;
                    newRow[LifetimeLgdColumns.GuaranteeLevel] = guaranteeLevel;
                    newRow[LifetimeLgdColumns.Stage] = loanStage;
                    newRow[LifetimeLgdColumns.Month] = month;
                    newRow[LifetimeLgdColumns.Value] = lifetimeLgdValue;
                    lifetimeLGD.Rows.Add(newRow);
                }
            }


            return lifetimeLGD;
        }

        protected DataRow FindDataRowInTable(DataTable dataTable, string searchColumn, string searchValue, string searchColumn2, string searchValue2)
        {
            DataRow row = dataTable.AsEnumerable().FirstOrDefault(x => x.Field<string>(searchColumn) == searchValue
                                                                    && x.Field<string>(searchColumn2) == searchValue2);
            return row;
        }
        protected DataRow FindDataRowInTable(DataTable dataTable, string searchColumn, string searchValue, string searchColumn2, int searchValue2)
        {
            DataRow row = dataTable.AsEnumerable().FirstOrDefault(x => x.Field<string>(searchColumn) == searchValue
                                                                    && x.Field<int>(searchColumn2) == searchValue2);
            return row;
        }
        protected double GetLifetimeEADPerMonth(DataTable lifetimeEAD, string contractId, int month)
        {
            return lifetimeEAD.AsEnumerable().FirstOrDefault(x => x.Field<string>(LifetimeEadColumns.ContractId) == contractId
                                                                                                      && x.Field<int>(LifetimeEadColumns.ProjectionMonth) == month)
                                                                                    .Field<double>(LifetimeEadColumns.ProjectionValue);
        }

        protected double GetCreditIndexPerMonth(DataTable creditIndex, int month)
        {
            string creditIndexColumn = CreditIndexColumns.BestEstimate;

            switch (_scenario)
            {
                case TempEclData.ScenarioBest:
                    creditIndexColumn = CreditIndexColumns.BestEstimate;
                    break;
                case TempEclData.ScenarioOptimistic:
                    creditIndexColumn = CreditIndexColumns.Optimistic;
                    break;
                case TempEclData.ScenarioDownturn:
                    creditIndexColumn = CreditIndexColumns.Downturn;
                    break;
                default:
                    creditIndexColumn = CreditIndexColumns.BestEstimate;
                    break;
            }

            return creditIndex.AsEnumerable().FirstOrDefault(x => x.Field<string>(CreditIndexColumns.ProjectionMonth) == (month > 60 ? "60" : month.ToString()))
                                                                                    .Field<double>(creditIndexColumn);
        }

        protected double ComputeLifetimeRedefaultPdValuePerMonth(DataTable lifetimePd, string pdGroup, int month)
        {
            double[] pds = lifetimePd.AsEnumerable().Where(x => x.Field<string>(MarginalLifetimeRedefaultPdColumns.PdGroup) == pdGroup
                                                                          && x.Field<long>(MarginalLifetimeRedefaultPdColumns.ProjectionMonth) >= 1
                                                                          && x.Field<long>(MarginalLifetimeRedefaultPdColumns.ProjectionMonth) <= month)
                                                                   .Select(x =>
                                                                   {
                                                                       return x.Field<double>(MarginalLifetimeRedefaultPdColumns.Value);
                                                                   }).ToArray();
            return pds.Aggregate(0.0, (acc, x) => acc + x);
        }

        protected double ComputeLifetimeCollateralValuePerMonth(DataTable lifetimeCollateral, string contractId, int month)
        {
            double lifetimeCollateralValue = lifetimeCollateral.AsEnumerable().FirstOrDefault(x => x.Field<string>(LifetimeCollateralColumns.ContractId) == contractId
                                                                                                                    && x.Field<int>(LifetimeCollateralColumns.ProjectionMonth) == month)
                                                                                                  .Field<double>(LifetimeCollateralColumns.ProjectionValue);
            return lifetimeCollateralValue;
        }
        protected string GetImpairmentAssumptionValue(DataTable assumptions, string assumptionKey)
        {
            return assumptions.AsEnumerable()
                              .FirstOrDefault(x => x.Field<string>(ImpairmentRowKeys.ColumnAssumption) == assumptionKey)
                              .Field<string>(ImpairmentRowKeys.ColumnValue);
        }

        protected List<EclAssumptions> GetImpairmentAssumptions()
        {
            var qry = Queries.eclAssumptions(this._eclId);
            var dt = DataAccess.i.GetData(qry);
            var eclAssumptions = new List<EclAssumptions>();

            foreach (DataRow dr in dt.Rows)
            {
                eclAssumptions.Add(DataAccess.i.ParseDataToObject(new EclAssumptions(), dr));
            }

            return eclAssumptions;
        }
        protected List<Refined_Raw_Retail_Wholesale> GetContractData()
        {
            return _lifetimeEadWorkings.GetRefinedLoanBookData();
        }
        protected List<WholesalePdMappings> GetPdIndexMappingResult()
        {
            return _pdMapping.GetPdMapping();
        }
        protected List<LgdInputAssumptions_UnsecuredRecovery> GetLgdAssumptionsData()
        {
            var qry = Queries.LGD_InputAssumptions_UnsecuredRecovery(this._eclId);
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

        protected List<StageClassification> GetStagingClassificationResult()
        {
            return _sicrWorkings.ComputeStageClassification();
        }
        protected List<LifetimeEad> GetLifetimeEadResult()
        {
            return _lifetimeEadWorkings.ComputeLifetimeEad();
        }
        protected List<LifeTimeObject> GetScenarioLifetimePdResult()
        {
            
            var qry = "";
            switch (_scenario)
            {
                case ECL_Scenario.Best:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.WholesalePdLifetimeBests_Table, this._eclId);
                    break;
                case ECL_Scenario.Optimistic:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.WholesalePdLifetimeOptimistics_Table, this._eclId);
                    break;
                case ECL_Scenario.Downturn:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.WholesalePdLifetimeDownturns_Table, this._eclId);
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
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.WholesalePdRedefaultLifetimeBests_Table, this._eclId);
                    break;
                case ECL_Scenario.Optimistic:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.WholesalePdRedefaultLifetimeOptimistics_Table, this._eclId);
                    break;
                case ECL_Scenario.Downturn:
                    qry = Queries.LifetimePD_Query(ECLStringConstants.i.WholesalePdRedefaultLifetimeDownturns_Table, this._eclId);
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
        protected List<LifetimeCollateral> GetScenarioLifetimeCollateralResult()
        {
            return _scenarioLifetimeCollateral.ComputeLifetimeCollateral();
        }
    }
}
