using IFRS9_ECL.Core.PDComputation;
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
    public class SicrWorkings
    {
        private Guid eclId;
        EclType _eclType;
        protected LifetimeEadWorkings _lifetimeEadWorkings;
        protected SicrInputWorkings _sicrInputs;
        protected PDMapping _pdMapping;
        protected ScenarioLifetimeLGD scenarioLifetimeLGD;

        public SicrWorkings(Guid eclId, EclType eclType)
        {
            this.eclId = eclId;
            this._eclType = eclType;
            _lifetimeEadWorkings = new LifetimeEadWorkings(eclId, this._eclType);
            _sicrInputs = new SicrInputWorkings(eclId, this._eclType);
            _pdMapping = new PDMapping(eclId, this._eclType);
            scenarioLifetimeLGD = new ScenarioLifetimeLGD(eclId, this._eclType);
        }
        
        internal List<StageClassification> ComputeStageClassification()
        {
            var stageClassification = new List<StageClassification>();

            var loanbook = GetLoanBookData();
            var sicrInput = GetSicrInputResult();
            var assumption = GetImpairmentAssumptionsData();
            var pdMapping = GetPdMappingResult();

            foreach (var row in sicrInput)
            {
                var loanbookRecord = loanbook.FirstOrDefault(x => x.ContractId == row.ContractId);
                var pdMappingRecord = pdMapping.FirstOrDefault(x => x.ContractId == row.ContractId);

                var newRow = new StageClassification();
                newRow.ContractId = row.ContractId;
                newRow.Stage= ComputeStage(row, loanbookRecord, assumption, pdMappingRecord.PdGroup);

                stageClassification.Add(newRow);
            }


            return stageClassification;
        }

        private int ComputeStage(SicrInputs sicrInputRecord, Loanbook_Data loanBookRecord, List<EclAssumptions> assumption, string pdMapping)
        {
            int pdAbsoluteScore = ComputePdAbsoluteScore(sicrInputRecord, loanBookRecord, assumption);
            int pdRelativeScore = ComputePdRelativeScore(sicrInputRecord, loanBookRecord, assumption);
            int creditRatingScore = ComputeCreditRatingScore(loanBookRecord, assumption);
            int watchlistScore = ComputeWatchlistIndicatorScore(loanBookRecord, assumption);
            int restructureScore = ComputeRestructureIndicatorScore(loanBookRecord, assumption);
            int forwardScore = ComputeForwardScore(sicrInputRecord, loanBookRecord, assumption);
            int backwardScore = ComputeBackwardScore(sicrInputRecord, assumption);
            int expDefault = ComputeExpDefaultScore(pdMapping);

            int maxScore1 = Math.Max(pdAbsoluteScore, pdRelativeScore);
            int maxScore2 = Math.Max(creditRatingScore, watchlistScore);
            int maxScore3 = Math.Max(forwardScore, backwardScore);
            int maxScore4 = Math.Max(restructureScore, expDefault);

            int maxScore5 = Math.Max(maxScore1, maxScore2);
            int maxScore6 = Math.Max(maxScore3, maxScore4);

            return Math.Max(maxScore5, maxScore6);
        }

        private int ComputeExpDefaultScore(string pdMapping)
        {
            return pdMapping == ECLStringConstants.i.ExpiredContractsPrefix ? 3 : 0;
        }

        private int ComputeBackwardScore(SicrInputs sicrInputRecord, List<EclAssumptions> assumption)
        {
            double stage2to1Backward = Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.BackwardTransitionsStage2to1));
            double stage3to2Backward = Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.BackwardTransitionsStage3to2));
            long stage1Transition = sicrInputRecord.Stage1Transition;
            long stage2Transition = sicrInputRecord.Stage2Transition;

            if (stage2Transition < stage3to2Backward && stage2Transition != 0)
            {
                return 3;
            }
            else
            {
                if ((stage1Transition < stage2to1Backward && stage1Transition != 0) || (stage2Transition < stage3to2Backward + stage2to1Backward && stage2Transition != 0))
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
        }

        private int ComputeForwardScore(SicrInputs sicrInputRecord, Loanbook_Data loanBookRecord, List<EclAssumptions> assumption)
        {

            int currentRating = loanBookRecord.CurrentRating??0;
            double currentCreditRankRating = Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.CreditRatingRank + currentRating.ToString()).Replace("-", "").Replace("+", ""));
            double stage2to3creditRating = Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.CreditRatingDefaultIndicator));
            double stage1to2Forward = Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.ForwardTransitionStage1to1));
            double stage2to3Forward = Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.ForwardTransitionStage2to3));
            long daysPastDue = sicrInputRecord.DaysPastDue;

            if (currentCreditRankRating < stage2to3creditRating)
            {
                return daysPastDue < stage1to2Forward ? 1 : (daysPastDue > stage2to3Forward ? 3 : 2);
            }
            else
            {
                return 3;
            }
        }

        private int ComputeRestructureIndicatorScore(Loanbook_Data loanBookRecord, List<EclAssumptions> assumption)
        {
            string useRestructureIndicator = GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.UseRestructureIndicator);
            if (useRestructureIndicator.ToLower() == ECLStringConstants.i.yes)
            {
                return loanBookRecord.RestructureIndicator
                        && loanBookRecord.RestructureRisk.ToLower() == ECLStringConstants.i.yes ? 2 : 1;
            }
            else
            {
                return 1;
            }
        }

        private int ComputeWatchlistIndicatorScore(Loanbook_Data loanBookRecord, List<EclAssumptions> assumption)
        {
            string useWatchlistIndicator = GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.UseWatchlistIndicator);
            if (useWatchlistIndicator.ToLower() == ECLStringConstants.i.yes)
            {
                return loanBookRecord.WatchlistIndicator ? 2 : 1;
            }
            else
            {
                return 1;
            }
        }

        private int ComputeCreditRatingScore(Loanbook_Data loanBookRecord, List<EclAssumptions> assumption)
        {
            double stage2to3CreditRating = Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.ForwardTransitionStage2to3));
            double lowHighRiskThreshold = Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.CreditRatingRankLowHighRisk));
            double normalRiskThreshold = Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.CreditRatingRankLowRisk));
            double highRiskThreshold = Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.CreditRatingRankHighRisk));
            long? currentRating = loanBookRecord.CurrentRating;
            long? originalRating = loanBookRecord.OriginalRating;

            double currentCreditRankRating = string.IsNullOrWhiteSpace(currentRating.ToString()) ? 1 : Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.CreditRatingRank + currentRating.ToString()).Replace("-", "").Replace("+", ""));
            double originalCreditRankRating = string.IsNullOrWhiteSpace(originalRating.ToString()) ? 1 : Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.CreditRatingRank + originalRating.ToString()).Replace("-", "").Replace("+", ""));

            if (currentCreditRankRating >= stage2to3CreditRating)
            {
                return 3;
            }
            else
            {
                if (currentCreditRankRating <= lowHighRiskThreshold)
                {
                    return currentCreditRankRating - originalCreditRankRating > normalRiskThreshold ? 2 : 1;
                }
                else
                {
                    return currentCreditRankRating - originalCreditRankRating > highRiskThreshold ? 2 : 1;
                }
            }


        }

        private int ComputePdRelativeScore(SicrInputs sicrInputRecord, Loanbook_Data loanBookRecord, List<EclAssumptions> assumption)
        {
            string relativeType = GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.RelativeCreditQualityCriteria);
            double relativeThreshold = Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.RelativeCreditQualityThreshold));

            switch (relativeType)
            {
                case FrameworkConstants.CreditQualityCriteriaLifetimePd:
                    double sicrLifetimePd = sicrInputRecord.LifetimePd;
                    double loanLifetimePd = loanBookRecord.LifetimePD??0;

                    return ((sicrLifetimePd / loanLifetimePd) - 1 > relativeThreshold) ? 2 : 1;

                case FrameworkConstants.CreditQualityCriteria12MonthPd:
                    double sicr12MonthPd = sicrInputRecord.Pd12Month;
                    double loan12MonthPd = loanBookRecord.Month12PD??0;

                    return ((sicr12MonthPd / loan12MonthPd) - 1 > relativeThreshold) ? 2 : 1;

                default:

                    return 0;
            }
        }

        private int ComputePdAbsoluteScore(SicrInputs sicrInputRecord, Loanbook_Data loanBookRecord, List<EclAssumptions> assumption)
        {
            string absoluteType = GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.AbsoluteCreditQualityCriteria);
            double absoluteThreshold = Convert.ToDouble(GetImpairmentAssumptionValue(assumption, ImpairmentRowKeys.AbsoluteCreditQualityThreshold));

            switch (absoluteType)
            {
                case FrameworkConstants.CreditQualityCriteriaLifetimePd:
                    double sicrLifetimePd = sicrInputRecord.LifetimePd;
                    double loanLifetimePd = loanBookRecord.LifetimePD??0;

                    return ((sicrLifetimePd - loanLifetimePd) > absoluteThreshold) ? 2 : 1;

                case FrameworkConstants.CreditQualityCriteria12MonthPd:
                    double sicr12MonthPd = sicrInputRecord.Pd12Month;
                    double loan12MonthPd = loanBookRecord.Month12PD??0;

                    return ((sicr12MonthPd - loan12MonthPd) > absoluteThreshold) ? 2 : 1;

                default:
                    return 0;
            }

        }




        protected string GetImpairmentAssumptionValue(List<EclAssumptions> assumptions, string assumptionKey)
        {
            return assumptions.FirstOrDefault(x => x.Key == assumptionKey).Value;
        }
        protected List<PdMappings> GetPdMappingResult()
        {
            return _pdMapping.GetPdMapping();
        }
        protected List<SicrInputs> GetSicrInputResult()
        {
            return _sicrInputs.GetSircInputResult();
        }
        protected List<EclAssumptions> GetImpairmentAssumptionsData()
        {
            return scenarioLifetimeLGD.GetECLLgdAssumptions(); 
            //JsonUtil.DeserializeToDatatable(DbUtil.GetImpairmentAssumptionsData());
        }
        protected List<Loanbook_Data> GetLoanBookData()
        {
            return _lifetimeEadWorkings.GetLoanBookData();
        }
    }
}
