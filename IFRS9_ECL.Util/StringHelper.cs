using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IFRS9_ECL.Util
{
    public static class StringHelper
    {
        public static string RemoveSpecialCharacters(string input)
        {
            if(string.IsNullOrEmpty(input))
            {
                return "0";
            }
            if (string.IsNullOrWhiteSpace(input))
            {
                return "0";
            }
            Regex r = new Regex("(?:[^0-9.]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            return r.Replace(input, String.Empty);
        }
    }
}
