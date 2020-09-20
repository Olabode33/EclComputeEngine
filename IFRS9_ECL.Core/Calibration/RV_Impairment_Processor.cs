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
    public class RV_Impairment_Processor
    {
        public bool ProcessCalibration(Guid calibrationId)
        {
            var baseAffPath = Path.Combine(Util.AppSettings.CalibrationModelPath);
            if (!Directory.Exists(baseAffPath))
            {
                Directory.CreateDirectory(baseAffPath);
            }

            var qry = Queries.CalibrationInput_RvImpairment_Parameters(calibrationId);
            var dt_Parameters = DataAccess.i.GetData(qry);

            qry = Queries.CalibrationInput_RvImpairment_ScenarioOptions(calibrationId);
            var dt_ScenarioOptions = DataAccess.i.GetData(qry);
            
            qry = Queries.CalibrationInput_RvImpairment_Haircut(calibrationId);
            var dt_Haircut = DataAccess.i.GetData(qry);

            qry = Queries.CalibrationInput_RvImpairment_Recoverys(calibrationId);
            var dt_recoverys = DataAccess.i.GetData(qry);
            if (dt_recoverys.Rows.Count == 0 || dt_recoverys.Rows.Count > 7)
                return true;

            qry = Queries.CalibrationInput_RvImpairment_Calibration(calibrationId);
            var dt_RvCalibration = DataAccess.i.GetData(qry);
            if (dt_RvCalibration.Rows.Count == 0 || dt_RvCalibration.Rows.Count > 7)
                return true;


            qry = Queries.CalibrationInput_RvImpairment_ResultImpairmentOverlay(calibrationId);
            var dt_ResultOverlay = DataAccess.i.GetData(qry);


            var path = $"{Path.Combine(AppSettings.CalibrationModelPath, "ETI_RV_Impairment_Model.xlsx")}";
            var fileGuid = Guid.NewGuid().ToString();
            var path1 = $"{Path.Combine(baseAffPath, "RvImpairmentModel", $"{fileGuid}_ETI_RV_Impairment_Model.xlsx")}";

            if (File.Exists(path1))
            {
                try { File.Delete(path1); } catch { };
            }


            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                ExcelWorksheet worksheet_inputs = package.Workbook.Worksheets[0];//.FirstOrDefault();

                int rows = worksheet_inputs.Dimension.Rows; // 10

                package.Workbook.CalcMode = ExcelCalcMode.Automatic;

                var inputParameter = DataAccess.i.ParseDataToObject(new RvImpairment_InputParameter(), dt_Parameters.Rows[0]);

                worksheet_inputs.Cells[5, 3].Value = inputParameter.ReportingDate;
                worksheet_inputs.Cells[7, 3].Value = inputParameter.CostOfCapital;
                worksheet_inputs.Cells[10, 3].Value = inputParameter.LoanAmount;

                for (int i = 0; i < dt_recoverys.Rows.Count; i++)
                {
                    DataRow dr = dt_recoverys.Rows[i];
                    var itm = DataAccess.i.ParseDataToObject(new RvImpairment_Recovery(), dr);

                    worksheet_inputs.Cells[i + 14, 2].Value = itm.Recovery;
                    worksheet_inputs.Cells[i + 14, 3].Value = itm.CashRecovery;
                    worksheet_inputs.Cells[i + 14, 4].Value = itm.Property;
                    worksheet_inputs.Cells[i + 14, 5].Value = itm.Shares;
                    worksheet_inputs.Cells[i + 14, 6].Value = itm.LoanSale;
                }

                var inputHaircut = DataAccess.i.ParseDataToObject(new RvImpairment_Haircut(), dt_Haircut.Rows[0]);
                worksheet_inputs.Cells[22, 3].Value = inputHaircut.CashRecovery;
                worksheet_inputs.Cells[22, 4].Value = inputHaircut.Property;
                worksheet_inputs.Cells[22, 5].Value = inputHaircut.Shares;
                worksheet_inputs.Cells[22, 6].Value = inputHaircut.LoanSale;

                var inputScenarioOption = DataAccess.i.ParseDataToObject(new RvImpairment_ScenarioOption(), dt_ScenarioOptions.Rows[0]);
                worksheet_inputs.Cells[25, 3].Value = inputScenarioOption.ScenarioOption;
                worksheet_inputs.Cells[28, 6].Value = inputScenarioOption.ApplyOverridesBaseScenario;
                worksheet_inputs.Cells[29, 6].Value = inputScenarioOption.ApplyOverridesOptimisticScenario;
                worksheet_inputs.Cells[30, 6].Value = inputScenarioOption.ApplyOverridesDownturnScenario;
                worksheet_inputs.Cells[28, 7].Value = inputScenarioOption.BestScenarioOverridesValue;
                worksheet_inputs.Cells[29, 7].Value = inputScenarioOption.OptimisticScenarioOverridesValue;
                worksheet_inputs.Cells[30, 7].Value = inputScenarioOption.DownturnScenarioOverridesValue;
                worksheet_inputs.Cells[33, 3].Value = inputScenarioOption.BaseScenario;
                worksheet_inputs.Cells[34, 3].Value = inputScenarioOption.OptimisticScenario;

                ExcelWorksheet worksheet_calibration = package.Workbook.Worksheets[2];//.FirstOrDefault();
                for (int i = 0; i < dt_RvCalibration.Rows.Count; i++)
                {
                    DataRow dr = dt_RvCalibration.Rows[i];
                    var itm = DataAccess.i.ParseDataToObject(new RvImpairment_CalibrationInput(), dr);

                    worksheet_calibration.Cells[i + 8, 2].Value = itm.Year;
                    worksheet_calibration.Cells[i + 8, 3].Value = itm.ExpectedCashFlow;
                    worksheet_calibration.Cells[i + 8, 4].Value = itm.RevisedCashFlow;
                }

                if (dt_ResultOverlay.Rows.Count > 0) {
                    ExcelWorksheet worksheet_result = package.Workbook.Worksheets[1];//.FirstOrDefault();
                    var overlay = DataAccess.i.ParseDataToObject(new RvImpairment_Result(), dt_Haircut.Rows[0]);
                    worksheet_result.Cells[7, 7].Value = overlay.BaseScenarioOverlay;
                    worksheet_result.Cells[8, 7].Value = overlay.OptimisticScenarioOverlay;
                    worksheet_result.Cells[9, 7].Value = overlay.DownturnScenarioOverlay;
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

            Worksheet result_worksheet = theWorkbook.Sheets[2];

            var result = new RvImpairment_Result();
            try{ result.BaseScenarioExposure = result_worksheet.Cells[7, 3].Value; } catch { result.BaseScenarioExposure = 0; }
            try{ result.BaseScenarioPreOverlay = result_worksheet.Cells[7, 4].Value; } catch { result.BaseScenarioPreOverlay = 0; }
            try{ result.BaseScenarioOverrideImpact = result_worksheet.Cells[7, 5].Value; } catch { result.BaseScenarioOverrideImpact = 0; }
            try{ result.BaseScenarioIPO = result_worksheet.Cells[7, 6].Value; } catch { result.BaseScenarioIPO = 0; }
            try{ result.BaseScenarioOverlay = result_worksheet.Cells[7, 7].Value; } catch { result.BaseScenarioOverlay = 0; }
            try{ result.BaseScenarioFinalImpairment = result_worksheet.Cells[7, 8].Value; } catch { result.BaseScenarioFinalImpairment = 0; }
            try{ result.OptimisticScenarioExposure = result_worksheet.Cells[8, 3].Value; } catch { result.OptimisticScenarioExposure = 0; }
            try{ result.OptimisticScenarioPreOverlay = result_worksheet.Cells[8, 4].Value; } catch { result.OptimisticScenarioPreOverlay = 0; }
            try{ result.OptimisticScenarioOverrideImpact = result_worksheet.Cells[8, 5].Value; } catch { result.OptimisticScenarioOverrideImpact = 0; }
            try{ result.OptimisticScenarioIPO = result_worksheet.Cells[8, 6].Value; } catch { result.OptimisticScenarioIPO = 0; }
            try{ result.OptimisticScenarioOverlay = result_worksheet.Cells[8, 7].Value; } catch { result.OptimisticScenarioOverlay = 0; }
            try{ result.OptimisticScenarioFinalImpairment = result_worksheet.Cells[8, 8].Value; } catch { result.OptimisticScenarioFinalImpairment = 0; }
            try{ result.DownturnScenarioExposure = result_worksheet.Cells[9, 3].Value; } catch { result.DownturnScenarioExposure = 0; }
            try{ result.DownturnScenarioPreOverlay = result_worksheet.Cells[9, 4].Value; } catch { result.DownturnScenarioPreOverlay = 0; }
            try{ result.DownturnScenarioOverrideImpact = result_worksheet.Cells[9, 5].Value; } catch { result.DownturnScenarioOverrideImpact = 0; }
            try{ result.DownturnScenarioIPO = result_worksheet.Cells[9, 6].Value; } catch { result.DownturnScenarioIPO = 0; }
            try{ result.DownturnScenarioOverlay = result_worksheet.Cells[9, 7].Value; } catch { result.DownturnScenarioOverlay = 0; }
            try{ result.DownturnScenarioFinalImpairment = result_worksheet.Cells[9, 8].Value; } catch { result.DownturnScenarioFinalImpairment = 0; }
            try{ result.ResultsExposure = result_worksheet.Cells[10, 3].Value; } catch { result.ResultsExposure = 0; }
            try{ result.ResultPreOverlay = result_worksheet.Cells[10, 4].Value; } catch { result.ResultPreOverlay = 0; }
            try{ result.ResultOverrideImpact = result_worksheet.Cells[10, 5].Value; } catch { result.ResultOverrideImpact = 0; }
            try{ result.ResultIPO = result_worksheet.Cells[10, 6].Value; } catch { result.ResultIPO = 0; }
            try{ result.ResultOverlay = result_worksheet.Cells[10, 7].Value; } catch { result.ResultOverlay = 0; }
            try{ result.ResultFinalImpairment = result_worksheet.Cells[10, 8].Value; } catch { result.ResultFinalImpairment = 0; }

            theWorkbook.Save();
            Log4Net.Log.Info("Save to Path");
            theWorkbook.Close(true);
            Log4Net.Log.Info("Close");
            excel.Quit();
            Log4Net.Log.Info("Quit");
            //File.Delete(path1);

            qry = Queries.CalibrationResult_RvImpairment(calibrationId, result.BaseScenarioExposure, result.BaseScenarioFinalImpairment, result.BaseScenarioIPO, result.BaseScenarioOverlay, result.BaseScenarioOverrideImpact, result.BaseScenarioPreOverlay,
                                                         result.DownturnScenarioExposure, result.DownturnScenarioFinalImpairment, result.DownturnScenarioIPO, result.DownturnScenarioOverlay, result.DownturnScenarioOverrideImpact, result.DownturnScenarioPreOverlay,
                                                         result.OptimisticScenarioExposure, result.OptimisticScenarioFinalImpairment, result.OptimisticScenarioIPO, result.OptimisticScenarioOverlay, result.OptimisticScenarioOverrideImpact, result.BaseScenarioPreOverlay,
                                                         result.ResultFinalImpairment, result.ResultIPO, result.ResultOverlay, result.ResultOverrideImpact, result.ResultPreOverlay, result.ResultsExposure);
            DataAccess.i.ExecuteQuery(qry);

            return true;

        }
    }
}
