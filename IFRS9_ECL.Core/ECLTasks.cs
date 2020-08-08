using Excel.FinancialFunctions;
using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Core.Calibration.Input;
using IFRS9_ECL.Core.FrameworkComputation;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Framework;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;


namespace IFRS9_ECL.Core
{
    public class ECLTasks
    {
        private Guid _eclId;
        private EclType _eclType;
        ScenarioLifetimeLGD _scenarioLifetimeLGD;

        
        List<EclAssumptions> _eclEadInputAssumption;
        DateTime reportingDate = new DateTime();
        List<EclAssumptions> ViR = new List<EclAssumptions>();
        public ECLTasks(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
            _scenarioLifetimeLGD = new ScenarioLifetimeLGD(eclId, eclType);
            this._eclEadInputAssumption = GetECLEADInputAssumptions();
            reportingDate = GetReportingDate(eclType, eclId);
            ViR= GetVIR(eclType, eclId);
        }
        public ECLTasks()
        {
        }
            private DateTime GetReportingDate(EclType _eclType, Guid eclId)
        {
            var ecls = Queries.EclsRegister(_eclType.ToString(), _eclId.ToString());
            var dtR = DataAccess.i.GetData(ecls);
            if (dtR.Rows.Count > 0)
            {
                var itm = DataAccess.i.ParseDataToObject(new EclRegister(), dtR.Rows[0]);
                return itm.ReportingDate;
            }
            return DateTime.Now;
        }

        private List<EclAssumptions> GetVIR(EclType _eclType, Guid eclId)
        {
            var qry = Queries.VariableInterestRate(_eclType.ToString(), _eclId.ToString());
            var dtR = DataAccess.i.GetData(qry);

            var virs = new List<EclAssumptions>();
            if (dtR.Rows.Count > 0)
            {
                foreach(DataRow dr in dtR.Rows)
                {
                    var itm = DataAccess.i.ParseDataToObject(new EclAssumptions(), dr);
                    virs.Add(itm);
                }
            }
            else
            {
                return  new List<EclAssumptions>();
            }
            return virs;
        }

        public List<EclAssumptions> GetECLEADInputAssumptions()
        {
            var qry = Queries.eclEadInputAssumptions(this._eclId, this._eclType);
            var dt = DataAccess.i.GetData(qry);
            var eclAssumptions = new List<EclAssumptions>();

            foreach (DataRow dr in dt.Rows)
            {
                eclAssumptions.Add(DataAccess.i.ParseDataToObject(new EclAssumptions(), dr));
            }

            return eclAssumptions;
        }



        public List<Refined_Raw_Retail_Wholesale> GenerateContractIdandRefinedData(List<Loanbook_Data> lstRaw)
        {
            var refineds = new List<Refined_Raw_Retail_Wholesale>();
            int i = 0;
            foreach (var rr in lstRaw)
            {
                i++;
                //Log4Net.Log.Info(i);
                var refined = new Refined_Raw_Retail_Wholesale();
                refined.contract_no = rr.ContractId;// GenerateContractId(rr);

                var filtLstRaw = lstRaw.FirstOrDefault(o => o.ContractId == refined.contract_no);
                var filtLstRawLst = lstRaw.Where(o => o.ContractId == refined.contract_no).ToList();

                var subContractNo = refined.contract_no;

                if (refined.contract_no.IndexOf('|') > -1)
                {
                    var split = refined.contract_no.Split('|');
                    if (split.Length > 1)
                    {
                        subContractNo = split[1];
                    }
                }
                var checkNumber = int.TryParse(subContractNo, out int n);

                if (filtLstRaw!=null)
                    if (refined.contract_no.StartsWith(ECLStringConstants.i.ExpiredContractsPrefix, StringComparison.InvariantCultureIgnoreCase) && !checkNumber)
                    {

                        var pos1 = refined.contract_no.IndexOf(' ');
                        var pos2 = refined.contract_no.IndexOf('|');
                        refined.product_type = refined.contract_no.Substring(pos1 + 1, pos2 - pos1 - 1);

                        refined.credit_limit_lcy = filtLstRawLst.Sum(o => o.CreditLimit ?? 0);
                        refined.original_bal_lcy = filtLstRawLst.Sum(o => o.OriginalBalanceLCY ?? 0).ToString();
                        refined.OUTSTANDING_BALANCE_LCY = filtLstRawLst.Sum(o => o.OutstandingBalanceLCY ?? 0).ToString();
                    }
                    else
                    {
                        refined.segment = filtLstRaw.Segment;
                        refined.currency = filtLstRaw.Currency;
                        refined.product_type = filtLstRaw.ProductType;
                        refined.credit_limit_lcy = filtLstRaw.CreditLimit != null ? filtLstRaw.CreditLimit : 0;
                        refined.original_bal_lcy = filtLstRaw.OriginalBalanceLCY != null ? filtLstRaw.OriginalBalanceLCY.ToString() : "0";
                        refined.OUTSTANDING_BALANCE_LCY = filtLstRaw.OutstandingBalanceLCY != null ? filtLstRaw.OutstandingBalanceLCY.ToString() : "0";
                        refined.CONTRACT_START_DATE = filtLstRaw.ContractStartDate;
                        refined.CONTRACT_END_DATE = filtLstRaw.ContractEndDate;
                        refined.RESTRUCTURE_INDICATOR = filtLstRaw.RestructureIndicator ? 1 : 0;
                        refined.RESTRUCTURE_START_DATE = filtLstRaw.RestructureStartDate;
                        refined.RESTRUCTURE_END_DATE = filtLstRaw.RestructureEndDate;
                        refined.IPT_O_PERIOD = filtLstRaw.IPTOPeriod.ToString();
                        refined.PRINCIPAL_PAYMENT_STRUCTURE = filtLstRaw.PrincipalPaymentStructure;
                        refined.INTEREST_PAYMENT_STRUCTURE = filtLstRaw.InterestPaymentStructure;
                        refined.BASE_RATE = filtLstRaw.BaseRate.ToString();
                        refined.ORIGINATION_CONTRACTUAL_INTEREST_RATE = filtLstRaw.OriginationContractualInterestRate;
                        refined.INTRODUCTORY_PERIOD = filtLstRaw.IntroductoryPeriod.ToString();
                        refined.POST_IP_CONTRACTUAL_INTEREST_RATE = filtLstRaw.PostIPContractualInterestRate != null ? filtLstRaw.PostIPContractualInterestRate.ToString() : "0";
                        refined.INTEREST_RATE_TYPE = filtLstRaw.InterestRateType;
                        refined.CURRENT_CONTRACTUAL_INTEREST_RATE = filtLstRaw.CurrentContractualInterestRate != null ? filtLstRaw.CurrentContractualInterestRate.ToString() : "0";
                        refined.EIR = filtLstRaw.EIR != null ? filtLstRaw.EIR.ToString() : "0";
                        refined.LIM_MONTH = filtLstRaw.LIM_MONTH;
                    }

                refineds.Add(refined);
            }


            
            return refineds;
        }


