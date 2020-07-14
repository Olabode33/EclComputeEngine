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
    public class CalibrationInput_LGD_RecoveryRate_Processor
    {

        public bool ProcessCalibration(Guid calibrationId, long affiliateId)
        {

            var baseAffPath = Path.Combine(Util.AppSettings.CalibrationModelPath, affiliateId.ToString());
            if (!Directory.Exists(baseAffPath))
            {
                Directory.CreateDirectory(baseAffPath);
            }

            var qry = Queries.CalibrationInput_RecoveryRate(calibrationId);
            var _dt = DataAccess.i.GetData(qry);

            //DataView dv = _dt.DefaultView;
            //dv.Sort = "Account_No,Contract_No,Date_Of_Recovery";
            var dt = _dt;// dv.ToTable();

            if (dt.Rows.Count == 0)
                return true;

            var counter = Util.AppSettings.GetCounter(affiliateId);

            var path = $"{Path.Combine(Util.AppSettings.CalibrationModelPath, counter.ToString(), "LGD_Recovery_Rate.xlsx")}";
            var path1 = $"{Path.Combine(baseAffPath, $"{Guid.NewGuid().ToString()}LGD_Recovery_Rate.xlsx")}";

            if (File.Exists(path1))
            {
                try { File.Delete(path1); } catch { };
            }


            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];//.FirstOrDefault();

                // get number of rows in the sheet
                int rows = worksheet.Dimension.Rows; // 10

                // loop through the worksheet rows

                package.Workbook.CalcMode = ExcelCalcMode.Automatic;

                for (int i = 0; i < dt.Rows.Count; i++)// DataRow dr in dt.Rows)
                {
                    Log4Net.Log.Info(i);
                    DataRow dr = dt.Rows[i];
                    var itm = DataAccess.i.ParseDataToObject(new Calibration_LGD_RecoveryRate(), dr);

                    if (string.IsNullOrEmpty(itm.Account_No) && string.IsNullOrEmpty(itm.Contract_No) && itm.Date_Of_Recovery == null)
                        continue;

                    try { worksheet.Cells[i + 2, 1].Value = itm.Customer_No; } catch { }
                    try { worksheet.Cells[i + 2, 2].Value = itm.Account_No; } catch { }
                    try {
                        itm.Account_Name = itm.Account_Name ?? "";
                        worksheet.Cells[i + 2, 3].Value = itm.Account_Name.Trim(); } catch { }
                    try {
                        itm.Contract_No = itm.Contract_No ?? "";
                        worksheet.Cells[i + 2, 4].Value = itm.Contract_No.Trim(); } catch { }
                    try { worksheet.Cells[i + 2, 5].Value = itm.Segment; } catch { }
                    try { worksheet.Cells[i + 2, 6].Value = itm.Product_Type; } catch { }
                    try { worksheet.Cells[i + 2, 7].Value = itm.Days_Past_Due; } catch { }
                    try { worksheet.Cells[i + 2, 8].Value = itm.Classification; } catch { }
                    try { worksheet.Cells[i + 2, 9].Value = itm.Default_Date; } catch { }

                    try { worksheet.Cells[i + 2, 10].Value = itm.Outstanding_Balance_Lcy; } catch { }
                    try { worksheet.Cells[i + 2, 11].Value = itm.Contractual_Interest_Rate; } catch { }
                    try { worksheet.Cells[i + 2, 12].Value = itm.Amount_Recovered; } catch { }
                    try
                    { worksheet.Cells[i + 2, 13].Value = itm.Date_Of_Recovery; }
                    catch { }
                    try
                    {
                        itm.Type_Of_Recovery = itm.Type_Of_Recovery ?? "";
                        worksheet.Cells[i + 2, 14].Value = itm.Type_Of_Recovery.Trim(); }
                    catch { }
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
            excel.Calculate();

            Worksheet worksheet1 = theWorkbook.Sheets[2];

            var r = new CalibrationResult_LGD_RecoveryRate();


            r.Overall_Exposure_At_Default = 0;
            try { r.Overall_Exposure_At_Default = worksheet1.Cells[2, 28].Value; } catch { }
            r.Overall_PvOfAmountReceived = 0;
            try { r.Overall_PvOfAmountReceived = worksheet1.Cells[3, 28].Value; } catch { }
            r.Overall_Count = 0;
            try { r.Overall_Count = worksheet1.Cells[4, 28].Value; } catch { }
            r.Overall_RecoveryRate = 0;
            try { r.Overall_RecoveryRate = worksheet1.Cells[5, 28].Value; } catch { }


            r.Corporate_Exposure_At_Default = 0;
            try { r.Corporate_Exposure_At_Default = worksheet1.Cells[2, 29].Value; } catch { }
            r.Corporate_PvOfAmountReceived = 0;
            try { r.Corporate_PvOfAmountReceived = worksheet1.Cells[3, 29].Value; } catch { }
            r.Corporate_Count = 0;
            try { r.Corporate_Count = worksheet1.Cells[4, 29].Value; } catch { }
            r.Corporate_RecoveryRate = 0;
            try { r.Corporate_RecoveryRate = worksheet1.Cells[5, 29].Value; } catch { }


            r.Commercial_Exposure_At_Default = 0;
            try { r.Commercial_Exposure_At_Default = worksheet1.Cells[2, 30].Value; } catch { }
            r.Commercial_PvOfAmountReceived = 0;
            try { r.Commercial_PvOfAmountReceived = worksheet1.Cells[3, 30].Value; } catch { }
            r.Commercial_Count = 0;
            try { r.Commercial_Count = worksheet1.Cells[4, 30].Value; } catch { }
            r.Commercial_RecoveryRate = 0;
            try { r.Commercial_RecoveryRate = worksheet1.Cells[5, 30].Value; } catch { }


            r.Consumer_Exposure_At_Default = 0;
            try { r.Consumer_Exposure_At_Default = worksheet1.Cells[2, 31].Value; } catch { }
            r.Consumer_PvOfAmountReceived = 0;
            try { r.Consumer_PvOfAmountReceived = worksheet1.Cells[3, 31].Value; } catch { }
            r.Consumer_Count = 0;
            try { r.Consumer_Count = worksheet1.Cells[4, 31].Value; } catch { }
            r.Consumer_RecoveryRate = 0;
            try { r.Consumer_RecoveryRate = worksheet1.Cells[5, 31].Value; } catch { }

            if (r.Corporate_RecoveryRate == -2146826281)
            {
                r.Corporate_RecoveryRate = 0;
            }
            if (r.Commercial_RecoveryRate == -2146826281)
            {
                r.Commercial_RecoveryRate = 0;
            }
            if (r.Consumer_RecoveryRate == -2146826281)
            {
                r.Consumer_RecoveryRate = 0;
            }


            theWorkbook.Save();
            theWorkbook.Close(true);
            excel.Quit();

            //File.Delete(path1);

            qry = Queries.CalibrationResult_LGD_RecoveryRate_Update(calibrationId, r.Overall_Exposure_At_Default, r.Overall_PvOfAmountReceived, r.Overall_Count, r.Overall_RecoveryRate, r.Corporate_Exposure_At_Default, r.Corporate_PvOfAmountReceived, r.Corporate_Count, r.Corporate_RecoveryRate, r.Commercial_Exposure_At_Default, r.Commercial_PvOfAmountReceived, r.Commercial_Count, r.Commercial_RecoveryRate, r.Consumer_Exposure_At_Default, r.Consumer_PvOfAmountReceived, r.Consumer_Count, r.Consumer_RecoveryRate);
            DataAccess.i.ExecuteQuery(qry);

            return true;
        }



        public double GetLGDRecoveryRateData(Guid eclId, EclType eclType)
        {
            string qry = Queries.GetLGDRecoveryRateData(eclId, eclType.ToString());
            var dt = DataAccess.i.GetData(qry);
            if (dt.Rows.Count == 0)
            {
                return 0;
            }
            DataRow dr = dt.Rows[0];
            try { return double.Parse(dr["Overall_RecoveryRate"].ToString().Trim()); } catch { return 0; }
           
        }
    }
}

