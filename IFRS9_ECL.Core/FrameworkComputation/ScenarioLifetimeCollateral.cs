using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Framework;
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

        ECL_Scenario _scenario;
        Guid _eclId;
        public ScenarioLifetimeCollateral(ECL_Scenario scenario, Guid eclId)
        {
            _scenario = scenario;
            this._eclId = eclId;
            _lifetimeEad = new LifetimeEadWorkings(eclId);
            _irFactorWorkings = new IrFactorWorkings(eclId);
            _updatedFSVsWorkings = new UpdatedFSVsWorkings(eclId);
        }
        public void Run()
        {
            var dataTable = ComputeLifetimeCollateral();
            string stop = "Ma te";
        }
        public List<LifetimeCollateral> ComputeLifetimeCollateral()
        {
            var lifetimeCollateral = new List<LifetimeCollateral>();

            var contractData = GetContractData();
            var marginalDiscountFactor = GetMarginalDiscountFactor();
            var eadInputs = GetTempEadInputData();
            var collateralProjections = GetScenarioCollateralProjection();
            var updatedFsv = GetUpdatedFsvResult();

            foreach (var row in contractData)
            {
                string contractId = row.CONTRACT_NO;
                string eirGroup = eadInputs.FirstOrDefault(x => x.Contract_no == contractId).Eir_Group;
                int eirIndex = marginalDiscountFactor.FirstOrDefault(x => x.EirGroup == eirGroup).Rank;
                int ttrMonth = Convert.ToInt32(Math.Round(row.TTR_YEARS * 12, 0));
                var tempFsv = updatedFsv.FirstOrDefault(x => x.ContractNo == contractId);
                double[] fsvArray = new double[9];
                fsvArray[0] = tempFsv.Cash;
                fsvArray[1] = tempFsv.Commercial_Property;
                fsvArray[2] = tempFsv.Debenture;
                fsvArray[3] = tempFsv.Inventory;
                fsvArray[4] = tempFsv.Plant_And_Equipment;
                fsvArray[5] = tempFsv.Receivables;
                fsvArray[6] = tempFsv.Residential_Property;
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
                    newRow.TtrMonths = ttrMonth;
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

     



        private List<LGDAccountData> GetContractData()
        {
            return this._lifetimeEad.GetTempContractData();
        }
        protected List<IrFactor> GetMarginalDiscountFactor()
        {
            return _irFactorWorkings.ComputeMarginalDiscountFactor();
        }
        protected List<LifeTimeProjections> GetTempEadInputData()
        {
            return this._lifetimeEad.GetTempEadInputData();
        }
        protected List<LgdCollateralProjection> GetScenarioCollateralProjection()
        {
            var qry = "";
            switch (_scenario)
            {
                case ECL_Scenario.Best:
                    qry = Queries.LgdCollateralCollateralProjection(this._eclId, 0);
                    break;
                case ECL_Scenario.Optimistic:
                    qry = Queries.LgdCollateralCollateralProjection(this._eclId,1);
                    break;
                case ECL_Scenario.Downturn:
                    qry = Queries.LgdCollateralCollateralProjection(this._eclId,2);
                    break;
                default:
                    return null;
            }


            var _lstRaw = DataAccess.i.GetData(qry);

            var lifetimePd = new List<LgdCollateralProjection>();
            foreach (DataRow dr in _lstRaw.Rows)
            {
                lifetimePd.Add(DataAccess.i.ParseDataToObject(new LgdCollateralProjection(), dr));
            }
            Console.WriteLine("Completed pass data to object");

            return lifetimePd;
        }
        protected List<LgdCollateralFsvProjectionUpdate> GetUpdatedFsvResult()
        {
            return _updatedFSVsWorkings.ComputeUpdatedFSV();
        }
    }
}
