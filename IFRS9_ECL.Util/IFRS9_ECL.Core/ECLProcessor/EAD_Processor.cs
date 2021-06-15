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
    public class EAD_Processor
    {

        public bool ProcessEAD(EADParameters input)
        {

            var loanbook = Path.Combine(input.BasePath, input.LoanBookFileName);
            var paymentschedule = Path.Combine(input.BasePath, input.PaymentScheduleFileName);
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
                startSheet.Cells[12, 5] = input.ConversionFactorObe;
                startSheet.Cells[13, 5] = input.PrePaymentFactor;
                startSheet.Cells[15, 5] = input.NonExpired;
                startSheet.Cells[16, 5] = input.Expired;

                startSheet.Cells[20, 5] = loanbook;
                var fileName = new FileInfo(loanbook).Name;
                startSheet.Cells[21, 5] = fileName;
                startSheet.Cells[22, 5] = paymentschedule;
                var psfileName = new FileInfo(paymentschedule).Name;
                startSheet.Cells[23, 5] = psfileName;
//                theWorkbook.Save();

                excel.Run("extract_ead_data");

                Worksheet projection = theWorkbook.Sheets[3];
                projection.Unprotect(AppSettings.SheetPassword);


                for(int i=4; i<40; i++)
                {
                    var key = Convert.ToString(projection.Cells[i, 2].Value);
                    if (key == "CORPORATE")
                    {
                        projection.Cells[i, 3] = input.CCF_Corporate;
                    }
                    if (key == "COMMERCIAL")
                    {
                        projection.Cells[i, 3] = input.CCF_Commercial;
                    }
                    if (key == "CONSUMER")
                    {
                        projection.Cells[i, 3] = input.CCF_Consumer;
                    }
                    if (key == "OBE")
                    {
                        projection.Cells[i, 3] = input.CCF_OBE;
                    }

                    if(input.VariableInterestRates.Any(o=>o.VIR_Name==key))
                    {
                        projection.Cells[i, 3]=input.VariableInterestRates.FirstOrDefault(o => o.VIR_Name == key).Value;
                    }

                    if (input.ExchangeRates.Any(o => o.Currency == key))
                    {
                        projection.Cells[i, 3] = input.ExchangeRates.FirstOrDefault(o => o.Currency == key).Value;
                    }
                }

                theWorkbook.Save();

                excel.Run("calculate_lifetime_eads");

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

    }
}
