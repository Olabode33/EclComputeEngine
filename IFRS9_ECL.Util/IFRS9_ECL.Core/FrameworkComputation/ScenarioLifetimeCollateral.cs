using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Framework;
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
    public class ScenarioLifetimeCollateral
    {
        protected IrFactorWorkings _irFactorWorkings;
        protected UpdatedFSVsWorkings _updatedFSVsWorkings;
        protected LifetimeEadWorkings _lifetimeEad;
        protected ScenarioLifetimeLGD _scenarioLifetimeLGD;

        ECL_Scenario _scenario;
        Guid _eclId;
        EclType _eclType;
        public ScenarioLifetimeCollateral(ECL_Scenario scenario, Guid eclId, EclType eclType)
        {
            _scenario = scenario;
            this._eclId = eclId;
            this._eclType = eclType;
            _lifetimeEad = new LifetimeEadWorkings(eclId, this._eclType);
            _irFactorWorkings = new IrFactorWorkings(eclId, this._eclType);
            _updatedFSVsWorkings = new UpdatedFSVsWorkings(eclId, this._eclType);
            _scenarioLifetimeLGD = new ScenarioLifetimeLGD(eclId, this._eclType);
        }




        List<LifetimeCollateral> lifetimeCollateral = new List<LifetimeCollateral>();
        List<IrFactor> marginalDiscountFactor = new List<IrFactor>();
        List<LgdCollateralProjection> collateralProjections = new List<LgdCollateralProjection>();
        List<updatedFSV> updatedFsv = new List<updatedFSV>();
        public List<LifetimeCollateral> ComputeLifetimeCollateral(List<Loanbook_Data> loanbook, List<LifeTimeProjections> eadInputs, List<LGDAccountData> contractData)//, 
        {
            
            Log4Net.Log.Info($"Get COntract at ComputeLifetimeCollateral {contractData.Count}");
            marginalDiscountFactor = GetMarginalDiscountFactor();
            Log4Net.Log.Info($"Get marginalDiscountFactor at ComputeLifetimeCollateral {marginalDiscountFactor.Count}");
            //var eadInputs = GetTempEadInputData(loanbook);
            collateralProjections = GetScenarioCollateralProjection();
            Log4Net.Log.Info($"Get collateralProjections at ComputeLifetimeCollateral {collateralProjections.Count}");
            updatedFsv = GetUpdatedFsvResult();
            Log4Net.Log.Info($"Get updatedFsv at ComputeLifetimeCollateral {updatedFsv.Count}");
            var actual_eadInputContractData = eadInputs.Select(o => o.Contract_no).Distinct().ToList();
            
            //contractData = contractData.Where(o => actual_eadInputContractData.Contains(o.CONTRACT_NO)).ToList();

            Log4Net.Log.Info($"starting Loop at ComputeLifetimeCollateral");




            var threads = contractData.Count / 500;
            threads = threads + 1;

            var taskLst = new List<Task>();

            var tskStatusLst = new List<TaskStatus> { TaskStatus.RanToCompletion, TaskStatus.Faulted, TaskStatus.Canceled };

            for (int i = 0; i < threads; i++)
            {
                var sub_contractData = contractData.Skip(i * 500).Take(500).ToList();
                var sub_contractData_contractno = sub_contractData.Select(o => o.CONTRACT_NO).ToList();
                var sub_eadInputs = eadInputs.Where(o => sub_contractData_contractno.Contains(o.Contract_no)).ToList();

                var task = Task.Run(() =>
                {
                    SubComputeLifetimeCollateral(sub_contractData, sub_eadInputs);
                });

                taskLst.Add(task);
            }


            while (0 < 1)
            {
                if (taskLst.All(o => tskStatusLst.Contains(o.Status)))
                {
                    break;
                }
                //Do Nothing
            }

            Log4Net.Log.Info($"Done ComputeLifetimeCollateral");

            return lifetimeCollateral;
        }


        private void SubComputeLifetimeCollateral(List<LGDAccountData> contractData, List<LifeTimeProjections> sub_eadInputs)
        {
            var sublifetimeCollateral = new List<LifetimeCollateral>();

            foreach (var row in contractData)
            {
                //Log4Net.Log.Info($"LP {row.CONTRACT_NO}");
                string contractId = row.CONTRACT_NO;
                string eirGroup = sub_eadInputs.FirstOrDefault(x => x.Contract_no == contractId).Eir_Group;
                long eirIndex = 0;
                try
                {
                    eirIndex = marginalDiscountFactor.FirstOrDefault(x => x.EirGroup == eirGroup).Rank;
                }
                catch { }
                long ttrMonth = Convert.ToInt64(Math.Round(row.TTR_YEARS * 12, 0));
                var tempFsv = updatedFsv.FirstOrDefault(x => x.ContractNo == contractId);
                double[] fsvArray = new double[9];
                if (tempFsv == null)
                {
                    fsvArray[0] = 0;
                    fsvArray[1] = 0;
                    fsvArray[2] = 0;
                    fsvArray[3] = 0;
                    fsvArray[4] = 0;
                    fsvArray[5] = 0;
                    fsvArray[6] = 0;
                    fsvArray[7] = 0;
                    fsvArray[8] = 0;
                }
                else
                {
                    fsvArray[0] = tempFsv.Cash;
                    fsvArray[1] = tempFsv.CommercialProperty;
                    fsvArray[2] = tempFsv.Debenture;
                    fsvArray[3] = tempFsv.Inventory;
                    fsvArray[4] = tempFsv.PlantAndEquipment;
                    fsvArray[5] = tempFsv.Receivables;
                    fsvArray[6] = tempFsv.ResidentialProperty;
                    fsvArray[7] = tempFsv.Shares;
                    fsvArray[8] = tempFsv.Vehicle;
                }


                var maxMonth = row.LIM_MONTHS + (row.LIM_MONTHS * 0.5);

                if (eirGroup == ECLStringConstants.i.ExpiredContractsPrefix)
                {
                    maxMonth = 627;// 161;
                }
                maxMonth = maxMonth + 1;
                //maxMonth = 627; //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                for (int month = 0; month < maxMonth; month++)
                {
                    //Log4Net.Log.Info($"LP {row.CONTRACT_NO} - {month}");
                    double product = GetProductValue(marginalDiscountFactor, eirGroup, ttrMonth, month);
                    double sumProduct = GetSumProductValue(collateralProjections, ttrMonth, fsvArray, month);
                    double value = product * sumProduct;

                    var newRow = new LifetimeCollateral();
                    newRow.ContractId = contractId;
                    newRow.EirIndex = eirIndex;
                    newRow.TtrMonths = tempFsv.Override_TTR_Year != null ? tempFsv.Override_TTR_Year.Value : ttrMonth;
                    newRow.ProjectionMonth = month;
                    newRow.ProjectionValue = value;

                    sublifetimeCollateral.Add(newRow);
                }
            }
            lock (lifetimeCollateral)
                lifetimeCollateral.AddRange(sublifetimeCollateral);
        }

        private double GetSumProductValue(List<LgdCollateralProjection> collateralProjections, long ttrMonth, double[] fsvArray, long month)
        {
            long minMonth = Math.Min(month + ttrMonth, FrameworkConstants.TempExcelVariable_LIM_CM);
            var projectionsDr = collateralProjections.FirstOrDefault(x => x.Month == minMonth);
            double[] projections = new double[9];
            projections[0] = projectionsDr.Cash;
            projections[1] = projectionsDr.Commercial_Property;
            projections[2] = projectionsDr.Debenture;
            projections[3] = projectionsDr.Inventory;
            projections[4] = projectionsDr.Plant_And_Equipment;
            projections[5] = projectionsDr.Receivables;
            projections[6] = projectionsDr.Residential_Property;
            projections[7] = projectionsDr.Shares;
            projections[8] = projectionsDr.Vehicle;

            double sumProduct = ExcelFormulaUtil.SumProduct(fsvArray, projections);
            return sumProduct;
        }

        private double GetProductValue(List<IrFactor> marginalDiscountFactor, string eirGroup, long ttrMonth, long month)
        {
            double[] temp = marginalDiscountFactor.Where(x => x.EirGroup == eirGroup
                                                                                && (x.ProjectionMonth >= 1 + month) && x.ProjectionMonth <= ttrMonth+ month)
                                                             .Select(x =>
                                                             {
                                                                 return x.ProjectionValue;
                                                             }).ToArray();
            double product = temp.Aggregate(1.0, (acc, x) => acc * x);
            return product;
        }





        private List<LGDAccountData> GetContractData(List<Loanbook_Data> loanbook)
        {
            return new ProcessECL_LGD(this._eclId, this._eclType).GetLgdContractData(loanbook);
        }
        protected List<IrFactor> GetMarginalDiscountFactor()
        {
            return _irFactorWorkings.ComputeMarginalDiscountFactor();
        }

        protected List<LgdCollateralProjection> GetScenarioCollateralProjection()
        {
            var qry = "";
            switch (_scenario)
            {
                case ECL_Scenario.Best:
                    qry = Queries.LgdCollateralProjection(this._eclId, 9, this._eclType);
                    break;
                case ECL_Scenario.Optimistic:
                    qry = Queries.LgdCollateralProjection(this._eclId, 10, this._eclType);
                    break;
                case ECL_Scenario.Downturn:
                    qry = Queries.LgdCollateralProjection(this._eclId, 11, this._eclType);
                    break;
                default:
                    return null;
            }


            var _lstRaw = DataAccess.i.GetData(qry);

            var collateralProjection_0 = new LgdCollateralProjection
            {
                Cash = 1,
                Vehicle = 1,
                Shares = 1,
                Receivables = 1,
                Residential_Property = 1,
                Commercial_Property = 1,
                Month = 0,
                Plant_And_Equipment = 1,
                Inventory = 1,
                CollateralProjectionType = _scenario,
                Debenture = 1
            };
            foreach (DataRow dr in _lstRaw.Rows)
            {
                try { collateralProjection_0.Debenture = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}Debenture"].ToString()); } catch { }
                try { collateralProjection_0.Cash = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}Cash"].ToString()); } catch { }
                try { collateralProjection_0.Inventory = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}Inventory"].ToString()); } catch { }
                try { collateralProjection_0.Plant_And_Equipment = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}PlantEquipment"].ToString()); } catch { }
                try { collateralProjection_0.Residential_Property = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}ResidentialProperty"].ToString()); } catch { }
                try { collateralProjection_0.Commercial_Property = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}CommercialProperty"].ToString()); } catch { }
                try { collateralProjection_0.Receivables = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}Receivables"].ToString()); } catch { }
                try { collateralProjection_0.Shares = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}Shares"].ToString()); } catch { }
                try { collateralProjection_0.Vehicle = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}Vehicle"].ToString()); } catch { }
            }


            var lifetimePd_Month0 = collateralProjection_0;//
            var assumptions = _scenarioLifetimeLGD.GetECLLgdAssumptions();
            if (_scenario == ECL_Scenario.Best)
            {
                assumptions = assumptions.Where(o => o.AssumptionGroup == 5).ToList();
            }
            if (_scenario == ECL_Scenario.Optimistic)
            {
                assumptions = assumptions.Where(o => o.AssumptionGroup == 6).ToList();
            }
            if (_scenario == ECL_Scenario.Downturn)
            {
                assumptions = assumptions.Where(o => o.AssumptionGroup == 7).ToList();
            }

            var debenture = 0.0;
            try { debenture=double.Parse(assumptions.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Debenture)).Value); } catch { }
            var cash = 0.0;
            try{ cash=double.Parse(assumptions.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Cash)).Value); } catch { }
            var commercialProperty = 0.0;
            try{ commercialProperty=double.Parse(assumptions.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.CommercialProperty)).Value); } catch { }
            var inventory = 0.0;
            try{ inventory=double.Parse(assumptions.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Inventory)).Value); } catch { }
            var plantEquipment = 0.0;
            try{ plantEquipment=double.Parse(assumptions.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.PlantEquipment)).Value); } catch { }
            var receivables = 0.0;
            try{ receivables=double.Parse(assumptions.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Receivables)).Value); } catch { }
            var residentialProperty = 0.0;
            try{ residentialProperty=double.Parse(assumptions.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.ResidentialProperty)).Value); } catch { }
            var shares = 0.0;
            try{ shares=double.Parse(assumptions.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Shares)).Value); } catch { }
            var vehicle = 0.0;
            try{ vehicle=double.Parse(assumptions.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Vehicle)).Value); } catch { }

            var itms = new List<LgdCollateralProjection>();

            itms.Add(lifetimePd_Month0);

            for (int i = 1; i <= 60; i++)
            {
                var col = new LgdCollateralProjection();

                col.Month = i;
                col.Debenture = Math.Max(itms[i - 1].Debenture * Math.Pow((1 + debenture), (1.0 / 12.0)), 0);
                col.Cash = Math.Max(itms[i - 1].Cash * Math.Pow((1 + cash), (1.0 / 12.0)), 0);
                col.Commercial_Property = Math.Max(itms[i - 1].Commercial_Property * Math.Pow((1 + commercialProperty), (1.0 / 12.0)), 0);
                col.Inventory = Math.Max(itms[i - 1].Inventory * Math.Pow((1 + inventory), (1.0 / 12.0)), 0);
                col.Plant_And_Equipment = Math.Max(itms[i - 1].Plant_And_Equipment * Math.Pow((1 + plantEquipment), (1.0 / 12.0)), 0);
                col.Receivables = Math.Max(itms[i - 1].Receivables * Math.Pow( (1 + receivables), (1.0 / 12.0)), 0);
                col.Residential_Property = Math.Max(itms[i - 1].Residential_Property * Math.Pow((1 + residentialProperty), (1.0 / 12.0)), 0);
                col.Shares = Math.Max(itms[i - 1].Shares * Math.Pow((1 + shares), (1.0 / 12.0)), 0);
                col.Vehicle = Math.Max(itms[i - 1].Vehicle * Math.Pow((1 + vehicle), (1.0 / 12.0)), 0);

                itms.Add(col);
            }
           

            Log4Net.Log.Info("Completed pass data to object");

            return itms;
        }
        protected List<updatedFSV> GetUpdatedFsvResult()
        {
            return _updatedFSVsWorkings.ComputeUpdatedFSV();
        }
    }
}
