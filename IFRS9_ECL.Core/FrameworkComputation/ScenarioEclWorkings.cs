﻿using IFRS9_ECL.Data;
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
    public class ScenarioEclWorkings
    {
        protected ScenarioLifetimeLGD _lifetimeLgd;
        protected LifetimeEadWorkings _lifetimeEad;
        protected IrFactorWorkings _irFactorWorkings;
        protected SicrWorkings _sicrWorkings;

        private ECL_Scenario _scenario;
        Guid _eclId;
        public ScenarioEclWorkings(Guid eclId, ECL_Scenario scenario)
        {
            this._eclId = eclId;
            _scenario = scenario;
            _lifetimeEad = new LifetimeEadWorkings(eclId);
            _lifetimeLgd = new ScenarioLifetimeLGD(eclId, _scenario);
            _irFactorWorkings = new IrFactorWorkings(eclId);
            _sicrWorkings = new SicrWorkings(eclId);
        }


        public void Run()
        {
            var dataTable = ComputeFinalEcl();
            string stop = "Ma te";
        }

        public List<FinalEcl> ComputeFinalEcl()
        {
            var finalEcl = new List<FinalEcl>();

            var monthlyEcl = ComputeMonthlyEcl();
            var cummulativeDiscountFactor = GetCummulativeDiscountFactor();
            var eadInput = GetTempEadInputData();
            var lifetimeEad = GetLifetimeEadResult();
            var lifetimeLGD = GetLifetimeLgdResult();
            var stageClassifcation = GetStageClassification();

            foreach (var row in stageClassifcation)
            {
                string contractId = row.ContractId;
                int stage = row.Stage;
                string eirGroup = eadInput.FirstOrDefault(x => x.Contract_no == contractId).Eir_Group;
                double finalEclValue = ComputeFinalEclValue(monthlyEcl, cummulativeDiscountFactor, lifetimeEad, lifetimeLGD, contractId, stage, eirGroup);

                var newRow = new FinalEcl();
                newRow.ContractId = contractId;
                newRow.Stage = stage;
                newRow.FinalEclValue = finalEclValue;

                finalEcl.Add(newRow);
            }

            return finalEcl;
        }

        private double ComputeFinalEclValue(List<FinalEcl> monthlyEcl, List<IrFactor> cummulativeDiscountFactor, List<LifetimeEad> lifetimeEad, List<LifetimeLgd> lifetimeLGD, string contractId, int stage, string eirGroup)
        {
            double lifetimeLgdMonth0Value = lifetimeLGD.FirstOrDefault(o => o.ContractId == contractId && o.Month == 0).Value;
            double lifetimeEadMonth0Value = lifetimeEad.FirstOrDefault(o => o.ContractId == contractId && o.ProjectionMonth == 0).ProjectionValue;

            double finalEclValue = 0;

            switch (stage)
            {
                case 1:
                    double[] monthEclArray = monthlyEcl.Where(o => o.ContractId == contractId && o.EclMonth >= 1 && o.EclMonth < FrameworkConstants.ScenerioWorkingMaxMonth).Select(n => n.MonthlyEclValue).ToArray();
                    double[] monthCdfArray = cummulativeDiscountFactor.Where(o => o.EirGroup == contractId && o.ProjectionMonth >= 1 && o.ProjectionMonth < FrameworkConstants.ScenerioWorkingMaxMonth).Select(n => n.ProjectionValue).ToArray();

                    finalEclValue = ExcelFormulaUtil.SumProduct(monthEclArray, monthCdfArray);
                    break;
                case 2:
                    double[] monthEclArray2 = monthlyEcl.Where(o => o.ContractId == contractId && o.EclMonth >= 1 && o.EclMonth < FrameworkConstants.ProjectionMonth).Select(n => n.MonthlyEclValue).ToArray();
                    double[] monthCdfArray2 = cummulativeDiscountFactor.Where(o => o.EirGroup == contractId && o.ProjectionMonth >= 1 && o.ProjectionMonth < FrameworkConstants.ProjectionMonth).Select(n => n.ProjectionValue).ToArray();
                    finalEclValue = ExcelFormulaUtil.SumProduct(monthEclArray2, monthCdfArray2);
                    break;
                default:
                    finalEclValue = lifetimeEadMonth0Value * lifetimeLgdMonth0Value;
                    break;

            }

            return finalEclValue;
        }

   
        public List<FinalEcl> ComputeMonthlyEcl()
        {
            var monthlyEcl = new List<FinalEcl>();
            
            var lifetimePds = Get_LifetimePd_And_RedefaultLifetimePD_Result();
            var lifetimeEads = GetLifetimeEadResult();
            var lifetimeLgds = GetLifetimeLgdResult().Where(x => x.Month != 0).ToList();

            foreach (var row in lifetimeLgds)
            {
                string contractId = row.ContractId;
                string pdGroup = row.PdIndex;
                int month = row.Month;
                double monthlyEclValue = ComputeMonthlyEclValue(lifetimePds, lifetimeEads, row, contractId, pdGroup, month);

                var newRow = new FinalEcl();
                newRow.ContractId = contractId;
                newRow.EclMonth = month;
                newRow.MonthlyEclValue = monthlyEclValue;

                monthlyEcl.Add(newRow);
            }

            return monthlyEcl;
        }

        private double ComputeMonthlyEclValue(List<LifeTimeObject> lifetimePds, List<LifetimeEad> lifetimeEads, LifetimeLgd row, string contractId, string pdGroup, int month)
        {
            double lgdValue = row.Value;
            double pdValue = GetLifetimePdValueFromTable(lifetimePds, pdGroup, month);
            double eadValue = GetLifetimeEadValueFromTable(lifetimeEads, contractId, month);
            double monthlyEclValue = pdValue * lgdValue * eadValue;
            return monthlyEclValue;
        }

        private double GetLifetimeEadValueFromTable(List<LifetimeEad> lifetimeEads, string contractId, int month)
        {
            return lifetimeEads.FirstOrDefault(x => x.ContractId == contractId && x.ProjectionMonth == month).ProjectionValue;
        }

        private double GetLifetimePdValueFromTable(List<LifeTimeObject> lifetimePds, string pdGroup, int month)
        {
            return lifetimePds.FirstOrDefault(x => x.PdGroup == pdGroup && x.Month == (month > 120 ? 120 : month)).Value;
        }

        public double[] ComputeMonthArray(DataTable dataTable, string contractColumnName, string monthColumnName, string valueColumnName, string contractId, int maxMonth)
        {
            double[] monthlyArray = dataTable.AsEnumerable()
                                                     .Where(x => x.Field<string>(contractColumnName) == contractId
                                                              && x.Field<int>(monthColumnName) >= 1
                                                              && x.Field<int>(monthColumnName) <= maxMonth)
                                                     .Select(x =>
                                                     {
                                                         return x.Field<double>(valueColumnName);
                                                     }).ToArray();

            return monthlyArray;
        }

        
        protected List<LifetimeLgd> GetLifetimeLgdResult()
        {
            return _lifetimeLgd.ComputeLifetimeLGD();
            
        }
        protected List<LifetimeEad> GetLifetimeEadResult()
        {
            return _lifetimeEad.ComputeLifetimeEad();
        }
        protected List<LifeTimeObject> Get_LifetimePd_And_RedefaultLifetimePD_Result()
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
        protected List<LifeTimeProjections> GetTempEadInputData()
        {
            return _lifetimeEad.GetTempEadInputData();// JsonUtil.DeserializeToDatatable(DbUtil.GetTempEadInputsData());
        }
        protected List<IrFactor> GetCummulativeDiscountFactor()
        {
            return _irFactorWorkings.ComputeCummulativeDiscountFactor();
        }
        protected List<StageClassification> GetStageClassification()
        {
            return _sicrWorkings.ComputeStageClassification();
        }

    }
}
