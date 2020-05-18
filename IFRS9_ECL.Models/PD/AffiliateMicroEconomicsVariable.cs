using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Models.PD
{
    /// <summary>
    /// Affiliate Macro Economic Variable BackDateValues
    /// </summary>
    public class AffiliateMEVBackDateValues
    {
        public int MicroEconomicId { get; set; }
        public int BackDateQuarters { get; set; }
    }

}
