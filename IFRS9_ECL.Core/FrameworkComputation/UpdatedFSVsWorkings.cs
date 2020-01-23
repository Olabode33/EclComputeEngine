using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Framework;
using IFRS9_ECL.Util;
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
        EclType _eclType;
        ProcessECL_LGD _processECL_LGD;
        public UpdatedFSVsWorkings(Guid eclId, EclType eclType)
        {
            this.eclId = eclId;
            this._eclType = eclType;
            _processECL_LGD = new ProcessECL_LGD(eclId, eclType);
        }

        internal List<updatedFSV> ComputeUpdatedFSV()
        {
            var updatedFSV = new List<updatedFSV>();
            
            var collateral = GetCollateralTypeResult();
            

            foreach (var row in collateral)
            {
             
                var newRow = new updatedFSV();
                newRow.ContractNo = row.contract_no;
                newRow.Cash = ComputeCollateralValue(row.cash_fsv,
                                                            row.cash_omv,
                                                            FrameworkConstants.CollateralHaircutApplied_Cash);
                newRow.CommercialProperty= ComputeCollateralValue(
                                                            row.commercial_property_fsv,
                                                            row.commercial_property_omv,
                                                            FrameworkConstants.CollateralHaircutApplied_CommercialProperty);
                newRow.Debenture= ComputeCollateralValue(
                                                            row.debenture_fsv,
                                                            row.debenture_omv,
                                                            FrameworkConstants.CollateralHaircutApplied_Debenture);
                newRow.Inventory= ComputeCollateralValue(
                                                            row.inventory_fsv,
                                                            row.inventory_omv,
                                                            FrameworkConstants.CollateralHaircutApplied_Invertory);
                newRow.PlantAndEquipment= ComputeCollateralValue(
                                                            row.plant_and_equipment_fsv,
                                                            row.plant_and_equipment_omv,
                                                            FrameworkConstants.CollateralHaircutApplied_PlantEquipment);
                newRow.Receivables= ComputeCollateralValue(
                                                            row.receivables_fsv,
                                                            row.receivables_omv,
                                                            FrameworkConstants.CollateralHaircutApplied_Receivables);
                newRow.ResidentialProperty= ComputeCollateralValue(
                                                            row.residential_property_fsv,
                                                            row.residential_property_omv,
                                                            FrameworkConstants.CollateralHaircutApplied_ResidentialProperty);
                newRow.Shares= ComputeCollateralValue(
                                                            row.shares_fsv,
                                                            row.shares_omv,
                                                            FrameworkConstants.CollateralHaircutApplied_Shares);
                newRow.Vehicle= ComputeCollateralValue(
                                                            row.vehicle_fsv,
                                                            row.vehicle_omv,
                                                            FrameworkConstants.CollateralHaircutApplied_Vehicle);

                updatedFSV.Add(newRow);
            }

            return updatedFSV;
        }

        protected double ComputeCollateralValue(double fsv, double omv, double haircut)
        {
            if (fsv > 0 && fsv < omv)
                return fsv;
            else
                return omv * (1 - haircut);
        }

        protected List<LGDCollateralData> GetCollateralTypeResult()
        {
            return _processECL_LGD.GetLGDCollateralData();
        }
        
    }
}
