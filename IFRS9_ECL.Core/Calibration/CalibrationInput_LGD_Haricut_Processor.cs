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
    public class CalibrationInput_LGD_Haricut_Processor
    {

        public bool ProcessCalibration(Guid calibrationId, long affiliateId)
        {

            var baseAffPath = Path.Combine(Util.AppSettings.CalibrationModelPath, affiliateId.ToString());
            if (!Directory.Exists(baseAffPath))
            {
                Directory.CreateDirectory(baseAffPath);
            }

            var qry = Queries.CalibrationInput_Haircut(calibrationId);
            var _dt = DataAccess.i.GetData(qry);

            //DataView dv = _dt.DefaultView;
            //dv.Sort = "Account_No,Contract_No,Snapshot_Date";
            var dt = _dt;// dv.ToTable();
            var rowCount = dt.Rows.Count + 1;

            if (dt.Rows.Count == 0)
                return true;

            var counter = Util.AppSettings.GetCounter(dt.Rows.Count);

            var path = $"{Path.Combine(Util.AppSettings.CalibrationModelPath, counter.ToString(), "LGD_Haircut.xlsx")}";
            var path1 = $"{Path.Combine(baseAffPath, $"{Guid.NewGuid().ToString()}LGD_Haircut.xlsx")}";

            if (File.Exists(path1))
            {
                try { File.Delete(path1); } catch { };
            }


            var outputDateList = new List<DateTime>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];//.FirstOrDefault();

                // get number of rows in the sheet
                int rows = worksheet.Dimension.Rows; // 10
                worksheet.DeleteRow(dt.Rows.Count + 1, rows - (dt.Rows.Count + 1));
                // loop through the worksheet rows

                package.Workbook.CalcMode = ExcelCalcMode.Automatic;


                var max_snapshotdate = new DateTime(2000, 01, 01);


                for (int i = 0; i < dt.Rows.Count; i++)// DataRow dr in dt.Rows)
                {
                    DataRow dr = dt.Rows[i];
                    var itm = DataAccess.i.ParseDataToObject(new LGD_HairCut(), dr);

                    if (string.IsNullOrEmpty(itm.Account_No) && string.IsNullOrEmpty(itm.Contract_No) && itm.Snapshot_Date == null)
                        continue;

                    worksheet.Cells[i + 2, 1].Value = itm.Customer_No ?? "";
                    worksheet.Cells[i + 2, 2].Value = itm.Account_No ?? "";
                    worksheet.Cells[i + 2, 3].Value = itm.Contract_No ?? "";
                    worksheet.Cells[i + 2, 4].Value = itm.Snapshot_Date;

                    if (itm.Outstanding_Balance_Lcy != 0)
                    worksheet.Cells[i + 2, 5].Value = itm.Outstanding_Balance_Lcy;

                    if (itm.Debenture_OMV != 0)
                        worksheet.Cells[i + 2, 6].Value = itm.Debenture_OMV;

                    if (itm.Debenture_FSV != 0)
                        worksheet.Cells[i + 2, 7].Value = itm.Debenture_FSV;

                    if (itm.Cash_OMV != 0)
                        worksheet.Cells[i + 2, 8].Value = itm.Cash_OMV;

                    if (itm.Cash_FSV != 0)
                        worksheet.Cells[i + 2, 9].Value = itm.Cash_FSV;

                    if (itm.Inventory_OMV != 0)
                        worksheet.Cells[i + 2, 10].Value = itm.Inventory_OMV;

                    if (itm.Inventory_FSV != 0)
                        worksheet.Cells[i + 2, 11].Value = itm.Inventory_FSV;

                    if (itm.Plant_And_Equipment_OMV != 0)
                        worksheet.Cells[i + 2, 12].Value = itm.Plant_And_Equipment_OMV;

                    if (itm.Plant_And_Equipment_FSV != 0)
                        worksheet.Cells[i + 2, 13].Value = itm.Plant_And_Equipment_FSV;

                    if (itm.Residential_Property_OMV != 0)
                        worksheet.Cells[i + 2, 14].Value = itm.Residential_Property_OMV;

                    if (itm.Residential_Property_FSV != 0)
                        worksheet.Cells[i + 2, 15].Value = itm.Residential_Property_FSV;

                    if (itm.Commercial_Property_OMV != 0)
                        worksheet.Cells[i + 2, 16].Value = itm.Commercial_Property_OMV;

                    if (itm.Commercial_Property_FSV != 0)
                        worksheet.Cells[i + 2, 17].Value = itm.Commercial_Property_FSV;

                    if (itm.Receivables_OMV != 0)
                        worksheet.Cells[i + 2, 18].Value = itm.Receivables_OMV;

                    if (itm.Receivables_FSV != 0)
                        worksheet.Cells[i + 2, 19].Value = itm.Receivables_FSV;

                    if (itm.Shares_OMV != 0)
                        worksheet.Cells[i + 2, 20].Value = itm.Shares_OMV;

                    if (itm.Shares_FSV != 0)
                        worksheet.Cells[i + 2, 21].Value = itm.Shares_FSV;

                    if (itm.Vehicle_OMV != 0)
                        worksheet.Cells[i + 2, 22].Value = itm.Vehicle_OMV;

                    if (itm.Vehicle_FSV != 0)
                        worksheet.Cells[i + 2, 23].Value = itm.Vehicle_FSV;

                    if (itm.Guarantee_Value != 0)
                        worksheet.Cells[i + 2, 24].Value = itm.Guarantee_Value;

                    if (itm.Snapshot_Date != null)
                    {
                        var _Snapshot_Date = itm.Snapshot_Date.Value;
                        if (_Snapshot_Date.Month == 12 || _Snapshot_Date.Month == 9 || _Snapshot_Date.Month == 6 || _Snapshot_Date.Month == 3)
                        {
                            if (_Snapshot_Date.Month == 12)
                            {
                                _Snapshot_Date = new DateTime(_Snapshot_Date.Year + 1, 1, 1).AddDays(-1);
                            }
                            else
                            {
                                _Snapshot_Date = new DateTime(_Snapshot_Date.Year, _Snapshot_Date.Month + 1, 1).AddDays(-1);
                            }
                            max_snapshotdate = _Snapshot_Date > max_snapshotdate ? _Snapshot_Date : max_snapshotdate;
                        }
                    }

                }

                

                worksheet.Cells[2, 86].Value = max_snapshotdate;
                worksheet.Cells[13, 86].Value = max_snapshotdate;
                outputDateList.Add(max_snapshotdate);
                for (int i = 1; i < 17; i++)
                {
                   var dt_ = max_snapshotdate.AddDays(1).AddMonths(-(i * 3)).AddDays(-1);
                    worksheet.Cells[2, 86 - i].Value = dt_;
                    worksheet.Cells[13, 86 - i].Value = dt_;
                    
                    //reduce by quarter (3 months) and resolve to last day of the month
                    outputDateList.Add(dt_);
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
            Range sortRange = calculationSheet.Range["A2", "X" + rowCount.ToString()];
            //sortRange.Sort(sortRange.Columns[9], DataOption1: XlSortDataOption.xlSortTextAsNumbers);
            sortRange.Sort(sortRange.Columns[4], DataOption1: XlSortDataOption.xlSortTextAsNumbers); //Snapshot date
            sortRange.Sort(sortRange.Columns[3], DataOption1: XlSortDataOption.xlSortTextAsNumbers); //Contract no


            //refresh and calculate to modify
            theWorkbook.RefreshAll();
            excel.Calculate();

            Worksheet worksheet1 = theWorkbook.Sheets[1];

            var qryList = new StringBuilder();

            for (int i = 0; i < outputDateList.Count; i++)
            {
                DateTime? Period = outputDateList[i];
                double? Debenture = 0;
                try
                {
                    Debenture = worksheet1.Cells[10, 23 - i].Value;
                    Debenture = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Debenture)? 0: Debenture;

                    Debenture = 0;// = new Random().Next(100, 1000);
                    Debenture = Debenture * 0.01;
                }
                catch { }
                double? Cash = 0;
                try{
                    Cash=worksheet1.Cells[11, 23 - i].Value;
                    Cash = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Cash) ?0: Cash;

                    Cash = 0;// = new Random().Next(100, 1000);
                    Cash = Cash * 0.01;
                }
                catch { }
                double? Inventory = 0;
                try{
                    Inventory=worksheet1.Cells[12, 23 - i].Value;
                    Inventory = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Inventory) ?0: Inventory;

                    Inventory = 0;// = new Random().Next(100, 1000);
                    Inventory = Inventory * 0.01;
                }
                catch { }
                double? Plant_And_Equipment = 0;
                try{
                    Plant_And_Equipment=worksheet1.Cells[13, 23 - i].Value;
                    Plant_And_Equipment = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Plant_And_Equipment) ?0: Plant_And_Equipment;

                    Plant_And_Equipment = 0;// = new Random().Next(100, 1000);
                    Plant_And_Equipment = Plant_And_Equipment*0.01;
                }
                catch { }
                double? Residential_Property = 0;
                try{
                    Residential_Property=worksheet1.Cells[14, 23 - i].Value;
                    Residential_Property = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Residential_Property) ?0: Residential_Property;

                    Residential_Property = 0;// = new Random().Next(100, 1000);
                    Residential_Property = Residential_Property*0.01;

                }
                catch { }
                double? Commercial_Property = 0;
                try{
                    Commercial_Property=worksheet1.Cells[15, 23 - i].Value;
                    Commercial_Property = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Commercial_Property) ?0: Commercial_Property;

                    Commercial_Property = 0;// = new Random().Next(100, 1000);
                    Commercial_Property = Commercial_Property*0.01;
                }
                catch { }
                double? Receivables = 0;
                try{
                    Receivables=worksheet1.Cells[16, 23 - i].Value;
                    Receivables = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Receivables) ?0: Receivables;

                    Receivables = 0;// = new Random().Next(100, 1000);
                    Receivables = Receivables*0.01;
                }
                catch { }
                double? Shares = 0;
                try{
                    Shares=worksheet1.Cells[17, 23 - i].Value;
                    Shares = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Shares) ?0: Shares;

                    Shares = 0;// = new Random().Next(100, 1000);
                    Shares = Shares*0.01;
                }
                catch { }
                double? Vehicle = 0;
                try{
                    Vehicle=worksheet1.Cells[18, 23 - i].Value;
                    Vehicle = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Vehicle) ?0: Vehicle;

                    Vehicle = 0;// = new Random().Next(100, 1000);
                    Vehicle = Vehicle*0.01;
                }
                catch { }


                qry = Queries.CalibrationResult_HairCut_Update(calibrationId, Period, Debenture, Cash, Inventory, Plant_And_Equipment, Residential_Property, Commercial_Property, Receivables, Shares, Vehicle);
                qryList.Append(qry);
            }

            
            double? Debenture_ = 0;
            try
            {
                Debenture_ = worksheet1.Cells[10, 25].Value;
                Debenture_ = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Debenture_)?0: Debenture_;
            }
            catch { }
            double? Cash_ = 0;
            try
            {
                Cash_ = worksheet1.Cells[11, 25].Value;
                Cash_ = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Cash_) ?0: Cash_;
            }
            catch { }
            double? Inventory_ = 0;
            try
            {
                Inventory_ = worksheet1.Cells[12, 25].Value;
                Inventory_ = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Inventory_) ?0: Inventory_; //new Random().Next(1, 100) * 0.01
            }
            catch { }
            double? Plant_And_Equipment_ = 0;
            try
            {
                Plant_And_Equipment_ = worksheet1.Cells[13, 25].Value;
                Plant_And_Equipment_ = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Plant_And_Equipment_) ?0: Plant_And_Equipment_;
            }
            catch { }
            double? Residential_Property_ = 0;
            try
            {
                Residential_Property_ = worksheet1.Cells[14, 25].Value;
                Residential_Property_ = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Residential_Property_) ?0: Residential_Property_;
            }
            catch { }
            double? Commercial_Property_ = 0;
            try
            {
                Commercial_Property_ = worksheet1.Cells[15, 25].Value;
                Commercial_Property_ = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Commercial_Property_) ?0: Commercial_Property_;
            }
            catch { }
            double? Receivables_ = 0;
            try
            {
                Receivables_ = worksheet1.Cells[16, 25].Value;
                Receivables_ = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Receivables_) ?0: Receivables_;
            }
            catch { }
            double? Shares_ = 0;
            try
            {
                Shares_ = worksheet1.Cells[17, 25].Value;
                Shares_ = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Shares_) ?0: Shares_;
            }
            catch { }
            double? Vehicle_ = 0;
            try
            {
                Vehicle_ = worksheet1.Cells[18, 25].Value;
                Vehicle_ = ECLNonStringConstants.i.ExcelDefaultValue.Contains(Vehicle_) ?0: Vehicle_;
            }
            catch { }

            qry = Queries.CalibrationResult_HairCut_Summary_Update(calibrationId, Debenture_, Cash_, Inventory_, Plant_And_Equipment_, Residential_Property_, Commercial_Property_, Receivables_, Shares_, Vehicle_);
            qryList.Append(qry);

            theWorkbook.Save();
            theWorkbook.Close(true);
            excel.Quit();

            //File.Delete(path1);

            qry = Queries.CalibrationResult_HairCut_UpdateFinal(calibrationId, qryList.ToString());
            DataAccess.i.ExecuteQuery(qry);

            return true;


        }

        public CalibrationResult_LGD_HairCut GetLGDHaircutSummaryData(Guid eclId, EclType eclType)
        {
            string qry = Queries.GetLGDHaircutSummaryData(eclId, eclType.ToString());
            var dt = DataAccess.i.GetData(qry);
            if (dt.Rows.Count == 0)
            {
                return new CalibrationResult_LGD_HairCut { Debenture = 0, Cash = 0, Inventory = 0, Plant_And_Equipment = 0, Residential_Property=0, Commercial_Property=0, Receivables=0, Shares=0, Vehicle=0 };
            }
            DataRow dr = dt.Rows[0];
            var itm = new CalibrationResult_LGD_HairCut();
            try { itm.Debenture = double.Parse(dr["Debenture"].ToString().Trim()); } catch { itm.Debenture = 0; }
            try { itm.Cash = double.Parse(dr["Cash"].ToString().Trim()); } catch { itm.Cash = 0; }
            try { itm.Inventory = double.Parse(dr["Inventory"].ToString().Trim()); } catch { itm.Inventory = 0; }
            try { itm.Plant_And_Equipment = double.Parse(dr["Plant_And_Equipment"].ToString().Trim()); } catch { itm.Plant_And_Equipment = 0; }

            try { itm.Residential_Property = double.Parse(dr["Residential_Property"].ToString().Trim()); } catch { itm.Residential_Property = 0; }
            try { itm.Commercial_Property = double.Parse(dr["Commercial_Property"].ToString().Trim()); } catch { itm.Commercial_Property = 0; }
            try { itm.Receivables = double.Parse(dr["Receivables"].ToString().Trim()); } catch { itm.Receivables = 0; }
            try { itm.Shares = double.Parse(dr["Shares"].ToString().Trim()); } catch { itm.Shares = 0; }
            try { itm.Vehicle = double.Parse(dr["Vehicle"].ToString().Trim()); } catch { itm.Vehicle = 0; }

            return itm;
        }
    }
}