        internal List<CoR> CalculateCoR_Main(List<LGDPrecalculationOutput> lGDPreCalc, List<Loanbook_Data> loanbook_Data, List<LGDCollateralData> lstCollateral)
        {
            var CoR_DT = new List<CoR>();
            LGD_Inputs inputs = new LGD_Inputs();

            var lgd_Assumptions_2 = _scenarioLifetimeLGD.GetECLLgdAssumptions();

            var lgd_Assumptions_2_first = lgd_Assumptions_2.Where(o => o.AssumptionGroup == 4).ToList();
            var lgd_Assumptions_2_last = lgd_Assumptions_2.Where(o => o.AssumptionGroup == 3).ToList();

            var lgd_first = new LGD_Assumptions_CollateralType_TTR_Years();

            try { lgd_first.collateral_value = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Collateral)).Value); } catch { lgd_first.collateral_value = 0; }
            try { lgd_first.debenture = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Debenture)).Value);        } catch { lgd_first.debenture = 0; }
            try { lgd_first.cash = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Cash)).Value);        } catch { lgd_first.cash = 0; }
            try { lgd_first.commercial_property = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.CommercialProperty)).Value);        } catch { lgd_first.commercial_property = 0; }
            try { lgd_first.Receivables = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Receivables)).Value);        } catch { lgd_first.Receivables = 0; }
            try { lgd_first.inventory = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Inventory)).Value);        } catch { lgd_first.inventory = 0; }
            try { lgd_first.plant_and_equipment = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.PlantEquipment)).Value);        } catch { lgd_first.plant_and_equipment = 0; }
            try { lgd_first.residential_property = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.ResidentialProperty)).Value);        } catch { lgd_first.residential_property = 0; }
            try { lgd_first.shares = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Shares)).Value);        } catch { lgd_first.shares = 0; }
            try { lgd_first.vehicle = double.Parse(lgd_Assumptions_2_first.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Vehicle)).Value);        } catch { lgd_first.vehicle = 0; }
            
                var lgd_last = new LGD_Assumptions_CollateralType_TTR_Years();



            try { lgd_last.collateral_value = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Collateral)).Value); } catch { lgd_first.collateral_value = 0; }
            try { lgd_last.debenture = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Debenture)).Value); } catch { lgd_first.debenture = 0; }
            try { lgd_last.cash = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Cash)).Value); } catch { lgd_first.cash = 0; }
            try { lgd_last.commercial_property = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.CommercialProperty)).Value); } catch { lgd_first.commercial_property = 0; }
            try { lgd_last.Receivables = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Receivables)).Value); } catch { lgd_first.Receivables = 0; }
            try { lgd_last.inventory = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Inventory)).Value); } catch { lgd_first.inventory = 0; }
            try { lgd_last.plant_and_equipment = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.PlantEquipment)).Value); } catch { lgd_first.plant_and_equipment = 0; }
            try { lgd_last.residential_property = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.ResidentialProperty)).Value); } catch { lgd_first.residential_property = 0; }
            try { lgd_last.shares = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Shares)).Value); } catch { lgd_first.shares = 0; }
            try { lgd_last.vehicle = double.Parse(lgd_Assumptions_2_last.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Vehicle)).Value); } catch { lgd_first.vehicle = 0; }

            for (int i = 0; i < loanbook_Data.Count; i++)
            {
                if(lGDPreCalc[i].project_finance_ind==1)
                {
                    //calc weight_avg_cor

                    inputs.debenture_omv = loanbook_Data[i].DebentureOMV ?? 0;
                    inputs.cash_omv = loanbook_Data[i].CashOMV ?? 0;
                    inputs.inventory_omv = loanbook_Data[i].InventoryOMV ?? 0;
                    inputs.plant_and_equipment_omv = loanbook_Data[i].PlantEquipmentOMV ?? 0;
                    inputs.residential_property_omv = loanbook_Data[i].ResidentialPropertyOMV ?? 0;
                    inputs.commercial_property_omv = loanbook_Data[i].CommercialPropertyOMV ?? 0;
                    inputs.shares_omv = loanbook_Data[i].SharesOMV ?? 0;
                    inputs.vehicle_omv = loanbook_Data[i].VehicleOMV ?? 0;

                    inputs.project_finance_ind = lGDPreCalc[i].project_finance_ind;
                    inputs.total = inputs.debenture_omv + inputs.cash_omv + inputs.inventory_omv + inputs.plant_and_equipment_omv + inputs.residential_property_omv + inputs.commercial_property_omv + inputs.shares_omv + inputs.vehicle_omv;


                    double[] rawData = { loanbook_Data[i].DebentureOMV ?? 0, loanbook_Data[i].CashOMV ?? 0, loanbook_Data[i].InventoryOMV ?? 0, loanbook_Data[i].PlantEquipmentOMV ?? 0, loanbook_Data[i].ResidentialPropertyOMV ?? 0, loanbook_Data[i].CommercialPropertyOMV ?? 0, loanbook_Data[i].SharesOMV ?? 0, loanbook_Data[i].VehicleOMV ?? 0 };

                    var weight_Avg_cor = 0.0;

                    if (inputs.total> lgd_first.collateral_value)
                    {
                        //Sum product of Raw Data and LGD Assumption First Row
                        double[] lgdAssumption = { lgd_first.debenture, lgd_first.cash, lgd_first.inventory, lgd_first.plant_and_equipment, lgd_first.residential_property, lgd_first.commercial_property, lgd_first.Receivables, lgd_first.shares, lgd_first.vehicle };

                        if (inputs.total != 0)
                            weight_Avg_cor = SumProduct(rawData, lgdAssumption) / inputs.total;

                    }
                    else
                    {
                        //Sum product of Raw Data and LGD Assumption Second Row
                        double[] lgdAssumption = { lgd_last.debenture, lgd_last.cash, lgd_last.inventory, lgd_last.plant_and_equipment, lgd_last.residential_property, lgd_last.commercial_property, lgd_last.Receivables, lgd_last.shares, lgd_last.vehicle };

                        if (inputs.total != 0)
                            weight_Avg_cor =SumProduct(rawData, lgdAssumption)/ inputs.total;
                    }
                    CoR_DT.Add(new CoR { contract_no = lstCollateral[i].contract_no, cor = weight_Avg_cor });
                }
                else
                {
                    double cor_debenture = CalculateCoR(lstCollateral[i].debenture_omv, lgd_first.collateral_value, lgd_first.debenture, lgd_last.debenture);
                    double cor_cash = CalculateCoR(lstCollateral[i].cash_omv, lgd_first.collateral_value, lgd_first.cash, lgd_last.cash);
                    double cor_inventory = CalculateCoR(lstCollateral[i].inventory_omv, lgd_first.collateral_value, lgd_first.inventory, lgd_last.inventory);
                    double cor_plant_and_equipment = CalculateCoR(lstCollateral[i].plant_and_equipment_omv, lgd_first.collateral_value, lgd_first.plant_and_equipment, lgd_last.plant_and_equipment);
                    double cor_residential = CalculateCoR(lstCollateral[i].residential_property_omv, lgd_first.collateral_value, lgd_first.residential_property, lgd_last.residential_property);
                    double cor_commercial = CalculateCoR(lstCollateral[i].commercial_property_omv, lgd_first.collateral_value, lgd_first.commercial_property, lgd_last.commercial_property);
                    double cor_receivables = CalculateCoR(lstCollateral[i].receivables_omv, lgd_first.collateral_value, lgd_first.Receivables, lgd_last.Receivables);
                    double cor_shares = CalculateCoR(lstCollateral[i].shares_omv, lgd_first.collateral_value, lgd_first.shares, lgd_last.shares);
                    double cor_vehicle = CalculateCoR(lstCollateral[i].vehicle_omv, lgd_first.collateral_value, lgd_first.vehicle, lgd_last.vehicle);

                    double cor_sum = cor_debenture + cor_cash + cor_inventory + cor_plant_and_equipment + cor_residential + cor_commercial + cor_receivables + cor_shares + cor_vehicle;
                    double omv_sum = lstCollateral[i].debenture_omv + lstCollateral[i].cash_omv + lstCollateral[i].inventory_omv + lstCollateral[i].plant_and_equipment_omv + lstCollateral[i].residential_property_omv + lstCollateral[i].commercial_property_omv + lstCollateral[i].receivables_omv + lstCollateral[i].shares_omv + lstCollateral[i].vehicle_omv;

                    
                        double cor_Val = 0;
                    if (omv_sum != 0 && cor_sum!=0)
                        cor_Val=cor_sum / omv_sum;

                    CoR_DT.Add(new CoR { contract_no = lstCollateral[i].contract_no, cor = cor_Val });

                }
            }

            return CoR_DT;



        }

        public List<LGDAccountData> AccountData(List<Loanbook_Data> refinedRawData, List<LGDPrecalculationOutput> tempDT, List<LGDCollateralData> collateralTable, List<CoR> coR)
        {
            
            var accountData = new List<LGDAccountData>();

            try
            {
                var lgd_Assumptions_2 = _scenarioLifetimeLGD.GetECLLgdAssumptions();

                lgd_Assumptions_2 = lgd_Assumptions_2.Where(o => o.AssumptionGroup == 8).ToList();
                var selection = new double[9];

                try { selection[0] = double.Parse(lgd_Assumptions_2.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Debenture.ToLower())).Value); } catch { }
                try{selection[1] = double.Parse(lgd_Assumptions_2.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Cash.ToLower())).Value); } catch { }
                try{selection[2] = double.Parse(lgd_Assumptions_2.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Inventory.ToLower())).Value); } catch { }
                try{selection[3] = double.Parse(lgd_Assumptions_2.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.PlantEquipment.ToLower())).Value); } catch { }
                try{selection[4] = double.Parse(lgd_Assumptions_2.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.ResidentialProperty.ToLower())).Value); } catch { }
                try{selection[5] = double.Parse(lgd_Assumptions_2.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.CommercialProperty.ToLower())).Value); } catch { }
                try{selection[6] = double.Parse(lgd_Assumptions_2.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Receivables.ToLower())).Value); } catch { }
                try{selection[7] = double.Parse(lgd_Assumptions_2.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Shares.ToLower())).Value); } catch { }
                try{selection[8] = double.Parse(lgd_Assumptions_2.FirstOrDefault(o => o.Key.ToLower().Contains(LGDCollateralGrowthAssumption.Vehicle.ToLower())).Value); } catch { }


                for (var i = 0; i < collateralTable.Count; i++)
                {
                    refinedRawData[i].GuaranteeValue = refinedRawData[i].GuaranteeValue ?? 0;
                    LGD_Inputs obj = new LGD_Inputs()
                    {
                        contractid = collateralTable[i].contract_no,
                        guarantee_value = refinedRawData[i].GuaranteeValue.Value.ToString(),
                        customer_no = refinedRawData[i].CustomerNo
                    };

                    var cor_value = coR.FirstOrDefault(o => o.contract_no == obj.contractid);
                    if (cor_value == null) cor_value = new CoR();
                    accountData.Add(new LGDAccountData { COST_OF_RECOVERY = cor_value.cor });

                    double[] tempOVMarray = {

                        collateralTable[i].debenture_omv ,
                                            collateralTable[i].cash_omv ,
                                            collateralTable[i].inventory_omv ,
                                            collateralTable[i].plant_and_equipment_omv ,
                                            collateralTable[i].residential_property_omv ,
                                            collateralTable[i].commercial_property_omv ,
                                            collateralTable[i].receivables_omv ,
                                            collateralTable[i].shares_omv ,
                                            collateralTable[i].vehicle_omv
                };

                    refinedRawData[i].DebentureOMV = refinedRawData[i].DebentureOMV ?? 0;
                    refinedRawData[i].CashOMV = refinedRawData[i].CashOMV ?? 0;
                    refinedRawData[i].InventoryOMV = refinedRawData[i].InventoryOMV ?? 0;
                    refinedRawData[i].PlantEquipmentOMV = refinedRawData[i].PlantEquipmentOMV ?? 0;
                    refinedRawData[i].ResidentialPropertyOMV = refinedRawData[i].ResidentialPropertyOMV ?? 0;
                    refinedRawData[i].CommercialPropertyOMV = refinedRawData[i].CommercialPropertyOMV ?? 0;
                    refinedRawData[i].ReceivablesOMV = refinedRawData[i].ReceivablesOMV ?? 0;
                    refinedRawData[i].SharesOMV = refinedRawData[i].SharesOMV ?? 0;
                    refinedRawData[i].VehicleOMV = refinedRawData[i].VehicleOMV ?? 0;

                    double valueArray2 = refinedRawData[i].DebentureOMV.Value + refinedRawData[i].CashOMV.Value + refinedRawData[i].InventoryOMV.Value + refinedRawData[i].PlantEquipmentOMV.Value + refinedRawData[i].ResidentialPropertyOMV.Value + refinedRawData[i].CommercialPropertyOMV.Value + refinedRawData[i].ReceivablesOMV.Value + refinedRawData[i].SharesOMV.Value + refinedRawData[i].VehicleOMV.Value;


                    double product_1 = SumProduct(tempOVMarray, selection);
                    double result;
                    double value1, value2;

                    if (valueArray2 != 0)
                    {
                        value1 = product_1 / valueArray2;
                    }
                    else
                    {
                        value1 = 0;
                    }


                    if (tempDT[i].project_finance_ind.ToString() == "1")
                    {
                        value2 = 0; //HLOOKUP("PF_"&'Collateral Type OMV'!$AE4,SPECIALISED_LENDING_TTR_TABLE,2,FALSE) - i do not understand this part so i hardcoded a 0
                    }
                    else
                    {
                        value2 = 0;
                    }

                    result = value1 + value2;

                    accountData[i].TTR_YEARS = result;
                    accountData[i].CONTRACT_NO = collateralTable[i].contract_no;

                    //END OF TTM

                    //GUARANTY_PD, GUARANTY_LGD, GUARANTEE_VALUE
                    if (refinedRawData[i].GuaranteeIndicator.ToString() == "1")
                    {
                        refinedRawData[i].GuarantorPD = string.IsNullOrEmpty(refinedRawData[i].GuarantorPD) ? "0" : refinedRawData[i].GuarantorPD;
                        accountData[i].GUARANTOR_PD = double.Parse(refinedRawData[i].GuarantorPD);

                        refinedRawData[i].GuarantorLGD = string.IsNullOrEmpty(refinedRawData[i].GuarantorLGD) ? "0" : refinedRawData[i].GuarantorLGD;
                        accountData[i].GUARANTOR_LGD = double.Parse(refinedRawData[i].GuarantorLGD);

                        value1 = refinedRawData[i].GuaranteeIndicator ? tempDT[i].pd_x_ead : 0;

                        var pd_x_ead_List = tempDT.Select(o => o.pd_x_ead).ToArray();

                        var guarantee_values = refinedRawData.Select(o => Convert.ToString(o.GuaranteeValue)).ToList();
                        var customer_nos = refinedRawData.Select(o => Convert.ToString(o.CustomerNo)).ToList();
                        string[] Guarantee_value_array = GetArray(guarantee_values, obj.guarantee_value);
                        string[] Customer_no_array = GetArray(customer_nos, obj.customer_no);

                        double product = SumProduct(pd_x_ead_List, Guarantee_value_array, Customer_no_array);
                        if (product != 0)
                        {
                            accountData[i].GUARANTEE_VALUE = value1 / product;
                        }
                        else
                        {
                            accountData[i].GUARANTEE_VALUE = 0;
                        }
                    }
                    else
                    {
                        accountData[i].GUARANTOR_PD = 0;
                        accountData[i].GUARANTOR_LGD = 0;
                        accountData[i].GUARANTEE_VALUE = 0;
                    }

                }
            }catch(Exception ex)
            {
                Log4Net.Log.Error(ex);
                var xx = ex;
            }
            return accountData;

        }



        private double SumProduct(double[] arrayA, string[] arrayB, string[] arrayC)
        {
            double result = 0;

            for (int i = 0; i < arrayA.Length; i++)
            {
                result += Convert.ToDouble(arrayA[i]) * Convert.ToDouble(arrayB[i]) + Convert.ToDouble(arrayC[i]);
            }
            return result;

        }


        private string[] GetArray(List<string> D_List, string value)
        {
            string[] _array = GetValue(D_List, value).ToArray();

            return _array;
        }
        private double CalculateCoR(double inputs, double collateralValue, double lgd_Assumption_first, double lgd_Assumption_last)
        {
            double value = 0;

            if (inputs > collateralValue)
            {
                value = lgd_Assumption_last;
            }
            else
            {
                value = lgd_Assumption_first;
            }

            value *= inputs;

            return value;
        }
        internal List<LGDCollateralData> Collateral_OMV_FSV(List<Loanbook_Data> lstRaw, List<LGDPrecalculationOutput> lGDPreCalc)
        {
            var collaterals = new List<LGDCollateralData>();
            LGD_Inputs input = new LGD_Inputs();



            var pd_x_ead_List = lGDPreCalc.Select(O => O.pd_x_ead).ToList();
            //calculate the value for Debenture_OMV
            //foreach (var itm in lstRaw)
            for (int i = 0; i < lstRaw.Count; i++)
            {
                var collateralTable = new LGDCollateralData();
                

                input.debenture_omv = lstRaw[i].DebentureOMV ?? 0;
                input.cash_omv = lstRaw[i].CashOMV ?? 0;
                input.inventory_omv = lstRaw[i].InventoryOMV ?? 0;
                input.plant_and_equipment_omv = lstRaw[i].PlantEquipmentOMV ?? 0;
                input.residential_property_omv = lstRaw[i].ResidentialPropertyOMV ?? 0;
                input.commercial_property_omv = lstRaw[i].CommercialPropertyOMV ?? 0;
                input.receivables_omv = lstRaw[i].ReceivablesOMV ?? 0;
                input.shares_omv = lstRaw[i].SharesOMV ?? 0;
                input.vehicle_omv = lstRaw[i].VehicleOMV ?? 0;

                input.debenture_fsv = lstRaw[i].DebentureFSV ?? 0;
                input.cash_fsv = lstRaw[i].CashFSV ?? 0;
                input.inventory_fsv = lstRaw[i].InventoryFSV ?? 0;
                input.plant_and_equipment_fsv = lstRaw[i].PlantEquipmentFSV ?? 0;
                input.residential_property_fsv = lstRaw[i].ResidentialPropertyFSV ?? 0;
                input.commercial_property_fsv = lstRaw[i].CommercialProperty ?? 0;
                input.receivables_fsv = lstRaw[i].ReceivablesFSV ?? 0;
                input.shares_fsv = lstRaw[i].SharesFSV ?? 0;
                input.vehicle_fsv = lstRaw[i].VehicleFSV ?? 0;

                input.customer_no = lstRaw[i].CustomerNo;
                input.contractid = lstRaw[i].ContractId;
                input.account_no = lstRaw[i].AccountNo;

                input.pd_x_ead = lGDPreCalc[i].pd_x_ead;//


                //lGDPreCalc = GetValue(lstRaw, lGDPreCalc, input.debenture);


                collateralTable.contract_no = input.contractid;
                collateralTable.customer_no = input.customer_no;
                collateralTable.debenture_omv = 0;
                collateralTable.cash_omv = 0;
                collateralTable.inventory_omv = 0;
                collateralTable.plant_and_equipment_omv = 0;
                collateralTable.residential_property_omv = 0;
                collateralTable.commercial_property_omv = 0;
                collateralTable.receivables_omv = 0;
                collateralTable.shares_omv = 0;
                collateralTable.vehicle_omv = 0;
                collateralTable.total_omv = 0;
                collateralTable.debenture_fsv = 0;
                collateralTable.cash_fsv = 0;
                collateralTable.inventory_fsv = 0;
                collateralTable.plant_and_equipment_fsv = 0;
                collateralTable.residential_property_fsv = 0;
                collateralTable.commercial_property_fsv = 0;
                collateralTable.receivables_fsv = 0;
                collateralTable.shares_fsv = 0;
                collateralTable.vehicle_fsv = 0;



                var dictionaryData = GetArrayRawData(lstRaw, input);

                var Debenture_Omv_array = dictionaryData[ECLStringConstants.i.Debenture_Omv_array];
                var Cash_Omv_array = dictionaryData[ECLStringConstants.i.Cash_Omv_array];
                var Inventory_Omv_array = dictionaryData[ECLStringConstants.i.Inventory_Omv_array];
                var Plant_Equipment_Omv_array = dictionaryData[ECLStringConstants.i.Plant_Equipment_Omv_array];
                var Residential_Omv_array = dictionaryData[ECLStringConstants.i.Residential_Omv_array];
                var Commercial_Omv_array = dictionaryData[ECLStringConstants.i.Commercial_Omv_array];
                var Receivables_Omv_array = dictionaryData[ECLStringConstants.i.Receivables_Omv_array];
                var Shares_Omv_array = dictionaryData[ECLStringConstants.i.Shares_Omv_array];
                var Vehicle_Omv_array = dictionaryData[ECLStringConstants.i.Vehicle_Omv_array];

                var Debenture_Fsv_array = dictionaryData[ECLStringConstants.i.Debenture_Fsv_array];
                var Cash_Fsv_array = dictionaryData[ECLStringConstants.i.Cash_Fsv_array];
                var Inventory_Fsv_array = dictionaryData[ECLStringConstants.i.Inventory_Fsv_array];
                var Plant_Equipment_Fsv_array = dictionaryData[ECLStringConstants.i.Plant_Equipment_Fsv_array];
                var Residential_Fsv_array = dictionaryData[ECLStringConstants.i.Residential_Fsv_array];
                var Commercial_Fsv_array = dictionaryData[ECLStringConstants.i.Commercial_Fsv_array];
                var Receivables_Fsv_array = dictionaryData[ECLStringConstants.i.Receivables_Fsv_array];
                var Shares_Fsv_array = dictionaryData[ECLStringConstants.i.Shares_Fsv_array];
                var Vehicle_Fsv_array = dictionaryData[ECLStringConstants.i.Vehicle_Fsv_array];

                var CustomerNo_array = dictionaryData[ECLStringConstants.i.CustomerNo_array];
                var ProjectFinance_array = new List<int>();
                var lstProject_Finance_Ind = lGDPreCalc.Select(o => o.project_finance_ind).ToList();

                foreach (var fin_itm in lstProject_Finance_Ind)
                {
                    ProjectFinance_array.Add(fin_itm == 0 ? 1 : 0);
                }

                //var dictionaryData_fsv = GetArrayRawData_Fsv(lstRaw, input);



                //collateralTable.contract_no = input.customer_no;

                collateralTable = SumProduct(pd_x_ead_List, collateralTable, Debenture_Omv_array, Cash_Omv_array, Inventory_Omv_array, Plant_Equipment_Omv_array, Residential_Omv_array, Commercial_Omv_array, Receivables_Omv_array, Shares_Omv_array, Vehicle_Omv_array, Debenture_Fsv_array, Cash_Fsv_array, Inventory_Fsv_array, Plant_Equipment_Fsv_array, Residential_Fsv_array, Commercial_Fsv_array, Receivables_Fsv_array, Shares_Fsv_array, Vehicle_Fsv_array, CustomerNo_array, ProjectFinance_array, input);

                collaterals.Add(collateralTable);
            }
            return collaterals;

        }

        private LGDCollateralData SumProduct(List<double> pd_x_ead_List, LGDCollateralData collateralTable, List<int> debenture_Omv_array, List<int> cash_Omv_array, List<int> inventory_Omv_array, List<int> plant_Equipment_Omv_array, List<int> residential_Omv_array, List<int> commercial_Omv_array, List<int> receivables_Omv_array, List<int> shares_Omv_array, List<int> vehicle_Omv_array, List<int> debenture_Fsv_array, List<int> cash_Fsv_array, List<int> inventory_Fsv_array, List<int> plant_Equipment_Fsv_array, List<int> residential_Fsv_array, List<int> commercial_Fsv_array, List<int> receivables_Fsv_array, List<int> shares_Fsv_array, List<int> vehicle_Fsv_array, List<int> customerNo_array, List<int> projectFinance_array, LGD_Inputs inputs)
        {
            for (int i = 0; i < pd_x_ead_List.Count; i++)
            {
                collateralTable.debenture_omv += pd_x_ead_List[i] * debenture_Omv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.cash_omv += pd_x_ead_List[i] * cash_Omv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.inventory_omv += pd_x_ead_List[i] * inventory_Omv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.plant_and_equipment_omv += pd_x_ead_List[i] * plant_Equipment_Omv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.residential_property_omv += pd_x_ead_List[i] * residential_Omv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.commercial_property_omv += pd_x_ead_List[i] * commercial_Omv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.receivables_omv += pd_x_ead_List[i] * receivables_Omv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.shares_omv += pd_x_ead_List[i] * shares_Omv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.vehicle_omv += pd_x_ead_List[i] * vehicle_Omv_array[i] * customerNo_array[i] * projectFinance_array[i];

                collateralTable.debenture_fsv += pd_x_ead_List[i] * debenture_Fsv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.cash_fsv += pd_x_ead_List[i] * cash_Fsv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.inventory_fsv += pd_x_ead_List[i] * inventory_Fsv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.plant_and_equipment_fsv += pd_x_ead_List[i] * plant_Equipment_Fsv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.residential_property_fsv += pd_x_ead_List[i] * residential_Fsv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.commercial_property_fsv += pd_x_ead_List[i] * commercial_Fsv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.receivables_fsv += pd_x_ead_List[i] * receivables_Fsv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.shares_fsv += pd_x_ead_List[i] * shares_Fsv_array[i] * customerNo_array[i] * projectFinance_array[i];
                collateralTable.vehicle_fsv += pd_x_ead_List[i] * vehicle_Fsv_array[i] * customerNo_array[i] * projectFinance_array[i];

            }

            collateralTable.debenture_omv= computeCollateralVariable(inputs.debenture_omv, inputs.pd_x_ead, inputs.project_finance_ind,collateralTable.debenture_omv);
            collateralTable.cash_omv = computeCollateralVariable(inputs.cash_omv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.cash_omv);
            collateralTable.inventory_omv = computeCollateralVariable(inputs.inventory_omv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.inventory_omv);
            collateralTable.plant_and_equipment_omv = computeCollateralVariable(inputs.plant_and_equipment_omv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.plant_and_equipment_omv);
            collateralTable.residential_property_omv = computeCollateralVariable(inputs.residential_property_omv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.residential_property_omv);
            collateralTable.commercial_property_omv = computeCollateralVariable(inputs.commercial_property_omv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.commercial_property_omv);
            collateralTable.receivables_omv = computeCollateralVariable(inputs.receivables_omv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.receivables_omv);
            collateralTable.shares_omv = computeCollateralVariable(inputs.shares_omv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.shares_omv);
            collateralTable.vehicle_omv = computeCollateralVariable(inputs.vehicle_omv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.vehicle_omv);

            collateralTable.total_omv = collateralTable.debenture_omv +
                                                 collateralTable.cash_omv +
                                                 collateralTable.inventory_omv +
                                                 collateralTable.plant_and_equipment_omv +
                                                 collateralTable.residential_property_omv +
                                                 collateralTable.commercial_property_omv +
                                                 collateralTable.receivables_omv +
                                                 collateralTable.shares_omv +
                                                 collateralTable.vehicle_omv;

            collateralTable.debenture_fsv = computeCollateralVariable(inputs.debenture_fsv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.debenture_fsv);
            collateralTable.cash_fsv = computeCollateralVariable(inputs.cash_fsv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.cash_fsv);
            collateralTable.inventory_fsv = computeCollateralVariable(inputs.inventory_fsv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.inventory_fsv);
            collateralTable.plant_and_equipment_fsv = computeCollateralVariable(inputs.plant_and_equipment_fsv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.plant_and_equipment_fsv);
            collateralTable.residential_property_fsv = computeCollateralVariable(inputs.residential_property_fsv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.residential_property_fsv);
            collateralTable.commercial_property_fsv = computeCollateralVariable(inputs.commercial_property_fsv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.commercial_property_fsv);
            collateralTable.receivables_fsv = computeCollateralVariable(inputs.receivables_fsv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.receivables_fsv);
            collateralTable.shares_fsv = computeCollateralVariable(inputs.shares_fsv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.shares_fsv);
            collateralTable.vehicle_fsv = computeCollateralVariable(inputs.vehicle_fsv, inputs.pd_x_ead, inputs.project_finance_ind, collateralTable.vehicle_fsv);

            return collateralTable;
        }

        private double computeCollateralVariable(double input_inventory, double inputs_pd_x_ead, double inputs_project_finance_ind, double collateralValue)
        {
            if(collateralValue==0)
            {
                return 0;
            }
            var val= (input_inventory * inputs_pd_x_ead * (1 - inputs_project_finance_ind)) / collateralValue;

            return val;
        }

        private List<LGDPrecalculationOutput> GetValue(List<Loanbook_Data> lstRaw, List<LGDPrecalculationOutput> lGDPreCalc, double value)
        {
            for(int i=0; i<lstRaw.Count; i++)
            {
                if(lstRaw[i].DebentureOMV==value)
                {
                    lGDPreCalc[i].value = "1";
                }
                else
                {
                    lGDPreCalc[i].value = "0";
                }
            }
            return lGDPreCalc;
        }
        //private Dictionary<string,List<string>> GetArray (List<Raw_Data> lstRaw, string value, string columnName )
        //{
        //    var D_List = lstRaw.Select(x => Convert.ToString(x.Field<double>(columnName))).ToList();

        //    List<string> boolValue = new List<string>();
        //    foreach (var item in contractID_list)
        //    {
        //        if (item == new_contract_no)
        //        {
        //            boolValue.Add("1");
        //        }
        //        else
        //        {
        //            boolValue.Add("0");
        //        }
        //    }
        //    string[] _array = GetValue(D_List, value).ToArray();

        //    return _array;
        //}

        private Dictionary<string, List<int>> GetArrayRawData(List<Loanbook_Data> lstRaw, LGD_Inputs input)
        {
            var Debenture_Omv_array = new List<int>();
            var Cash_Omv_array = new List<int>();
            var Inventory_Omv_array = new List<int>();
            var Plant_Equipment_Omv_array = new List<int>();
            var Residential_Omv_array = new List<int>();
            var Commercial_Omv_array = new List<int>();
            var Receivables_Omv_array = new List<int>();
            var Shares_Omv_array = new List<int>();
            var Vehicle_Omv_array = new List<int>();

            var Debenture_Fsv_array = new List<int>();
            var Cash_Fsv_array = new List<int>();
            var Inventory_Fsv_array = new List<int>();
            var Plant_Equipment_Fsv_array = new List<int>();
            var Residential_Fsv_array = new List<int>();
            var Commercial_Fsv_array = new List<int>();
            var Receivables_Fsv_array = new List<int>();
            var Shares_Fsv_array = new List<int>();
            var Vehicle_Fsv_array = new List<int>();

            var CustomerNo_array = new List<int>();

            ////var ProjectFinance_array = new List<string>();
            ////var projectFinance_raw_lst = lstRaw.Select(x => x.project_finance_ind).ToList();


            foreach (var item in lstRaw)
            {
                Debenture_Omv_array.Add(item.DebentureOMV == input.debenture_omv ? 1:0);
                Cash_Omv_array.Add(item.CashOMV == input.cash_omv ? 1:0);
                Inventory_Omv_array.Add(item.InventoryOMV == input.inventory_omv ? 1:0);
                Plant_Equipment_Omv_array.Add(item.PlantEquipmentOMV == input.plant_and_equipment_omv ? 1:0);
                Residential_Omv_array.Add(item.ResidentialPropertyOMV == input.residential_property_omv ? 1:0);
                Commercial_Omv_array.Add(item.CommercialPropertyOMV == input.commercial_property_omv ? 1:0);
                Receivables_Omv_array.Add(item.ReceivablesOMV == input.receivables_omv ? 1:0);
                Shares_Omv_array.Add(item.SharesOMV == input.shares_omv ? 1:0);
                Vehicle_Omv_array.Add(item.VehicleOMV == input.vehicle_omv ? 1:0);

                Debenture_Fsv_array.Add(item.DebentureFSV == input.debenture_fsv ? 1 : 0);
                Cash_Fsv_array.Add(item.CashFSV == input.cash_fsv ? 1 : 0);
                Inventory_Fsv_array.Add(item.InventoryFSV == input.inventory_fsv ? 1 : 0);
                Plant_Equipment_Fsv_array.Add(item.PlantEquipmentFSV == input.plant_and_equipment_fsv ? 1 : 0);
                Residential_Fsv_array.Add(item.ResidentialPropertyFSV == input.residential_property_fsv ? 1 : 0);
                Commercial_Fsv_array.Add(item.CommercialProperty == input.commercial_property_fsv ? 1 : 0);
                Receivables_Fsv_array.Add(item.ReceivablesFSV == input.receivables_fsv ? 1 : 0);
                Shares_Fsv_array.Add(item.SharesFSV == input.shares_fsv ? 1 : 0);
                Vehicle_Fsv_array.Add(item.VehicleFSV == input.vehicle_fsv ? 1 : 0);

                CustomerNo_array.Add(item.CustomerNo == input.customer_no ? 1:0);
            }

            var dic = new Dictionary<string, List<int>>();
            dic.Add(ECLStringConstants.i.Debenture_Omv_array, Debenture_Omv_array);
            dic.Add(ECLStringConstants.i.Cash_Omv_array, Cash_Omv_array);
            dic.Add(ECLStringConstants.i.Inventory_Omv_array, Inventory_Omv_array);
            dic.Add(ECLStringConstants.i.Plant_Equipment_Omv_array, Plant_Equipment_Omv_array);
            dic.Add(ECLStringConstants.i.Residential_Omv_array, Residential_Omv_array);
            dic.Add(ECLStringConstants.i.Commercial_Omv_array, Commercial_Omv_array);
            dic.Add(ECLStringConstants.i.Receivables_Omv_array, Receivables_Omv_array);
            dic.Add(ECLStringConstants.i.Shares_Omv_array, Shares_Omv_array);
            dic.Add(ECLStringConstants.i.Vehicle_Omv_array, Vehicle_Omv_array);

            dic.Add(ECLStringConstants.i.Debenture_Fsv_array, Debenture_Fsv_array);
            dic.Add(ECLStringConstants.i.Cash_Fsv_array, Cash_Fsv_array);
            dic.Add(ECLStringConstants.i.Inventory_Fsv_array, Inventory_Fsv_array);
            dic.Add(ECLStringConstants.i.Plant_Equipment_Fsv_array, Plant_Equipment_Fsv_array);
            dic.Add(ECLStringConstants.i.Residential_Fsv_array, Residential_Fsv_array);
            dic.Add(ECLStringConstants.i.Commercial_Fsv_array, Commercial_Fsv_array);
            dic.Add(ECLStringConstants.i.Receivables_Fsv_array, Receivables_Fsv_array);
            dic.Add(ECLStringConstants.i.Shares_Fsv_array, Shares_Fsv_array);
            dic.Add(ECLStringConstants.i.Vehicle_Fsv_array, Vehicle_Fsv_array);

            dic.Add(ECLStringConstants.i.CustomerNo_array, CustomerNo_array);

            return dic;
        }


        internal void EAD_LifeTimeProjections(List<Refined_Raw_Retail_Wholesale> refined_lstRaw, List<LifeTimeEADs> lifeTimeEAD_w, List<CIRProjections> cirProjections, List<PaymentSchedule> _paymentScheduleProjection, CalibrationResult_EAD_CCF_Summary ccfData)
        {
            var lifetimeEadInputs = new List<LifeTimeProjections>();
            var lstContractIds = lifeTimeEAD_w.Select(o => o.contract_no).Distinct().ToList();

            try
            {
                foreach (var contract in lstContractIds)
                {

                    var lifetime_query = lifeTimeEAD_w.FirstOrDefault(o => o.contract_no == contract);
                    
                    string eir_group_value = lifetime_query.eir_base_premium;
                    string cir_group_value = lifetime_query.cir_base_premium;

                    //Perform Projections
                    double noOfMonths = 1;
                    if (lifetime_query.end_date != null)
                    {
                        try
                        {
                            var maximumDate = lifetime_query.end_date;
                            try
                            {
                                double noOfDays = (maximumDate.Value - reportingDate).Days;
                                noOfMonths = Math.Ceiling(noOfDays * 12 / 365);
                            }
                            catch { }
                        }
                        catch (Exception ex)
                        {
                            noOfMonths = 1;
                            Log4Net.Log.Error(ex);
                            //Log4Net.Log.Error(ex.ToString());
                        }
                    }



                    var refined_query = refined_lstRaw.FirstOrDefault(o => o.contract_no == contract);
                    refined_query.credit_limit_lcy = refined_query.credit_limit_lcy ?? 0;
                    refined_query.OUTSTANDING_BALANCE_LCY = refined_query.OUTSTANDING_BALANCE_LCY ?? "0";
                    lifetime_query.mths_in_force = !string.IsNullOrEmpty(lifetime_query.mths_in_force) ? lifetime_query.mths_in_force : "0";
                    lifetime_query.mths_to_expiry = !string.IsNullOrEmpty(lifetime_query.mths_to_expiry) ? lifetime_query.mths_to_expiry : "0";
                    lifetime_query.first_interest_month = !string.IsNullOrEmpty(lifetime_query.first_interest_month) ? lifetime_query.first_interest_month : "0";
                    lifetime_query.rem_interest_moritorium = !string.IsNullOrEmpty(lifetime_query.rem_interest_moritorium) ? lifetime_query.rem_interest_moritorium : "0";
                    EAD_Inputs obj = new EAD_Inputs()
                    {
                        outstanding_balance_lcy = double.Parse(refined_query.OUTSTANDING_BALANCE_LCY),// Convert.ToDouble(refinedRawData.Rows[contractIndex][ColumnNames.outstanding_bal_lcy]),
                        product_type = refined_query.product_type, //refinedRawData.Rows[contractIndex][ColumnNames.product_type].ToString(),
                        months_to_expiry = double.Parse(lifetime_query.mths_to_expiry), //Convert.ToDouble(lifeTimeEAD_w.Rows[contractIndex][ColumnNames.mths_to_expiry]),
                        segment = refined_query.segment, //refinedRawData.Rows[contractIndex][ColumnNames.segment].ToString(),
                        credit_limit_lcy = refined_query.credit_limit_lcy.Value,   ///Convert.ToDouble(refinedRawData.Rows[contractIndex][ColumnNames.credit_limit_lcy]),
                        rem_interest_moritorium = double.Parse(lifetime_query.rem_interest_moritorium),  //Convert.ToDouble(lifeTimeEAD_w.Rows[contractIndex][ColumnNames.rem_interest_moritorium]),
                        interest_divisor = lifetime_query.interest_divisor,  // lifeTimeEAD_w.Rows[contractIndex][ColumnNames.interest_divisor].ToString()
                        months_in_force = double.Parse(lifetime_query.mths_in_force),
                        first_interest_month = lifetime_query.first_interest_month
                    };

                    //noOfMonths reset to one because value is same accross board (as adviced by Femi Longe)
                    // noOfMonths = 27; /// for Sao tome testing

                    var CCF_OBE = 1.0;
                    try { CCF_OBE = Convert.ToDouble(_eclEadInputAssumption.FirstOrDefault(o => o.Key == "ConversionFactorOBE").Value); } catch { }

                    double value = projection_Calulcation_lifetimeEAD_0(obj.outstanding_balance_lcy, obj.product_type);
                    var product_type = obj.product_type.ToLower();
                    if (product_type.Contains(ECLStringConstants.i._productType_loan.ToLower()) || product_type.Contains(ECLStringConstants.i._productType_od.ToLower()) || product_type.Contains(ECLStringConstants.i.CARDS.ToLower()) || product_type.Contains(ECLStringConstants.i._productType_lease.ToLower()) ||  product_type.Contains(ECLStringConstants.i._productType_mortgage.ToLower()))
                    {
                        //do nothing
                    }
                    else
                    {
                        value = value * CCF_OBE;
                    }
                    //if(contract.Contains("0010123600327101"))// for account with amortise in sheet, but not in payment schedule on EAD input (contract.Contains("1CRLO172640022"))
                    //{
                    //    // Do nothing
                    //}
                    //else
                    //{
                    //    continue;
                    //}
                    var contract_lifetimeEadInputs = new List<LifeTimeProjections>();
                    contract_lifetimeEadInputs.Add(new LifeTimeProjections { Contract_no = contract, Eir_Group = eir_group_value, Cir_Group = cir_group_value, Month = 0, Value = value });
                    
                    double overallvalue = 0;

                    var realContractId = contract;// Computation.GetActualContractId(contract);
                    
                    var cirgroup_cirProjections = cirProjections.Where(o => o.cir_group == cir_group_value).ToList();
                    var contract_paymentScheduleProjection = _paymentScheduleProjection.Where(o => o.ContractId == realContractId).ToList();
                    contract_paymentScheduleProjection.OrderBy(o => o.NoOfSchedules).ToList();

                    var ps_proj = _paymentScheduleProjection.FirstOrDefault(o => o.ContractId == realContractId);

                    var PrePaymentFactor = 0.0;
                    try { PrePaymentFactor = Convert.ToDouble(_eclEadInputAssumption.FirstOrDefault(o => o.Key == "PrePaymentFactor)").Value); } catch { }

                    var outstandingBalance = 0.0;
                    try
                    {
                        outstandingBalance = double.Parse(refined_query.OUTSTANDING_BALANCE_LCY);
                    }
                    catch { }

                    for (int monthIndex = 1; monthIndex <= lifetime_query.LIM_MONTH; monthIndex++)
                    {
                        double value1 = 0, value2 = 0;
                        overallvalue = 0;
                        obj.product_type = obj.product_type ?? "";
                        try
                        {
                            if (obj.product_type.ToLower() != ECLStringConstants.i._productType_loan.ToLower() && obj.product_type.ToLower() != ECLStringConstants.i._productType_lease.ToLower() && obj.product_type.ToLower() != ECLStringConstants.i._productType_mortgage.ToLower())
                            {
                                value1 = obj.outstanding_balance_lcy + Math.Max((obj.credit_limit_lcy - obj.outstanding_balance_lcy) * ccfData.Overall_CCF.Value, 0);

                                if (obj.product_type.ToLower() != ECLStringConstants.i._productType_od.ToLower() && obj.product_type.ToLower() != ECLStringConstants.i._productType_card.ToLower())
                                {
                                    value2 = CCF_OBE;
                                }
                                else
                                {
                                    value2 = 1;
                                }

                                overallvalue = value1 * value2;
                            }
                            else
                            {
                                double previousMonth = 0;
                                double d_value = 0;
                                try { previousMonth = contract_lifetimeEadInputs.FirstOrDefault(o => o.Month == (monthIndex - 1) && o.Contract_no == contract).Value; } catch { };

                                
                                string component="";
                                if (ps_proj != null)
                                {
                                    component = ps_proj.PaymentType;
                                }


                                    //component = ps_proj.PaymentType;

                                    double c_value = cirgroup_cirProjections.FirstOrDefault(o => o.cir_group == cir_group_value && o.months == monthIndex-1).cir_effective;
                                    component = component ?? "";
                                    if (component.ToLower() != ECLStringConstants.i._amortise.ToLower())
                                    {
                                        int a_value = (monthIndex > obj.rem_interest_moritorium || obj.rem_interest_moritorium == 0) ? 1 : 0;
                                        int b_value = (obj.interest_divisor != "1") ? 1 : 0;

                                        d_value = a_value * b_value * c_value * previousMonth;
                                    }
                                    else
                                    {
                                        d_value = c_value * previousMonth;
                                    }

                                    overallvalue = previousMonth + d_value;


                                    double f_value = 0;
                                    try { f_value = contract_paymentScheduleProjection.FirstOrDefault(o => o.ContractId == contract && o.Months == monthIndex.ToString()).Value; } catch { };

                                    outstandingBalance = outstandingBalance - f_value;
                                    if (outstandingBalance <= 0)
                                        continue;

                                    f_value = f_value * ECLNonStringConstants.i.Local_Currency;


                                    double g_value = 0;
                                obj.interest_divisor = obj.interest_divisor ?? "";
                                    if (obj.interest_divisor.ToLower() == ECLStringConstants.i._interestDivisior.ToLower())
                                    {
                                        //x = ($H4=T$3)*SUMPRODUCT(OFFSET(T4, 0, -1, 1, -T$3), OFFSET(CIR_EFF_MONTHLY_RANGE, $M4-1, T$3, 1, -T$3))*($H4+$G4)/T$3
                                        if (obj.months_to_expiry == monthIndex)
                                        {
                                            //get range
                                            double[] h_value = contract_lifetimeEadInputs.Where(o => o.Contract_no == contract
                                                                            && o.Month >= 0
                                                                            && o.Month <= monthIndex)
                                                                            .Select(x => x.Value)
                                                                            .ToArray();
                                            double[] i_value = cirgroup_cirProjections.Where(o => o.cir_group == cir_group_value
                                                                            && o.months >= 0
                                                                            && o.months <= monthIndex)
                                                                            .Select(x => x.value)
                                                                    .ToArray();
                                            g_value = SumProduct(h_value, i_value) * (obj.months_to_expiry + obj.months_in_force) / monthIndex;
                                        }
                                    }
                                    else
                                    {
                                        //r = ($N4 <> "AMORTISE")*(MOD((T$3-$J4),$I4)=0)*(T$3>$F4)

                                        var k_value = (component != ECLStringConstants.i._amortise) ? 1 : 0;
                                        var l_value = (monthIndex - double.Parse(obj.first_interest_month)) % Convert.ToDouble(obj.interest_divisor)==0?1:0;
                                        var m_value = (monthIndex > obj.rem_interest_moritorium) ? 1 : 0;

                                        double n_value = k_value * l_value * m_value;
                                        double o_value;
                                        
                                        if (monthIndex < Convert.ToDouble(obj.interest_divisor))
                                        {
                                            double[] p_value = contract_lifetimeEadInputs.Where(o => o.Contract_no == contract
                                                                            && o.Month >= 0
                                                                            && o.Month <= monthIndex)
                                                                            .Select(x => x.Value)
                                                                            .ToArray();
                                            double[] i_value = cirgroup_cirProjections.Where(o => o.cir_group == cir_group_value
                                                                            && o.months >= 0
                                                                            && o.months < monthIndex)
                                                                            .Select(x => x.cir_effective)
                                                                            .ToArray();
                                            //o = SUMPRODUCT(OFFSET(T4, 0, -1, 1, -T$3), OFFSET(CIR_EFF_MONTHLY_RANGE, $M4-1, T$3, 1, -T$3))*$I4/T$3
                                            o_value = SumProduct(p_value, i_value) * (Convert.ToDouble(obj.interest_divisor) / monthIndex);
                                        }
                                        else
                                        {
                                            double[] p_value = contract_lifetimeEadInputs.Where(o => o.Contract_no == contract
                                                                            && o.Month == monthIndex-1)
                                                                            .Select(x => x.Value)
                                                                            .ToArray();
                                            double[] i_value = cirgroup_cirProjections.Where(o => o.cir_group == cir_group_value
                                                                            && o.months == monthIndex-1)
                                                                            .Select(x => x.cir_effective)
                                                                            .ToArray();
                                            //o = SUMPRODUCT(OFFSET(T4, 0, -1, 1, -T$3), OFFSET(CIR_EFF_MONTHLY_RANGE, $M4-1, T$3, 1, -T$3))
                                            o_value = SumProduct(p_value, i_value);
                                        }
                                        //x = r * o
                                        g_value = n_value * o_value;
                                    }
                                    overallvalue = overallvalue - (f_value + g_value);
                                    //f_value += g_value;
                                    //                                    overallvalue = overallvalue - Math.Max(f_value, 0) * (1 - val);
                                    overallvalue = Math.Max(overallvalue, 0) * (1 - PrePaymentFactor);

                                    if(overallvalue==0)
                                    {
                                        overallvalue = previousMonth;
                                    }
                                //}
                                //else
                                //{
                                //    overallvalue = previousMonth;
                                //}
                            }

                        }
                        catch (Exception ex)
                        {
                            var cc = ex;
                            Log4Net.Log.Error(ex);
                        }
                        //if (overallvalue == 0)
                        //{
                        //    try { overallvalue = double.Parse(refined_query.OUTSTANDING_BALANCE_LCY.Trim()) + (double.Parse(refined_query.OUTSTANDING_BALANCE_LCY.Trim()) * 0.00000099); } catch { };
                        //}
                        contract_lifetimeEadInputs.Add(new LifeTimeProjections { Contract_no = contract, Eir_Group = eir_group_value, Cir_Group = cir_group_value, Month = monthIndex, Value = overallvalue });


                    }

                    lifetimeEadInputs.AddRange(contract_lifetimeEadInputs);
                }
            }catch(Exception ex)
            {
                var cc = ex;
                Log4Net.Log.Error(ex);
            }

            Log4Net.Log.Info($"Saving EAD_LifeTimeProjections...{lifetimeEadInputs.Count}");
            ExecuteNative.SaveLifeTimeProjections(lifetimeEadInputs, this._eclId, _eclType);
            //return lifetimeEadInputs;
        }

        private double SumProduct(double[] arrayA, double[] arrayB)
        {
            double result = 0;

            for (int i = 0; i < arrayA.Length; i++)
            {
                try
                {
                    result += arrayA[i] * arrayB[i];
                }
                catch { }
            }

            return result;
        }


        private double SumProduct(List<double> arrayA, List<int> arrayB, List<int> arrayC, List<int> arrayD)
        {
            double result = 0;

            for (int i = 0; i < arrayA.Count; i++)
            {
                result += arrayA[i] * arrayB[i] + arrayC[i] + arrayD[i];
            }
            return result;
        }
        private double projection_Calulcation_lifetimeEAD_0(double outstanding_bal_lcy, string product_type)
        {
            double value=0;
            product_type = product_type ?? "";
            if (product_type.ToLower() != ECLStringConstants.i._productType_loan.ToLower() && product_type.ToLower() != ECLStringConstants.i._productType_lease.ToLower() && product_type.ToLower() != ECLStringConstants.i._productType_mortgage.ToLower() && product_type.ToLower() != ECLStringConstants.i._productType_od.ToLower() && product_type.ToLower() != ECLStringConstants.i._productType_card.ToLower())
            {
                try { value = double.Parse(_eclEadInputAssumption.FirstOrDefault(o => o.Key == "CreditConversionFactorObe").Value); } catch { }
            }
            else
            {
                value = 1;
            }
            value = outstanding_bal_lcy * value;

            return value;
        }


        public string GenerateContractId(Loanbook_Data r)
        {
            r.CreditLimit = r.CreditLimit ?? 0;
            r.OriginalBalanceLCY = r.OriginalBalanceLCY ?? 0;

            if (r.ContractStartDate == null && r.ContractEndDate == null && r.CreditLimit == 0 && r.OriginalBalanceLCY == 0)
            {
                var colSum = r.DebentureOMV ?? +r.CashOMV ?? +r.InventoryOMV ?? +r.PlantEquipmentOMV ?? +r.ResidentialPropertyOMV ?? +r.CommercialPropertyOMV ?? +r.ReceivablesOMV ?? +r.SharesOMV ?? +r.VehicleOMV ?? +(r.GuaranteeIndicator ? 1 : 0);
                return colSum == 0 ? $"{ECLStringConstants.i.ExpiredContractsPrefix}{r.ProductType}|{r.Segment}" : $"{ECLStringConstants.i.ExpiredContractsPrefix}{r.ProductType}|{r.ContractNo}";
            }
            else
            {
                return r.ContractNo;
            }
            
        }

        internal List<EIRProjections> EAD_EIRProjections(List<LifeTimeEADs> lifeTimeEAD)
        {
            var rs = new List<EIRProjections>();

            var eir_base_premiums = lifeTimeEAD.Select(o => o.eir_base_premium).Distinct().ToList();

            double noOfMonths = 1;
            var maximumDate = lifeTimeEAD.Max(o => o.end_date);
            if (maximumDate != null)
            {
                try
                {
                    double noOfDays = (maximumDate.Value - reportingDate).Days;
                    noOfMonths = Math.Ceiling(noOfDays * 12 / 365);
                }
                catch (Exception ex)
                {
                    noOfMonths = 1;
                    Log4Net.Log.Error(ex);
                }
            }

            foreach (var group_value in eir_base_premiums)
            {

                for (int mnthIdx = 0; mnthIdx < noOfMonths; mnthIdx++)
                {
                    var val = 0.0;
                    if (group_value != ECLStringConstants.i.ExpiredContractsPrefix)
                    {
                        var temp = group_value.Split(ECLStringConstants.i._splitValue);
                        if (temp[1] != ECLStringConstants.i._fixed)
                        {
                            var _ViRVal = 0.0;
                            try
                            {
                                _ViRVal = double.Parse(ViR.FirstOrDefault(o => o.InputName == temp[1]).Value.Trim());
                            }
                            catch { }
                            val = Math.Round(_ViRVal * 100, 2); //+ Convert.ToDouble(temp[2].Substring(0, temp[2].Length - 1)), 1) / 100;
                        }
                        else
                        {
                            val = 0;// Convert.ToDouble(ViR) / 100;
                        }
                        val = val + (Convert.ToDouble(temp[2].Substring(0, temp[2].Length - 1)) * 0.01);

                    }

                //calculate the eir effective
                //double effectiveValue, power = Convert.ToDouble(1m / 12m);
                //effectiveValue = Math.Pow(1 + val, power) - 1;
                rs.Add(new EIRProjections { eir_group = group_value, months = mnthIdx, value = val });
            }
            }
            return rs;
        }

        internal List<CIRProjections> EAD_CIRProjections(List<LifeTimeEADs> lifeTimeEAD)
        {




            var rs = new List<CIRProjections>();

            var cir_base_premiums = lifeTimeEAD.Select(o => o.cir_base_premium).Distinct().ToList();

            double noOfMonths = 1;
            var maximumDate = lifeTimeEAD.Max(o => o.end_date);
            if (maximumDate != null)
            {
                try
                {
                    double noOfDays = (maximumDate.Value - reportingDate).Days;
                    noOfMonths = Math.Ceiling(noOfDays * 12 / 365);
                }
                catch (Exception ex)
                {
                    noOfMonths = 1;
                    Log4Net.Log.Error(ex);
                }
            }

            foreach (var group_value in cir_base_premiums)
            {

                for (int mnthIdx = 0; mnthIdx < noOfMonths; mnthIdx++)
                {
                    var val = 0.0;
                    if (group_value != ECLStringConstants.i.ExpiredContractsPrefix)
                    {
                        var temp = group_value.Split(ECLStringConstants.i._splitValue);
                        if (temp[1] != ECLStringConstants.i._fixed)
                        {
                            var _ViRVal = 0.0;
                            try
                            {
                                _ViRVal = double.Parse(ViR.FirstOrDefault(o => o.InputName == temp[1]).Value.Trim());
                            }
                            catch { }
                            val = Math.Round(_ViRVal * 100, 2); //+ Convert.ToDouble(temp[2].Substring(0, temp[2].Length - 1)), 1) / 100;
                        }
                        else
                        {
                            val = 0;// Convert.ToDouble(ViR) / 100;
                        }
                        try
                        {
                            val = val + (Convert.ToDouble(temp[2].Substring(0, temp[2].Length - 1)) * 0.01);
                        }
                        catch { }

                    }

                    //calculate the eir effective
                    double effectiveValue, power = Convert.ToDouble(1m / 12m);
                    effectiveValue = Math.Pow(1 + val, power) - 1;
                    rs.Add(new CIRProjections { cir_group = group_value, months = mnthIdx, value = val, cir_effective= effectiveValue });
                }
            }
            return rs;

        }

        internal List<LifeTimeEADs> GenerateLifeTimeEAD(List<Refined_Raw_Retail_Wholesale> r_lst)
        {
            var rs = new List<LifeTimeEADs>();

            var behavioral = new CalibrationInput_EAD_Behavioural_Terms_Processor().GetBehaviouralData(this._eclId, this._eclType);
            foreach (var i in r_lst)
            {
                try
                {

                    var r = new LifeTimeEADs();

                    r.contract_no = i.contract_no;
                    r.segment = i.segment;
                    r.credit_limit_lcy = i.credit_limit_lcy != null ? i.credit_limit_lcy.Value.ToString() : "0";
                    r.start_date = S_E_Date(i.RESTRUCTURE_START_DATE.ToString(), i.RESTRUCTURE_INDICATOR.ToString(), i.CONTRACT_START_DATE.ToString());
                    r.LIM_MONTH = i.LIM_MONTH;
                    ///end date
                    r.end_date = S_E_Date(i.RESTRUCTURE_END_DATE.ToString(), i.RESTRUCTURE_INDICATOR.ToString(), i.CONTRACT_END_DATE.ToString());

                    //remaining IP
                    r.remaining_ip = Remaining_IP(i, reportingDate).ToString();

                    if (i.contract_no.Substring(0, 3) == ECLStringConstants.i.ExpiredContractsPrefix)
                    {
                        r.revised_base = ECLStringConstants.i.ExpiredContractsPrefix;
                        r.cir_premium = String.Empty;
                        r.cir_base_premium = ECLStringConstants.i.ExpiredContractsPrefix;
                        r.eir_base_premium = ECLStringConstants.i.ExpiredContractsPrefix;
                        r.mths_in_force = String.Empty;
                        r.mths_to_expiry = "0";
                    }
                    else
                    {
                        ////populate revised base 
                        r.revised_base = Revised_Base(i.INTEREST_RATE_TYPE, i.BASE_RATE);

                        ///start CIR/EIRpremium
                        var AA_Value = (r.remaining_ip == "0") ? i.CURRENT_CONTRACTUAL_INTEREST_RATE : i.POST_IP_CONTRACTUAL_INTEREST_RATE;
                        r.cir_premium = CIR_EIR_Premium(r.revised_base, AA_Value).ToString();
                        r.eir_premium = CIR_EIR_Premium(r.revised_base, i.EIR).ToString();


                        r.cir_base_premium = CIR_Base_Premium(r.remaining_ip, i.ORIGINATION_CONTRACTUAL_INTEREST_RATE,
                                                                            r.revised_base, r.eir_premium);

                        r.eir_base_premium = EIR_Base_Premium(r.revised_base, r.eir_premium);
                        r.mths_in_force = "0";

                        try
                        {
                            //***********************************************
                            if (r.start_date != null && r.end_date != null)
                                if (r.start_date < r.end_date)
                                    r.mths_in_force = Math.Round(Financial.YearFrac(Convert.ToDateTime(r.start_date), Convert.ToDateTime(r.end_date), DayCountBasis.ActualActual) * 12, 0).ToString();
                        }
                        catch { }

                        r.rem_interest_moritorium = Remaining_IR(i.IPT_O_PERIOD, r.mths_in_force).ToString();

                        //************************************
                        var endDate_Temp = new DateTime();
                        try { endDate_Temp = Convert.ToDateTime(r.end_date); } catch { }
                        r.mths_to_expiry = Months_To_Expiry(reportingDate, endDate_Temp, i.product_type, behavioral.Expired).ToString();

                        r.interest_divisor = Interest_Divisor(i.INTEREST_PAYMENT_STRUCTURE);

                        string interest_divisor = r.interest_divisor;
                        double mths_to_expiry = Convert.ToDouble(r.mths_to_expiry);
                        double rem_interest_moritorium = Convert.ToDouble(r.rem_interest_moritorium);
                        double mths_in_force = Convert.ToDouble(r.mths_in_force);
                        i.IPT_O_PERIOD = string.IsNullOrEmpty(i.IPT_O_PERIOD) ? "0" : i.IPT_O_PERIOD;
                        double ipt_o_period = Convert.ToDouble(i.IPT_O_PERIOD);
                        r.first_interest_month = First_Interest_Month(interest_divisor, mths_to_expiry, rem_interest_moritorium, mths_in_force, ipt_o_period).ToString();

                    }
                    rs.Add(r);
                }
                catch (Exception ex)
                {
                    var cc = ex;
                    Log4Net.Log.Error(ex);
                }
            }

            return rs;
        }

        private string Revised_Base(string interest_rate_type, string base_rate)
        {
            string value = string.Empty;
            interest_rate_type = interest_rate_type ?? "";
            if (interest_rate_type.ToLower() != ECLStringConstants.i.FLOATING.ToLower())
            {
                value = ECLStringConstants.i._fixed;
            }
            else
            {
                if (!string.IsNullOrEmpty(base_rate))
                {
                    value = base_rate;
                }
                else
                {
                    value = ECLStringConstants.i.MPR;
                }
            }
            return value;
        }

        private double CIR_EIR_Premium(string L_revisedBase, string AA_Value)
        {
            double value1 = (string.IsNullOrEmpty(AA_Value)) ? 0 : Math.Pow((Convert.ToDouble(AA_Value) / 1200) + 1, 12) - 1;
            double value2=0.0;

            if (L_revisedBase != ECLStringConstants.i._fixed)
            {
                try
                {
                    value2 = double.Parse(ViR.FirstOrDefault(o => o.InputName.ToUpper().Contains(L_revisedBase.ToUpper())).Value);
                }
                catch { }
                
            }
            else
            {
                value2 = 0;
            }

            return Math.Round(value1 - value2, 3);
        }

        private double Remaining_IP(Refined_Raw_Retail_Wholesale i, DateTime reportingDate)
        {

            double value = 0;
            if (i.CONTRACT_START_DATE == null)
            {
                if (!String.IsNullOrEmpty(i.INTRODUCTORY_PERIOD))
                {
                    //calculate yearfrac
                    var yearFrac = Math.Round((Financial.YearFrac(Convert.ToDateTime(i.CONTRACT_START_DATE), reportingDate, 0)) * 12, 5);
                    if (yearFrac < Convert.ToDouble(i.INTRODUCTORY_PERIOD))
                    {
                        //MAX(ROUND($S5-YEARFRAC($Y5, REPORT_DATE, 0)*12, 0), 1)
                        value = Math.Max(Math.Round(Convert.ToDouble(i.INTRODUCTORY_PERIOD) - (Financial.YearFrac(Convert.ToDateTime(i.CONTRACT_START_DATE), reportingDate, 0) * 12)), 1);
                    }
                }
            }

            return value;
        }

        private DateTime? S_E_Date(string restructure_dt, string restructure_indicator, string contract_dt)
        {
            try
            {
                string value = String.Empty;
                if (!String.IsNullOrEmpty(restructure_dt) && restructure_indicator == "1")
                {
                    value = restructure_dt;
                }
                else
                {
                    if (!String.IsNullOrEmpty(contract_dt))
                    {
                        value = contract_dt;
                    }
                }
                return DateTime.Parse(value);
            }catch
            {
                return null;
            }
        }



        private string CIR_Base_Premium(string remaining_ip, string orig_contractual_ir, string revised_base, string cir_premium)
        {
            string value;
            double value1, value2 = 0;
            string concatenateValue1 = "CIR";
            string concatenateValue2;
            string concatenateValue3;
            if (remaining_ip != "0") //AA5 <> ""
            {
                value1 = (String.IsNullOrEmpty(orig_contractual_ir)) ? 0 : Math.Round((Math.Pow((Convert.ToDouble(orig_contractual_ir) / 1200) + 1, 12) - 1) * 100, 1);
                concatenateValue2 = (value1 == 0) ? "(" + remaining_ip + "RIP@" + "%)_" : "(" + remaining_ip + "RIP@" + value1 + "%)_";
            }
            else
            {
                concatenateValue2 = "_";
            }

            concatenateValue3 = revised_base + "_";
            if (Convert.ToDouble(cir_premium) < 0)
            {
                value2 = Math.Round(Convert.ToDouble(cir_premium) * 100, 1);
                concatenateValue3 = concatenateValue3 + value2 + "%";
            }
            else
            {
                value2 = Math.Round(Convert.ToDouble(cir_premium) * 100, 1);
                concatenateValue3 = concatenateValue3 + "+" + value2 + "%";
            }

            value = concatenateValue1 + concatenateValue2 + concatenateValue3;

            return value;
        }

        private string EIR_Base_Premium(string revised_base, string eir_premium)
        {
            //=IF(LEFT($B5, 3) = VariableNames_E_.expired, VariableNames_E_.expired, "EIR_" & $AB5 & "_" & IF(AD5<0, ROUND($AD5*100, 1) & "%", "+" & ROUND($AD5*100, 1) & "%"))
            string value;
            string concatenateValue1 = "EIR_" + revised_base + "_";

            string concatenateValue2;
            if (Convert.ToDouble(eir_premium) < 0)
            {
                concatenateValue2 = Math.Round(Convert.ToDouble(eir_premium) * 100, 1) + "%";
            }
            else
            {
                concatenateValue2 = "+" + Math.Round(Convert.ToDouble(eir_premium) * 100, 1) + "%";
            }

            value = concatenateValue1 + concatenateValue2;

            return value;
        }

        private double Remaining_IR(string ipt_o_period, string mths_in_force)
        {
            double value = 0;
            if (!String.IsNullOrEmpty(ipt_o_period) && ipt_o_period != "0")
            {
                value = Math.Max(Convert.ToDouble(ipt_o_period) - Convert.ToDouble(mths_in_force), 0);
            }
            return value;
        }

        private string Interest_Divisor(string ir_payment_struct)
        {
            //=IF($P5="B","B", IF($P5 = "H", 6, IF($P5 = "Y", 12, IF($P5 = "M", 1, IF($P5 = "Q", 3, IF($P5 = "S", "Error",IF($P5 = "", "",)))))))
            string value = String.Empty;
            if (ir_payment_struct == "B")
            {
                value = "B";
            }
            else if (ir_payment_struct == "H")
            {
                value = "6";
            }
            else if (ir_payment_struct == "Y")
            {
                value = "12";
            }
            else if (ir_payment_struct == "M")
            {
                value = "1";
            }
            else if (ir_payment_struct == "Q")
            {
                value = "3";
            }
            else if (ir_payment_struct == "S")
            {
                value = "Error";
            }
            return value;
        }

        private double Months_To_Expiry(DateTime reportingDate, DateTime endDate, string productType, double expired)
        {
            productType = productType ?? "";
            double value = 0;
            if (endDate < reportingDate || productType.ToLower() == ECLStringConstants.i._productType_od.ToLower() || productType.ToLower() == ECLStringConstants.i._productType_card.ToLower())
            {
                DateTime EOM = EndOfMonth(endDate, Convert.ToInt32(expired));
                if (reportingDate > EOM)
                {
                    value = 0;
                }
                else
                {
                    if(reportingDate< EOM)
                        value = Math.Max(Math.Round(Financial.YearFrac(reportingDate, EOM, DayCountBasis.ActualActual) * 12), 0);
                }
            }
            else
            {
                DateTime EOM = EndOfMonth(endDate, Convert.ToInt32(expired));
                if (productType.ToLower() == ECLStringConstants.i.ID.ToLower() || productType.ToLower() == ECLStringConstants.i.CARDS.ToLower())
                {
                    value = Math.Max(Math.Round(Financial.YearFrac(reportingDate, EOM, DayCountBasis.ActualActual) * 12), 0);
                }
            }
            return value;
        }

        private double First_Interest_Month(string interest_divisor, double mths_to_expiry, double rem_interest_moritorium, double mths_in_force, double ipt_o_period)
        {
            double value = 0;
            // = IF(interest_divisor <> "", IF(interest_divisor = "B", mths_to_expiry, iF(rem_interest_moritorium <> "", IF(rem_interest_moritorium > 0, rem_interest_moritorium + interest_divisor,
            //(ROUNDUP((mths_in_force -$N5) /interest_divisor, 0) - (mths_in_force -$N5) /interest_divisor) *interest_divisor),(ROUNDUP((mths_in_force) /interest_divisor, 0)-(mths_in_force)/interest_divisor)*interest_divisor)),"")

            if (!String.IsNullOrEmpty(interest_divisor))
            {
                if (interest_divisor == "B")
                {
                    value = mths_to_expiry;
                }
                else
                {
                    if (rem_interest_moritorium != 0)
                    {
                        if (rem_interest_moritorium > 0)
                        {
                            value = rem_interest_moritorium + Convert.ToDouble(interest_divisor);
                        }
                        else
                        {
                            value = (Math.Ceiling(mths_in_force - ipt_o_period / Convert.ToDouble(interest_divisor)) - (mths_in_force - ipt_o_period) / Convert.ToDouble(interest_divisor)) / Convert.ToDouble(interest_divisor);
                        }
                    }
                    else
                    {
                        value = (Math.Ceiling(mths_in_force / Convert.ToDouble(interest_divisor)) - (mths_in_force) / Convert.ToDouble(interest_divisor)) / Convert.ToDouble(interest_divisor);
                    }
                }
            }

            return value;
        }

        private DateTime EndOfMonth(DateTime myDate, int numberOfMonths)
        {
            //Update Value ************************************************
            //Update Value ************************************************
            try
            {
                DateTime startOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(numberOfMonths).AddMonths(1).AddDays(-1);
                return endOfMonth;
            }
            catch (Exception ex)
            {
                Log4Net.Log.Error(ex);
                myDate = DateTime.Today;
                DateTime startOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(numberOfMonths).AddMonths(1).AddDays(-1);
                return endOfMonth;
            }
        }

        public List<LGDPrecalculationOutput> LGDPreCalculation(List<Loanbook_Data> lstRaw)
        {
            

            var pd_assumptions = new List<LGD_PD_Assumptions>();
           

            var cali12Month = new CalibrationInput_PD_CR_RD_Processor().GetPD12MonthsPD(this._eclId, this._eclType);
            var pd_internalModelInputs_Credit= new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_Assumptions().Where(o=>o.PdGroup== Models.PD.PdInputAssumptionGroupEnum.CreditPD).ToList();
            var pd_non_InternalModelInputs_Credit= new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_NonInternalModelInputs(12);
            
            foreach (var itm in cali12Month)
            {
                pd_assumptions.Add(new LGD_PD_Assumptions { eclId=this._eclId, pd_group=itm.Rating.ToString(), pd=itm.Months_PDs_12 });
            }
            foreach (var itm in pd_non_InternalModelInputs_Credit)
            {
                pd_assumptions.Add(new LGD_PD_Assumptions { eclId = this._eclId, pd_group = itm.PdGroup, pd =1- itm.CummulativeSurvival });
            }
            pd_assumptions.Add(new LGD_PD_Assumptions { eclId = this._eclId, pd_group = ECLStringConstants.i.ExpiredContractsPrefix, pd = 0.1 });

            List<double?> outstanding_Bal_Lcy_array = lstRaw.Select(o=>o.OutstandingBalanceLCY).ToList();
            List<string> ContractID_list = lstRaw.Select(o=>o.ContractId).ToList();
            

            var r_arry = new List<double>();
            foreach (var r_itm in outstanding_Bal_Lcy_array)
            {
                
                if (r_itm != null)
                {
                    r_arry.Add(r_itm.Value);
                }
                else
                {
                    r_arry.Add(0);
                }
            }


            // Convert.ToDouble(input.debenture_omv) * Convert.ToDouble(input.pd_x_ead) * (1 - project_finance)

            //create temp table

            var lstTempDT = new List<LGDPrecalculationOutput>();
            var behavioral = new CalibrationInput_EAD_Behavioural_Terms_Processor().GetBehaviouralData(this._eclId, this._eclType);

            foreach (var itm in lstRaw)
            {
                var input = new LGD_Inputs();
                input.new_contract_no = itm.ContractId;
                List<string> newContractID_array = GetValue(ContractID_list, input.new_contract_no);
                var c_arry = new List<double>();
                foreach (var c_itm in newContractID_array)
                {
                    if (c_itm != null)
                    {
                        c_arry.Add(double.Parse(c_itm));
                    }
                    else
                    {
                        c_arry.Add(0);
                    }
                }
                var tempDT = new LGDPrecalculationOutput();
                itm.CurrentRating = itm.CurrentRating ?? "";
                itm.DaysPastDue = itm.DaysPastDue ?? 0;

                input.customer_no = itm.CustomerNo;
                input.product_type = itm.ProductType;
                input.new_contract_no = itm.ContractId;
                input.restructure_indicator = itm.RestructureIndicator;
                input.restructure_end_date = itm.RestructureEndDate;
                input.contract_end_date = itm.ContractEndDate;
                input.rating_model = itm.RatingModel;
                input.segment = itm.Segment;
                input.days_past_due = itm.DaysPastDue.Value;
                input.current_rating = int.Parse(itm.CurrentRating.Replace("+","").Trim());
                input.specialised_lending = itm.SpecialisedLending;
                var check_customer = itm.CustomerNo;

                input.rating_used = input.current_rating > 10? input.current_rating.ToString().Substring(0, 1) : input.current_rating.ToString();

                var ttm_months = TTM_Inputs(input, behavioral);
                string pd_mapping = PD_Mapping(input, ttm_months);

                //Get details from the DATABASE....this is where you aren
                input.month_pd_12 = pd_assumptions.FirstOrDefault(o => o.pd_group == pd_mapping).pd;

                //=IF(OR(LEFT(B4,3)="EXP",AND(OR(V4="CARD",V4="CARDS",V4="OD"),Z4=0)),"EXP",IF($P4="YES",R4,IF($U4="COMMERCIAL","COMM","CONS")&IF($T4<30,"_STAGE_1","_STAGE_2")))
                
                input.pd_x_ead = SumProduct(r_arry.ToArray(), c_arry.ToArray()) * input.month_pd_12;


                //add to table
                tempDT.pd_x_ead = input.pd_x_ead;
                input.specialised_lending = input.specialised_lending ?? "";
                tempDT.project_finance_ind = input.project_finance_ind = (input.specialised_lending.ToUpper() == ECLStringConstants.i.PROJECT_FINANCE) ? 1 : 0;
                tempDT.customer_no = input.customer_no;
                tempDT.contract_id = input.new_contract_no;


                lstTempDT.Add(tempDT);

            }
            return lstTempDT;
        }

        private List<string> GetValue(List<string> contractID_list, string new_contract_no)
        {
            List<string> boolValue = new List<string>();
            foreach (var item in contractID_list)
            {
                if (item == new_contract_no)
                {
                    boolValue.Add("1");
                }
                else
                {
                    boolValue.Add("0");
                }
            }
            return boolValue;
        }

        private double TTM_Inputs(LGD_Inputs input, CalibrationResult_EAD_Behavioural behave)
        {
            double ttm_months=0;
            if (input.new_contract_no.Contains(ECLStringConstants.i.ExpiredContractsPrefix))
            {
                return 0;
            }
            else
            {
                long longDate = 0;
                long reportDate = ConvertToTimeStamp(reportingDate);
                double temp_value1 = 0;

                if (input.restructure_indicator && input.restructure_end_date!=null)
                {
                    var temp_value = input.restructure_end_date.Value;
                    longDate=ConvertToTimeStamp(temp_value);

                    if (longDate > reportDate)
                    {
                        temp_value1 = Math.Floor(Financial.YearFrac(reportingDate, temp_value, 0) * 12);
                    }
                }
                else if (input.contract_end_date != null)
                {
                    var temp_value = input.contract_end_date.Value;
                    longDate = ConvertToTimeStamp(temp_value);

                    if (longDate > reportDate)
                    {
                        temp_value1 = Math.Floor(Financial.YearFrac(reportingDate, temp_value, 0) * 12);
                    }
                }
                    
                
                double temp_value2=0;
                if (input.product_type == "CARD" || input.product_type == "OD")
                {

                    if (input.restructure_indicator && input.restructure_end_date != null)
                    {
                        var temp_value = input.restructure_end_date.Value;
                        longDate = ConvertToTimeStamp(temp_value);

                        if (longDate < reportDate)
                        {
                            temp_value2 = behave.Expired - Math.Floor(Financial.YearFrac(temp_value, reportingDate, 0) * 12);
                            //temp_value2 = Convert.ToDouble(Expired);
                        }
                        else
                        {
                            temp_value2 = behave.NonExpired;
                        }
                    }
                    else if (input.contract_end_date != null)
                    {
                        var temp_value = input.contract_end_date.Value;
                        longDate = ConvertToTimeStamp(temp_value);

                        if (longDate < reportDate)
                        {
                            temp_value2 = behave.Expired - Math.Floor(Financial.YearFrac(temp_value, reportingDate, 0) * 12);
                            //temp_value2 = Convert.ToDouble(Expired);
                        }
                        else
                        {
                            temp_value2 = behave.NonExpired;
                        }
                    }
                   
                }
                else
                {
                    temp_value2 = 0;
                }

                ttm_months = temp_value1 + temp_value2;
            }

            return ttm_months;
        }


        private string PD_Mapping(LGD_Inputs input, double ttm_months)
        {
            string pd_mapping;
            input.product_type = input.product_type ?? "";
            input.new_contract_no = input.new_contract_no ?? "";
            if (input.new_contract_no.ToLower().Contains(ECLStringConstants.i.ExpiredContractsPrefix.ToLower()) || ((input.product_type.ToLower() == ECLStringConstants.i.CARDS.ToLower() || input.product_type.ToLower() == ECLStringConstants.i.CARDS.ToLower() || input.product_type.ToLower() == ECLStringConstants.i._productType_od.ToLower()) && ttm_months == 0))
            {
                pd_mapping = ECLStringConstants.i.ExpiredContractsPrefix;
            }
            else
            {
                input.rating_model = input.rating_model ?? "";
                if (input.rating_model.ToUpper() == ECLStringConstants.i.RatingModel_Yes)
                {
                    pd_mapping = input.rating_used;
                }
                else
                {
                    input.segment = input.segment ?? "";
                    string temp_value = (input.segment == ECLStringConstants.i.COMMERCIAL) ? ECLStringConstants.i.COMM : ECLStringConstants.i.CONS;
                    string temp_value1 = input.days_past_due < 30 ? ECLStringConstants.i._STAGE_1 : ECLStringConstants.i._STAGE_2;
                    pd_mapping = temp_value + temp_value1;
                }
            }

            return pd_mapping;
        }

        private long ConvertToTimeStamp(DateTime date)
        {
            var dateTimeOffset = new DateTimeOffset(date);

            return dateTimeOffset.ToUnixTimeSeconds();
        }

        private DateTime ConvertFromTimeStamp(long timeStamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timeStamp).DateTime.ToLocalTime();
        }
    }
}
