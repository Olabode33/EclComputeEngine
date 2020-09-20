using IFRS9_ECL.Core.Calibration.Entities;
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
    public class CalibrationInput_PD_CR_RD_Processor
    {

        public bool ProcessCalibration(Guid calibrationId, long affiliateId)
        {

            var baseAffPath = Path.Combine(Util.AppSettings.CalibrationModelPath, affiliateId.ToString());
            if (!Directory.Exists(baseAffPath))
            {
                Directory.CreateDirectory(baseAffPath);
            }
            
            var qry = Queries.CalibrationInput_PD_CR_DR(calibrationId);
            var _dt = DataAccess.i.GetData(qry);

            qry = Queries.CalibrationResultHistoric_PD_CommsCons(affiliateId);
            var dt_HistoricCommsCons = DataAccess.i.GetData(qry);
            qry = Queries.CalibrationResultHistoric_PD_Corporate(affiliateId);
            var dt_HistoricCorporate = DataAccess.i.GetData(qry);
            qry = Queries.CalibrationResultHistoric_PD_Output(affiliateId);
            var dt_HistoricOutput = DataAccess.i.GetData(qry);

            //DataView dv = _dt.DefaultView;
            //dv.Sort = "Account_No,Contract_No,RAPP_Date";
            var dt = _dt;// dv.ToTable();
            var rowCount = dt.Rows.Count + 2;

            if (dt.Rows.Count == 0)
                return true;

            var counter = Util.AppSettings.GetCounter(dt.Rows.Count);

            var path = $"{Path.Combine(Util.AppSettings.CalibrationModelPath, counter.ToString(), "PD_CR_RD.xlsx")}";
            var fileGuid = Guid.NewGuid().ToString();
            var path1 = $"{Path.Combine(baseAffPath, $"{fileGuid}PD_CR_RD.xlsx")}";

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
                //for(int i=0; i< dt.Rows.Count-48; i++)
                //{
                //    worksheet.InsertRow(1, 1, 2);
                //}

                //1 is for header
                if (rows > (dt.Rows.Count + 3))
                    worksheet.DeleteRow(dt.Rows.Count + 3, rows - (dt.Rows.Count + 3)); //TODO::: Enable after testing
                // loop through the worksheet rows

                //package.Workbook.CalcMode = ExcelCalcMode.Automatic;

                for (int i = 0; i < dt.Rows.Count; i++)// DataRow dr in dt.Rows)
                {
                    Log4Net.Log.Info(i);
                    DataRow dr = dt.Rows[i];
                    var itm = DataAccess.i.ParseDataToObject(new CalibrationInput_PD_CR_DR(), dr);

                    if (string.IsNullOrEmpty(itm.Account_No) && string.IsNullOrEmpty(itm.Contract_No) && itm.RAPP_Date == null)
                        continue;

                    worksheet.Cells[i + 3, 1].Value = itm.Customer_No;
                    worksheet.Cells[i + 3, 2].Value = itm.Account_No;
                    worksheet.Cells[i + 3, 3].Value = itm.Contract_No;
                    worksheet.Cells[i + 3, 4].Value = itm.Product_Type;
                    try { worksheet.Cells[i + 3, 5].Value = Convert.ToInt32(itm.Current_Rating); } catch { worksheet.Cells[i + 3, 5].Value = itm.Current_Rating; }
                    worksheet.Cells[i + 3, 6].Value = itm.Days_Past_Due;
                    worksheet.Cells[i + 3, 7].Value = itm.Classification;
                    worksheet.Cells[i + 3, 8].Value = itm.Outstanding_Balance_Lcy;
                    worksheet.Cells[i + 3, 9].Value = itm.Contract_Start_Date;
                    worksheet.Cells[i + 3, 10].Value = itm.Contract_End_Date;
                    worksheet.Cells[i + 3, 11].Value = itm.RAPP_Date;
                    worksheet.Cells[i + 3, 12].Value = itm.Segment;
                }

                //Write Historic Result Cons Comms
                if (dt_HistoricCommsCons.Rows.Count > 0)
                {
                    ExcelWorksheet worksheet_comm_cons = package.Workbook.Worksheets[3];//.FirstOrDefault();
                    for (int i = 0; i < dt_HistoricCommsCons.Rows.Count; i++)
                    {
                        var itm = DataAccess.i.ParseDataToObject(new CalibrationResultHistoric_PD_CommsCons(), dt_HistoricCommsCons.Rows[i]);
                        int stage = itm.Stage;
                        switch (stage)
                        {
                            case 1:
                                try { worksheet_comm_cons.Cells[15, 15].Value = itm.Comm_1; } catch { worksheet_comm_cons.Cells[15, 15].Value = 0; }
                                try { worksheet_comm_cons.Cells[15, 16].Value = itm.Comm_2; } catch { worksheet_comm_cons.Cells[15, 16].Value = 0; }
                                try { worksheet_comm_cons.Cells[15, 17].Value = itm.Comm_3; } catch { worksheet_comm_cons.Cells[15, 17].Value = 0; }
                                try { worksheet_comm_cons.Cells[15, 20].Value = itm.Cons_1; } catch { worksheet_comm_cons.Cells[15, 20].Value = 0; }
                                try { worksheet_comm_cons.Cells[15, 21].Value = itm.Cons_2; } catch { worksheet_comm_cons.Cells[15, 21].Value = 0; }
                                try { worksheet_comm_cons.Cells[15, 22].Value = itm.Cons_3; } catch { worksheet_comm_cons.Cells[15, 22].Value = 0; }
                                break;
                            case 2:
                                try { worksheet_comm_cons.Cells[16, 15].Value = itm.Comm_1; } catch { worksheet_comm_cons.Cells[16, 15].Value = 0; }
                                try { worksheet_comm_cons.Cells[16, 16].Value = itm.Comm_2; } catch { worksheet_comm_cons.Cells[16, 16].Value = 0; }
                                try { worksheet_comm_cons.Cells[16, 17].Value = itm.Comm_3; } catch { worksheet_comm_cons.Cells[16, 17].Value = 0; }
                                try { worksheet_comm_cons.Cells[16, 20].Value = itm.Cons_1; } catch { worksheet_comm_cons.Cells[16, 20].Value = 0; }
                                try { worksheet_comm_cons.Cells[16, 21].Value = itm.Cons_2; } catch { worksheet_comm_cons.Cells[16, 21].Value = 0; }
                                try { worksheet_comm_cons.Cells[16, 22].Value = itm.Cons_3; } catch { worksheet_comm_cons.Cells[16, 22].Value = 0; }
                                break;
                            case 3:
                                try { worksheet_comm_cons.Cells[17, 15].Value = itm.Comm_1; } catch { worksheet_comm_cons.Cells[17, 15].Value = 0; }
                                try { worksheet_comm_cons.Cells[17, 16].Value = itm.Comm_2; } catch { worksheet_comm_cons.Cells[17, 16].Value = 0; }
                                try { worksheet_comm_cons.Cells[17, 17].Value = itm.Comm_3; } catch { worksheet_comm_cons.Cells[17, 17].Value = 0; }
                                try { worksheet_comm_cons.Cells[17, 20].Value = itm.Cons_1; } catch { worksheet_comm_cons.Cells[17, 20].Value = 0; }
                                try { worksheet_comm_cons.Cells[17, 21].Value = itm.Cons_2; } catch { worksheet_comm_cons.Cells[17, 21].Value = 0; }
                                try { worksheet_comm_cons.Cells[17, 22].Value = itm.Cons_3; } catch { worksheet_comm_cons.Cells[17, 22].Value = 0; }
                                break;
                            default:
                                break;
                        }
                        
                    }
                }

                //Write Historic Result Corporate
                if (dt_HistoricCorporate.Rows.Count > 0)
                {
                    ExcelWorksheet worksheet_corp = package.Workbook.Worksheets[2];//.FirstOrDefault();
                    ///worksheet_corp.Calculate();
                    for (int i = 0; i < dt_HistoricCorporate.Rows.Count; i++)
                    {
                        var itm = DataAccess.i.ParseDataToObject(new CalibrationResultHistoric_PD_Corporate(), dt_HistoricCorporate.Rows[i]);
                        //Outstanding Balance
                        for (int row = 5; row < 25; row++)
                        {
                            int rappDate = -1;
                            var t = worksheet_corp.Cells[row, 2].Value;
                            try { rappDate = Convert.ToInt32(t); } catch { }

                            if (rappDate == itm.RAPPDATE)
                            {
                                try { worksheet_corp.Cells[row, 3].Value = itm.OutstandingBalance_1; } catch { worksheet_corp.Cells[row, 3].Value = 0; }
                                try { worksheet_corp.Cells[row, 4].Value = itm.OutstandingBalance_2; } catch { worksheet_corp.Cells[row, 4].Value = 0; }
                                try { worksheet_corp.Cells[row, 5].Value = itm.OutstandingBalance_3; } catch { worksheet_corp.Cells[row, 5].Value = 0; }
                                try { worksheet_corp.Cells[row, 6].Value = itm.OutstandingBalance_4; } catch { worksheet_corp.Cells[row, 6].Value = 0; }
                                try { worksheet_corp.Cells[row, 7].Value = itm.OutstandingBalance_5; } catch { worksheet_corp.Cells[row, 7].Value = 0; }
                                try { worksheet_corp.Cells[row, 8].Value = itm.OutstandingBalance_6; } catch { worksheet_corp.Cells[row, 8].Value = 0; }
                                try { worksheet_corp.Cells[row, 9].Value = itm.OutstandingBalance_7; } catch { worksheet_corp.Cells[row, 9].Value = 0; }
                                try { worksheet_corp.Cells[row, 10].Value = itm.OutstandingBalance_8; } catch { worksheet_corp.Cells[row, 10].Value = 0; }
                                try { worksheet_corp.Cells[row, 11].Value = itm.OutstandingBalance_9; } catch { worksheet_corp.Cells[row, 11].Value = 0; }
                                try { worksheet_corp.Cells[row, 12].Value = itm.OutstandingBalance_10; } catch { worksheet_corp.Cells[row, 12].Value = 0; }
                            }
                        }
                        //Balance Transition to Default
                        for (int row = 32; row < 52; row++)
                        {
                            int rappDate = -1;
                            var t = worksheet_corp.Cells[row, 2].Value;
                            try { rappDate = Convert.ToInt32(t); } catch { }

                            if (rappDate == itm.RAPPDATE)
                            {
                                try { worksheet_corp.Cells[row, 3].Value = itm.Balance_1; } catch { worksheet_corp.Cells[row, 3].Value = 0; }
                                try { worksheet_corp.Cells[row, 4].Value = itm.Balance_2; } catch { worksheet_corp.Cells[row, 4].Value = 0; }
                                try { worksheet_corp.Cells[row, 5].Value = itm.Balance_3; } catch { worksheet_corp.Cells[row, 5].Value = 0; }
                                try { worksheet_corp.Cells[row, 6].Value = itm.Balance_4; } catch { worksheet_corp.Cells[row, 6].Value = 0; }
                                try { worksheet_corp.Cells[row, 7].Value = itm.Balance_5; } catch { worksheet_corp.Cells[row, 7].Value = 0; }
                                try { worksheet_corp.Cells[row, 8].Value = itm.Balance_6; } catch { worksheet_corp.Cells[row, 8].Value = 0; }
                                try { worksheet_corp.Cells[row, 9].Value = itm.Balance_7; } catch { worksheet_corp.Cells[row, 9].Value = 0; }
                                try { worksheet_corp.Cells[row, 10].Value = itm.Balance_8; } catch { worksheet_corp.Cells[row, 10].Value = 0; }
                                try { worksheet_corp.Cells[row, 11].Value = itm.Balance_9; } catch { worksheet_corp.Cells[row, 11].Value = 0; }
                                try { worksheet_corp.Cells[row, 12].Value = itm.Balance_10; } catch { worksheet_corp.Cells[row, 12].Value = 0; }
                            }
                        }
                    }
                }

                //Write Historic Result Output
                if (dt_HistoricOutput.Rows.Count > 0)
                {
                    ExcelWorksheet worksheet_output = package.Workbook.Worksheets[0];//.FirstOrDefault();
                    var itm = DataAccess.i.ParseDataToObject(new CalibrationResultHistoric_PD_Output(), dt_HistoricOutput.Rows[0]);

                    try { worksheet_output.Cells[4, 3].Value = itm.Rating_1; } catch { worksheet_output.Cells[4, 3].Value = 0; }
                    try { worksheet_output.Cells[5, 3].Value = itm.Rating_2; } catch { worksheet_output.Cells[5, 3].Value = 0; }
                    try { worksheet_output.Cells[6, 3].Value = itm.Rating_3; } catch { worksheet_output.Cells[6, 3].Value = 0; }
                    try { worksheet_output.Cells[7, 3].Value = itm.Rating_4; } catch { worksheet_output.Cells[7, 3].Value = 0; }
                    try { worksheet_output.Cells[8, 3].Value = itm.Rating_5; } catch { worksheet_output.Cells[8, 3].Value = 0; }
                    try { worksheet_output.Cells[9, 3].Value = itm.Rating_6; } catch { worksheet_output.Cells[9, 3].Value = 0; }
                    try { worksheet_output.Cells[10, 3].Value = itm.Rating_7; } catch { worksheet_output.Cells[10, 3].Value = 0; }
                    try { worksheet_output.Cells[11, 3].Value = itm.Rating_8; } catch { worksheet_output.Cells[11, 3].Value = 0; }
                    try { worksheet_output.Cells[12, 3].Value = itm.Rating_9; } catch { worksheet_output.Cells[12, 3].Value = 0; }
                    try { worksheet_output.Cells[13, 3].Value = itm.Rating_10; } catch { worksheet_output.Cells[13, 3].Value = 0; }
                    try { worksheet_output.Cells[14, 3].Value = itm.Rating_Comm; } catch { worksheet_output.Cells[14, 3].Value = 0; }
                    try { worksheet_output.Cells[15, 3].Value = itm.Rating_Cons; } catch { worksheet_output.Cells[15, 3].Value = 0; }

                    try { worksheet_output.Cells[19, 3].Value = itm.Defaulted_Loan; } catch { worksheet_output.Cells[19, 3].Value = 0; }
                    try { worksheet_output.Cells[20, 3].Value = itm.Cured_Loan; } catch { worksheet_output.Cells[20, 3].Value = 0; }
                    try { worksheet_output.Cells[24, 3].Value = itm.Redefaulted_Loans; } catch { worksheet_output.Cells[24, 3].Value = 0; }
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


            Log4Net.Log.Info("Done updating excel");
            //refresh and calculate to modify
            theWorkbook.RefreshAll();
            Log4Net.Log.Info("Done refreshing");
            excel.Calculate();
            Log4Net.Log.Info("Done Calculating");
            //Get inputs for solver template


            //Sort
            Worksheet calculationSheet = theWorkbook.Sheets[2];
            Range sortRange = calculationSheet.Range["A3", "L" + rowCount.ToString()];
            //sortRange.Sort(sortRange.Columns[13]); // Unique ID
            sortRange.Sort(sortRange.Columns[11], XlSortOrder.xlAscending, DataOption1: XlSortDataOption.xlSortTextAsNumbers); // RAPPDATE
            sortRange.Sort(sortRange.Columns[3], XlSortOrder.xlAscending, DataOption1: XlSortDataOption.xlSortTextAsNumbers); // Contract no
            sortRange.Sort(sortRange.Columns[12], XlSortOrder.xlAscending, DataOption1: XlSortDataOption.xlSortTextAsNumbers); // Segment

            //Temp fix for #REF error after deleting rows
            Range tempRange = calculationSheet.Range["A" + (rowCount + 1).ToString(), "M700000"];
            tempRange.EntireRow.Delete();



            Log4Net.Log.Info("Done updating excel");
            //refresh and calculate to modify
            theWorkbook.RefreshAll();
            Log4Net.Log.Info("Done refreshing");
            excel.Calculate();
            Log4Net.Log.Info("Done Calculating");
            //Get inputs for solver template
            Worksheet pdCalculationSheet = theWorkbook.Sheets[3];
            Dictionary<int, string> solverInputs12MonthsPd = new Dictionary<int, string>();
            Dictionary<int, string> solverInputsOutstandingBal = new Dictionary<int, string>();
            for (int i = 0; i < 10; i++)
            {
                var pdValue = pdCalculationSheet.Cells[79, 3 + i].Value;
                var outstandingBalValue = pdCalculationSheet.Cells[53, 3 + i].Value;
                //solverSheet.Cells[5, 3 + i] = pdCalculationSheet.Cells[79, 3 + i].Value;
                solverInputs12MonthsPd[i + 1] = pdValue.ToString();
                solverInputsOutstandingBal[i + 1] = outstandingBalValue.ToString();
            }

            theWorkbook.Save();
            Log4Net.Log.Info("Save to Path");
            theWorkbook.Close(true);
            Log4Net.Log.Info("Close");
            //excel.Quit();

            #region ExcelSolver
            //Solution for Excel Solver

            var solverTemplatePath = $"{Path.Combine(Util.AppSettings.CalibrationModelPath, "PD_CR_RD_Solver_Template.xlsm")}";
            var solverFilePath = $"{Path.Combine(baseAffPath, $"{fileGuid}_PD_CR_RD_Solver.xlsm")}";

            //Save solver file for affiliate
            Application solverExcel = new Application();
            solverExcel.Visible = false;
            solverExcel.AutomationSecurity = Microsoft.Office.Core.MsoAutomationSecurity.msoAutomationSecurityLow;
            var solverTemplateWorkbook = solverExcel.Workbooks.Open(solverTemplatePath,
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
            solverTemplateWorkbook.SaveAs(solverFilePath);
            solverTemplateWorkbook.Close();

            //Reopen for calculation 
            var solverWorkbook = solverExcel.Workbooks.Open(solverFilePath,
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

            Worksheet solverSheet = solverWorkbook.Sheets[1];
            for (int i = 0; i < 10; i++)
            {
                try{ solverSheet.Cells[4, 3 + i] = Convert.ToDouble(solverInputsOutstandingBal[i + 1]); } catch { solverSheet.Cells[4, 3 + i] = 0; }
                try{ solverSheet.Cells[8, 3 + i] = Convert.ToDouble(solverInputs12MonthsPd[i + 1]); } catch { solverSheet.Cells[8, 3 + i] = 0; }
            }

            //refresh and calculate to modify
            solverWorkbook.RefreshAll();
            Log4Net.Log.Info("Done initial solver calculation");
            solverExcel.Calculate();
            Log4Net.Log.Info("Done initial solver calculation");

            var solverValueG = 0.0;
            var solverValueI = 0.0;
            if (solverExcel.AddIns["Solver Add-In"].Installed)
            {
                solverExcel.Run("PdCrDrSolverMacro");
                //update solver result and recalculate
                solverValueG = solverSheet.Cells[11, 7].Value;
                solverValueI = solverSheet.Cells[11, 9].Value;
                //pdCalculationSheet.Cells[83,7] = solverSheet.Cells[8,7].Value;
                //pdCalculationSheet.Cells[83,9] = solverSheet.Cells[8,9].Value;

                solverWorkbook.Save();
                solverWorkbook.Close(true);
                solverExcel.Quit();
            }
            else
            {
                Log4Net.Log.Error("Solver Add-In not installed");
            }

            #endregion ExcelSolver

            theWorkbook = excel.Workbooks.Open(txtLocation,
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


            pdCalculationSheet = theWorkbook.Sheets[3];
            pdCalculationSheet.Cells[83,7] = solverValueG;
            pdCalculationSheet.Cells[83,9] = solverValueI;
            Log4Net.Log.Info("Done updating excel");
            //refresh and calculate to modify
            theWorkbook.RefreshAll();
            Log4Net.Log.Info("Done refreshing");
            excel.Calculate();
            Log4Net.Log.Info("Done Calculating");

            Worksheet worksheet1 = theWorkbook.Sheets[1];

            var sb = new StringBuilder();
            
            for (int i=0; i<10; i++)
            {
                var r = new CalibrationResult_PD_12Months();
                
                r.Rating = worksheet1.Cells[4+i, 1].Value;
                r.Outstanding_Balance = worksheet1.Cells[4 + i, 2].Value;
                r.Redefault_Balance = worksheet1.Cells[4 + i, 3].Value;
                r.Redefaulted_Balance = worksheet1.Cells[4 + i, 4].Value;
                r.Total_Redefault = worksheet1.Cells[4 + i, 5].Value;
                r.Months_PDs_12 = worksheet1.Cells[4 + i, 6].Value;

                qry = Queries.CalibrationResult_PD_Update(calibrationId, r.Rating, r.Outstanding_Balance, r.Redefault_Balance, r.Redefaulted_Balance, r.Total_Redefault, r.Months_PDs_12);
                sb.Append(qry);
            }

            //PD Comms Cons Marginal Default rates
            var commCons = new StringBuilder();
            for (int i = 0; i < 240; i++)
            {
                var r = new CalibrationResult_PD_CommsCons_MarginalDefaultRate();

                r.Month = i + 1;
                r.Comm1 = worksheet1.Cells[11 + i, 11].Value;
                r.Cons1 = worksheet1.Cells[11 + i, 12].Value;
                r.Comm2 = worksheet1.Cells[11 + i, 13].Value;
                r.Cons2 = worksheet1.Cells[11 + i, 14].Value;

                qry = Queries.CalibrationResult_PD_CommCons_Update(calibrationId, r.Month, r.Comm1, r.Cons1, r.Comm2, r.Cons2);
                commCons.Append(qry);
            }

            Log4Net.Log.Info("Done Extracting");

            var rs = new CalibrationResult_PD_12Months_Summary();

            rs.Normal_12_Months_PD = worksheet1.Cells[16,6].Value;
            rs.Normal_12_Months_PD = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.Normal_12_Months_PD) ?0 : rs.Normal_12_Months_PD;


            rs.DefaultedLoansA = worksheet1.Cells[19, 3].Value;
            rs.DefaultedLoansA = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.DefaultedLoansA) ?0 : rs.DefaultedLoansA;

            rs.DefaultedLoansB = worksheet1.Cells[19, 4].Value;
            rs.DefaultedLoansB = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.DefaultedLoansB) ?0 : rs.DefaultedLoansB;

            rs.CuredLoansA = worksheet1.Cells[20, 3].Value;
            rs.CuredLoansA = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.CuredLoansA) ?0 : rs.CuredLoansA;

            rs.CuredLoansB = worksheet1.Cells[20, 4].Value;
            rs.CuredLoansB = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.CuredLoansB) ?0 : rs.CuredLoansB;

            rs.Cure_Rate = worksheet1.Cells[21, 5].Value;
            rs.Cure_Rate = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.Cure_Rate) ?0 : rs.Cure_Rate;

            rs.CuredPopulationA = worksheet1.Cells[23, 3].Value;
            rs.CuredPopulationA = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.CuredPopulationA) ?0 : rs.CuredPopulationA;

            rs.CuredPopulationB = worksheet1.Cells[23, 4].Value;
            rs.CuredPopulationB = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.CuredPopulationB) ?0 : rs.CuredPopulationB;

            rs.RedefaultedLoansA = worksheet1.Cells[24, 3].Value;
            rs.RedefaultedLoansA = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.RedefaultedLoansA) ?0 : rs.RedefaultedLoansA;

            rs.RedefaultedLoansB = worksheet1.Cells[24, 4].Value;
            rs.RedefaultedLoansB = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.RedefaultedLoansB) ?0 : rs.RedefaultedLoansB;

            rs.Redefault_Rate = worksheet1.Cells[25, 5].Value;
            rs.Redefault_Rate = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.Redefault_Rate) ?0 : rs.Redefault_Rate;

            rs.Redefault_Factor = worksheet1.Cells[27, 3].Value;
            rs.Redefault_Factor = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.Redefault_Factor) ?0 : rs.Redefault_Factor;


            rs.Commercial_CureRate = worksheet1.Cells[31, 3].Value;
            rs.Commercial_CureRate = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.Commercial_CureRate) ? 0 : rs.Commercial_CureRate;


            rs.Commercial_RedefaultRate = worksheet1.Cells[7, 11].Value;
            rs.Commercial_RedefaultRate = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.Commercial_RedefaultRate) ? 0 : rs.Commercial_RedefaultRate;


            rs.Consumer_CureRate = worksheet1.Cells[32, 3].Value;
            rs.Consumer_CureRate = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.Consumer_CureRate) ? 0 : rs.Consumer_CureRate;


            rs.Consumer_RedefaultRate = worksheet1.Cells[7, 12].Value;
            rs.Consumer_RedefaultRate = ECLNonStringConstants.i.ExcelDefaultValue.Contains(rs.Consumer_RedefaultRate) ? 0 : rs.Consumer_RedefaultRate;

            Log4Net.Log.Info("Got SUmmary");

            theWorkbook.Save();
            Log4Net.Log.Info("Save to Path");
            theWorkbook.Close(true);
            Log4Net.Log.Info("Close");
            excel.Quit();
            Log4Net.Log.Info("Quite");
            //File.Delete(path1);

            qry =Queries.CalibrationResult_PD_Update_Summary(calibrationId, sb.ToString(), commCons.ToString(), rs.Normal_12_Months_PD, rs.DefaultedLoansA, rs.DefaultedLoansB, rs.CuredLoansA, rs.CuredLoansB, rs.Cure_Rate, rs.CuredPopulationA, rs.CuredPopulationB, rs.RedefaultedLoansA, rs.RedefaultedLoansB, rs.Redefault_Rate, rs.Redefault_Factor
                                                             ,rs.Commercial_CureRate, rs.Commercial_RedefaultRate, rs.Consumer_CureRate, rs.Consumer_RedefaultRate);
            DataAccess.i.ExecuteQuery(qry);

            return true;


        }

        /// <summary>
        /// return index 1= Redefault_Factor
        /// return index 2= Cure_Rate
        /// </summary>
        /// <param name="eclId"></param>
        /// <param name="eclType"></param>
        /// <returns></returns>
        public double[] GetPDRedefaultFactorCureRate(Guid eclId, EclType eclType)
        {
            string qry = Queries.GetPDRedefaultFactor(eclId, eclType.ToString());
            var dt = DataAccess.i.GetData(qry);
            if (dt.Rows.Count == 0)
            {
                return new double[] { 0, 0 };
            }
            DataRow dr = dt.Rows[0];
            var Redefault_Factor = 0.0;
            var Cure_Rate = 0.0;
            var Commercial_CureRate = 0.0;
            var Consumer_CureRate = 0.0;
            try { Redefault_Factor= double.Parse(dr["Redefault_Factor"].ToString().Trim()); } catch { Redefault_Factor= 0; }
            try { Cure_Rate = double.Parse(dr["Cure_Rate"].ToString().Trim()); } catch { Cure_Rate = 0; }
            try { Commercial_CureRate = double.Parse(dr["Commercial_CureRate"].ToString().Trim()); } catch { Commercial_CureRate = 0; }
            try { Consumer_CureRate = double.Parse(dr["Consumer_CureRate"].ToString().Trim()); } catch { Consumer_CureRate = 0; }

            return new double[] { Redefault_Factor, Cure_Rate, Commercial_CureRate, Consumer_CureRate };
        }


        public List<PD12Months> GetPD12MonthsPD(Guid eclId, EclType eclType)
        {
            string qry = Queries.GetPD12MonthsPD(eclId, eclType.ToString());
            var dt = DataAccess.i.GetData(qry);
            if (dt.Rows.Count == 0)
            {
                return new List<PD12Months>();
            }
            var lst = new List<PD12Months>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                var itm = new PD12Months();
                try { itm.Rating = int.Parse(dr["Rating"].ToString().Trim()); } catch { itm.Rating = 0; }
                try { itm.Months_PDs_12 = double.Parse(dr["Months_PDs_12"].ToString().Trim()); } catch { itm.Months_PDs_12 = 0; }
                lst.Add(itm);
            }
            return lst;
        }


    }
}
