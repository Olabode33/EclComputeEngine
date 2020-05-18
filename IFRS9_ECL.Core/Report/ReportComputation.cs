using IFRS9_ECL.Data;
using IFRS9_ECL.Models.ECL_Result;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Report
{
    public class ReportComputation
    {
        public ResultSummary GetResultSummary(EclType eclType, Guid eclId, ResultDetail rd)
        {
            var rs= new ResultSummary();

            rs.Overrall = new List<ReportBreakdown>();
            var totalExposure = string.Format("{0:N}", rd.OutStandingBalance);
            var preOverrideOverlay = string.Format("{0:N}", rd.Pre_Impairment_ModelOutput);
            var postOverrideOverlay = string.Format("{0:N}", rd.Post_Impairment_ModelOutput);
            var PortfoliOverlay = 0;
            var totalImpairment = string.Format("{0:N}", PortfoliOverlay + rd.Post_Impairment_ModelOutput);
            var finalCoverage = string.Format("{0:N}", (PortfoliOverlay + rd.Post_Impairment_ModelOutput / rd.OutStandingBalance));
            rs.Overrall.Add(new ReportBreakdown { Field1=totalExposure, Exposure_Pre= preOverrideOverlay, Impairment_Pre= postOverrideOverlay, CoverageRatio_Pre="", Exposure_Post= totalImpairment, Impairment_Post=finalCoverage, CoverageRatio_Post="" });

            rs.Scenario = new List<ReportBreakdown>();




            return rs;
        }

        public ResultDetail GetResultDetail(EclType eclType, Guid eclId)
        {
            var rd= new ResultDetail();
            var _eclId = eclId.ToString();
            var _eclType = eclType.ToString();
            var _eclTypeTable = eclType.ToString();

            var qry = $"select [Status] from {_eclType}Ecls where Id='{_eclId}'";
            var dt = DataAccess.i.GetData(qry);

            if (dt.Rows.Count > 0)
            {
                var eclStatus = int.Parse(dt.Rows[0][0].ToString());
                if(eclStatus==2)
                {
                    _eclTypeTable = $"IFRS9_DB_Archive.dbo.{_eclTypeTable}";
                }
            }
            
            qry = $"select " +
                $" NumberOfContracts=0, " +
                $"  SumOutStandingBalance=0," +
                $"   Pre_EclBestEstimate=0," +
                $"   Pre_Optimistic=0," +
                $"   Pre_Downturn=0," +
                
                $"   Post_EclBestEstimate=0," +
                $"   Post_Optimistic=0," +
                $"   Post_Downturn=0," +

                // $"isnull((select sum(value) from {_eclTypeTable}EadInputs where {_eclType}EclId='{_eclId}'),0) SumOutStandingBalance," +
                //$"   isnull((select sum(FinalEclValue) from {_eclTypeTable}ECLFrameworkFinal where {_eclType}EclId = '{_eclId}' and EclMonth = 0 and Scenario = 1),0) Pre_EclBestEstimate," +
                //$"   isnull((select sum(FinalEclValue) from {_eclTypeTable}ECLFrameworkFinal where {_eclType}EclId = '{_eclId}' and EclMonth = 0 and Scenario = 2),0) Pre_Optimistic," +
                //$"   isnull((select sum(FinalEclValue) from {_eclTypeTable}ECLFrameworkFinal where {_eclType}EclId = '{_eclId}' and EclMonth = 0 and Scenario = 3),0) Pre_Downturn," +

                //$"   isnull((select sum(FinalEclValue) from {_eclTypeTable}ECLFrameworkFinalOverride where {_eclType}EclId = '{_eclId}' and EclMonth = 0 and Scenario = 1),0) Post_EclBestEstimate," +
                //$"   isnull((select sum(FinalEclValue) from {_eclTypeTable}ECLFrameworkFinalOverride where {_eclType}EclId = '{_eclId}' and EclMonth = 0 and Scenario = 2),0) Post_Optimistic," +
                //$"   isnull((select sum(FinalEclValue) from {_eclTypeTable}ECLFrameworkFinalOverride where {_eclType}EclId = '{_eclId}' and EclMonth = 0 and Scenario = 3),0) Post_Downturn," +


                $"   try_convert(float, isnull((select UserInputValue from {_eclType}ReportUserInput where {_eclType}EclId = '{_eclId}' and UserInputKey = 2), 0)) UserInput_EclBE," +
                $"   try_convert(float, isnull((select UserInputValue from {_eclType}ReportUserInput where {_eclType}EclId = '{_eclId}' and UserInputKey = 3), 0)) UserInput_EclO," +
                $"   try_convert(float, isnull((select UserInputValue from {_eclType}ReportUserInput where {_eclType}EclId = '{_eclId}' and UserInputKey = 4), 0)) UserInput_EclD";

            dt=DataAccess.i.GetData(qry);

            var rde = new ReportDetailExtractor();
            var temp_header = DataAccess.i.ParseDataToObject(rde, dt.Rows[0]);

            var overrides_overlay = 0;

            qry = $"select f.Stage, f.FinalEclValue, f.Scenerio, f.ContractId, fo.Stage StageOverride, fo.FinalEclValue FinalEclValueOverride, fo.Scenerio ScenerioOverride, fo.ContractId ContractIOverride from {_eclTypeTable}ECLFrameworkFinal f left join {_eclTypeTable}ECLFrameworkFinalOverride on (f.contractId=fo.contractId and f.EclMonth=fo.EclMonth and f.Scenario=fo.Scenario) where f.{_eclType}EclId = '{_eclId}' and f.EclMonth=0 and fo.{_eclType}EclId = '{_eclId}' and fo.EclMonth=0";
            dt = DataAccess.i.GetData(qry);

            var lstTfer = new List<TempFinalEclResult>();

            foreach(DataRow dr in dt.Rows)
            {
                var tfer = new TempFinalEclResult();
                lstTfer.Add(DataAccess.i.ParseDataToObject(tfer, dr));
            }


            qry = $"select ContractId, [Value] from {_eclTypeTable}EadInput where {_eclType}EclId='{_eclId}' and Months=0";
            dt = DataAccess.i.GetData(qry);

            var lstTWEI = new List<TempEadInput>();

            foreach (DataRow dr in dt.Rows)
            {
                var twei= new TempEadInput();
                lstTWEI.Add(DataAccess.i.ParseDataToObject(twei, dr));
            }

            rd.ResultDetailDataMore = new List<ResultDetailDataMore>();

            qry = $"select ContractNo, AccountNo, CustomerNo, Segment, ProductType, Sector from {_eclTypeTable}EclDataLoanBooks where {_eclType}EclUploadId='{_eclId}'";
            dt = DataAccess.i.GetData(qry);


            foreach (DataRow dr in dt.Rows)
            {
                var rdd = new ResultDetailData();
                var itm = DataAccess.i.ParseDataToObject(rdd, dr);

                var _lstTfer = lstTfer.Where(o => o.ContractId == itm.ContractNo).ToList();

                var stage = 1;
                try { stage = _lstTfer.FirstOrDefault(o => o.Scenerio == 1).Stage; } catch { }

                var stage_Override = 1;
                try { stage_Override = _lstTfer.FirstOrDefault(o => o.Scenerio == 1).StageOverride; } catch { }

                var BE_Value = 0M;
                try { BE_Value = _lstTfer.FirstOrDefault(o => o.Scenerio == 1).FinalEclValue; } catch { }

                var O_Value = 0M;
                try { O_Value = _lstTfer.FirstOrDefault(o => o.Scenerio == 2).FinalEclValue; } catch { }

                var D_Value = 0M;
                try { D_Value = _lstTfer.FirstOrDefault(o => o.Scenerio == 3).FinalEclValue; } catch { }

                var BE_Value_Override = 0M;
                try { BE_Value_Override = _lstTfer.FirstOrDefault(o => o.Scenerio == 1).FinalEclValue; } catch { BE_Value_Override = BE_Value; }

                var O_Value_Override = 0M;
                try { O_Value_Override = _lstTfer.FirstOrDefault(o => o.Scenerio == 2).FinalEclValue; } catch { O_Value_Override = O_Value; }

                var D_Value_Override = 0M;
                try { D_Value_Override = _lstTfer.FirstOrDefault(o => o.Scenerio == 3).FinalEclValue; } catch { D_Value_Override = D_Value; }

                var outStandingBal = 0M;
                try { outStandingBal = lstTWEI.FirstOrDefault(o => o.ContractId == itm.ContractNo).Value; } catch { }

                var rddm = new ResultDetailDataMore
                {
                    AccountNo = itm.AccountNo,
                    ContractNo = itm.ContractNo,
                    CustomerNo = itm.CustomerNo,
                    ProductType = itm.ProductType,
                    Sector = itm.Sector,
                    Stage = stage,
                    Overrides_Stage = stage_Override,
                    ECL_Best_Estimate = BE_Value,
                    ECL_Downturn = D_Value,
                    ECL_Optimistic = O_Value,
                    Overrides_ECL_Best_Estimate = BE_Value_Override * (1 + overrides_overlay),
                    Overrides_ECL_Downturn = D_Value_Override * (1 + overrides_overlay),
                    Overrides_ECL_Optimistic = O_Value_Override * (1 + overrides_overlay),
                    Segment = itm.Segment,
                    Overrides_FSV = 0,
                    Outstanding_Balance = outStandingBal,
                    Overrides_TTR_Years = 0,
                    Overrides_Overlay = 0,
                    Impairment_ModelOutput = 0,
                    Overrides_Impairment_Manual = 0
                };

                rddm.Impairment_ModelOutput = (rddm.ECL_Best_Estimate * temp_header.UserInput_EclBE) + (rddm.ECL_Optimistic + temp_header.UserInput_EclO) + (rddm.ECL_Downturn * temp_header.UserInput_EclD);
                rddm.Overrides_Impairment_Manual = (rddm.Overrides_ECL_Best_Estimate * temp_header.UserInput_EclBE) + (rddm.Overrides_ECL_Optimistic + temp_header.UserInput_EclO) + (rddm.Overrides_ECL_Downturn * temp_header.UserInput_EclD);

                rd.ResultDetailDataMore.Add(rddm);
            }

            rd.NumberOfContracts = rd.ResultDetailDataMore.Count();
            rd.OutStandingBalance = rd.ResultDetailDataMore.Sum(o=>o.Outstanding_Balance);
            rd.Pre_ECL_Best_Estimate = rd.ResultDetailDataMore.Sum(o => o.ECL_Best_Estimate);
            rd.Pre_ECL_Optimistic = rd.ResultDetailDataMore.Sum(o => o.ECL_Optimistic);
            rd.Pre_ECL_Downturn = rd.ResultDetailDataMore.Sum(o => o.ECL_Downturn);
            rd.Pre_Impairment_ModelOutput = (rd.Pre_ECL_Best_Estimate * temp_header.UserInput_EclBE) + (rd.Pre_ECL_Optimistic + temp_header.UserInput_EclO) + (rd.Pre_ECL_Downturn * temp_header.UserInput_EclD);

            rd.Post_ECL_Best_Estimate = rd.ResultDetailDataMore.Sum(o => o.Overrides_ECL_Best_Estimate);
            rd.Post_ECL_Optimistic = rd.ResultDetailDataMore.Sum(o => o.Overrides_ECL_Optimistic);
            rd.Post_ECL_Downturn = rd.ResultDetailDataMore.Sum(o => o.Overrides_ECL_Downturn);

            rd.Post_Impairment_ModelOutput = (rd.Pre_ECL_Best_Estimate * temp_header.UserInput_EclBE) + (rd.Pre_ECL_Optimistic + temp_header.UserInput_EclO) + (rd.Pre_ECL_Downturn * temp_header.UserInput_EclD);

            return rd;
        }
    }
}
