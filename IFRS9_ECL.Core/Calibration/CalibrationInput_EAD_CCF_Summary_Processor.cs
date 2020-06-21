using IFRS9_ECL.Core.Calibration.Input;
using IFRS9_ECL.Data;
using IFRS9_ECL.Util;
using Microsoft.Office.Interop.Excel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.Calibration
{
    public class CalibrationInput_EAD_CCF_Summary_Processor
    {

        public bool ProcessCalibration(Guid calibrationId, long affiliateId)
        {

            var path = $"{Path.Combine(Util.AppSettings.CalibrationModelPath, affiliateId.ToString(), "EAD_CCF.xlsx")}";
            var path1 = $"{Path.Combine(Util.AppSettings.CalibrationModelPath, affiliateId.ToString(), $"{calibrationId.ToString()}_EAD_CCF.xlsx")}";
            if (File.Exists(path1))
            {
                File.Delete(path1);
            }

            var qry = Queries.CalibrationInput_EAD_CCF(calibrationId);
            var dt=DataAccess.i.GetData(qry);




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
                    Console.WriteLine(i);
                    DataRow dr = dt.Rows[i];
                    var itm = DataAccess.i.ParseDataToObject(new EAD_CCF_Summary(), dr);

                    worksheet.Cells[i + 2, 1].Value = itm.Customer_No ?? "";
                    worksheet.Cells[i + 2, 2].Value = itm.Account_No ?? "";
                    worksheet.Cells[i + 2, 3].Value = itm.Settlement_Account ?? "";
                    worksheet.Cells[i + 2, 4].Value = itm.Product_Type ?? "";
                    if(!itm.Snapshot_Date.ToString().Contains("0001"))
                        worksheet.Cells[i + 2, 5].Value = itm.Snapshot_Date;

                    if (itm.Contract_Start_Date != null)
                    {
                        worksheet.Cells[i + 2, 6].Value = itm.Contract_Start_Date;
                    }
                    
                    if(itm.Contract_End_Date!=null)
                    {
                        worksheet.Cells[i + 2, 7].Value = itm.Contract_End_Date;
                    }

                    worksheet.Cells[i + 2, 8].Value = itm.Limit;
                    worksheet.Cells[i + 2, 9].Value = itm.Outstanding_Balance;
                    worksheet.Cells[i + 2, 10].Value = itm.Classification ?? "";

                }

                //package.Workbook.Worksheets[1].Calculate();
                //package.Workbook.Worksheets[0].Calculate();

                //ExcelCalculationOption o = new ExcelCalculationOption();
                //o.AllowCircularReferences = true;
                //package.Workbook.Calculate(o);

                var fi = new FileInfo(path1);
                package.SaveAs(fi);
            }

            string txtLocation = Path.GetFullPath(path1);

            object _missingValue = System.Reflection.Missing.Value;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
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

                var r = new CalibrationResult_EAD_CCF_Summary();


                r.OD_TotalLimitOdDefaultedLoan = worksheet1.Cells[2, 2].Value;
                r.OD_BalanceAtDefault = worksheet1.Cells[3, 2].Value;
                r.OD_Balance12MonthBeforeDefault = worksheet1.Cells[4, 2].Value;
                r.OD_TotalConversation = worksheet1.Cells[5, 2].Value;
                r.OD_CCF = worksheet1.Cells[6, 2].Value;

                r.Card_TotalLimitOdDefaultedLoan = worksheet1.Cells[2, 3].Value;
                r.Card_BalanceAtDefault = worksheet1.Cells[3, 3].Value;
                r.Card_Balance12MonthBeforeDefault = worksheet1.Cells[4, 3].Value;
                r.Card_TotalConversation = worksheet1.Cells[5, 3].Value;
                r.Card_CCF = worksheet1.Cells[6, 3].Value;

                r.Overall_TotalLimitOdDefaultedLoan = worksheet1.Cells[2, 4].Value;
                r.Overall_BalanceAtDefault = worksheet1.Cells[3, 4].Value;
                r.Overall_Balance12MonthBeforeDefault = worksheet1.Cells[4, 4].Value;
                r.Overall_TotalConversation = worksheet1.Cells[5, 4].Value;
                r.Overall_CCF = worksheet1.Cells[6, 4].Value;



                theWorkbook.Save();
                theWorkbook.Close(true);
                excel.Quit();

            qry = Queries.CalibrationResult_EAD_CCF_Summary_Update(calibrationId, r.OD_TotalLimitOdDefaultedLoan??0, r.OD_BalanceAtDefault??0, r.OD_Balance12MonthBeforeDefault??0, 
                r.OD_TotalConversation??0, r.OD_CCF??0, r.Card_TotalLimitOdDefaultedLoan??0, r.Card_BalanceAtDefault??0, r.Card_Balance12MonthBeforeDefault??0,
                r.Card_TotalConversation??0, r.Card_CCF??0, r.Overall_TotalLimitOdDefaultedLoan??0, r.Overall_BalanceAtDefault??0, r.Overall_Balance12MonthBeforeDefault??0, r.Overall_TotalConversation??0,
                r.Overall_CCF??0);
            DataAccess.i.ExecuteQuery(qry);
            }
            catch (Exception ex)
            {
                theWorkbook.Save();
                theWorkbook.Close(true);
                excel.Quit();
            }
            //File.Delete(path1);



            return true;


        }


        public CalibrationResult_EAD_CCF_Summary GetCCFData(Guid eclId, EclType eclType)
        {
            string qry = Queries.GetEADCCFData(eclId, eclType.ToString());
            var dt = DataAccess.i.GetData(qry);
            if (dt.Rows.Count == 0)
            {
                return new CalibrationResult_EAD_CCF_Summary { OD_TotalLimitOdDefaultedLoan = 0, OD_BalanceAtDefault = 0, OD_Balance12MonthBeforeDefault = 0, OD_TotalConversation = 0, OD_CCF = 0, Card_TotalLimitOdDefaultedLoan = 0, Card_BalanceAtDefault = 0, Card_Balance12MonthBeforeDefault = 0, Card_TotalConversation = 0, Card_CCF = 0, Overall_TotalLimitOdDefaultedLoan=0, Overall_BalanceAtDefault=0, Overall_Balance12MonthBeforeDefault=0, Overall_TotalConversation=0, Overall_CCF=0 };
            }
            DataRow dr = dt.Rows[0];
            var itm = new CalibrationResult_EAD_CCF_Summary();
            try { itm.OD_TotalLimitOdDefaultedLoan = double.Parse(dr["OD_TotalLimitOdDefaultedLoan"].ToString().Trim()); } catch { itm.OD_TotalLimitOdDefaultedLoan = 0; }
            try { itm.OD_BalanceAtDefault = double.Parse(dr["OD_BalanceAtDefault"].ToString().Trim()); } catch { itm.OD_BalanceAtDefault = 0; }
            try { itm.OD_Balance12MonthBeforeDefault = double.Parse(dr["OD_Balance12MonthBeforeDefault"].ToString().Trim()); } catch { itm.OD_Balance12MonthBeforeDefault = 0; }
            try { itm.OD_TotalConversation = double.Parse(dr["OD_TotalConversation"].ToString().Trim()); } catch { itm.OD_TotalConversation = 0; }


            try { itm.OD_CCF = double.Parse(dr["OD_CCF"].ToString().Trim()); } catch { itm.OD_CCF = 0; }
            try { itm.Card_TotalLimitOdDefaultedLoan = double.Parse(dr["Card_TotalLimitOdDefaultedLoan"].ToString().Trim()); } catch { itm.Card_TotalLimitOdDefaultedLoan = 0; }
            try { itm.Card_BalanceAtDefault = double.Parse(dr["Card_BalanceAtDefault"].ToString().Trim()); } catch { itm.Card_BalanceAtDefault = 0; }
            try { itm.Card_Balance12MonthBeforeDefault = double.Parse(dr["Card_Balance12MonthBeforeDefault"].ToString().Trim()); } catch { itm.Card_Balance12MonthBeforeDefault = 0; }

            try { itm.Card_TotalConversation = double.Parse(dr["Card_TotalConversation"].ToString().Trim()); } catch { itm.Card_TotalConversation = 0; }
            try { itm.Card_CCF = double.Parse(dr["Card_CCF"].ToString().Trim()); } catch { itm.Card_CCF = 0; }
            try { itm.Overall_TotalLimitOdDefaultedLoan = double.Parse(dr["Overall_TotalLimitOdDefaultedLoan"].ToString().Trim()); } catch { itm.Overall_TotalLimitOdDefaultedLoan = 0; }
            try { itm.Overall_BalanceAtDefault = double.Parse(dr["Overall_BalanceAtDefault"].ToString().Trim()); } catch { itm.Overall_BalanceAtDefault = 0; }

            try { itm.Overall_Balance12MonthBeforeDefault = double.Parse(dr["Overall_Balance12MonthBeforeDefault"].ToString().Trim()); } catch { itm.Overall_Balance12MonthBeforeDefault = 0; }
            try { itm.Overall_TotalConversation = double.Parse(dr["Overall_TotalConversation"].ToString().Trim()); } catch { itm.Overall_TotalConversation = 0; }
            try { itm.Overall_CCF = double.Parse(dr["Overall_CCF"].ToString().Trim()); } catch { itm.Overall_CCF = 0; }
            
            return itm;
        }
    }
}
