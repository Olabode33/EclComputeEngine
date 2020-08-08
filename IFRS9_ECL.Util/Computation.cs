using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Util
{
    public static class Computation
    {
        public static double GetStandardDeviationS(IEnumerable<double> values)
        {
            double standardDeviation = 0;

            if (values.Any())
            {
                // Compute the average.     
                double avg = values.Average();

                // Perform the Sum of (value-avg)_2_2.      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));

                // Put it all together.      
                standardDeviation = Math.Sqrt((sum) / (values.Count() - 1));
            }

            return standardDeviation;
        }


        public static double GetStandardDeviationP(this List<double> values)
        {
            double total = 0, average = 0;

            foreach (double num in values)
            {
                total += num;
            }

            average = total / values.Count();

            double runningTotal = 0;

            foreach (double num in values)
            {
                runningTotal += ((num - average) * (num - average));
            }

            double calc = runningTotal / values.Count();
            double standardDeviationP = Math.Sqrt(calc);

            return standardDeviationP;
        }

        public static string GetActualContractId(string contractId)
        {
            return contractId.Contains(ECLStringConstants.i.ExpiredContractsPrefix) ? contractId.Split('|')[1] : contractId;
        }
    }
}
