﻿using IFRS9_ECL.Core.Calibration.Input;
using IFRS9_ECL.Data;
using IFRS9_ECL.Util;
using Microsoft.Office.Interop.Excel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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
    public class CalibrationInput_EAD_Behavioural_Terms_Processor
    {

        public bool ProcessCalibration(Guid calibrationId, long affiliateId)
        {
            var baseAffPath= Path.Combine(Util.AppSettings.CalibrationModelPath, affiliateId.ToString());
            if(!Directory.Exists(baseAffPath))
            {
                Directory.CreateDirectory(baseAffPath);
            }
            var path = $"{Path.Combine(Util.AppSettings.CalibrationModelPath, "EAD_Behavioural_Term.xlsx")}";
            var path1 = $"{Path.Combine(baseAffPath, $"{calibrationId.ToString()}_EAD_Behavioural_Term.xlsx")}";
            if (File.Exists(path1))
            {
                File.Delete(path1);
            }

            var qry = Queries.CalibrationInput_EAD_Behavioural_Terms(calibrationId);
            var dt = DataAccess.i.GetData(qry);

            if (dt.Rows.Count == 0)
                return true;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[2];//.FirstOrDefault();

                // get number of rows in the sheet
                int rows = worksheet.Dimension.Rows; // 10

                // loop through the worksheet rows


                package.Workbook.CalcMode = ExcelCalcMode.Automatic;

                for (int i = 0; i < dt.Rows.Count; i++)// DataRow dr in dt.Rows)
                {
                    Console.WriteLine(i);
                    DataRow dr = dt.Rows[i];
                    var itm = DataAccess.i.ParseDataToObject(new EAD_Behavioural_Terms_Data(), dr);

                    worksheet.Cells[i + 2, 1].Value = itm.Customer_No ?? "";
                    worksheet.Cells[i + 2, 2].Value = itm.Account_No ?? "";
                    worksheet.Cells[i + 2, 3].Value = itm.Contract_No ?? "";
                    worksheet.Cells[i + 2, 4].Value = itm.Customer_Name ?? "";
                    if (!itm.Snapshot_Date.ToString().Contains("0001"))
                        worksheet.Cells[i + 2, 5].Value = itm.Snapshot_Date;
                    worksheet.Cells[i + 2, 6].Value = itm.Classification ?? "";
                    worksheet.Cells[i + 2, 7].Value = itm.Original_Balance_Lcy;
                    worksheet.Cells[i + 2, 8].Value = itm.Outstanding_Balance_Lcy;
                    worksheet.Cells[i + 2, 9].Value = itm.Outstanding_Balance_Acy;
                    if (!itm.Contract_Start_Date.ToString().Contains("0001"))
                        worksheet.Cells[i + 2, 10].Value = itm.Contract_Start_Date;
                    if (!itm.Contract_End_Date.ToString().Contains("0001"))
                        worksheet.Cells[i + 2, 11].Value = itm.Contract_End_Date;
                    worksheet.Cells[i + 2, 12].Value = itm.Restructure_Indicator ?? "";
                    worksheet.Cells[i + 2, 13].Value = itm.Restructure_Type ?? "";
                    worksheet.Cells[i + 2, 14].Value = itm.Restructure_Start_Date == null ? "" : itm.Restructure_Start_Date.ToString();
                    worksheet.Cells[i + 2, 15].Value = itm.Restructure_End_Date == null ? "" : itm.Restructure_End_Date.ToString();
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


            try
            {
                //refresh and calculate to modify
                theWorkbook.RefreshAll();
                excel.Calculate();

                Worksheet worksheet1 = theWorkbook.Sheets[1];


                var Assumption_NonExpired = "";
                try { Assumption_NonExpired = worksheet1.Cells[10, 3].Value.ToString(); } catch { }

                var Freq_NonExpired = "";
                try { Freq_NonExpired = worksheet1.Cells[10, 4].Value.ToString(); } catch { }

                var Assumption_Expired = "";
                try { Assumption_Expired = worksheet1.Cells[11, 3].Value.ToString(); } catch { }

                var Freq_Expired = "";
                try { Freq_Expired = worksheet1.Cells[11, 4].Value.ToString(); } catch { }

                theWorkbook.Save();
                theWorkbook.Close(true);
                excel.Quit();
                //File.Delete(path1);

                qry = Queries.CalibrationResult_EAD_Behavioural_Terms_Update(calibrationId, Assumption_NonExpired, Freq_NonExpired, Assumption_Expired, Freq_Expired);
                DataAccess.i.ExecuteQuery(qry);

            }
            catch (Exception ex)
            {
                theWorkbook.Save();
                theWorkbook.Close(true);
                excel.Quit();
            }


            return true;


        }

        public CalibrationResult_EAD_Behavioural GetBehaviouralData(Guid eclId, EclType eclType)
        {
            string qry = Queries.GetEADBehaviouralData(eclId, eclType.ToString());
            var dt = DataAccess.i.GetData(qry);
            if (dt.Rows.Count == 0)
            {
                return new CalibrationResult_EAD_Behavioural { Expired = 0, FrequencyExpired = 0, FrequencyNonExpired = 0, NonExpired = 0 };
            }
            DataRow dr = dt.Rows[0];
            var itm = new CalibrationResult_EAD_Behavioural();
            try { itm.Expired = double.Parse(dr["Expired"].ToString().Trim()); } catch { itm.Expired = 0; }
            try { itm.FrequencyNonExpired = double.Parse(dr["FrequencyNonExpired"].ToString().Trim()); } catch { itm.FrequencyNonExpired = 0; }
            try { itm.FrequencyExpired = double.Parse(dr["Expired"].ToString().Trim()); } catch { itm.FrequencyExpired = 0; }
            try { itm.NonExpired = double.Parse(dr["NonExpired"].ToString().Trim()); } catch { itm.NonExpired = 0; }
            return itm;
        }
    }
}
