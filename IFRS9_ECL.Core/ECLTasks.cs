using Excel.FinancialFunctions;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using static IFRS9_ECL.Util.ECLStringConstants;

namespace IFRS9_ECL.Core
{
    public class ECLTasks
    {
        public static readonly ECLTasks i = new ECLTasks();
        public double Conversion_Factor_OBE = 1;

        public List<Refined_Raw_Retail_Wholesale> GenerateContractIdandRefinedData(List<Loanbook_Data> lstRaw)
        {
            var refineds = new List<Refined_Raw_Retail_Wholesale>();
            int i = 0;
            foreach (var rr in lstRaw)
            {
                i++;
                Console.WriteLine(i);
                var refined = new Refined_Raw_Retail_Wholesale();
                refined.contract_no=GenerateContractId(rr);

                var filtLstRaw = lstRaw.Where(o => o.ContractNo == refined.contract_no).ToList();

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

                if (filtLstRaw.Count > 0)
                    if (refined.contract_no.StartsWith(ECLStringConstants.i.ExpiredContractsPrefix, StringComparison.InvariantCultureIgnoreCase) && !checkNumber)
                    {

                        var pos1 = refined.contract_no.IndexOf(' ');
                        var pos2 = refined.contract_no.IndexOf('|');
                        refined.product_type = refined.contract_no.Substring(pos1 + 1, pos2 - pos1 - 1);

                        refined.credit_limit_lcy = filtLstRaw.Sum(o => o.CreditLimit ?? 0);
                        refined.original_bal_lcy = filtLstRaw.Sum(o => o.OriginalBalanceLCY ?? 0).ToString();
                        refined.OUTSTANDING_BALANCE_LCY = filtLstRaw.Sum(o => o.OutstandingBalanceLCY ?? 0).ToString();
                    }
                    else
                    {
                        refined.segment = filtLstRaw.FirstOrDefault().Segment;
                        refined.currency = filtLstRaw.FirstOrDefault().Currency;
                        refined.product_type = filtLstRaw.FirstOrDefault().ProductType;
                        refined.credit_limit_lcy = filtLstRaw.FirstOrDefault().CreditLimit != null ? filtLstRaw.FirstOrDefault().CreditLimit : 0;
                        refined.original_bal_lcy = filtLstRaw.FirstOrDefault().OriginalBalanceLCY != null ? filtLstRaw.FirstOrDefault().OriginalBalanceLCY.ToString() : "0";
                        refined.OUTSTANDING_BALANCE_LCY = filtLstRaw.FirstOrDefault().OutstandingBalanceLCY != null ? filtLstRaw.FirstOrDefault().OutstandingBalanceLCY.ToString() : "0";
                        refined.CONTRACT_START_DATE = filtLstRaw.FirstOrDefault().ContractStartDate;
                        refined.CONTRACT_END_DATE = filtLstRaw.FirstOrDefault().ContractEndDate;
                        refined.RESTRUCTURE_INDICATOR = filtLstRaw.FirstOrDefault().RestructureIndicator ? 1 : 0;
                        refined.RESTRUCTURE_START_DATE = filtLstRaw.FirstOrDefault().RestructureStartDate;
                        refined.RESTRUCTURE_END_DATE = filtLstRaw.FirstOrDefault().RestructureEndDate;
                        refined.IPT_O_PERIOD = filtLstRaw.FirstOrDefault().IPTOPeriod.ToString();
                        refined.PRINCIPAL_PAYMENT_STRUCTURE = filtLstRaw.FirstOrDefault().PrincipalPaymentStructure;
                        refined.INTEREST_PAYMENT_STRUCTURE = filtLstRaw.FirstOrDefault().InterestPaymentStructure;
                        refined.BASE_RATE = filtLstRaw.FirstOrDefault().BaseRate.ToString();
                        refined.ORIGINATION_CONTRACTUAL_INTEREST_RATE = filtLstRaw.FirstOrDefault().OriginationContractualInterestRate;
                        refined.INTRODUCTORY_PERIOD = filtLstRaw.FirstOrDefault().IntroductoryPeriod.ToString();
                        refined.POST_IP_CONTRACTUAL_INTEREST_RATE = filtLstRaw.FirstOrDefault().PostIPContractualInterestRate != null ? filtLstRaw.FirstOrDefault().PostIPContractualInterestRate.ToString() : "0";
                        refined.INTEREST_RATE_TYPE = filtLstRaw.FirstOrDefault().InterestRateType;
                        refined.CURRENT_CONTRACTUAL_INTEREST_RATE = filtLstRaw.FirstOrDefault().CurrentContractualInterestRate != null ? filtLstRaw.FirstOrDefault().CurrentContractualInterestRate.ToString() : "0";
                        refined.EIR = filtLstRaw.FirstOrDefault().EIR != null ? filtLstRaw.FirstOrDefault().EIR.ToString() : "0";
                    }

                refineds.Add(refined);
            }


            
            return refineds;
        }



        //internal List<CoR> CalculateCoR_Main(List<LGDPrecalculationOutput> lGDPreCalc, List<Collateral> lstCollateral)
        //{
        //    var CoR_DT = new List<CoR>();
        //    LGD_Inputs inputs = new LGD_Inputs();
        //    //create temporary table
        //    //DataTable temporaryDT = new DataTable();
        //    //temporaryDT.Columns.Add(ColumnNames.debenture, typeof(string));
        //    //temporaryDT.Columns.Add(ColumnNames.cash, typeof(string));
        //    //temporaryDT.Columns.Add(ColumnNames.inventory, typeof(string));
        //    //temporaryDT.Columns.Add(ColumnNames.plant_and_equipment, typeof(string));
        //    //temporaryDT.Columns.Add(ColumnNames.residential_property, typeof(string));
        //    //temporaryDT.Columns.Add(ColumnNames.commercial_property, typeof(string));
        //    //temporaryDT.Columns.Add(ColumnNames.receivables, typeof(string));
        //    //temporaryDT.Columns.Add(ColumnNames.shares, typeof(string));
        //    //temporaryDT.Columns.Add(ColumnNames.vehicle, typeof(string));

        //    var dt = DataAccess.i.GetData("Select [collateral value] collateral_value,debenture, cash, inventory, plant_and_equipment, residential_property, commercial_property, shares, vehicle, costOfRecovery from LGD_Assumptions");

        //    var lgd_Assumptions = new List<LGD_Assumptions>();

        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        lgd_Assumptions.Add(DataAccess.i.ParseDataToObject(new LGD_Assumptions(), dr));
        //    }



        //    for (int i = 0; i < lstCollateral.Count; i++)
        //    {
        //        inputs.new_contract_no = lstCollateral[i].contract_no;
        //        inputs.debenture_omv = lstCollateral[i].debenture_omv;
        //        inputs.cash_omv = lstCollateral[i].cash_omv;
        //        inputs.inventory_omv = lstCollateral[i].inventory_omv;
        //        inputs.plant_and_equipment_omv = lstCollateral[i].plant_and_equipment_omv;
        //        inputs.residential_property_omv = lstCollateral[i].residential_property_omv;
        //        inputs.commercial_property_omv = lstCollateral[i].commercial_property_omv;
        //        inputs.shares_omv = lstCollateral[i].shares_omv;
        //        inputs.vehicle_omv = lstCollateral[i].vehicle_omv;

        //        inputs.project_finance_ind = lGDPreCalc[i].project_finance_ind;
        //        inputs.total = lstCollateral[i].total_omv;

        //        var lgd_first = lgd_Assumptions.FirstOrDefault();
        //        if (lgd_first == null) lgd_first = new LGD_Assumptions();
        //        var lgd_last = lgd_Assumptions.LastOrDefault();
        //        if (lgd_last == null) lgd_last = new LGD_Assumptions();

