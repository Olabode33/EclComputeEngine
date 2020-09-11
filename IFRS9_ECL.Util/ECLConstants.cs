using System;
using System.Collections.Generic;

namespace IFRS9_ECL.Util
{
    public class ECLStringConstants
    {
        public static readonly ECLStringConstants i = new ECLStringConstants();
        public string yes = "yes";

        public string EadLifetimeProjections_Table(EclType eclType) { return $"{eclType.ToString()}EadLifetimeProjections"; }
        public string EadEirProjections_Table(EclType eclType) { return $"{eclType.ToString()}EadEirProjections";  }
        public string EadCirProjections_Table(EclType eclType) { return $"{eclType.ToString()}EadCirProjections";  }
        public string LGDCollateral_Table(EclType eclType) { return $"{eclType.ToString()}LGDCollateral";  }
        public string LGDAccountData_Table(EclType eclType) { return $"{eclType.ToString()}LGDAccountData";  }

        public string FrameworkResult(EclType eclType) { return $"{eclType.ToString()}ECLFrameworkFinal";  }
        public string FrameworkResultOverride(EclType eclType) { return $"{eclType.ToString()}ECLFrameworkFinalOverride"; }
        public string EclFramworkReportDetail(EclType eclType) { return $"{eclType.ToString()}EclFramworkReportDetail"; }

        //Wholesale PD tables
        public string PDCreditIndex_Table(EclType eclType) { return $"{eclType.ToString()}PDCreditIndex";  }
        public string PdMappings_Table(EclType eclType) { return $"{eclType.ToString()}PdMappings"; }

        public string PdLifetimeBests_Table(EclType eclType)
        {
            return $"{eclType.ToString()}PdLifetimeBests";
        }
        public string PdLifetimeDownturns_Table(EclType eclType)
        {
            return $"{eclType.ToString()}PdLifetimeDownturns";
        }
        public string PdLifetimeOptimistics_Table(EclType eclType)
        {
            return $"{eclType.ToString()}PdLifetimeOptimistics";
        }

        public string PdRedefaultLifetimeBests_Table(EclType eclType) { return $"{eclType.ToString()}PdRedefaultLifetimeBests";  }
        public string PdRedefaultLifetimeDownturns_Table(EclType eclType) { return $"{eclType.ToString()}PdRedefaultLifetimeDownturns";  }
        public string PdRedefaultLifetimeOptimistics_Table(EclType eclType) { return $"{eclType.ToString()}PdRedefaultLifetimeOptimistics";  }



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
        public string AccountNo_array { get { return "AccountNo_array"; } }

        ///this is called EXP_OD_PERFORMACE_PAST_EXPIRY on the excel and it is obtained from the EAD calibration. It will be obtained from the DB

    }

    public class ECLNonStringConstants
    {
        public static readonly ECLNonStringConstants i = new ECLNonStringConstants();

        

        //public double Corporate { get { return 1; } } ///It will be obtained from the DB
        //public double Commercial { get { return 1; } } ///It will be obtained from the DB
        //public double Consumer { get { return 1; } } ///It will be obtained from the DB
        public double Local_Currency { get { return 1; } }
        

        public List<double?> ExcelDefaultValue { get { return new List<double?> { -2146826281, -2146826246 }; } }



        ///It will be obtained from the DB this is in percentage
        //*********************************************************

        public double Rho(long affliateId)
        {
            if (affliateId==41)
            {
                return 0.0280579042261724;
            }
            if (affliateId == 5 || affliateId == 46 || affliateId == 47)
            {
                return 0.217333590280369;
            }
            return 0.00470275844438257;// 0.21733359; //******************************
        }



        public string SnpMapping = "SnpMapping";
        public int MaxMarginalLifetimeRedefaultPdMonth = 120;
    }



    public class nonInternalModelInput_Types
    {
        public const string CONS_STAGE_1 = "CONS_STAGE_1";
        public const string CONS_STAGE_2 = "CONS_STAGE_2";
        public const string COMM_STAGE_1 = "COMM_STAGE_1";
        public const string COMM_STAGE_2 = "COMM_STAGE_2";
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
        public const int S_number = -1000;
        public const string S_value = "Error";
    }

    public enum ECL_Scenario
    {
        Best,
        Optimistic,
        Downturn
    }

