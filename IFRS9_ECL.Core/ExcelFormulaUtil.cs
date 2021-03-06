﻿using Excel.FinancialFunctions;
using IFRS9_ECL.Util;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core
{
    public static class ExcelFormulaUtil
    {

        public static double YearFrac(DateTime startDate, DateTime? endDate, DayCountBasis dayCountBasis = DayCountBasis.UsPsa30_360)
        {
            if (endDate == null)
                return 0;

            if (startDate == endDate)
                return 0;
            if (startDate < endDate)
                return Financial.YearFrac(startDate, endDate.Value, dayCountBasis);
            else
                return Financial.YearFrac(endDate.Value, startDate, dayCountBasis);
        }

        public static DateTime? EOMonth(DateTime? date, int months = 0)
        {
            try
            {
                var dt = date.Value;
                dt=dt.AddMonths(months);
                if (dt.Month == 12)
                {
                    return new DateTime(dt.Year, 12, 31);
                }
                DateTime eoMonth = new DateTime(dt.Year, dt.Month + 1, 1).AddDays(-1);
                return eoMonth;//.AddMonths(months);
            }
            catch (Exception ex)
            {
                //Log4Net.Log.Error(ex);
                var cc = ex;
            }
            return null;
        }

        public static double NormSDist(double p, bool cummulative = true)
        {
            return ExcelFunctions.NormSDist(p);
            //return _excelWorksheetFunctions.Norm_S_Dist(p, cummulative);
        }

        public static double NormSInv(double p)
        {
            return ExcelFunctions.NormSInv(p);
            //return _excelWorksheetFunctions.Norm_S_Inv(p);
        }

        public static double SumProduct(object arg1)
        {
            return 0;
        }

        public static double SumProduct(double[] arg1, double[] arg2)
        {
            double result = 0;
            for (int i = 0; i < arg1.Length; i++)
            {
                var _arg2 = 1.0; //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                if (arg2.Length-1 >= i)
                    _arg2 = arg2[i];

                result += arg1[i] * _arg2;
            }
            return result;
        }

        public static double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                double avg = values.Average();
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }
    }
}
