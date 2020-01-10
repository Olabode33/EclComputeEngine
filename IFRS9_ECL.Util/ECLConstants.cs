﻿using System;

namespace IFRS9_ECL.Util
{
    public class ECLStringConstants
    {
        public static readonly ECLStringConstants i = new ECLStringConstants();
        public string yes= "yes";

        public string WholesaleEadLifetimeProjections_Table { get { return "WholesaleEadLifetimeProjections"; } }
        public string WholesaleEadEirProjections_Table { get { return "WholesaleEadEirProjections"; } }
        public string WholesaleEadCirProjections_Table { get { return "WholesaleEadCirProjections"; } }
        public string WholesaleLGDCollateral_Table { get { return "WholesaleLGDCollateral"; } }
        public string WholesaleLGDAccountData_Table { get { return "WholesaleLGDAccountData"; } }


        //Wholesale PD tables
        public string WholesalePDCreditIndex_Table { get { return "WholesalePDCreditIndex"; } }
        public string WholesalePdMappings_Table { get { return "WholesalePdMappings"; } }

        public string WholesalePdLifetimeBests_Table { get { return "WholesalePdLifetimeBests"; } }
        public string WholesalePdLifetimeDownturns_Table { get { return "WholesalePdLifetimeDownturns"; } }
        public string WholesalePdLifetimeOptimistics_Table { get { return "WholesalePdLifetimeOptimistics"; } }

        public string WholesalePdRedefaultLifetimeBests_Table { get { return "WholesalePdRedefaultLifetimeBests"; } }
        public string WholesalePdRedefaultLifetimeDownturns_Table { get { return "WholesalePdRedefaultLifetimeDownturns"; } }
        public string WholesalePdRedefaultLifetimeOptimistics_Table { get { return "WholesalePdRedefaultLifetimeOptimistics"; } }



        public string filterValue_cir_effective { get { return "CIR_EFFECTIVE"; } }
        public string filterValue_lifetime { get { return "LIFETIME"; } }
        public string ExpiredContractsPrefix { get { return "EXP"; } }
        public string _fixed { get { return "FIXED"; } }
        public char _splitValue = '_';
        public string _productType_loan { get { return "LOAN"; } }
        public string _productType_lease { get { return "LEASE"; } }
        public string _productType_mortgage { get { return "MORTGAGE"; } }
        public string _productType_od { get { return "OD"; } }
        public string _productType_card { get { return "CARD"; } }
        public string _productType_cards { get { return "CARDS"; } }
        public string _corporate { get { return "CORPORATE"; } }
        public string _commercial { get { return "COMMERCIAL"; } }
        public string _consumer { get { return "CONSUMER"; } }
        public string _obe { get { return "OBE"; } }
        public string _amortise { get { return "AMORTISE"; } }
        public string _month0 { get { return "0"; } }
        public string _interestDivisior { get { return "B"; } }

        public string FLOATING { get { return "FLOATING"; } }


        public string tempString { get { return string.Empty; } } //for holding temporary values


        public string ID { get { return "ID"; } }
        public string CARDS { get { return "CARDS"; } }

        public string MPR { get { return "MPR"; } }

        public string RatingModel_Yes { get { return "YES"; } }

        public string COMMERCIAL { get { return "COMMERCIAL"; } }
        public string COMM { get { return "COMM"; } }
        public string CONS { get { return "CONS"; } }

        public string _STAGE_1 { get { return "_STAGE_1"; } }

        public string _STAGE_2 { get { return "_STAGE_2"; } }

        public string PROJECT_FINANCE { get { return "PROJECT FINANCE"; } }

        public string Debenture_Omv_array { get { return "Debenture_Omv_array"; } }
        public string Cash_Omv_array { get { return "Cash_Omv_array"; } }
        public string Inventory_Omv_array { get { return "Inventory_Omv_array"; } }
        public string Plant_Equipment_Omv_array { get { return "Plant_Equipment_Omv_array"; } }
        public string Residential_Omv_array { get { return "Residential_Omv_array"; } }
        public string Commercial_Omv_array { get { return "Commercial_Omv_array"; } }
        public string Receivables_Omv_array { get { return "Receivables_Omv_array"; } }
        public string Shares_Omv_array { get { return "Shares_Omv_array"; } }
        public string Vehicle_Omv_array { get { return "Vehicle_Omv_array"; } }

        public string Debenture_Fsv_array { get { return "Debenture_Fsv_array"; } }
        public string Cash_Fsv_array { get { return "Cash_Fsv_array"; } }
        public string Inventory_Fsv_array { get { return "Inventory_Fsv_array"; } }
        public string Plant_Equipment_Fsv_array { get { return "Plant_Equipment_Fsv_array"; } }
        public string Residential_Fsv_array { get { return "Residential_Fsv_array"; } }
        public string Commercial_Fsv_array { get { return "Commercial_Fsv_array"; } }
        public string Receivables_Fsv_array { get { return "Receivables_Fsv_array"; } }
        public string Shares_Fsv_array { get { return "Shares_Fsv_array"; } }
        public string Vehicle_Fsv_array { get { return "Vehicle_Fsv_array"; } }


        public string CustomerNo_array { get { return "CustomerNo_array"; } }
        


        ///this is called EXP_OD_PERFORMACE_PAST_EXPIRY on the excel and it is obtained from the EAD calibration. It will be obtained from the DB

    }

    public class ECLNonStringConstants
    {
        public static readonly ECLNonStringConstants i = new ECLNonStringConstants();

        public double virProjections { get { return 0.14; } } //GOTTEN FROM DB
        public int Non_Expired { get { return 31; } }  ///this is called OD_PERFORMACE_PAST_EXPIRY on the excel and it is obtained from the EAD calibration. It will be obtained from the DB
        public int Expired { get { return 22; } }
        public double Corporate { get { return 1; } } ///It will be obtained from the DB
        public double Commercial { get { return 1; } } ///It will be obtained from the DB
        public double Consumer { get { return 1; } } ///It will be obtained from the DB
        public double NGN_Currency { get { return 1; } }
        public double Conversion_Factor_OBE { get { return 1; } } ///It will be obtained from the DB this is in percentage
        public DateTime reportingDate { get {return new DateTime(2016, 12, 31); } }

        public double IndexWeight1 = 0.575691023137874;
        public double IndexWeight2 = 0.424308976862126;
        public double Rho = 0.217333590280369;

        public int prepaymentFactor { get { return 0; } }

        public string SnpMapping = "SnpMapping";
        public int MaxMarginalLifetimeRedefaultPdMonth= 120;
    }


    public class ECLPDConstants
    {
        private const double _indexWeightW1 = 0.58;
        private const double _indexWeightW2 = 0.42;
        private const double _statisticsStandardDeviation = 0.84;
        private const double _statisticsAverage = 0.00;
    }
    public class ECLScheduleConstants
    {
        public const string Monthly = "M";
        public const int Monthly_number = 1;
        public const string Quarterly = "Q";
        public const int Quarterly_number = 3;
        public const string Bullet = "B";
        public const string Yearly = "Y";
        public const int Yearly_number = 12;
        public const string HalfYear = "H";
        public const int HalfYear_number = 6;
        public const string S = "S";
        public const string S_value = "Error";
    }

    public enum ECL_Scenario
    {
        Best,
        Optimistic,
        Downturn
    }
}
