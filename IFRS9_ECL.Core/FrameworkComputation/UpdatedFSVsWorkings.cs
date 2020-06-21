using IFRS9_ECL.Core.Calibration;
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

            var overrideData = GetOverrideDataResult();

            var cali = new CalibrationInput_LGD_Haricut_Processor().GetLGDHaircutSummaryData(this.eclId, this._eclType);

            foreach (var row in collateral)
            {
             
                var newRow = new updatedFSV();
                newRow.ContractNo = row.contract_no;


                var overrideItem = new EclOverrides();
                var _overrideItem=overrideData.FirstOrDefault(o => o.ContractId == newRow.ContractNo);
                if(_overrideItem!=null)
                {
                    overrideItem = _overrideItem;
                }

                newRow.Cash = ComputeCollateralValue(row.cash_fsv,
                                                            row.cash_omv, overrideItem.FSV_Cash,
                                                            cali.Cash);
                newRow.CommercialProperty= ComputeCollateralValue(
                                                            row.commercial_property_fsv,
                                                            row.commercial_property_omv, overrideItem.FSV_CommercialProperty,
                                                            cali.Commercial_Property);
                newRow.Debenture= ComputeCollateralValue(
                                                            row.debenture_fsv,
                                                            row.debenture_omv, overrideItem.FSV_Debenture,
                                                            cali.Debenture);
                newRow.Inventory= ComputeCollateralValue(
                                                            row.inventory_fsv,
                                                            row.inventory_omv, overrideItem.FSV_Inventory,
                                                            cali.Inventory);
                newRow.PlantAndEquipment= ComputeCollateralValue(
                                                            row.plant_and_equipment_fsv,
                                                            row.plant_and_equipment_omv, overrideItem.FSV_PlantAndEquipment,
                                                            cali.Plant_And_Equipment);
                newRow.Receivables= ComputeCollateralValue(
                                                            row.receivables_fsv,
                                                            row.receivables_omv, overrideItem.FSV_Receivables,
                                                            cali.Receivables);
                newRow.ResidentialProperty= ComputeCollateralValue(
                                                            row.residential_property_fsv,
                                                            row.residential_property_omv, overrideItem.FSV_ResidentialProperty,
                                                            cali.Residential_Property);
                newRow.Shares= ComputeCollateralValue(
                                                            row.shares_fsv,
                                                            row.shares_omv, overrideItem.FSV_Shares,
                                                            cali.Shares);
                newRow.Vehicle= ComputeCollateralValue(
                                                            row.vehicle_fsv,
                                                            row.vehicle_omv, overrideItem.FSV_Vehicle,
                                                            cali.Vehicle);

                newRow.Override_TTR_Year = overrideItem.TtrYears;

                updatedFSV.Add(newRow);
            }

            return updatedFSV;
        }

        protected double ComputeCollateralValue(double fsv, double omv, double? override_fsv, double haircut)
        {
            override_fsv = override_fsv ?? 0;

            if (override_fsv > 0 && override_fsv < omv)
                return override_fsv.Value;

            if (fsv > 0 && fsv < omv)
                return fsv;
            else
                return omv * (1 - haircut);
        }

        protected List<LGDCollateralData> GetCollateralTypeResult()
        {
            return _processECL_LGD.GetLGDCollateralData();
        }
        protected List<EclOverrides> GetOverrideDataResult()
        {
            return _processECL_LGD.GetOverrideData(1);
        }
    }
}