        //        double value_debenture = CalculateCoR(inputs.debenture_omv, lgd_first.collateral_value, lgd_first.debenture, lgd_last.debenture);
        //        double value_cash = CalculateCoR(inputs.cash_omv, lgd_first.collateral_value, lgd_first.cash, lgd_first.cash);
        //        double value_inventory = CalculateCoR(inputs.inventory_omv, lgd_first.collateral_value, lgd_first.inventory, lgd_first.inventory);
        //        double value_plant_and_equipment = CalculateCoR(inputs.plant_and_equipment_omv, lgd_first.collateral_value, lgd_first.plant_and_equipment, lgd_first.plant_and_equipment);
        //        double value_residential = CalculateCoR(inputs.residential_property_omv, lgd_first.collateral_value, lgd_first.residential_property, lgd_first.residential_property);
        //        double value_commercial = CalculateCoR(inputs.commercial_property_omv, lgd_first.collateral_value, lgd_first.commercial_property, lgd_first.commercial_property);
        //        double value_shares = CalculateCoR(inputs.shares_omv, lgd_first.collateral_value, lgd_first.shares, lgd_first.shares);
        //        double value_vehicle = CalculateCoR(inputs.vehicle_omv, lgd_first.collateral_value, lgd_first.vehicle, lgd_first.vehicle);

               

        //        ///CALCULATE Weighted Average CoR
        //        double sum = value_debenture + value_cash + value_inventory + value_plant_and_equipment + value_residential + value_commercial + value_shares + value_vehicle;
        //        double collateralValue = lgd_first.collateral_value;
        //        double[] vs = { value_debenture, value_cash, value_inventory, value_plant_and_equipment, value_residential, value_commercial, value_shares, value_vehicle };
        //        double main_value;

        //        double result = 0;
        //        //if (sum > collateralValue)
        //        if(lstCollateral[i].total_omv>collateralValue)
        //        { //////TO TRANSPOSE

        //            var lgd_filtered = lgd_Assumptions.Where(o => o.costOfRecovery.Contains("=>")).FirstOrDefault();
        //            if (lgd_filtered == null) lgd_filtered = new LGD_Assumptions();

        //            result = result + (value_debenture * lgd_filtered.debenture);
        //            result = result + (value_cash * lgd_filtered.cash);
        //            result = result + (value_inventory * lgd_filtered.inventory);
        //            result = result + (value_plant_and_equipment * lgd_filtered.plant_and_equipment);
        //            result = result + (value_residential * lgd_filtered.residential_property);
        //            result = result + (value_commercial * lgd_filtered.commercial_property);
        //            result = result + (value_shares * lgd_filtered.shares);
        //            result = result + (value_vehicle * lgd_filtered.vehicle);

        //        }
        //        else
        //        {
        //            var lgd_filtered = lgd_Assumptions.Where(o => o.costOfRecovery.Contains("<")).FirstOrDefault();
        //            if (lgd_filtered == null) lgd_filtered = new LGD_Assumptions();

        //            result = result + (value_debenture * lgd_filtered.debenture);
        //            result = result + (value_cash * lgd_filtered.cash);
        //            result = result + (value_inventory * lgd_filtered.inventory);
        //            result = result + (value_plant_and_equipment * lgd_filtered.plant_and_equipment);
        //            result = result + (value_residential * lgd_filtered.residential_property);
        //            result = result + (value_commercial * lgd_filtered.commercial_property);
        //            result = result + (value_shares * lgd_filtered.shares);
        //            result = result + (value_vehicle * lgd_filtered.vehicle);

        //        }

        //        main_value = (sum == 0) ? 0 : result / sum;
        //        double CoR;
        //        ////Calculate COR
        //        if (inputs.project_finance_ind == 1)
        //        {
        //            //AP4  - main_value
        //            CoR = main_value;
        //        }
        //        else
        //        {
        //            CoR = (inputs.total != 0) ? sum / inputs.total : 0;
        //        }

        //        CoR_DT.Add(new Models.CoR { contract_no = inputs.new_contract_no, cor = CoR });
        //    }
        //    return CoR_DT;
        //}


