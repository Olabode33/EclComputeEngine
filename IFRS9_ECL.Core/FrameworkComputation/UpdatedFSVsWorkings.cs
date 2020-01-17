using IFRS9_ECL.Models.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.FrameworkComputation
{
    public class UpdatedFSVsWorkings
    {
        private Guid eclId;

        public UpdatedFSVsWorkings(Guid eclId)
        {
            this.eclId = eclId;
        }

        internal List<LgdCollateralFsvProjectionUpdate> ComputeUpdatedFSV()
        {
            throw new NotImplementedException();
        }
    }
}
