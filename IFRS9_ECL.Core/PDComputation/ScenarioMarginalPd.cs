using IFRS9_ECL.Core.PDComputation.cmPD;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.PDComputation
{
    public class ScenarioMarginalPd
    {
        private ECL_Scenario _scenario;
        protected PdInternalModelWorkings _pdInternalModelWorkings;
        protected VasicekWorkings _vasicekWorkings;

        Guid _eclId;
        EclType _eclType;

        public ScenarioMarginalPd(ECL_Scenario scenario, Guid eclId, EclType eclType)
        {
            _scenario = scenario;
            this._eclId = eclId;
            this._eclType = eclType;
            _pdInternalModelWorkings = new PdInternalModelWorkings(this._eclId, this._eclType);
            _vasicekWorkings = new VasicekWorkings(_scenario, this._eclId, this._eclType);
        }

        public void Run()
        {
            var dataTable = ComputeMaginalPd();
            string stop = "Stop";
        }

        public List<LifeTimeObject> ComputeMaginalPd()
        {
            var marginalPds = new List<LifeTimeObject>();

            var logOddsRatio = GetMonthlyLogOddsRatio();
            var varsicekIndex = GetVasicekScenario();
            var nonInternalModelInput = GetNonInternalModelInputsData();

            for (int month = 1; month <= ECLNonStringConstants.i.MaxMarginalLifetimeRedefaultPdMonth; month++)
            {

                int vasicekSearchMonth = Convert.ToInt32((month - 1) / 3) + 1;
                double vasicekIndexMonthValue = varsicekIndex.FirstOrDefault(row => row.Month == (vasicekSearchMonth < 24 ? vasicekSearchMonth : 24)).ScenarioFactor;

                //Pd group 1 to 9
                for (int pdGroup = 1; pdGroup < 10; pdGroup++)
                {
                    string pdGroupName = pdGroup.ToString();
                    double logOddsRatioMonthRankValue = logOddsRatio.FirstOrDefault(row => row.Rank == pdGroup && row.Month == month).CreditRating;

                    var dr = new LifeTimeObject();
                    dr.PdGroup = pdGroupName;
                    dr.Month = month;
                    dr.Value = logOddsRatioMonthRankValue * vasicekIndexMonthValue;

                    marginalPds.Add(dr);
                }

                //Pd Group Cons Stage 1
                var pdGroup10 = new LifeTimeObject();
                var consStage1Row = new LifeTimeObject();
                var consStage2Row = new LifeTimeObject();
                var commStage1Row = new LifeTimeObject();
                var commStage2Row = new LifeTimeObject();
                var pdGroupExp = new LifeTimeObject();

                pdGroup10 = GetPdGroupForConsAndComm(pdGroup10, nonInternalModelInput, "10", month, vasicekIndexMonthValue);
                consStage1Row = GetPdGroupForConsAndComm(consStage1Row, nonInternalModelInput, nonInternalModelInput_Types.CONS_STAGE_1, month, vasicekIndexMonthValue);
                consStage2Row = GetPdGroupForConsAndComm(consStage2Row, nonInternalModelInput, nonInternalModelInput_Types.CONS_STAGE_2, month, vasicekIndexMonthValue);
                commStage1Row = GetPdGroupForConsAndComm(commStage1Row, nonInternalModelInput, nonInternalModelInput_Types.COMM_STAGE_1, month, vasicekIndexMonthValue);
                commStage2Row = GetPdGroupForConsAndComm(commStage2Row, nonInternalModelInput, nonInternalModelInput_Types.COMM_STAGE_2, month, vasicekIndexMonthValue);
                pdGroupExp = GetPdGroupForConsAndComm(pdGroupExp, nonInternalModelInput, ECLStringConstants.i.ExpiredContractsPrefix, month, vasicekIndexMonthValue);

                marginalPds.Add(pdGroup10);
                marginalPds.Add(consStage1Row);
                marginalPds.Add(consStage2Row);
                marginalPds.Add(commStage1Row);
                marginalPds.Add(commStage2Row);
                marginalPds.Add(pdGroupExp);
            }


            return marginalPds;
        }

        private LifeTimeObject GetPdGroupForConsAndComm(LifeTimeObject dr, List<PdInputAssumptionNonInternalModels> nonInternalModelInput, string columnName, int month, double vasicekIndexMonthValue)
        {
            if (columnName == "10" || columnName == ECLStringConstants.i.ExpiredContractsPrefix)
            {
                dr.PdGroup = columnName;
                dr.Month = month;
                dr.Value = month == 1 ? 1 : 0;

                return dr;
            }
            else
            {
                var consStageObj = nonInternalModelInput.FirstOrDefault(row => row.Month == month && row.PdGroup== columnName);

                //var consStageVal = 0.0;
                //if(columnName== "CONS_STAGE_1")
                //{
                //    consStageVal = consStageObj.CONS_STAGE_1;
                //}
                //if (columnName == "CONS_STAGE_2")
                //{
                //    consStageVal = consStageObj.CONS_STAGE_2;
                //}
                //if (columnName == "COMM_STAGE_1")
                //{
                //    consStageVal = consStageObj.COMM_STAGE_1;
                //}
                //if (columnName == "COMM_STAGE_2")
                //{
                //    consStageVal = consStageObj.COMM_STAGE_2;
                //}

                dr.PdGroup = consStageObj.PdGroup;
                dr.Month = month;
                dr.Value = consStageObj.MarginalDefaultRate * vasicekIndexMonthValue;

                return dr;
            }
        }

        protected List<MonthlyLogOddsRatio> GetMonthlyLogOddsRatio()
        {
            return _pdInternalModelWorkings.ComputeMonthlyLogOddsRatio();
        }
        protected List<VasicekEtiNplIndex> GetVasicekScenario()
        {
            return _vasicekWorkings.ComputeVasicekScenario();
        }
        protected List<PdInputAssumptionNonInternalModels> GetNonInternalModelInputsData()
        {
            return new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_NonInternalModelInputs();
        }
    }
}
