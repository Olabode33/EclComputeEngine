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
    class ETIReceivables_Processor
    {
        public bool ProcessCalibration(Guid calibrationId)
        {
            var baseAffPath = Path.Combine(Util.AppSettings.CalibrationModelPath);
            if (!Directory.Exists(baseAffPath))
            {
                Directory.CreateDirectory(baseAffPath);
            }

            var qry = Queries.CalibrationInput_IVReceivables_CurrentPeriodDates(calibrationId);
            var dt_CurrentPeriodDates = DataAccess.i.GetData(qry);

            qry = Queries.CalibrationInput_IVReceivables_ReceivablesInputs(calibrationId);
            var dt_ReceivablesInputs = DataAccess.i.GetData(qry);

            if (dt_ReceivablesInputs.Rows.Count == 0)
                return true;


            qry = Queries.CalibrationInput_IVReceivables_ReceivablesForecasts(calibrationId);
            var dt_ReceivablesForecasts = DataAccess.i.GetData(qry);


            var path = $"{Path.Combine(AppSettings.CalibrationModelPath, "IVReceivables.xlsx")}";
            var fileGuid = Guid.NewGuid().ToString();
            var path1 = $"{Path.Combine(baseAffPath, $"{fileGuid}IVReceivables.xlsx")}";

            if (File.Exists(path1))
            {
                try { File.Delete(path1); } catch { };
            }


            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];//.FirstOrDefault();

                int rows = worksheet.Dimension.Rows; // 10
               
                package.Workbook.CalcMode = ExcelCalcMode.Automatic;


                var receivableInput = DataAccess.i.ParseDataToObject(new ReceivablesInputs(), dt_ReceivablesInputs.Rows[0]);

                worksheet.Cells[7,4].Value = receivableInput.ReportingDate;
                worksheet.Cells[10,4].Value = receivableInput.ScenarioOptimistic;
                worksheet.Cells[11,4].Value = receivableInput.ScenarioBase;
                worksheet.Cells[14,4].Value = receivableInput.LossDefinition;
                worksheet.Cells[16, 4].Value = receivableInput.LossRate;
                worksheet.Cells[18, 4].Value = receivableInput.FLIOverlay;
                worksheet.Cells[20, 4].Value = receivableInput.OverlayOptimistic;
                worksheet.Cells[20, 5].Value = receivableInput.OverlayBase;
                worksheet.Cells[20, 6].Value = receivableInput.OverlayDownturn;
                worksheet.Cells[24, 4].Value = receivableInput.InterceptCoefficient;
                worksheet.Cells[24, 4].Value = receivableInput.IndexCoefficient;
                worksheet.Cells[26, 4].Value = receivableInput.LossRateCoefficient;

                for (int i = 0; i < dt_CurrentPeriodDates.Rows.Count; i++)
                {
                    DataRow dr = dt_CurrentPeriodDates.Rows[i];
                    var itm = DataAccess.i.ParseDataToObject(new ReceivablesCurrentPeriodDates(), dr);

                    worksheet.Cells[i + 29, 3].Value = itm.Account;
                    worksheet.Cells[i + 29, 4].Value = itm.ZeroTo90;
                    worksheet.Cells[i + 29, 5].Value = itm.NinetyOneTo180;
                    worksheet.Cells[i + 29, 6].Value = itm.OneEightyOneTo365;
                    worksheet.Cells[i + 29, 7].Value = itm.Over365;
                }

                for (int i = 0; i < dt_ReceivablesForecasts.Rows.Count; i++)
                {
                    DataRow dr = dt_CurrentPeriodDates.Rows[i];
                    var itm = DataAccess.i.ParseDataToObject(new ReceivablesForecasts(), dr);

                    worksheet.Cells[i + 29, 12].Value = itm.Period;
                    worksheet.Cells[i + 29, 13].Value = itm.Optimistic;
                    worksheet.Cells[i + 29, 14].Value = itm.Base;
                    worksheet.Cells[i + 29, 15].Value = itm.Downturn;
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
            //Get inputs for solver template
            Worksheet result = theWorkbook.Sheets[4];

            var r = new ReceivablesResults();
            r.TotalExposure = result.Cells[8,4].Value;
            r.TotalImpairment = result.Cells[9,4].Value;
            r.AdditionalProvision = result.Cells[10,4].Value;
            r.Coverage = result.Cells[11,4].Value;
            r.OptimisticExposure = result.Cells[15,4].Value;
            r.BaseExposure = result.Cells[16,4].Value;
            r.DownturnExposure = result.Cells[17,4].Value;
            r.ECLTotalExposure = result.Cells[18,4].Value;
            r.OptimisticImpairment = result.Cells[15,5].Value;
            r.BaseImpairment = result.Cells[16, 5].Value;
            r.DownturnImpairment = result.Cells[17, 5].Value;
            r.ECLTotalImpairment = result.Cells[18, 5].Value;
            r.OptimisticCoverageRatio = result.Cells[15, 6].Value;
            r.BaseCoverageRatio = result.Cells[16, 6].Value;
            r.DownturnCoverageRatio = result.Cells[17,  6].Value;
            r.TotalCoverageRatio = result.Cells[18, 6].Value;


            theWorkbook.Save();
            Log4Net.Log.Info("Save to Path");
            theWorkbook.Close(true);
            Log4Net.Log.Info("Close");
            excel.Quit();
            Log4Net.Log.Info("Quite");
            //File.Delete(path1);

            qry = Queries.CalibrationResult_IVReceivables(calibrationId, r.TotalExposure, r.TotalImpairment, r.AdditionalProvision, r.Coverage
                , r.OptimisticExposure, r.BaseExposure, r.DownturnExposure, r.ECLTotalExposure, r.OptimisticImpairment, r.BaseImpairment, r.DownturnImpairment
                , r.ECLTotalImpairment, r.OptimisticCoverageRatio, r.BaseCoverageRatio, r.DownturnCoverageRatio, r.TotalCoverageRatio);
            DataAccess.i.ExecuteQuery(qry);

            return true;

        }
    }
}
