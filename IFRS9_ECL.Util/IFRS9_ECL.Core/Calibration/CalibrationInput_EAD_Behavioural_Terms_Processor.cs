using IFRS9_ECL.Core.Calibration.Input;
using IFRS9_ECL.Data;
using IFRS9_ECL.Util;
using Microsoft.Office.Interop.Excel;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
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

          

            var qry = Queries.CalibrationInput_EAD_Behavioural_Terms(calibrationId);
            var _dt = DataAccess.i.GetData(qry);

            //DataView dv = _dt.DefaultView;
            //dv.Sort = "Account_No,Contract_No,Snapshot_Date";
            var dt = _dt;// dv.ToTable();
            var rowCount = dt.Rows.Count + 1;

            if (dt.Rows.Count == 0)
                return true;

            var counter=Util.AppSettings.GetCounter(dt.Rows.Count+48);

            var path = $"{Path.Combine(Util.AppSettings.CalibrationModelPath, counter.ToString(), "EAD_Behavioural_Term.xlsx")}";
            var path1 = $"{Path.Combine(baseAffPath, $"{Guid.NewGuid().ToString()}_EAD_Behavioural_Term.xlsx")}";
            var path2 = $"{Path.Combine(baseAffPath)}";

            Log4Net.Log.Info(path);
            if (File.Exists(path1))
            {
                try { File.Delete(path1); } catch { };
            }

            Log4Net.Log.Info($"Output File Path - {path1}");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[2];//.FirstOrDefault();
                
                Log4Net.Log.Info("Read Base File");
                // get number of rows in the sheet
                int rows = worksheet.Dimension.Rows; // 10
                                                     //for (int i = 0; i < dt.Rows.Count - 48; i++)
                                                     //{
                                                     //    worksheet.InsertRow(1, 1, 2);
                                                     //}

                //1 is for header
                //48 is for computation done on the excel
                worksheet.DeleteRow(dt.Rows.Count + 1+48, rows - (dt.Rows.Count + 1));
                // loop through the worksheet rows

                //var result_=worksheet.Cells[3, 1,34,3].Value;
                

                package.Workbook.CalcMode = ExcelCalcMode.Automatic;

                for (int i = 0; i < dt.Rows.Count; i++)// DataRow dr in dt.Rows)
                {
                    Log4Net.Log.Info(i);
                    DataRow dr = dt.Rows[i];
                    var itm = DataAccess.i.ParseDataToObject(new EAD_Behavioural_Terms_Data(), dr);

                    if (string.IsNullOrEmpty(itm.Account_No) && string.IsNullOrEmpty(itm.Contract_No) && itm.Snapshot_Date==null)
                        continue;

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
                    try { worksheet.Cells[i + 2, 12].Value = Convert.ToDouble(itm.Restructure_Indicator);  } catch { worksheet.Cells[i + 2, 12].Value = 0;  }
                    worksheet.Cells[i + 2, 13].Value = itm.Restructure_Type ?? "";
                    worksheet.Cells[i + 2, 14].Value = itm.Restructure_Start_Date; // == null ? "" : itm.Restructure_Start_Date;
                    worksheet.Cells[i + 2, 15].Value = itm.Restructure_End_Date; // == null ? "" : itm.Restructure_End_Date;
                }

                Log4Net.Log.Info("Writing Output File");
                var fi = new FileInfo(path1);

                if (Directory.Exists(baseAffPath))
                {
                    var att1 = fi.Attributes.HasFlag(FileAttributes.ReadOnly);
                    var attr2 = new DirectoryInfo(baseAffPath).Attributes.HasFlag(FileAttributes.ReadOnly);
                    var accessControlList = Directory.GetAccessControl(baseAffPath);
                    var accessRules = accessControlList.GetAccessRules(true, true,
                                    typeof(System.Security.Principal.SecurityIdentifier));
                    foreach (FileSystemAccessRule rule in accessRules)
                    {
                        if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
                            break;

                        //if (rule.AccessControlType == AccessControlType.Allow)
                        //    //writeAllow = true;
                        //else if (rule.AccessControlType == AccessControlType.Deny)
                        //    writeDeny = true;
                    }
                }

                package.SaveAs(fi);
                Log4Net.Log.Info("Done Writing Output File");
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

                //Sort
                Worksheet calculationSheet = theWorkbook.Sheets[3];
                Range sortRange = calculationSheet.Range["A2", "O" + rowCount.ToString()];
                //sortRange.Sort(sortRange.Columns[9], DataOption1: XlSortDataOption.xlSortTextAsNumbers);
                sortRange.Sort(sortRange.Columns[5], DataOption1: XlSortDataOption.xlSortTextAsNumbers); //Snapshot date
                sortRange.Sort(sortRange.Columns[3], XlSortOrder.xlDescending, DataOption1: XlSortDataOption.xlSortTextAsNumbers); //Contract No: 3; Account No: 2;
                
                //refresh and calculate to modify
                theWorkbook.RefreshAll();
                excel.Calculate();
                Log4Net.Log.Info("Reading Output File");
                Worksheet worksheet1 = theWorkbook.Sheets[1];
                Log4Net.Log.Info("Read Sheet 1 of File");
                var Assumption_NonExpired = "";
                try { Assumption_NonExpired = worksheet1.Cells[10, 3].Value.ToString(); } catch { }

                if(Assumption_NonExpired.ToLower().Contains("data"))
                {
                    //Assumption_NonExpired = "0";
                    Assumption_NonExpired = "0";// new Random().Next(2, 25).ToString();
                }

                var Freq_NonExpired = "";
                try { Freq_NonExpired = worksheet1.Cells[10, 4].Value.ToString(); } catch { }

                var Assumption_Expired = "";
                try { Assumption_Expired = worksheet1.Cells[11, 3].Value.ToString(); } catch { }

                if (Assumption_Expired.ToLower().Contains("data"))
                {
                    //Assumption_Expired = "0";
                    Assumption_Expired = "0";// new Random().Next(2, 45).ToString();
                }
                
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
                Log4Net.Log.Error(ex);
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
            try { itm.Expired = double.Parse(dr["Assumption_Expired"].ToString().Trim()); } catch { itm.Expired = 0; }
            try { itm.FrequencyNonExpired = double.Parse(dr["Freq_NonExpired"].ToString().Trim()); } catch { itm.FrequencyNonExpired = 0; }
            try { itm.FrequencyExpired = double.Parse(dr["Freq_Expired"].ToString().Trim()); } catch { itm.FrequencyExpired = 0; }
            try { itm.NonExpired = double.Parse(dr["Assumption_NonExpired"].ToString().Trim()); } catch { itm.NonExpired = 0; }
            return itm;
        }
    }
}
