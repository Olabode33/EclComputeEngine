﻿using IFRS9_ECL.Data;
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
      
        public List<LifetimeCollateral> ComputeLifetimeCollateral(List<Loanbook_Data> loanbook)
        {
            var lifetimeCollateral = new List<LifetimeCollateral>();

            var contractData = GetContractData(loanbook);
            var marginalDiscountFactor = GetMarginalDiscountFactor();
            var eadInputs = GetTempEadInputData(loanbook);
            var collateralProjections = GetScenarioCollateralProjection();
            var updatedFsv = GetUpdatedFsvResult();

            var eadInputContractData = eadInputs.Select(o => o.Contract_no).ToList();
            contractData = contractData.Where(o => eadInputContractData.Contains(o.CONTRACT_NO)).ToList();

            foreach (var row in contractData)
            {
                string contractId = row.CONTRACT_NO;
                string eirGroup = eadInputs.FirstOrDefault(x => x.Contract_no == contractId).Eir_Group;
                int eirIndex = marginalDiscountFactor.FirstOrDefault(x => x.EirGroup == eirGroup).Rank;
                int ttrMonth = Convert.ToInt32(Math.Round(row.TTR_YEARS * 12, 0));
                var tempFsv = updatedFsv.FirstOrDefault(x => x.ContractNo == contractId);
                double[] fsvArray = new double[9];
                fsvArray[0] = tempFsv.Cash;
                fsvArray[1] = tempFsv.CommercialProperty;
                fsvArray[2] = tempFsv.Debenture;
                fsvArray[3] = tempFsv.Inventory;
                fsvArray[4] = tempFsv.PlantAndEquipment;
                fsvArray[5] = tempFsv.Receivables;
                fsvArray[6] = tempFsv.ResidentialProperty;
                fsvArray[7] = tempFsv.Shares;
                fsvArray[8] = tempFsv.Vehicle;

                for (int month = 0; month < FrameworkConstants.MaxIrFactorProjectionMonths; month++)
                {
                    double product = GetProductValue(marginalDiscountFactor, eirIndex, ttrMonth, month);
                    double sumProduct = GetSumProductValue(collateralProjections, ttrMonth, fsvArray, month);
                    double value = product * sumProduct;

                    var newRow = new LifetimeCollateral();
                    newRow.ContractId = contractId;
                    newRow.EirIndex = eirIndex;
                    newRow.TtrMonths = tempFsv.Override_TTR_Year != null ? tempFsv.Override_TTR_Year.Value : ttrMonth;
                    newRow.ProjectionMonth = month;
                    newRow.ProjectionValue = value;

                    lifetimeCollateral.Add(newRow);
                }
            }


            return lifetimeCollateral;
        }

        private double GetSumProductValue(List<LgdCollateralProjection> collateralProjections, int ttrMonth, double[] fsvArray, int month)
        {
            int minMonth = Math.Min(1 + month + ttrMonth, FrameworkConstants.TempExcelVariable_LIM_CM);
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

        private double GetProductValue(List<IrFactor> marginalDiscountFactor, int eirIndex, int ttrMonth, int month)
        {
            double[] temp = marginalDiscountFactor.Where(x => x.Rank == eirIndex
                                                                                && (x.ProjectionMonth >= 2 + month) && x.ProjectionMonth <= ttrMonth)
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
        protected List<LifeTimeProjections> GetTempEadInputData(List<Loanbook_Data> loanbook)
        {
            return this._lifetimeEad.GetTempEadInputData(loanbook);
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

            var lifetimePd = new LgdCollateralProjection
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
                try { lifetimePd.Debenture = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}Debenture"].ToString()); } catch { }
                try { lifetimePd.Cash = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}Cash"].ToString()); } catch { }
                try { lifetimePd.Inventory = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}Inventory"].ToString()); } catch { }
                try { lifetimePd.Plant_And_Equipment = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}PlantEquipment"].ToString()); } catch { }
                try { lifetimePd.Residential_Property = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}ResidentialProperty"].ToString()); } catch { }
                try { lifetimePd.Commercial_Property = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}CommercialProperty"].ToString()); } catch { }
                try { lifetimePd.Receivables = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}Receivables"].ToString()); } catch { }
                try { lifetimePd.Shares = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}Shares"].ToString()); } catch { }
                try { lifetimePd.Vehicle = double.Parse(dr[$"CollateralProjection{_scenario.ToString()}Vehicle"].ToString()); } catch { }
            }


            var lifetimePd_Month0 = lifetimePd;//
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
                col.Debenture = Math.Max(Math.Pow(itms[i - 1].Debenture * (1 + debenture), (1 / 12)), 0);
                col.Cash = Math.Max(Math.Pow(itms[i - 1].Cash * (1 + cash), (1 / 12)), 0);
                col.Commercial_Property = Math.Max(Math.Pow(itms[i - 1].Commercial_Property * (1 + commercialProperty), (1 / 12)), 0);
                col.Inventory = Math.Max(Math.Pow(itms[i - 1].Inventory * (1 + inventory), (1 / 12)), 0);
                col.Plant_And_Equipment = Math.Max(Math.Pow(itms[i - 1].Plant_And_Equipment * (1 + plantEquipment), (1 / 12)), 0);
                col.Receivables = Math.Max(Math.Pow(itms[i - 1].Receivables * (1 + receivables), (1 / 12)), 0);
                col.Residential_Property = Math.Max(Math.Pow(itms[i - 1].Residential_Property * (1 + residentialProperty), (1 / 12)), 0);
                col.Shares = Math.Max(Math.Pow(itms[i - 1].Shares * (1 + shares), (1 / 12)), 0);
                col.Vehicle = Math.Max(Math.Pow(itms[i - 1].Vehicle * (1 + vehicle), (1 / 12)), 0);

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
