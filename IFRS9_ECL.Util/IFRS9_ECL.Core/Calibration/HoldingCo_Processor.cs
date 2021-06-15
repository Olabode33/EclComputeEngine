using IFRS9_ECL.Core.Calibration.Entities;
using IFRS9_ECL.Data;
using IFRS9_ECL.Util;
using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration
{
    public class HoldingCo_Processor
    {
        public bool ProcessCalibration(Guid calibrationId)
        {
            var baseAffPath = Path.Combine(Util.AppSettings.CalibrationModelPath);
            if (!Directory.Exists(baseAffPath))
            {
                Directory.CreateDirectory(baseAffPath);
            }

            var qry = Queries.CalibrationInput_HoldingCo_Parameter(calibrationId);
            var dt_Parameters = DataAccess.i.GetData(qry);

            qry = Queries.CalibrationInput_HoldingCo_MacroEconomicCreditIndices(calibrationId);
            var dt_MacroEconomicCreditIndices = DataAccess.i.GetData(qry);
            if (dt_MacroEconomicCreditIndices.Rows.Count == 0)
                return true;


            qry = Queries.CalibrationInput_HoldingCo_AssetBooks(calibrationId);
            var dt_assetbooks = DataAccess.i.GetData(qry);
            if (dt_assetbooks.Rows.Count == 0)
                return true;


            var path = $"{Path.Combine(AppSettings.CalibrationModelPath, "ETI_HoldCoIntercompany_Loans.xlsx")}";
            var fileGuid = Guid.NewGuid().ToString();
            var path1 = $"{Path.Combine(baseAffPath, "HoldingCo", $"{fileGuid}_ETI_HoldCoIntercompany_Loans.xlsx")}";

            if (File.Exists(path1))
            {
                try { File.Delete(path1); } catch { };
            }


            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                ExcelWorksheet worksheet_inputs = package.Workbook.Worksheets[1];//.FirstOrDefault();

                int rows = worksheet_inputs.Dimension.Rows; // 10

                package.Workbook.CalcMode = ExcelCalcMode.Automatic;

                var holdingParameters = DataAccess.i.ParseDataToObject(new HoldCo_InputParameter(), dt_Parameters.Rows[0]);

                worksheet_inputs.Cells[6, 4] .Value = holdingParameters.ValuationDate;
                worksheet_inputs.Cells[11, 4].Value = holdingParameters.Optimistic;
                worksheet_inputs.Cells[12, 4].Value = holdingParameters.BestEstimate;
                worksheet_inputs.Cells[13, 4].Value = holdingParameters.Downturn;

                worksheet_inputs.Cells[15, 4].Value = holdingParameters.AssumedRating;
                worksheet_inputs.Cells[16, 4].Value = holdingParameters.DefaultLoanRating;

                worksheet_inputs.Cells[19, 4].Value = holdingParameters.RecoveryRate;

                worksheet_inputs.Cells[24, 4].Value = holdingParameters.AssumedStartDate;
                worksheet_inputs.Cells[25, 4].Value = holdingParameters.AssumedMaturityDate;

                for (int i = 0; i < dt_MacroEconomicCreditIndices.Rows.Count; i++)
                {
                    DataRow dr = dt_MacroEconomicCreditIndices.Rows[i];
                    var itm = DataAccess.i.ParseDataToObject(new HoldCo_MacroEconomicCreditIndex(), dr);

                    worksheet_inputs.Cells[i + 29, 3].Value = itm.Month;
                    worksheet_inputs.Cells[i + 29, 4].Value = itm.BestEstimate;
                    worksheet_inputs.Cells[i + 29, 5].Value = itm.Optimistic;
                    worksheet_inputs.Cells[i + 29, 6].Value = itm.Downturn;
                }


                ExcelWorksheet worksheet_assetbook = package.Workbook.Worksheets[2];//.FirstOrDefault();
                for (int i = 0; i < dt_assetbooks.Rows.Count; i++)
                {
                    DataRow dr = dt_assetbooks.Rows[i];
                    var itm = DataAccess.i.ParseDataToObject(new HoldCo_AssetBook(), dr);

                    worksheet_assetbook.Cells[i + 4, 2].Value = i;
                    worksheet_assetbook.Cells[i + 4, 3].Value = itm.Entity;
                    worksheet_assetbook.Cells[i + 4, 4].Value = itm.AssetDescription;
                    worksheet_assetbook.Cells[i + 4, 5].Value = itm.AssetType;
                    worksheet_assetbook.Cells[i + 4, 6].Value = itm.RatingAgency;
                    worksheet_assetbook.Cells[i + 4, 7].Value = itm.PurchaseDateCreditRating;
                    worksheet_assetbook.Cells[i + 4, 8].Value = itm.CurrentCreditRating;
                    worksheet_assetbook.Cells[i + 4, 9].Value = itm.NominalAmountACY;
                    worksheet_assetbook.Cells[i + 4, 10].Value = itm.NominalAmountLCY;
                    worksheet_assetbook.Cells[i + 4, 11].Value = itm.PrincipalAmortisation;
                    worksheet_assetbook.Cells[i + 4, 12].Value = itm.PrincipalRepaymentTerms;
                    worksheet_assetbook.Cells[i + 4, 13].Value = itm.InterestRepaymentTerms;
                    worksheet_assetbook.Cells[i + 4, 14].Value = itm.OutstandingBalanceACY;
                    worksheet_assetbook.Cells[i + 4, 15].Value = itm.OutstandingBalanceLCY;
                    worksheet_assetbook.Cells[i + 4, 16].Value = itm.Coupon;
                    worksheet_assetbook.Cells[i + 4, 17].Value = itm.EIR;
                    worksheet_assetbook.Cells[i + 4, 18].Value = itm.LoanOriginationDate;
                    worksheet_assetbook.Cells[i + 4, 19].Value = itm.LoanMaturityDate;
                    worksheet_assetbook.Cells[i + 4, 20].Value = itm.DaysPastDue;
                    worksheet_assetbook.Cells[i + 4, 21].Value = itm.PrudentialClassification;
                    worksheet_assetbook.Cells[i + 4, 22].Value = itm.ForebearanceFlag;
                }



                var fi = new FileInfo(path1);
                package.SaveAs(fi);
            }


            string txtLocation = Path.GetFullPath(path1);

            object _missingValue = System.Reflection.Missing.Value;
            Application excel = new Application();
            var theWorkbook = excel.Workbooks.Open(txtLocation,
                                                                    _missingValue,
                                                                    false,
                                                                    _missingValue,
                                                                    _missingValue,
                                                                    _missingValue,
                                                                    true,
                                                                    _missingValue,
                                                                    _missingValue,
                                                                    true,
                                                                    _missingValue,
                                                                    _missingValue,
                                                                    _missingValue);



            //refresh and calculate to modify
            theWorkbook.RefreshAll();
            Log4Net.Log.Info("Done refreshing");
            excel.Calculate();
            Log4Net.Log.Info("Done Calculating");


            Worksheet result_worksheet = theWorkbook.Sheets[4];

            var result_summary = new HoldCo_ResultSummary();
            result_summary.BestEstimateExposure = result_worksheet.Cells[6, 13].Value;
            result_summary.BestEstimateTotal = result_worksheet.Cells[6, 14].Value;
            result_summary.BestEstimateImpairmentRatio = result_worksheet.Cells[6, 15].Value;
            result_summary.OptimisticExposure = result_worksheet.Cells[7, 13].Value;
            result_summary.OptimisticTotal = result_worksheet.Cells[7, 14].Value;
            result_summary.OptimisticImpairmentRatio = result_worksheet.Cells[7, 15].Value;
            result_summary.DownturnExposure = result_worksheet.Cells[8, 13].Value;
            result_summary.DownturnTotal = result_worksheet.Cells[8, 14].Value;
            result_summary.DownturnImpairmentRatio = result_worksheet.Cells[8, 15].Value;
            result_summary.Exposure = result_worksheet.Cells[9, 13].Value;
            result_summary.Total = result_worksheet.Cells[9, 14].Value;
            result_summary.ImpairmentRatio = result_worksheet.Cells[9, 15].Value;
            result_summary.Check = result_worksheet.Cells[5, 17].Value;
            result_summary.Diff = result_worksheet.Cells[5, 19].Value;

            var result_summary_by_stage = new HoldCo_ResultSummaryByStage();
            result_summary_by_stage.StageOneExposure = result_worksheet.Cells[14, 13].Value;
            result_summary_by_stage.StageOneImpairment = result_worksheet.Cells[14, 14].Value;
            result_summary_by_stage.StageOneImpairmentRatio = result_worksheet.Cells[14, 15].Value;
            result_summary_by_stage.StageTwoExposure = result_worksheet.Cells[15, 13].Value;
            result_summary_by_stage.StageTwoImpairment = result_worksheet.Cells[15, 14].Value;
            result_summary_by_stage.StageTwoImpairmentRatio = result_worksheet.Cells[15, 15].Value;
            result_summary_by_stage.StageThreeExposure = result_worksheet.Cells[16, 13].Value;
            result_summary_by_stage.StageThreeImpairment = result_worksheet.Cells[16, 14].Value;
            result_summary_by_stage.StageThreeImpairmentRatio = result_worksheet.Cells[16, 15].Value;
            result_summary_by_stage.TotalExposure = result_worksheet.Cells[17, 13].Value;
            result_summary_by_stage.TotalImpairment = result_worksheet.Cells[17, 14].Value;
            result_summary_by_stage.TotalImpairmentRatio = result_worksheet.Cells[17, 15].Value;


            //var result = new List<HoldCo_ResultDetail>();
            StringBuilder resultQry = new StringBuilder();
            for (int i = 0; i <= 1000; i++)
            {
                if (result_worksheet.Cells[i + 5, 2].Value != null && result_worksheet.Cells[i + 5, 3].Value != "")
                {
                    var result_details = new HoldCo_ResultDetail();
                    result_details.AssetType = result_worksheet.Cells[i + 5, 3].Value;
                    result_details.AssetDescription = result_worksheet.Cells[i + 5, 4].Value;
                    result_details.Stage = result_worksheet.Cells[i + 5, 5].Value;
                    result_details.OutstandingBalance = result_worksheet.Cells[i + 5, 6].Value;
                    result_details.BestEstimate = result_worksheet.Cells[i + 5, 7].Value;
                    result_details.Optimistic = result_worksheet.Cells[i + 5, 8].Value;
                    result_details.Downturn = result_worksheet.Cells[i + 5, 9].Value;
                    result_details.Impairment = result_worksheet.Cells[i + 5, 10].Value;

                    var q = Queries.CalibrationResult_HoldingCo_ResultDetail_Items(calibrationId, result_details.AssetType, result_details.AssetDescription, result_details.Stage,
                                                                                   result_details.OutstandingBalance, result_details.BestEstimate, result_details.Optimistic, result_details.Downturn, result_details.Impairment);
                    resultQry.Append(q + " \n");
                    //result.Add(result_details);
                }
            }


            theWorkbook.Save();
            Log4Net.Log.Info("Save to Path");
            theWorkbook.Close(true);
            Log4Net.Log.Info("Close");
            excel.Quit();
            Log4Net.Log.Info("Quite");
            //File.Delete(path1);

            qry = Queries.CalibrationResult_HoldingCo_ResultSummary(calibrationId, result_summary.BestEstimateExposure, result_summary.OptimisticExposure, result_summary.DownturnExposure,
                                                                    result_summary.BestEstimateTotal, result_summary.OptimisticTotal, result_summary.DownturnTotal,
                                                                    result_summary.BestEstimateImpairmentRatio, result_summary.OptimisticImpairmentRatio, result_summary.DownturnImpairmentRatio,
                                                                    result_summary.Exposure, result_summary.Total, result_summary.ImpairmentRatio, result_summary.Check ? 1 : 0, result_summary.Diff);
            DataAccess.i.ExecuteQuery(qry);

            qry = Queries.CalibrationResult_HoldingCo_ResultSummaryByStage(calibrationId, result_summary_by_stage.StageOneExposure, result_summary_by_stage.StageTwoExposure, result_summary_by_stage.StageThreeExposure, result_summary_by_stage.TotalExposure,
                                                                           result_summary_by_stage.StageOneImpairment, result_summary_by_stage.StageTwoImpairment, result_summary_by_stage.StageThreeImpairment, result_summary_by_stage.TotalImpairment,
                                                                           result_summary_by_stage.StageOneImpairmentRatio, result_summary_by_stage.StageTwoImpairmentRatio, result_summary_by_stage.StageThreeImpairmentRatio, result_summary_by_stage.TotalImpairmentRatio);
            DataAccess.i.ExecuteQuery(qry);

            qry = Queries.CalibrationResult_HoldingCo_ResultDetails(calibrationId, resultQry);
            DataAccess.i.ExecuteQuery(qry);

            return true;

        }
    }
}