    public class FrameworkConstants
    {
        public const string EIR = "EIR";
        public const string CIR = "CIR";

        public const int ScenerioWorkingMaxMonth = 12;
        public const int ProjectionMonth = 106;
        public const int TempExcelVariable_LIM_CM = 60;

        public const string CreditQualityCriteriaNone = "None";
        public const string CreditQualityCriteria12MonthPd = "12-month PD";
        public const string CreditQualityCriteriaLifetimePd = "Lifetime PD";

    }


    public static class ImpairmentRowKeys
    {
        public static string CreditIndexThreshold = "CreditIndexThresholdforDownturnRecoveries";
        public static string BestScenarioLikelihood = "BestEstimateScenarioLikelihood";
        public static string OptimisticScenarioLikelihood = "OptimisticScenarioLikelihood";
        public static string DownturnScenarioLikelihood = "DownturnScenarioLikelihood";
        public static string AbsoluteCreditQualityCriteria = "AbsoluteCreditQualityCriteria";
        public static string AbsoluteCreditQualityThreshold = "AbsoluteCreditQualityThreshold";
        public static string RelativeCreditQualityCriteria = "RelativeCreditQualityCriteria";
        public static string RelativeCreditQualityThreshold = "RelativeCreditQualityThreshold";
        public static string CreditRatingRankLowHighRisk = "CreditRatingRankLowHighRisk";
        public static string CreditRatingRankLowRisk = "CreditRatingRankNotchesLowRisk";
        public static string CreditRatingRankHighRisk = "CreditRatingRankNotchesHighRisk";
        public static string CreditRatingDefaultIndicator = "CreditRatingDefaultIndicator";
        public static string UseWatchlistIndicator = "UseWatchlistIndicator";
        public static string UseRestructureIndicator = "UseRestructureIndicator?";
        public static string ForwardTransitionStage1to1 = "ForwardTransitionsStage1to2";
        public static string ForwardTransitionStage2to3 = "ForwardTransitionsStage2toStage3";
        public static string BackwardTransitionsStage2to1 = "BackwardTransitionsProbationPeriodStage2to1";
        public static string BackwardTransitionsStage3to2 = "BackwardTransitionsProbationPeriodStage3to2";
        public static string CreditRatingRank = "CreditRatingRank";

        public static string CreditRatingRank1 = "CreditRatingRank1";
        public static string CreditRatingRank2 = "CreditRatingRank2";
        public static string CreditRatingRank3 = "CreditRatingRank3";
        public static string CreditRatingRank4 = "CreditRatingRank4";
        public static string CreditRatingRank5 = "CreditRatingRank5";
        public static string CreditRatingRank6 = "CreditRatingRank6";
        public static string CreditRatingRank7 = "CreditRatingRank7";
        public static string CreditRatingRank8 = "CreditRatingRank8";
        public static string CreditRatingRank9 = "CreditRatingRank9";
        public static string CreditRatingRank10 = "CreditRatingRank10";
        public static string CreditRatingRank11 = "CreditRatingRank11";
        public static string CreditRatingRank12 = "CreditRatingRank12";
        public static string CreditRatingRank13 = "CreditRatingRank13";
        public static string CreditRatingRank14 = "CreditRatingRank14";
        public static string CreditRatingRank15 = "CreditRatingRank15";
        public static string CreditRatingRank16 = "CreditRatingRank16";
        public static string CreditRatingRank17 = "CreditRatingRank17";
        public static string CreditRatingRank18 = "CreditRatingRank18";
        public static string CreditRatingRank19 = "CreditRatingRank19";
        public static string CreditRatingRank20 = "CreditRatingRank20";



    }



    public static class LGDCollateralGrowthAssumption
    {
        public static string Debenture = "debenture";
        public static string Cash = "cash";
        public static string Inventory = "inventory";
        public static string PlantEquipment = "plantequipment";
        public static string ResidentialProperty = "residentialproperty";
        public static string CommercialProperty = "commercialproperty";
        public static string Receivables = "receivables";
        public static string Shares = "shares";
        public static string Vehicle = "vehicle";
        public static string Collateral = "collateral";
    }

    public enum EclType
    {
        None=-1,
        Retail,
        Wholesale,
        Obe
    }
}
