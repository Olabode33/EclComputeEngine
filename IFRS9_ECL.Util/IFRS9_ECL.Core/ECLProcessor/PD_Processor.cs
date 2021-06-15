using IFRS9_ECL.Core.Calibration.Input;
using IFRS9_ECL.Core.ECLProcessor.Entities;
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
    public class PD_Processor
    {

        public bool ProcessPD(PDParameters input)
        {

            var loanbook = Path.Combine(input.BasePath, input.LoanBookFileName);
            var model = Path.Combine(input.BasePath, input.ModelFileName);
            string txtLocation = Path.GetFullPath(model);

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
                Worksheet startSheet = theWorkbook.Sheets[1];
                startSheet.Unprotect(AppSettings.SheetPassword);

                startSheet.Cells[9, 5] = input.ReportDate.ToString("dd MMMM yyyy");
                startSheet.Cells[12, 5] = input.RedefaultAdjustmentFactor;
                startSheet.Cells[13, 5] = input.SandPMapping;

                startSheet.Cells[15, 5] = input.NonExpired;
                startSheet.Cells[16, 5] = input.Expired;

                startSheet.Cells[20, 5] = loanbook;

                var fileName = new FileInfo(loanbook).Name;
                startSheet.Cells[21, 5] = fileName;

                excel.ScreenUpdating=false;


                excel.Run("unhide_unprotect");
                excel.Run("primary_condition_extractor");
                excel.Run("centre_sheets");
                excel.Run("hide_protect");

                excel.Run("unhide_unprotect");
                excel.Run("primary_condition_extractor");               
                excel.Run("centre_sheets");
                excel.Run("hide_protect");

                excel.Run("unhide_unprotect");
                excel.Run("resize_pd_workbook");
                excel.Run("centre_sheets");
                excel.Run("hide_protect");

                excel.ScreenUpdating = true;

                theWorkbook.Save();
                theWorkbook.Close(true);
                
            }
            catch(Exception ex)
            {
                theWorkbook.Close(true);
                excel.Quit();
                Console.WriteLine(ex);
            }
            finally
            {
                excel.Quit();
            }

            return true;


        }

        //excel.SheetFollowHyperlink += Excel_SheetFollowHyperlink;

        //private void Excel_SheetFollowHyperlink(object Sh, Hyperlink Target)
        //{
        //    Target.
        //    throw new NotImplementedException();
        //}
    }
}
