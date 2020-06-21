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

            var path = $"{Path.Combine(Util.AppSettings.CalibrationModelPath, affiliateId.ToString(), "PD_CR_RD.xlsx")}";
            var path1 = $"{Path.Combine(Util.AppSettings.CalibrationModelPath, affiliateId.ToString(), $"{calibrationId.ToString()}_PD_CR_RD.xlsx")}";
            if (File.Exists(path1))
            {
                File.Delete(path1);
            }

            var qry = Queries.CalibrationInput_PD_CR_DR(calibrationId);
            var dt=DataAccess.i.GetData(qry);




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
                    var itm = DataAccess.i.ParseDataToObject(new CalibrationInput_PD_CR_DR(), dr);

                    worksheet.Cells[i + 2, 1].Value = itm.Customer_No;
                    worksheet.Cells[i + 2, 2].Value = itm.Account_No;
                    worksheet.Cells[i + 2, 3].Value = itm.Contract_No;
                    worksheet.Cells[i + 2, 4].Value = itm.Product_Type;
                    worksheet.Cells[i + 2, 5].Value = itm.Current_Rating;
                    worksheet.Cells[i + 2, 6].Value = itm.Days_Past_Due;
                    worksheet.Cells[i + 2, 7].Value = itm.Classification;
                    worksheet.Cells[i + 2, 8].Value = itm.Outstanding_Balance_Lcy;
                    worksheet.Cells[i + 2, 9].Value = itm.Contract_Start_Date;
                    worksheet.Cells[i + 2, 10].Value = itm.Contract_End_Date;
                    worksheet.Cells[i + 2, 11].Value = itm.RAPP_Date;
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



            Console.WriteLine("Done updating excel");
            //refresh and calculate to modify
            theWorkbook.RefreshAll();
            Console.WriteLine("Done refreshing");
            excel.Calculate();
            Console.WriteLine("Done Calculating");

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

            Console.WriteLine("Done Extracting");

            var rs = new CalibrationResult_PD_12Months_Summary();

            rs.Normal_12_Months_PD = worksheet1.Cells[14,6].Value;
            rs.DefaultedLoansA = worksheet1.Cells[17, 3].Value;
            rs.DefaultedLoansB = worksheet1.Cells[17, 4].Value;
            rs.CuredLoansA = worksheet1.Cells[18, 3].Value;
            rs.CuredLoansB = worksheet1.Cells[18, 4].Value;

            rs.Cure_Rate = worksheet1.Cells[19, 5].Value;

            rs.CuredPopulationA = worksheet1.Cells[21, 3].Value;
            rs.CuredPopulationB = worksheet1.Cells[21, 4].Value;

            rs.RedefaultedLoansA = worksheet1.Cells[22, 3].Value;
            rs.RedefaultedLoansB = worksheet1.Cells[22, 4].Value;

            rs.Redefault_Rate = worksheet1.Cells[23, 5].Value;

            rs.Redefault_Factor = worksheet1.Cells[25, 5].Value;

            Console.WriteLine("Got SUmmary");

            theWorkbook.Save();
            Console.WriteLine("Save to Path");
            theWorkbook.Close(true);
            Console.WriteLine("Close");
            excel.Quit();
            Console.WriteLine("Quite");
            //File.Delete(path1);

            qry =Queries.CalibrationResult_PD_Update_Summary(calibrationId, sb.ToString(), rs.Normal_12_Months_PD, rs.DefaultedLoansA, rs.DefaultedLoansB, rs.CuredLoansA, rs.CuredLoansB, rs.Cure_Rate, rs.CuredPopulationA, rs.CuredPopulationB, rs.RedefaultedLoansA, rs.RedefaultedLoansB, rs.Redefault_Rate, rs.Redefault_Factor);
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
            try { Redefault_Factor= double.Parse(dr["Redefault_Factor"].ToString().Trim()); } catch { Redefault_Factor= 0; }
            try { Cure_Rate = double.Parse(dr["Cure_Rate"].ToString().Trim()); } catch { Cure_Rate = 0; }

            return new double[] { Redefault_Factor, Cure_Rate };
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
