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
    public class Framework_Processor
    {

        public bool ProcessFramework(FrameworkParameters input)
        {

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
                Worksheet startSheet = theWorkbook.Sheets[3];
                startSheet.Unprotect(AppSettings.SheetPassword);

                Directory.CreateDirectory(Path.Combine(input.BasePath, input.ReportFolderName));

                startSheet.Cells[6, 4] = input.ReportDate.ToString("dd MMMM yyyy");

                startSheet.Cells[9, 4] = Path.Combine(input.BasePath, input.ReportFolderName);

                startSheet.Cells[10, 4] = input.PdFileName;
                startSheet.Cells[11, 4] = Path.Combine(input.BasePath, input.PdFileName);

                startSheet.Cells[12, 4] = input.LgdFile;
                startSheet.Cells[13, 4] = Path.Combine(input.BasePath, input.LgdFile);

                startSheet.Cells[14, 4] = input.EadFileName;
                startSheet.Cells[15, 4] = Path.Combine(input.BasePath, input.EadFileName);
                
                excel.Run("calculate_ecl");

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
