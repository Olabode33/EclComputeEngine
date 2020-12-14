using IFRS9_ECL.Models;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IFRS9_ECL.Data
{
    public static class ExecuteNative
    {

        public static string SaveEIRProjections(List<EIRProjections> d, Guid master_G, EclType eclType)
        {

            var r=Util.FileSystemStorage<EIRProjections>.WriteCsvData(master_G, ECLStringConstants.i.EadEirProjections_Table(eclType), d);

            return r ? "" : $"Could not Bulk Insert [{ECLStringConstants.i.EadEirProjections_Table(eclType)}]";

        }

        public static string SaveLGDAccountdata(List<LGDAccountData> d, Guid masterGuid, EclType eclType)
        {
            var r = Util.FileSystemStorage<LGDAccountData>.WriteCsvData(masterGuid, ECLStringConstants.i.LGDAccountData_Table(eclType), d);
            
                return r ? "" : $"Could not Bulk Insert [{ECLStringConstants.i.LGDAccountData_Table(eclType)}]";
            
        }

        public static string SaveLGDCollaterals(List<LGDCollateralData> d, Guid masterGuid, EclType eclType)
        {

            var r = Util.FileSystemStorage<LGDCollateralData>.WriteCsvData(masterGuid, ECLStringConstants.i.LGDCollateral_Table(eclType), d);

                return r? "" : $"Could not Bulk Insert [{ECLStringConstants.i.LGDCollateral_Table(eclType)}]";
          
        }

        public static string SaveCIRProjections(List<CIRProjections> d, Guid master_G, EclType eclType)
        {
            var r=Util.FileSystemStorage<CIRProjections>.WriteCsvData(master_G, ECLStringConstants.i.EadCirProjections_Table(eclType), d);

            return r? "" : $"Could not Bulk Insert [{ECLStringConstants.i.EadCirProjections_Table(eclType)}]";
        }

        public static string SaveLifeTimeProjections(List<LifeTimeProjections> d, Guid master_G, EclType eclType)
        {
            var r = Util.FileSystemStorage<LifeTimeProjections>.WriteCsvData(master_G, ECLStringConstants.i.EadLifetimeProjections_Table(eclType), d);

            return r? "" : $"Could not Bulk Insert [{ECLStringConstants.i.EadLifetimeProjections_Table(eclType)}]";
            
        }
    }
}