        internal List<CoR> CalculateCoR_Main(List<LGDPrecalculationOutput> lGDPreCalc, List<Loanbook_Data> loanbook_Data, List<Collateral> lstCollateral)
        {
            var CoR_DT = new List<CoR>();
            LGD_Inputs inputs = new LGD_Inputs();

            var dt = DataAccess.i.GetData(Queries.LGD_Assumption);

            var lgd_Assumptions = new List<LGD_Assumptions>();

            foreach (DataRow dr in dt.Rows)
            {
                lgd_Assumptions.Add(DataAccess.i.ParseDataToObject(new LGD_Assumptions(), dr));
            }


            var lgd_first = lgd_Assumptions.FirstOrDefault();
            if (lgd_first == null) lgd_first = new LGD_Assumptions();
            var lgd_last = lgd_Assumptions.LastOrDefault();
            if (lgd_last == null) lgd_last = new LGD_Assumptions();

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
                        double[] lgdAssumption = { lgd_first.debenture, lgd_first.cash, lgd_first.inventory, lgd_first.plant_and_equipment, lgd_first.residential_property, lgd_first.commercial_property, lgd_first.shares, lgd_first.vehicle };

                        if (inputs.total != 0)
                            weight_Avg_cor = SumProduct(rawData, lgdAssumption) / inputs.total;

                    }
                    else
                    {
                        //Sum product of Raw Data and LGD Assumption Second Row
                        double[] lgdAssumption = { lgd_last.debenture, lgd_last.cash, lgd_last.inventory, lgd_last.plant_and_equipment, lgd_last.residential_property, lgd_last.commercial_property, lgd_last.shares, lgd_last.vehicle };

                        if (inputs.total != 0)
                            weight_Avg_cor =SumProduct(rawData, lgdAssumption)/ inputs.total;
                    }
                    CoR_DT.Add(new CoR { contract_no = lstCollateral[i].contract_no, cor = weight_Avg_cor });
                }
                else
                {
                    double cor_debenture = CalculateCoR(lstCollateral[i].debenture_omv, lgd_first.collateral_value, lgd_first.debenture, lgd_last.debenture);
                    double cor_cash = CalculateCoR(lstCollateral[i].cash_omv, lgd_first.collateral_value, lgd_first.cash, lgd_first.cash);
                    double cor_inventory = CalculateCoR(lstCollateral[i].inventory_omv, lgd_first.collateral_value, lgd_first.inventory, lgd_first.inventory);
                    double cor_plant_and_equipment = CalculateCoR(lstCollateral[i].plant_and_equipment_omv, lgd_first.collateral_value, lgd_first.plant_and_equipment, lgd_first.plant_and_equipment);
                    double cor_residential = CalculateCoR(lstCollateral[i].residential_property_omv, lgd_first.collateral_value, lgd_first.residential_property, lgd_first.residential_property);
                    double cor_commercial = CalculateCoR(lstCollateral[i].commercial_property_omv, lgd_first.collateral_value, lgd_first.commercial_property, lgd_first.commercial_property);
                    double cor_shares = CalculateCoR(lstCollateral[i].shares_omv, lgd_first.collateral_value, lgd_first.shares, lgd_first.shares);
                    double cor_vehicle = CalculateCoR(lstCollateral[i].vehicle_omv, lgd_first.collateral_value, lgd_first.vehicle, lgd_first.vehicle);

                    double cor_sum = cor_debenture + cor_cash + cor_inventory + cor_plant_and_equipment + cor_residential + cor_commercial + cor_shares + cor_vehicle;
                    double omv_sum = lstCollateral[i].debenture_omv + lstCollateral[i].cash_omv + lstCollateral[i].inventory_omv + lstCollateral[i].plant_and_equipment_omv + lstCollateral[i].residential_property_omv + lstCollateral[i].commercial_property_omv + lstCollateral[i].shares_omv + lstCollateral[i].vehicle_omv;

                    
                        double cor_Val = 0;
                    if (omv_sum != 0 && cor_sum!=0)
                        cor_Val=cor_sum / omv_sum;

                    CoR_DT.Add(new CoR { contract_no = lstCollateral[i].contract_no, cor = cor_Val });

                }
            }

            return CoR_DT;






            //for (int i = 0; i < loanbook_Data.Count; i++)
            //{
            //    inputs.new_contract_no = loanbook_Data[i].ContractNo;
            //    inputs.debenture_omv = loanbook_Data[i].DebentureOMV??0;
            //    inputs.cash_omv = loanbook_Data[i].CashOMV ?? 0;
            //    inputs.inventory_omv = loanbook_Data[i].InventoryOMV ?? 0;
            //    inputs.plant_and_equipment_omv = loanbook_Data[i].PlantEquipmentOMV ?? 0;
            //    inputs.residential_property_omv = loanbook_Data[i].ResidentialPropertyOMV ?? 0;
            //    inputs.commercial_property_omv = loanbook_Data[i].CommercialPropertyOMV ?? 0;
            //    inputs.shares_omv = loanbook_Data[i].SharesOMV ?? 0;
            //    inputs.vehicle_omv = loanbook_Data[i].VehicleOMV ?? 0;

            //    inputs.project_finance_ind = lGDPreCalc[i].project_finance_ind;
            //    inputs.total = inputs.debenture_omv+ inputs.cash_omv+ inputs.inventory_omv+ inputs.plant_and_equipment_omv+ inputs.residential_property_omv+ inputs.commercial_property_omv+ inputs.shares_omv+ inputs.vehicle_omv;

            //    //var lgd_first = lgd_Assumptions.FirstOrDefault();
            //    //if (lgd_first == null) lgd_first = new LGD_Assumptions();
            //    //var lgd_last = lgd_Assumptions.LastOrDefault();
            //    //if (lgd_last == null) lgd_last = new LGD_Assumptions();

            //    double value_debenture = CalculateCoR(inputs.debenture_omv, lgd_first.collateral_value, lgd_first.debenture, lgd_last.debenture);
            //    double value_cash = CalculateCoR(inputs.cash_omv, lgd_first.collateral_value, lgd_first.cash, lgd_first.cash);
            //    double value_inventory = CalculateCoR(inputs.inventory_omv, lgd_first.collateral_value, lgd_first.inventory, lgd_first.inventory);
            //    double value_plant_and_equipment = CalculateCoR(inputs.plant_and_equipment_omv, lgd_first.collateral_value, lgd_first.plant_and_equipment, lgd_first.plant_and_equipment);
            //    double value_residential = CalculateCoR(inputs.residential_property_omv, lgd_first.collateral_value, lgd_first.residential_property, lgd_first.residential_property);
            //    double value_commercial = CalculateCoR(inputs.commercial_property_omv, lgd_first.collateral_value, lgd_first.commercial_property, lgd_first.commercial_property);
            //    double value_shares = CalculateCoR(inputs.shares_omv, lgd_first.collateral_value, lgd_first.shares, lgd_first.shares);
            //    double value_vehicle = CalculateCoR(inputs.vehicle_omv, lgd_first.collateral_value, lgd_first.vehicle, lgd_first.vehicle);



            //    ///CALCULATE Weighted Average CoR
            //    double sum = value_debenture + value_cash + value_inventory + value_plant_and_equipment + value_residential + value_commercial + value_shares + value_vehicle;
            //    double collateralValue = lgd_first.collateral_value;
            //    double[] vs = { value_debenture, value_cash, value_inventory, value_plant_and_equipment, value_residential, value_commercial, value_shares, value_vehicle };
            //    double main_value;

            //    double result = 0;
            //    //if (sum > collateralValue)
            //    if(loanbook_Data[i].total_omv>collateralValue)
            //    { //////TO TRANSPOSE

            //        var lgd_filtered = lgd_Assumptions.Where(o => o.costOfRecovery.Contains("=>")).FirstOrDefault();
            //        if (lgd_filtered == null) lgd_filtered = new LGD_Assumptions();

            //        result = result + (value_debenture * lgd_filtered.debenture);
            //        result = result + (value_cash * lgd_filtered.cash);
            //        result = result + (value_inventory * lgd_filtered.inventory);
            //        result = result + (value_plant_and_equipment * lgd_filtered.plant_and_equipment);
            //        result = result + (value_residential * lgd_filtered.residential_property);
            //        result = result + (value_commercial * lgd_filtered.commercial_property);
            //        result = result + (value_shares * lgd_filtered.shares);
            //        result = result + (value_vehicle * lgd_filtered.vehicle);

            //    }
            //    else
            //    {
            //        var lgd_filtered = lgd_Assumptions.Where(o => o.costOfRecovery.Contains("<")).FirstOrDefault();
            //        if (lgd_filtered == null) lgd_filtered = new LGD_Assumptions();

            //        result = result + (value_debenture * lgd_filtered.debenture);
            //        result = result + (value_cash * lgd_filtered.cash);
            //        result = result + (value_inventory * lgd_filtered.inventory);
            //        result = result + (value_plant_and_equipment * lgd_filtered.plant_and_equipment);
            //        result = result + (value_residential * lgd_filtered.residential_property);
            //        result = result + (value_commercial * lgd_filtered.commercial_property);
            //        result = result + (value_shares * lgd_filtered.shares);
            //        result = result + (value_vehicle * lgd_filtered.vehicle);

            //    }

            //    main_value = (sum == 0) ? 0 : result / sum;
            //    double CoR;
            //    ////Calculate COR
            //    if (inputs.project_finance_ind == 1)
            //    {
            //        //AP4  - main_value
            //        CoR = main_value;
            //    }
            //    else
            //    {
            //        CoR = (inputs.total != 0) ? sum / inputs.total : 0;
            //    }

            //    CoR_DT.Add(new Models.CoR { contract_no = inputs.new_contract_no, cor = CoR });
            //}
            //return CoR_DT;
        }

        public List<AccountData> AccountData(List<Loanbook_Data> refinedRawData, List<LGDPrecalculationOutput> tempDT, List<Collateral> collateralTable, List<CoR> coR)
        {
            var accountData = new List<AccountData>();

            //var dt = DataAccess.i.GetData("Select [collateral value] collateral_value,debenture, cash, inventory, plant_and_equipment, residential_property, commercial_property, shares, vehicle, costOfRecovery from LGD_Assumptions");
            var dt = DataAccess.i.GetData(Queries.LGD_Assumption_2);

            var lgd_Assumptions_2 = new List<LGD_Assumptions_2>();

            foreach (DataRow dr in dt.Rows)
            {
                lgd_Assumptions_2.Add(DataAccess.i.ParseDataToObject(new LGD_Assumptions_2(), dr));
            }


            var selection = lgd_Assumptions_2.Select(o => o.ttr_years).ToArray();

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
                accountData.Add(new AccountData { COST_OF_RECOVERY = cor_value.cor });

                double[] tempOVMarray = {
                                        refinedRawData[i].DebentureOMV??0 ,
                     refinedRawData[i].CashOMV??0 ,
                     refinedRawData[i].InventoryOMV??0 ,
                     refinedRawData[i].PlantEquipmentOMV??0 ,
                     refinedRawData[i].ResidentialPropertyOMV??0 ,
                     refinedRawData[i].CommercialPropertyOMV??0 ,
                     refinedRawData[i].ReceivablesOMV??0 ,
                     refinedRawData[i].SharesOMV??0 ,
                     refinedRawData[i].VehicleOMV??0
                };

                double valueArray2 =

                    collateralTable[i].debenture_omv+
                                        collateralTable[i].cash_omv +
                                        collateralTable[i].inventory_omv +
                                        collateralTable[i].plant_and_equipment_omv +
                                        collateralTable[i].residential_property_omv +
                                        collateralTable[i].commercial_property_omv +
                                        collateralTable[i].receivables_omv +
                                        collateralTable[i].shares_omv +
                                        collateralTable[i].vehicle_omv;
                    

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
                    refinedRawData[i].GuarantorPD = refinedRawData[i].GuarantorPD ?? 0;
                    accountData[i].GUARANTOR_PD = refinedRawData[i].GuarantorPD.Value;

                    refinedRawData[i].GuarantorLGD = refinedRawData[i].GuarantorLGD ?? 0;
                    accountData[i].GUARANTOR_LGD = refinedRawData[i].GuarantorLGD.Value;
                     
                    value1 = refinedRawData[i].GuaranteeIndicator?tempDT[i].pd_x_ead:0;

                    var pd_x_ead_List = tempDT.Select(o=>o.pd_x_ead).ToArray();

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
        private static double CalculateCoR(double inputs, double collateralValue, double lgd_Assumption_first, double lgd_Assumption_last)
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
        internal List<Collateral> Collateral_OMV_FSV(List<Loanbook_Data> lstRaw, List<LGDPrecalculationOutput> lGDPreCalc)
        {
            var collaterals= new List<Collateral>();
            LGD_Inputs input = new LGD_Inputs();

            var pd_x_ead_List = lGDPreCalc.Select(O => O.pd_x_ead).ToList();

            //calculate the value for Debenture_OMV
            //foreach (var itm in lstRaw)
                for(int i=0; i< lstRaw.Count; i++)
            {
                var collateralTable = new Collateral();

                input.debenture_omv = lstRaw[i].DebentureOMV??0;
                input.cash_omv = lstRaw[i].CashOMV ?? 0;
                input.inventory_omv = lstRaw[i].InventoryOMV??0;
                input.plant_and_equipment_omv = lstRaw[i].PlantEquipmentOMV ?? 0;
                input.residential_property_omv = lstRaw[i].ResidentialPropertyOMV ?? 0;
                input.commercial_property_omv = lstRaw[i].CommercialPropertyOMV??0;
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
                input.contractid = lstRaw[i].ContractNo;
                input.account_no = lstRaw[i].AccountNo;

                input.pd_x_ead = pd_x_ead_List[i];


                //lGDPreCalc = GetValue(lstRaw, lGDPreCalc, input.debenture);


                collateralTable.contract_no = input.contractid;
                collateralTable.customer_no = input.customer_no;
                        collateralTable.debenture_omv=0;
        collateralTable.cash_omv=0;
        collateralTable.inventory_omv=0;
        collateralTable.plant_and_equipment_omv=0;
        collateralTable.residential_property_omv=0;
        collateralTable.commercial_property_omv=0;
        collateralTable.receivables_omv=0;
        collateralTable.shares_omv=0;
        collateralTable.vehicle_omv=0;
        collateralTable.total_omv=0;
        collateralTable.debenture_fsv=0;
        collateralTable.cash_fsv=0;
        collateralTable.inventory_fsv=0;
        collateralTable.plant_and_equipment_fsv=0;
        collateralTable.residential_property_fsv=0;
        collateralTable.commercial_property_fsv=0;
        collateralTable.receivables_fsv=0;
        collateralTable.shares_fsv=0;
        collateralTable.vehicle_fsv=0;



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

                foreach(var fin_itm in lstProject_Finance_Ind)
                {
                    ProjectFinance_array.Add(fin_itm == 0 ? 1:0);
                }

                //var dictionaryData_fsv = GetArrayRawData_Fsv(lstRaw, input);



                //collateralTable.contract_no = input.customer_no;


                collateralTable = SumProduct(pd_x_ead_List, collateralTable, Debenture_Omv_array, Cash_Omv_array, Inventory_Omv_array, Plant_Equipment_Omv_array, Residential_Omv_array, Commercial_Omv_array, Receivables_Omv_array, Shares_Omv_array, Vehicle_Omv_array, Debenture_Fsv_array, Cash_Fsv_array, Inventory_Fsv_array, Plant_Equipment_Fsv_array, Residential_Fsv_array, Commercial_Fsv_array, Receivables_Fsv_array, Shares_Fsv_array, Vehicle_Fsv_array, CustomerNo_array, ProjectFinance_array, input);


                //var product = SumProduct(pd_x_ead_List, Debenture_Omv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.debenture_omv = 0;
                //if (product != 0)
                //    collateralTable.debenture_omv = (input.debenture * input.pd_x_ead * (1 - input.project_finance_ind)) / product;
                

                //product = SumProduct(pd_x_ead_List, Cash_Omv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.cash_omv = 0;
                //if (product != 0)
                //    collateralTable.cash_omv = (input.cash * input.pd_x_ead * (1 - input.project_finance_ind)) / product;
                
                
                //product = SumProduct(pd_x_ead_List, Inventory_Omv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.inventory_omv = 0;
                //if (product != 0)
                //    collateralTable.inventory_omv = (input.inventory * input.pd_x_ead * (1 - input.project_finance_ind)) / product;
                

                //product = SumProduct(pd_x_ead_List, Plant_Equipment_Omv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.plant_and_equipment_omv = 0;
                //if (product != 0)
                //    collateralTable.plant_and_equipment_omv = (input.plant_and_equipment * input.pd_x_ead * (1 - input.project_finance_ind)) / product;
                
              

                //product = SumProduct(pd_x_ead_List, Residential_Omv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.residential_property_omv = 0;
                //if (product != 0)
                //    collateralTable.residential_property_omv = (input.residential_property * input.pd_x_ead * (1 - input.project_finance_ind)) / product;


                //product = SumProduct(pd_x_ead_List, Commercial_Omv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.commercial_property_omv = 0;
                //if (product != 0)
                //    collateralTable.commercial_property_omv = (input.commercial_property * input.pd_x_ead * (1 - input.project_finance_ind)) / product;

                //product = SumProduct(pd_x_ead_List, Receivables_Omv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.receivables_omv = 0;
                //if (product != 0)
                //    collateralTable.receivables_omv = (input.receivables * input.pd_x_ead * (1 - input.project_finance_ind)) / product;

                //product = SumProduct(pd_x_ead_List, Shares_Omv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.shares_omv = 0;
                //if (product != 0)
                //    collateralTable.shares_omv = (input.shares * input.pd_x_ead * (1 - input.project_finance_ind)) / product;


                //product = SumProduct(pd_x_ead_List, Vehicle_Omv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.vehicle_omv = 0;
                //if (product != 0)
                //    collateralTable.vehicle_omv = (input.vehicle * input.pd_x_ead * (1 - input.project_finance_ind)) / product;



                //collateralTable.total_omv = collateralTable.debenture_omv +
                //                                                 collateralTable.cash_omv+
                //                                                 collateralTable.inventory_omv+
                //                                                 collateralTable.plant_and_equipment_omv+
                //                                                 collateralTable.residential_property_omv+
                //                                                 collateralTable.commercial_property_omv+
                //                                                 collateralTable.receivables_omv+
                //                                                 collateralTable.shares_omv+
                //                                                 collateralTable.vehicle_omv;



               
                //product = SumProduct(pd_x_ead_List, Debenture_Fsv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.debenture_fsv = 0;
                //if (product != 0)
                //    collateralTable.debenture_fsv = (input.debenture * input.pd_x_ead * (1 - input.project_finance_ind)) / product;


                //product = SumProduct(pd_x_ead_List, Cash_Fsv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.cash_fsv = 0;
                //if (product != 0)
                //    collateralTable.cash_fsv = (input.cash * input.pd_x_ead * (1 - input.project_finance_ind)) / product;


                //product = SumProduct(pd_x_ead_List, Inventory_Fsv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.inventory_fsv = 0;
                //if (product != 0)
                //    collateralTable.inventory_fsv = (input.inventory * input.pd_x_ead * (1 - input.project_finance_ind)) / product;


                //product = SumProduct(pd_x_ead_List, Plant_Equipment_Fsv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.plant_and_equipment_fsv = 0;
                //if (product != 0)
                //    collateralTable.plant_and_equipment_fsv = (input.plant_and_equipment * input.pd_x_ead * (1 - input.project_finance_ind)) / product;



                //product = SumProduct(pd_x_ead_List, Residential_Fsv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.residential_property_fsv = 0;
                //if (product != 0)
                //    collateralTable.residential_property_fsv = (input.residential_property * input.pd_x_ead * (1 - input.project_finance_ind)) / product;


                //product = SumProduct(pd_x_ead_List, Commercial_Fsv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.commercial_property_fsv = 0;
                //if (product != 0)
                //    collateralTable.commercial_property_fsv = (input.commercial_property * input.pd_x_ead * (1 - input.project_finance_ind)) / product;

                //product = SumProduct(pd_x_ead_List, Receivables_Fsv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.receivables_fsv = 0;
                //if (product != 0)
                //    collateralTable.receivables_fsv = (input.receivables * input.pd_x_ead * (1 - input.project_finance_ind)) / product;


                //product = SumProduct(pd_x_ead_List, Shares_Fsv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.shares_fsv = 0;
                //if (product != 0)
                //    collateralTable.shares_fsv = (input.shares * input.pd_x_ead * (1 - input.project_finance_ind)) / product;


                //product = SumProduct(pd_x_ead_List, Vehicle_Fsv_array, CustomerNo_array, ProjectFinance_array);
                //collateralTable.vehicle_fsv = 0;
                //if (product != 0)
                //    collateralTable.vehicle_fsv = (input.vehicle * input.pd_x_ead * (1 - input.project_finance_ind)) / product;

                collaterals.Add(collateralTable);
            }
            return collaterals;
            
        }

        private Collateral SumProduct(List<double> pd_x_ead_List, Collateral collateralTable, List<int> debenture_Omv_array, List<int> cash_Omv_array, List<int> inventory_Omv_array, List<int> plant_Equipment_Omv_array, List<int> residential_Omv_array, List<int> commercial_Omv_array, List<int> receivables_Omv_array, List<int> shares_Omv_array, List<int> vehicle_Omv_array, List<int> debenture_Fsv_array, List<int> cash_Fsv_array, List<int> inventory_Fsv_array, List<int> plant_Equipment_Fsv_array, List<int> residential_Fsv_array, List<int> commercial_Fsv_array, List<int> receivables_Fsv_array, List<int> shares_Fsv_array, List<int> vehicle_Fsv_array, List<int> customerNo_array, List<int> projectFinance_array, LGD_Inputs inputs)
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

        //private Dictionary<string, List<int>> GetArrayRawData_Fsv(List<Loanbook_Data> lstRaw, LGD_Inputs input)
        //{
        //    var Debenture_Fsv_array = new List<int>();
        //    var Cash_Fsv_array = new List<int>();
        //    var Inventory_Fsv_array = new List<int>();
        //    var Plant_Equipment_array = new List<int>();
        //    var Residential_array = new List<int>();
        //    var Commercial_array = new List<int>();
        //    var Receivables_array = new List<int>();
        //    var Shares_array = new List<int>();
        //    var Vehicle_array = new List<int>();
        //    var CustomerNo_array = new List<int>();

        //    ////var ProjectFinance_array = new List<string>();
        //    ////var projectFinance_raw_lst = lstRaw.Select(x => x.project_finance_ind).ToList();


        //    foreach (var item in lstRaw)
        //    {
        //        Debenture_Fsv_array.Add(item.DebentureFSV == input.debenture_omv ? 1 : 0);
        //        Cash_Fsv_array.Add(item.CashFSV == input.cash_omv ? 1 : 0);
        //        Inventory_Fsv_array.Add(item.InventoryFSV == input.inventory_omv ? 1 : 0);
        //        Plant_Equipment_array.Add(item.PlantEquipmentFSV == input.plant_and_equipment_omv ? 1 : 0);
        //        Residential_array.Add(item.ResidentialPropertyFSV == input.residential_property_omv ? 1 : 0);
        //        Commercial_array.Add(item.CommercialProperty == input.commercial_property_omv ? 1 : 0);
        //        Receivables_array.Add(item.ReceivablesFSV == input.receivables_omv ? 1 : 0);
        //        Shares_array.Add(item.SharesFSV == input.shares_omv ? 1 : 0);
        //        Vehicle_array.Add(item.VehicleFSV == input.vehicle_omv ? 1 : 0);
        //        CustomerNo_array.Add(item.CustomerNo == input.customer_no ? 1 : 0);
        //    }

        //    var dic = new Dictionary<string, List<int>>();
        //    dic.Add(ECLStringConstants.i.Debenture_array, Debenture_Fsv_array);
        //    dic.Add(ECLStringConstants.i.Cash_array, Cash_Fsv_array);
        //    dic.Add(ECLStringConstants.i.Inventory_array, Inventory_Fsv_array);
        //    dic.Add(ECLStringConstants.i.Plant_Equipment_array, Plant_Equipment_array);
        //    dic.Add(ECLStringConstants.i.Residential_array, Residential_array);
        //    dic.Add(ECLStringConstants.i.Commercial_array, Commercial_array);
        //    dic.Add(ECLStringConstants.i.Receivables_array, Receivables_array);
        //    dic.Add(ECLStringConstants.i.Shares_array, Shares_array);
        //    dic.Add(ECLStringConstants.i.Vehicle_array, Vehicle_array);
        //    dic.Add(ECLStringConstants.i.CustomerNo_array, CustomerNo_array);

        //    return dic;
        //}


        internal List<LifeTimeProjections> EAD_LifeTimeProjections(List<Refined_Raw_Retail_Wholesale> refined_lstRaw, List<LifeTimeEADs> lifeTimeEAD_w, List<string> lstContractIds, List<CIRProjections> cirProjections, List<PaymentSchedule> paymentScheduleProjection)
        {
            var lifetimeEadInputs = new List<LifeTimeProjections>();

            foreach (var contract in lstContractIds)
            {

                var lifetime_query = lifeTimeEAD_w.FirstOrDefault(o => o.contract_no == contract);

                string eir_group_value = lifetime_query.eir_base_premium;
                string cir_group_value = lifetime_query.cir_base_premium;

                //Perform Projections
                double noOfMonths = 0;
                if (lifetime_query.end_date != null)
                {
                    try
                    {
                        var maximumDate = DateTime.Parse(lifetime_query.end_date);
                        double noOfDays = (maximumDate - ECLNonStringConstants.i.reportingDate).Days;
                        noOfMonths = Math.Ceiling(noOfDays * 12 / 365);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }



                var refined_query = refined_lstRaw.FirstOrDefault(o => o.contract_no == contract);
                refined_query.credit_limit_lcy = refined_query.credit_limit_lcy ?? 0;
                refined_query.OUTSTANDING_BALANCE_LCY = refined_query.OUTSTANDING_BALANCE_LCY ?? "0";
                lifetime_query.mths_in_force = !string.IsNullOrEmpty(lifetime_query.mths_in_force) ? lifetime_query.mths_in_force : "0";
                lifetime_query.mths_to_expiry = !string.IsNullOrEmpty(lifetime_query.mths_to_expiry) ? lifetime_query.mths_to_expiry : "0";
                lifetime_query.first_interest_month = !string.IsNullOrEmpty(lifetime_query.first_interest_month) ? lifetime_query.first_interest_month : "0";
                lifetime_query.rem_interest_moritorium = !string.IsNullOrEmpty(lifetime_query.rem_interest_moritorium) ? lifetime_query.first_interest_month : "0";
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
                noOfMonths = 1;
                for (int monthIndex = 0; monthIndex <= noOfMonths; monthIndex++)
                {
                    if (monthIndex == 0)
                    {
                        double value = projection_Calulcation_lifetimeEAD_0(obj.outstanding_balance_lcy, obj.product_type);

                        lifetimeEadInputs.Add(new LifeTimeProjections { contract_no = contract, eir_group = eir_group_value, cir_group = cir_group_value, months = monthIndex, value = value });

                    }
                    else
                    {
                        double overallvalue = 0, value1, value2;
                        double previousMonth = lifetimeEadInputs.FirstOrDefault(o => o.months == (monthIndex - 1) && o.contract_no == contract).value;


                        if (obj.product_type != ECLStringConstants.i._productType_loan && obj.product_type != ECLStringConstants.i._productType_lease & obj.product_type != ECLStringConstants.i._productType_mortgage)
                        {
                            if (monthIndex <= obj.months_to_expiry)
                            {
                                if (obj.segment == ECLStringConstants.i._corporate)
                                {
                                    value1 = obj.outstanding_balance_lcy + Math.Max((obj.credit_limit_lcy - obj.outstanding_balance_lcy) * ECLNonStringConstants.i.Corporate, 0);
                                }
                                else if (obj.segment == ECLStringConstants.i._consumer)
                                {
                                    value1 = obj.outstanding_balance_lcy + Math.Max((obj.credit_limit_lcy - obj.outstanding_balance_lcy) * Convert.ToDouble(ECLNonStringConstants.i.Consumer), 0);
                                }
                                else if (obj.segment == ECLStringConstants.i._commercial)
                                {
                                    value1 = obj.outstanding_balance_lcy + Math.Max((obj.credit_limit_lcy - obj.outstanding_balance_lcy) * Convert.ToDouble(ECLNonStringConstants.i.Commercial), 0);
                                }
                                else //OBE
                                {
                                    value1 = obj.outstanding_balance_lcy + Math.Max((obj.credit_limit_lcy - obj.outstanding_balance_lcy) * Convert.ToDouble(ECLNonStringConstants.i.Corporate), 0);
                                }

                                if (obj.product_type != ECLStringConstants.i._productType_od && obj.product_type != ECLStringConstants.i._productType_card)
                                {
                                    value2 = Convert.ToDouble(Conversion_Factor_OBE);
                                }
                                else
                                {
                                    value2 = 1;
                                }

                                overallvalue = value1 * value2;
                            }
                        }
                        else
                        {
                            var ps_proj = paymentScheduleProjection.FirstOrDefault(o => o.CONTRACT_ID == contract);
                            string component;
                            if (ps_proj!=null)
                            {
                                component = ps_proj.PAYMENT_TYPE;
                                //this should be obtained from the payment schedule

                                double d_value;

                                if (monthIndex <= obj.months_to_expiry)
                                {
                                    double c_value = cirProjections.FirstOrDefault(o => o.cir_group == cir_group_value && o.months == monthIndex).cir_effective;

                                    if (component != ECLStringConstants.i._amortise)
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


                                    //obtain value from payment schedule and multiply by exchange rate
                                    double f_value = paymentScheduleProjection.FirstOrDefault(o => o.CONTRACT_ID == contract && o.MONTHS == monthIndex.ToString()).VALUE * ECLNonStringConstants.i.NGN_Currency;
                                    //NGN_Currency will be obtained from the DB
                                    //(f_value + x)

                                    double g_value = 0;
                                    if (obj.interest_divisor == ECLStringConstants.i._interestDivisior)
                                    {
                                        //x = ($H4=T$3)*SUMPRODUCT(OFFSET(T4, 0, -1, 1, -T$3), OFFSET(CIR_EFF_MONTHLY_RANGE, $M4-1, T$3, 1, -T$3))*($H4+$G4)/T$3
                                        if (obj.months_to_expiry == monthIndex)
                                        {
                                            //get range
                                            double[] h_value = lifetimeEadInputs.Where(o => o.contract_no == contract
                                                                            && o.months >= 0
                                                                            && o.months <= monthIndex)
                                                                            .Select(x => x.value)
                                                                            .ToArray();
                                            double[] i_value = cirProjections.Where(o => o.cir_group == cir_group_value
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
                                        var l_value = (monthIndex - double.Parse(obj.first_interest_month)) % Convert.ToDouble(obj.interest_divisor);
                                        var m_value = (monthIndex > obj.rem_interest_moritorium) ? 1 : 0;

                                        double n_value = k_value * l_value * m_value;
                                        double o_value;
                                        double[] p_value = lifetimeEadInputs.Where(o => o.contract_no == contract
                                                                            && o.months >= 0
                                                                            && o.months <= monthIndex)
                                                                            .Select(x => x.value)
                                                                            .ToArray();
                                        double[] i_value = cirProjections.Where(o => o.cir_group == cir_group_value
                                                                        && o.months >= 0
                                                                        && o.months <= monthIndex)
                                                                        .Select(x => x.value)
                                                                        .ToArray();
                                        if (monthIndex < Convert.ToDouble(obj.interest_divisor))
                                        {
                                            //o = SUMPRODUCT(OFFSET(T4, 0, -1, 1, -T$3), OFFSET(CIR_EFF_MONTHLY_RANGE, $M4-1, T$3, 1, -T$3))*$I4/T$3
                                            o_value = SumProduct(p_value, i_value) * (Convert.ToDouble(obj.interest_divisor) / monthIndex);
                                        }
                                        else
                                        {
                                            //o = SUMPRODUCT(OFFSET(T4, 0, -1, 1, -T$3), OFFSET(CIR_EFF_MONTHLY_RANGE, $M4-1, T$3, 1, -T$3))
                                            o_value = SumProduct(p_value, i_value);
                                        }
                                        //x = r * o
                                        g_value = n_value * o_value;
                                    }

                                    f_value += g_value;

                                    overallvalue = Math.Max(f_value, 0) * (1 - ECLNonStringConstants.i.prepaymentFactor);
                                }
                            }

                            else
                            {
                                overallvalue = 0;
                            }
                        }

                        lifetimeEadInputs.Add(new LifeTimeProjections { contract_no = contract, eir_group = eir_group_value, cir_group = cir_group_value, months = monthIndex, value = overallvalue });
                    }
                }

            }

            return lifetimeEadInputs;
        }

        private double SumProduct(double[] arrayA, double[] arrayB)
        {
            double result = 0;

            for (int i = 0; i < arrayA.Length; i++)
            {
                result += arrayA[i] * arrayB[i];
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
            double value;
            if (product_type != ECLStringConstants.i._productType_loan && product_type != ECLStringConstants.i._productType_lease && product_type != ECLStringConstants.i._productType_mortgage && product_type != ECLStringConstants.i._productType_od && product_type != ECLStringConstants.i._productType_card)
            {
                value = Conversion_Factor_OBE;
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

            if (r.ContractStartDate == null && r.ContractEndDate == null && r.CreditLimit == 0 && r.CreditLimit == 0)
            {
                var colSum = r.DebentureOMV ?? +r.CashOMV ?? +r.InventoryOMV ?? +r.PlantEquipmentOMV ?? +r.ResidentialPropertyOMV ?? +r.CommercialPropertyOMV ?? +r.ReceivablesOMV ?? +r.SharesOMV ?? +r.VehicleOMV ?? +(r.GuaranteeIndicator ? 1 : 0);
                return colSum == 0 ? $"{ECLStringConstants.i.ExpiredContractsPrefix}{r.ProductType}|{r.Segment}" : $"{ECLStringConstants.i.ExpiredContractsPrefix}{r.ProductType}|{r.ContractNo}";
            }
            else
            {
                return r.ContractNo;
            }
            
        }

        internal List<EIRProjections> EAD_EIRProjections(List<LifeTimeEADs> lifeTimeEAD, List<string> lstContractIds)
        {
            var rs = new List<EIRProjections>();

            foreach (var crctId in lstContractIds)
            {
                var _ltEAD = lifeTimeEAD.FirstOrDefault(o => o.contract_no == crctId);
                var group_value=_ltEAD.eir_base_premium;

                //Perform Projections
                double noOfMonths = 0;
                if (_ltEAD.end_date!=null)
                {
                    try { 
                    var maximumDate = DateTime.Parse(_ltEAD.end_date);
                    double noOfDays = (maximumDate - ECLNonStringConstants.i.reportingDate).Days;
                    noOfMonths = Math.Ceiling(noOfDays * 12 / 365);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                //noOfMonths reset to one because value is same accross board (as adviced by Femi Longe)
                noOfMonths = 1;
                for (int mnthIdx = 0; mnthIdx < noOfMonths; mnthIdx++)
                {
                    var val = 0.0;
                    if(group_value!=ECLStringConstants.i.ExpiredContractsPrefix)
                    {
                        var temp = group_value.Split(ECLStringConstants.i._splitValue);
                        if (temp[1] != ECLStringConstants.i._fixed)
                        {
                            val = Math.Round((Convert.ToDouble(ECLNonStringConstants.i.virProjections) * 100) + Convert.ToDouble(temp[2].Substring(0, temp[2].Length - 1)), 1) / 100;
                        }
                        else
                        {
                            val = Convert.ToDouble(ECLNonStringConstants.i.virProjections) / 100;
                        }
                    }

                    //calculate the eir effective
                        double effectiveValue, power = Convert.ToDouble(1m / 12m);
                        effectiveValue = Math.Pow(1 + val, power) - 1;
                        rs.Add(new EIRProjections { eir_group= group_value, months= mnthIdx, value=val });
                }
            }
            return rs;
        }

        internal List<CIRProjections> EAD_CIRProjections(List<LifeTimeEADs> lifeTimeEAD, List<string> lstContractIds)
        {
            var rs = new List<CIRProjections>();

            foreach (var crctId in lstContractIds)
            {
                var _ltEAD = lifeTimeEAD.FirstOrDefault(o => o.contract_no == crctId);
                var group_value = _ltEAD.cir_base_premium;

                //Perform Projections
                double noOfMonths = 0;
                if (_ltEAD.end_date != null)
                {
                    try
                    {
                        var maximumDate = DateTime.Parse(_ltEAD.end_date);
                        double noOfDays = (maximumDate - ECLNonStringConstants.i.reportingDate).Days;
                        noOfMonths = Math.Ceiling(noOfDays * 12 / 365);
                    }catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }


                //noOfMonths reset to one because value is same accross board (as adviced by Femi Longe)
                noOfMonths = 1;
                for (int mnthIdx = 0; mnthIdx < noOfMonths; mnthIdx++)
                {
                    var val = 0.0;
                    if (group_value != ECLStringConstants.i.ExpiredContractsPrefix)
                    {
                        var temp = group_value.Split(ECLStringConstants.i._splitValue);
                        if (temp[1] != ECLStringConstants.i._fixed)
                        {
                            val = Math.Round((Convert.ToDouble(ECLNonStringConstants.i.virProjections) * 100) + Convert.ToDouble(temp[2].Substring(0, temp[2].Length - 1)), 1) / 100;
                        }
                        else
                        {
                            val = Convert.ToDouble(ECLNonStringConstants.i.virProjections) / 100;
                        }
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

            foreach (var i in r_lst)
            {

                var r = new LifeTimeEADs();

                r.contract_no = i.contract_no;
                r.segment = i.segment;
                r.credit_limit_lcy = i.credit_limit_lcy != null ? i.credit_limit_lcy.Value.ToString() : "0";
                r.start_date = S_E_Date(i.RESTRUCTURE_START_DATE.ToString(), i.RESTRUCTURE_INDICATOR.ToString(), i.CONTRACT_START_DATE.ToString());

                ///end date
                r.end_date = S_E_Date(i.RESTRUCTURE_END_DATE.ToString(), i.RESTRUCTURE_INDICATOR.ToString(), i.CONTRACT_END_DATE.ToString());

                //remaining IP
                r.remaining_ip = Remaining_IP(i, ECLNonStringConstants.i.reportingDate).ToString();

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
                    if (Convert.ToDateTime(r.start_date)< Convert.ToDateTime(r.end_date))
                        r.mths_in_force = Math.Round(Financial.YearFrac(Convert.ToDateTime(r.start_date), Convert.ToDateTime(r.end_date), DayCountBasis.ActualActual) * 12, 0).ToString();

                    r.rem_interest_moritorium = Remaining_IR(i.IPT_O_PERIOD, r.mths_in_force).ToString();

                    r.mths_to_expiry = Months_To_Expiry(ECLNonStringConstants.i.reportingDate, Convert.ToDateTime(r.end_date), i.product_type).ToString();

                    r.interest_divisor = Interest_Divisor(i.INTEREST_PAYMENT_STRUCTURE);

                    string interest_divisor = r.interest_divisor;
                    double mths_to_expiry = Convert.ToDouble(r.mths_to_expiry);
                    double rem_interest_moritorium = Convert.ToDouble(r.rem_interest_moritorium);
                    double mths_in_force = Convert.ToDouble(r.mths_in_force);
                    double ipt_o_period = Convert.ToDouble(i.IPT_O_PERIOD);
                    r.first_interest_month = First_Interest_Month(interest_divisor, mths_to_expiry, rem_interest_moritorium, mths_in_force, ipt_o_period).ToString();

                }
                rs.Add(r);
            }
            return rs;
        }

        private string Revised_Base(string interest_rate_type, string base_rate)
        {
            string value = string.Empty;
            if (interest_rate_type != ECLStringConstants.i.FLOATING)
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
            double value2;

            if (L_revisedBase != ECLStringConstants.i._fixed)
            {
                value2 = Convert.ToDouble(ECLNonStringConstants.i.virProjections);
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

        private string S_E_Date(string restructure_dt, string restructure_indicator, string contract_dt)
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
            return value;
        }



        private static string CIR_Base_Premium(string remaining_ip, string orig_contractual_ir, string revised_base, string cir_premium)
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

        private static string EIR_Base_Premium(string revised_base, string eir_premium)
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

        private static double Remaining_IR(string ipt_o_period, string mths_in_force)
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

        private double Months_To_Expiry(DateTime reportingDate, DateTime endDate, string productType)
        {
            double value = 0;
            if (endDate < reportingDate || productType == "OD" || productType == "CARD")
            {
                DateTime EOM = EndOfMonth(endDate, Convert.ToInt32(ECLNonStringConstants.i.Expired));
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
                DateTime EOM = EndOfMonth(endDate, Convert.ToInt32(ECLNonStringConstants.i.Expired));
                if (productType == ECLStringConstants.i.ID || productType == ECLStringConstants.i.CARDS)
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
            DateTime startOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(numberOfMonths).AddDays(-1);
            return endOfMonth;
        }

        public List<LGDPrecalculationOutput> LGDPreCalculation(List<Loanbook_Data> lstRaw)
        {
            var input = new LGD_Inputs();
            ////This will be obtained from the DB
            var qry=Queries.LGD_PD_AssumptionSelectQry;

            var _pd_assumptions = DataAccess.i.GetData(qry);

            var pd_assumptions = new List<LGD_PD_Assumptions>();
            foreach (DataRow dr in _pd_assumptions.Rows)
            {
                pd_assumptions.Add(DataAccess.i.ParseDataToObject(new LGD_PD_Assumptions(), dr));
            }

            List<double?> outstanding_Bal_Lcy_array = lstRaw.Where(n => n.OutstandingBalanceLCY != null).Select(o=>o.OutstandingBalanceLCY).Distinct().ToList();
            List<string> ContractID_list = lstRaw.Where(n=>n.ContractNo!=null).Select(o=>o.ContractNo).Distinct().ToList();
            

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

            foreach (var itm in lstRaw)
            {
                input.new_contract_no = itm.ContractNo;
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
                itm.CurrentRating = itm.CurrentRating ?? 0;
                itm.DaysPastDue = itm.DaysPastDue ?? 0;

                input.customer_no = itm.CustomerNo;
                input.product_type = itm.ProductType;
                input.new_contract_no = itm.ContractNo;
                input.restructure_indicator = itm.RestructureIndicator;
                input.restructure_end_date = itm.RestructureEndDate;
                input.contract_end_date = itm.ContractEndDate;
                input.rating_model = itm.RatingModel;
                input.segment = itm.Segment;
                input.days_past_due = itm.DaysPastDue.Value;
                input.current_rating = itm.CurrentRating.Value;
                input.specialised_lending = itm.SpecialisedLending;
                var check_customer = itm.CustomerNo;

                input.rating_used = input.current_rating > 10? input.current_rating.ToString().Substring(0, 1) : input.current_rating.ToString();

                var ttm_months = TTM_Inputs(input);
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

        public List<PaymentSchedule> PaymentSchedule_Projection(List<PaymentSchedule> ps, List<string> ps_contract_ref_no)
        {
            var _ps = new List<PaymentSchedule>();

            int wholeIndex = 0;
            foreach(var refNo in ps_contract_ref_no)
            {
                var contractblock = ps.Where(o => o.CONTRACT_REF_NO == refNo).ToList();
                bool start_month_adjustment = false;
                int frequency_factor;
                int no_schedules;
                double amount;
                DateTime start_date;
                double start_month = 0;
                double start_schedule;
                int monthIndex = 1;

                //Determine frequency factor
                foreach (var item in contractblock)
                {
                    string frequency = item.FREQUENCY.Trim();
                    if (ECLScheduleConstants.Bullet == frequency)
                    {
                        frequency_factor = 0;
                    }
                    else if (ECLScheduleConstants.Monthly == frequency)
                    {
                        frequency_factor = ECLScheduleConstants.Monthly_number;
                    }
                    else if (ECLScheduleConstants.Quarterly == frequency)
                    {
                        frequency_factor = ECLScheduleConstants.Quarterly_number;
                    }
                    else if (ECLScheduleConstants.Yearly == frequency)
                    {
                        frequency_factor = ECLScheduleConstants.Yearly_number;
                    }
                    else if (ECLScheduleConstants.HalfYear == frequency)
                    {
                        frequency_factor = ECLScheduleConstants.HalfYear_number;
                    }
                    else
                    {
                        frequency_factor = 0;
                    }

                    //Run through each schedule
                    no_schedules = item.NO_OF_SCHEDULES;

                    //set amount
                    amount = item.AMOUNT;

                    //Determine the rounded months from the report date at which the entry starts.
                    //Allowed for this to be negative. This will be used later.
                    start_date = item.START_DATE;

                    if (start_date > ECLNonStringConstants.i.reportingDate)
                    {
                        if (!start_month_adjustment)
                        {
                            start_month = Math.Round(Financial.YearFrac(ECLNonStringConstants.i.reportingDate, start_date, DayCountBasis.ActualActual) * 12, 0);
                            if (start_month == 0)
                            {
                                start_month_adjustment = true;
                            }
                        }
                        if (start_month_adjustment)
                        {
                            var start_date_ = EndOfMonth(start_date, 0);
                            if (ECLNonStringConstants.i.reportingDate < start_date_)
                            {
                                start_month = Math.Round(Financial.YearFrac(ECLNonStringConstants.i.reportingDate, start_date_, DayCountBasis.ActualActual) * 12, 0);
                            }
                            else
                            {
                                start_month = 0;
                            }
                            
                        }
                        start_schedule = 0;
                    }
                    else
                    {
                        //'Set negative number of months if the payment entry started in the past. If it is a bullet payment entry it should not pull through.
                        if (start_date>ECLNonStringConstants.i.reportingDate)
                        {
                            start_month = -1 * Math.Round(Financial.YearFrac(start_date, ECLNonStringConstants.i.reportingDate, DayCountBasis.ActualActual) * 12, 0);
                        }
                        else
                        {
                            start_month = 0;
                        }
                        
                        if (frequency_factor != 0)
                        {
                            var w = (-start_month + 1) / frequency_factor;
                            start_schedule = Math.Ceiling(w);
                        }
                        else
                        {
                            start_schedule = no_schedules;
                            //This way if the schedule entry is a bullet payment before the reporting date the function will not step into the loop.
                            //The +1 is to allow for the current months payment.
                        }
                    }

                    

                    //'Check whether the last schedule in this entry is more months from the reporting date than the max_ttm derived from the loan book snapshot.
                    for (double schedule = start_schedule; schedule <= no_schedules - 1; schedule++)
                    {// Assume advance from start date.
                        _ps.Add(new PaymentSchedule { CONTRACT_ID=item.CONTRACT_ID, PAYMENT_TYPE=item.PAYMENT_TYPE, MONTHS=monthIndex.ToString(), VALUE=amount });

                        wholeIndex++;
                        monthIndex++;
                    }
                }
            }
            return _ps;
        }

        private double TTM_Inputs(LGD_Inputs input)
        {
            double ttm_months=0;
            if (input.new_contract_no.Contains(ECLStringConstants.i.ExpiredContractsPrefix))
            {
                return 0;
            }
            else
            {
                long longDate = 0;
                long reportDate = ConvertToTimeStamp(ECLNonStringConstants.i.reportingDate);
                double temp_value1 = 0;

                if (input.restructure_indicator && input.restructure_end_date!=null)
                {
                    var temp_value = input.restructure_end_date.Value;
                    longDate=ConvertToTimeStamp(temp_value);

                    if (longDate > reportDate)
                    {
                        temp_value1 = Math.Floor(Financial.YearFrac(ECLNonStringConstants.i.reportingDate, temp_value, 0) * 12);
                    }
                }
                else if (input.contract_end_date != null)
                {
                    var temp_value = input.contract_end_date.Value;
                    longDate = ConvertToTimeStamp(temp_value);

                    if (longDate > reportDate)
                    {
                        temp_value1 = Math.Floor(Financial.YearFrac(ECLNonStringConstants.i.reportingDate, temp_value, 0) * 12);
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
                            temp_value2 = ECLNonStringConstants.i.Expired - Math.Floor(Financial.YearFrac(temp_value, ECLNonStringConstants.i.reportingDate, 0) * 12);
                            //temp_value2 = Convert.ToDouble(Expired);
                        }
                        else
                        {
                            temp_value2 = ECLNonStringConstants.i.Non_Expired;
                        }
                    }
                    else if (input.contract_end_date != null)
                    {
                        var temp_value = input.contract_end_date.Value;
                        longDate = ConvertToTimeStamp(temp_value);

                        if (longDate < reportDate)
                        {
                            temp_value2 = ECLNonStringConstants.i.Expired - Math.Floor(Financial.YearFrac(temp_value, ECLNonStringConstants.i.reportingDate, 0) * 12);
                            //temp_value2 = Convert.ToDouble(Expired);
                        }
                        else
                        {
                            temp_value2 = ECLNonStringConstants.i.Non_Expired;
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


        private static string PD_Mapping(LGD_Inputs input, double ttm_months)
        {
            string pd_mapping;
            if (input.new_contract_no.Contains(ECLStringConstants.i.ExpiredContractsPrefix) || ((input.product_type == ECLStringConstants.i.CARDS || input.product_type == ECLStringConstants.i.CARDS || input.product_type == ECLStringConstants.i._productType_od) && ttm_months == 0))
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
