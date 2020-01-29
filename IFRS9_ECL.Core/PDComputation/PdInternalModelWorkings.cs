using IFRS9_ECL.Core.PDComputation.cmPD;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.PDComputation
{
    public class PdInternalModelWorkings
    {
        protected const int _maxLogRateYear = 15;
        protected const int _maxRatingYear = 20;
        protected const int _maxRatingRank = 9;

        Guid _eclId;
        EclType _eclType;

        public PdInternalModelWorkings(Guid eclId, EclType eclType)
        {
            this._eclType = eclType;
            this._eclId = eclId;
        }

        public void Run()
        {
            List<MonthlyLogOddsRatio> dataTable = ComputeMonthlyCummulativeSurvival();

            string stop = "stop";
        }

        protected List<MonthlyLogOddsRatio> ComputeMonthlyCummulativeSurvival()
        {
            var monthlyLogOddsRatio = ComputeMonthlyLogOddsRatio();

            var monthlyCummulativeSurvivalResult = new List<MonthlyLogOddsRatio>();

            ///Month 1 Computation
            var tempDt = monthlyLogOddsRatio.AsEnumerable()
                                    .Where(row => row.Month == 1).ToList();
            foreach (var dr in tempDt)
            {
                var dataRow = new MonthlyLogOddsRatio();
                dataRow.Month = dr.Month;
                dataRow.Rank = dr.Rank;
                dataRow.Rating = dr.Rating;
                dataRow.CreditRating = 1.0 - dr.CreditRating;
                monthlyCummulativeSurvivalResult.Add(dataRow);
            }

            ///Month 2 to max computation
            for (int month = 2; month <= (_maxRatingYear * 12); month++)
            {
                var prevMonthCreditRating = monthlyCummulativeSurvivalResult.Where(row => row.Month == month - 1).ToList();

                var currMonthCreditRating = monthlyLogOddsRatio.Where(row => row.Month == month)
                                                    .Select(row =>
                                                    {
                                                        double prev = prevMonthCreditRating.AsEnumerable()
                                                                        .FirstOrDefault(x => x.Rank == row.Rank)
                                                                        .CreditRating;
                                                        row.CreditRating = prev * (1 - row.CreditRating);

                                                        return row;
                                                    }).ToList();

                monthlyCummulativeSurvivalResult.AddRange(currMonthCreditRating);
            }


            return monthlyCummulativeSurvivalResult;
        }
        public List<MonthlyLogOddsRatio> ComputeMonthlyLogOddsRatio()
        {
            var marginalDefaultRate = ComputeMarginalDefaultRate();
            var monthlyLogOddsRatioResult = new List<MonthlyLogOddsRatio>();

            int monthCount = 1;

            for (int year = 1; year <= _maxRatingYear; year++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    for (int rank = 1; rank <= _maxRatingRank; rank++)
                    {
                        var rate = marginalDefaultRate.FirstOrDefault(row => row.Year == year && row.Rank == rank);

                        var dataRow = new MonthlyLogOddsRatio();
                        dataRow.Month = monthCount;
                        dataRow.Rank = rank;
                        dataRow.Rating = rate.Rating;
                        dataRow.CreditRating = 1.0 - Math.Pow((1.0 - rate.LogOddsRatio), (1.0 / 12.0)); ;
                        monthlyLogOddsRatioResult.Add(dataRow);
                    }
                    monthCount += 1;
                }
            }



            return monthlyLogOddsRatioResult;
        }
        protected List<LogOddRatio> ComputeMarginalDefaultRate()
        {
            var cummulativeDefaultRate = ComputeCummulativeDefaultRate();

            ///Get cummulative values for year 1
            var marginalDefaultRateResult = new List<LogOddRatio>();
            //marginalDefaultRateResult = cummulativeDefaultRate;


            for(int i=0; i<cummulativeDefaultRate.Count; i++)
            {
                var itm = new LogOddRatio();

                itm.Rank = cummulativeDefaultRate[i].Rank;
                itm.LogOddsRatio = cummulativeDefaultRate[i].LogOddsRatio;
                itm.Rating = cummulativeDefaultRate[i].Rating;
                itm.Year = cummulativeDefaultRate[i].Year;

                if (cummulativeDefaultRate[i].Year!=1)
                {
                    try
                    {
                        var prev = cummulativeDefaultRate.FirstOrDefault(o => o.Year == cummulativeDefaultRate[i].Year - 1 && o.Rank == cummulativeDefaultRate[i].Rank && o.Rating == cummulativeDefaultRate[i].Rating);
                        itm.LogOddsRatio = (cummulativeDefaultRate[i].LogOddsRatio - prev.LogOddsRatio) / (1 - prev.LogOddsRatio);
                    }
                    catch { itm.LogOddsRatio = 1; }
                }
                marginalDefaultRateResult.Add(itm);
            }
           

            return marginalDefaultRateResult;
        }
        protected List<LogOddRatio> ComputeCummulativeDefaultRate()
        {
            var logOddsRatio = ComputeLogsOddsRatio();

            var cummulativeDefaultRateResult = logOddsRatio.Select(row => {
                                                                    row.LogOddsRatio = 1 / (1 + Math.Exp(row.LogOddsRatio));
                                                                    return row;
                                                                }).ToList();

            return cummulativeDefaultRateResult;
        }
        protected List<LogOddRatio> ComputeLogsOddsRatio()
        {
            //var pd12MonthAssumption = new ProcessECL_Wholesale_PD(this._eclId).Get_PDI_Assumptions(); //.Get_PDI_12MonthPds();
            var pdInputAssumptions = new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_Assumptions();
            var logRates = ComputeLogRates();

            var logOddsRatioResult = new List<LogOddRatio>();

            string snpMappingInput = pdInputAssumptions.FirstOrDefault(o => o.PdGroup == PdInputAssumptionGroupEnum.General && o.Key== ECLNonStringConstants.i.SnpMapping).Value;

            for (int rank = 1; rank <= _maxRatingRank; rank++)
            {
                var _12MonthAssumption = new PDI_Assumptions();
                if(snpMappingInput== PdAssumptionsRowKey.SnpMappingValueBestFit)
                {
                    _12MonthAssumption = pdInputAssumptions.Where(o=>o.PdGroup== PdInputAssumptionGroupEnum.CreditBestFit).FirstOrDefault(o => o.InputName == rank.ToString());
                }
                if (snpMappingInput == PdAssumptionsRowKey.SnpMappingValueEtiCreditPolicy)
                {
                    _12MonthAssumption = pdInputAssumptions.Where(o => o.PdGroup == PdInputAssumptionGroupEnum.CreditEtiPolicy).FirstOrDefault(o => o.InputName == rank.ToString());
                }

                string rating = _12MonthAssumption.Value;// snpMappingInput == _12MonthAssumption.Policy ? _12MonthAssumption.Policy : _12MonthAssumption.Fit;

                //Year 1 computation
                double pdValue =double.Parse(pdInputAssumptions.FirstOrDefault(o => o.PdGroup == PdInputAssumptionGroupEnum.CreditPD && o.InputName == rank.ToString()).Value);
                
                double year1LogOddRatio = Math.Log((1 - pdValue) / pdValue);

                var dataRow = new LogOddRatio();
                dataRow.Rank = rank;
                dataRow.Rating = rating;
                dataRow.Year = 1;
                dataRow.LogOddsRatio = year1LogOddRatio;
                logOddsRatioResult.Add(dataRow);

                //Year to Max computation
                double year1RatingLogRate = logRates.FirstOrDefault(row =>row.Rating == rating && row.Year == 1).LogOddsRatio;

                for (int year = 2; year <= _maxRatingYear; year++)
                {
                    double currentYearRatingLogRate = logRates.FirstOrDefault(row => row.Rating == rating && row.Year == Math.Min(year, _maxLogRateYear)).LogOddsRatio;

                    double currentYearLogOddRatio = year1LogOddRatio + currentYearRatingLogRate - year1RatingLogRate;

                    var currentYeardataRow = new LogOddRatio();
                    currentYeardataRow.Rank = rank;
                    currentYeardataRow.Rating = rating;
                    currentYeardataRow.Year = year;
                    currentYeardataRow.LogOddsRatio = currentYearLogOddRatio;
                    logOddsRatioResult.Add(currentYeardataRow);
                }
            }

            return logOddsRatioResult;
        }
        protected List<LogOddRatio> ComputeLogRates()
        {
            var snpCummulativeRate = new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_SnPCummlativeDefaultRate();

            var logRateResult = new List<LogOddRatio>();

            //DataTable snpCummulativeRating = snpCummulativeRate.DefaultView.ToTable(false,  SnPCummlativeDefaultRateColumns.Rating );

            foreach (var row in snpCummulativeRate)
            {
                string rating = row.Rating;

                //Type myObjOriginalType = row.GetType();
                //PropertyInfo[] myProps = myObjOriginalType.GetProperties();

                //for (int year = 1; year <= _maxLogRateYear; year++)
                //{
                    //double defaultRate = double.Parse(myProps.FirstOrDefault(o => o.Name.ToString() == $"_{row.Years.ToString()}").GetValue(row).ToString());
                    double log = Math.Log((1 - row.Value) / row.Value);

                    var dataRow = new LogOddRatio();
                    dataRow.Rating = rating;
                    dataRow.Year = row.Years;
                    dataRow.LogOddsRatio = log;

                    logRateResult.Add(dataRow);
                //}
            }


            return logRateResult;
        }

    }
}
