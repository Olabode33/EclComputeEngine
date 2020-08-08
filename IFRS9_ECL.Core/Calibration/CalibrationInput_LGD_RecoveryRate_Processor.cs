using IFRS9_ECL.Core.Calibration.Input;
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
            var rowCount = dt.Rows.Count + 1;

            if (dt.Rows.Count == 0)
                return true;

            var counter = Util.AppSettings.GetCounter(dt.Rows.Count);

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
                worksheet.DeleteRow(dt.Rows.Count + 2, rows - (dt.Rows.Count + 2));
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


            //Sort
            Worksheet calculationSheet = theWorkbook.Sheets[2];
            Range sortRange = calculationSheet.Range["A2", "N" + rowCount.ToString()];
            sortRange.Sort(sortRange.Columns[10], XlSortOrder.xlDescending, DataOption1: XlSortDataOption.xlSortTextAsNumbers); //Outstanding balance
            //sortRange.Sort(sortRange.Columns[9], DataOption1: XlSortDataOption.xlSortTextAsNumbers); //Default date
            sortRange.Sort(sortRange.Columns[4], DataOption1: XlSortDataOption.xlSortTextAsNumbers); // Contract no

            //Temp fix for #REF error after deleting rows
            Range tempRange = calculationSheet.Range["A" + (rowCount + 1).ToString(), "N" + (rowCount + 1).ToString()];
            tempRange.EntireRow.Delete();

            //refresh and calculate to modify
            theWorkbook.RefreshAll();
            excel.Calculate();

            Worksheet worksheet1 = theWorkbook.Sheets[1];

            var r = new CalibrationResult_LGD_RecoveryRate();


            r.Overall_Exposure_At_Default = 0;
            try { r.Overall_Exposure_At_Default = worksheet1.Cells[2, 2].Value; } catch { }
            r.Overall_Exposure_At_Default = ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Overall_Exposure_At_Default) ? 0 : r.Overall_Exposure_At_Default;

            r.Overall_PvOfAmountReceived = 0;
            try { r.Overall_PvOfAmountReceived = worksheet1.Cells[3, 2].Value; } catch { }
            r.Overall_PvOfAmountReceived = ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Overall_PvOfAmountReceived) ? 0 : r.Overall_PvOfAmountReceived;

            r.Overall_Count = 0;
            try { r.Overall_Count = worksheet1.Cells[4, 2].Value; } catch { }
            r.Overall_Count = ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Overall_Count) ? 0 : r.Overall_Count;

            r.Overall_RecoveryRate = 0;
            try { r.Overall_RecoveryRate = worksheet1.Cells[5, 2].Value; } catch { }
            r.Overall_RecoveryRate = ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Overall_RecoveryRate) ? 0 : r.Overall_RecoveryRate;


            r.Corporate_Exposure_At_Default = 0;
            try { r.Corporate_Exposure_At_Default = worksheet1.Cells[2, 3].Value; } catch { }
            r.Corporate_Exposure_At_Default = ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Corporate_Exposure_At_Default) ? 0 : r.Corporate_Exposure_At_Default;

            r.Corporate_PvOfAmountReceived = 0;
            try { r.Corporate_PvOfAmountReceived = worksheet1.Cells[3, 3].Value; } catch { }
            r.Corporate_PvOfAmountReceived = ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Corporate_PvOfAmountReceived) ? 0 : r.Corporate_PvOfAmountReceived;

            r.Corporate_Count = 0;
            try { r.Corporate_Count = worksheet1.Cells[4, 3].Value; } catch { }
            r.Corporate_Count = ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Corporate_Count) ? 0 : r.Corporate_Count;

            r.Corporate_RecoveryRate = 0;
            try { r.Corporate_RecoveryRate = worksheet1.Cells[5, 3].Value; } catch { }
            r.Corporate_RecoveryRate = ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Corporate_RecoveryRate) ? 0 : r.Corporate_RecoveryRate;


            r.Commercial_Exposure_At_Default = 0;
            try { r.Commercial_Exposure_At_Default = worksheet1.Cells[2, 4].Value; } catch { }
            r.Commercial_Exposure_At_Default = ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Commercial_Exposure_At_Default) ? 0 : r.Commercial_Exposure_At_Default;

            r.Commercial_PvOfAmountReceived = 0;
            try { r.Commercial_PvOfAmountReceived = worksheet1.Cells[3, 4].Value; } catch { }
            r.Commercial_PvOfAmountReceived = ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Commercial_PvOfAmountReceived) ? 0 : r.Commercial_PvOfAmountReceived;

            r.Commercial_Count = 0;
            try { r.Commercial_Count = worksheet1.Cells[4, 4].Value; } catch { }
            r.Commercial_Count = ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Commercial_Count) ? 0 : r.Commercial_Count;

            r.Commercial_RecoveryRate = 0;
            try { r.Commercial_RecoveryRate = worksheet1.Cells[5, 4].Value; } catch { }
            r.Commercial_RecoveryRate = ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Commercial_RecoveryRate) ? 0 : r.Commercial_RecoveryRate;


            r.Consumer_Exposure_At_Default = 0;
            try { r.Consumer_Exposure_At_Default = worksheet1.Cells[2, 5].Value; } catch { }
            r.Consumer_PvOfAmountReceived = 0;
            try { r.Consumer_PvOfAmountReceived = worksheet1.Cells[3, 5].Value; } catch { }
            r.Consumer_Count = 0;
            try { r.Consumer_Count = worksheet1.Cells[4, 5].Value; } catch { }
            r.Consumer_RecoveryRate = 0;
            try { r.Consumer_RecoveryRate = worksheet1.Cells[5, 5].Value; } catch { }


            if (ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Corporate_RecoveryRate))
            {
                r.Overall_RecoveryRate = 0;
                //r.Corporate_RecoveryRate=new Random().Next(10, 100);
                //if(r.Corporate_RecoveryRate== 0)
                //    r.Corporate_RecoveryRate = r.Corporate_RecoveryRate * 0.01;
            }
            if (ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Commercial_RecoveryRate))
            {
                r.Commercial_RecoveryRate = 0;// = new Random().Next(10, 100);
               // r.Commercial_RecoveryRate = r.Commercial_RecoveryRate * 0.01;
            }
            if (ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Consumer_RecoveryRate))
            {
                r.Consumer_RecoveryRate = 0;// = new Random().Next(10, 100);
                //r.Consumer_RecoveryRate = r.Consumer_RecoveryRate * 0.01;
            }
            if (ECLNonStringConstants.i.ExcelDefaultValue.Contains(r.Overall_RecoveryRate))
            {
                r.Overall_RecoveryRate = 0;// = new Random().Next(10, 100);
                //r.Consumer_RecoveryRate = r.Consumer_RecoveryRate * 0.01;
            }

            theWorkbook.Save();
            theWorkbook.Close(true);
            excel.Quit();

            //File.Delete(path1);

            qry = Queries.CalibrationResult_LGD_RecoveryRate_Update(calibrationId, r.Overall_Exposure_At_Default, r.Overall_PvOfAmountReceived, r.Overall_Count, r.Overall_RecoveryRate, r.Corporate_Exposure_At_Default, r.Corporate_PvOfAmountReceived, r.Corporate_Count, r.Corporate_RecoveryRate, r.Commercial_Exposure_At_Default, r.Commercial_PvOfAmountReceived, r.Commercial_Count, r.Commercial_RecoveryRate, r.Consumer_Exposure_At_Default, r.Consumer_PvOfAmountReceived, r.Consumer_Count, r.Consumer_RecoveryRate);
            DataAccess.i.ExecuteQuery(qry);

            return true;
        }



        public CalibrationResult_LGD_RecoveryRate GetLGDRecoveryRateData(Guid eclId, EclType eclType)
        {
            string qry = Queries.GetLGDRecoveryRateData(eclId, eclType.ToString());
            var dt = DataAccess.i.GetData(qry);
            if (dt.Rows.Count == 0)
            {
                return new CalibrationResult_LGD_RecoveryRate { Corporate_RecoveryRate=1, Commercial_RecoveryRate=1, Consumer_RecoveryRate=1, Overall_RecoveryRate=1 };
            }

            DataRow dr = dt.Rows[0];
            var itm = new CalibrationResult_LGD_RecoveryRate();
            try { itm.Corporate_RecoveryRate = double.Parse(dr["Corporate_RecoveryRate"].ToString().Trim()); } catch { itm.Corporate_RecoveryRate = 1; }
            try { itm.Commercial_RecoveryRate = double.Parse(dr["Commercial_RecoveryRate"].ToString().Trim()); } catch { itm.Commercial_RecoveryRate = 1; }
            try { itm.Consumer_RecoveryRate = double.Parse(dr["Consumer_RecoveryRate"].ToString().Trim()); } catch { itm.Consumer_RecoveryRate = 1; }
            try { itm.Overall_RecoveryRate = double.Parse(dr["Overall_RecoveryRate"].ToString().Trim()); } catch { itm.Overall_RecoveryRate = 1; }

            return itm;

        }
    }
}

