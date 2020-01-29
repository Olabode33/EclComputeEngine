using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
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

    public class AffiliateMicroEconomicsVariable
    {
        public List<AffiliateMEVBackDateValues> AffiliateMEVBackDateValues(Guid eclId, EclType eclType)
        {
            var itms = new List<AffiliateMEVBackDateValues>();
            itms.Add(new PD.AffiliateMEVBackDateValues { BackDateQuarters=2, MicroEconomicId=1 });
            itms.Add(new PD.AffiliateMEVBackDateValues { BackDateQuarters = 3, MicroEconomicId = 2 });
            itms.Add(new PD.AffiliateMEVBackDateValues { BackDateQuarters = 1, MicroEconomicId = 3 });
            itms.Add(new PD.AffiliateMEVBackDateValues { BackDateQuarters = 1, MicroEconomicId = 4 });

            return itms;
        }
    }

}
