using IFRS9_ECL.Data;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.PDComputation
{
    public class PDMapping
    {

        protected ScenarioLifetimePd _scenarioLifetimePd;
        protected ScenarioRedefaultLifetimePds _scenarioRedefaultLifetimePd;
        //protected PDMapping _pdMapping;

        Guid _eclId;
        EclType _eclType;
        public PDMapping(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
            _scenarioLifetimePd = new ScenarioLifetimePd(ECL_Scenario.Best, this._eclId, this._eclType);
            _scenarioRedefaultLifetimePd = new ScenarioRedefaultLifetimePds(ECL_Scenario.Best, this._eclId, this._eclType);
            //_pdMapping = new PDMapping(this._eclId);
        }

        public string Run()
        {
            var pdMappings = ComputePdMappingTable();

            var c = new PdMappings();

            Type myObjOriginalType = c.GetType();
            PropertyInfo[] myProps = myObjOriginalType.GetProperties();

            var dt = new DataTable();

            for (int i = 0; i < myProps.Length; i++)
            {
                dt.Columns.Add(myProps[i].Name, myProps[i].PropertyType);
            }
            dt.Columns.Remove("AccountNo");
            dt.Columns.Remove("ProductType");
            dt.Columns.Remove("RatingModel");
            dt.Columns.Remove("RatingUsed");
            dt.Columns.Remove("ClassificationScore");
            dt.Columns.Remove("Segment");

            foreach (var _d in pdMappings)
            {
                _d.Id = Guid.NewGuid();
                _d.WholesaleEclId = _eclId;

                dt.Rows.Add(new object[]
                    {
                            _d.Id, _d.ContractId, _d.PdGroup, _d.TtmMonths, _d.MaxDpd, _d.MaxClassificationScore, _d.Pd12Month, _d.LifetimePd, _d.RedefaultLifetimePD, _d.Stage1Transition, _d.Stage2Transition, _d.DaysPastDue, _d.WholesaleEclId
                    });
            }

          
            var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.PdMappings_Table(this._eclType));

            return r > 0 ? "" : $"Could not Bulk Insert [{ECLStringConstants.i.PdMappings_Table(this._eclType)}]";
        }

        public List<PdMappings> ComputePdMappingTable()
        {
            var temp = new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_Assumptions();
            //string[] testAccounts = { "103ABLD150330005", "15036347", "222017177" };

            int expOdPerformacePastRepoting = Convert.ToInt32(temp.FirstOrDefault(o => o.PdGroup == PdInputAssumptionGroupEnum.General && o.Key== PdAssumptionsRowKey.Expired).Value);
            int odPerformancePastExpiry = Convert.ToInt32(temp.FirstOrDefault(o => o.PdGroup == PdInputAssumptionGroupEnum.General && o.Key == PdAssumptionsRowKey.NonExpired).Value);

            //Get Data Excel/Database
            var qry = Queries.Raw_Data(this._eclId,this._eclType);
            var _lstRaw = DataAccess.i.GetData(qry);

            var lstRaw = new List<Loanbook_Data>();
            foreach (DataRow dr in _lstRaw.Rows)
            {
                lstRaw.Add(DataAccess.i.ParseDataToObject(new Loanbook_Data(), dr));
            }

            var _NonExpLoanbook_data = lstRaw.Where(o => o.ContractId.Substring(0, 3) != ECLStringConstants.i.ExpiredContractsPrefix).ToList();


            var pdMappingTable = new List<PdMappings>();

            var lifetimePds = _scenarioLifetimePd.ComputeLifetimePd();
            var redefaultLifetimePds = _scenarioRedefaultLifetimePd.ComputeRedefaultLifetimePd();

            foreach (var loanbookRecord in _NonExpLoanbook_data)
            {
                var mappingRow = new PdMappings();
                mappingRow.ContractId = loanbookRecord.ContractId;
                mappingRow.AccountNo = loanbookRecord.AccountNo;
                mappingRow.ProductType = loanbookRecord.ProductType;
                mappingRow.RatingModel = loanbookRecord.RatingModel;
                mappingRow.Segment = loanbookRecord.Segment;
                mappingRow.RatingUsed = ComputeRatingUsedPerRecord(loanbookRecord);
                mappingRow.ClassificationScore = ComputeClassificationScorePerRecord(loanbookRecord)??0;
                mappingRow.MaxDpd = ComputeMaxDpdPerRecord(loanbookRecord, _NonExpLoanbook_data);
                mappingRow.TtmMonths = ComputeTimeToMaturityMonthsPerRecord(loanbookRecord, expOdPerformacePastRepoting, odPerformancePastExpiry);
                mappingRow.PdGroup = ComputePdGroupingPerRecord(mappingRow);

                
               
                pdMappingTable.Add(mappingRow);
            }
            pdMappingTable = pdMappingTable.Select(row =>
            {
                row.MaxClassificationScore = ComputeMaxClassificationScorePerRecord(row, pdMappingTable);
                return row;
            }).ToList();
            var sicrInputWorking = new SicrInputWorkings(this._eclId, this._eclType);
            for (int i = 0; i < pdMappingTable.Count; i++)
            {
                var sicrinput = sicrInputWorking.ComputeSICRInput(_NonExpLoanbook_data[i], pdMappingTable[i], lifetimePds, redefaultLifetimePds);

                pdMappingTable[i].DaysPastDue = sicrinput.DaysPastDue;
                pdMappingTable[i].LifetimePd = sicrinput.LifetimePd;
                pdMappingTable[i].Pd12Month = sicrinput.Pd12Month;
                pdMappingTable[i].RedefaultLifetimePD = sicrinput.RedefaultLifetimePd;
                pdMappingTable[i].Stage1Transition = sicrinput.Stage1Transition;
                pdMappingTable[i].Stage2Transition = sicrinput.Stage2Transition;
            }

            return pdMappingTable;
        }

        
        protected string ComputePdGroupingPerRecord(PdMappings pdMappingWorkingRecord)
        {
            string pdGrouping = "";
            string[] productTypes = { ECLStringConstants.i._productType_od.ToLower(), ECLStringConstants.i._productType_card.ToLower(), ECLStringConstants.i._productType_cards.ToLower() };
            if (pdMappingWorkingRecord.ContractId.Substring(0, 3) == ECLStringConstants.i.ExpiredContractsPrefix || productTypes.Contains(pdMappingWorkingRecord.ProductType.ToLower()) && pdMappingWorkingRecord.TtmMonths == 0)
            {
                pdGrouping = ECLStringConstants.i.ExpiredContractsPrefix;
            }
            else
            {
                if (pdMappingWorkingRecord.RatingModel.ToLower() == ECLStringConstants.i.yes)
                {
                    pdGrouping = pdMappingWorkingRecord.RatingUsed.ToString();
                }
                else
                {
                    pdGrouping = pdMappingWorkingRecord.Segment.ToLower() == ECLStringConstants.i.COMMERCIAL.ToLower() ? ECLStringConstants.i.COMM : ECLStringConstants.i.CONS;
                    pdGrouping += pdMappingWorkingRecord.MaxDpd < 30 ? ECLStringConstants.i._STAGE_1 : ECLStringConstants.i._STAGE_2;
                }
            }

            return pdGrouping;
        }
        protected int ComputeTimeToMaturityMonthsPerRecord(Loanbook_Data loanbookRecord, int expOdPerformacePastRepoting, int odPerformancePastExpiry)
        {

            if (loanbookRecord.ContractId.Substring(0, 3) == ECLStringConstants.i.ExpiredContractsPrefix)
            {
                return 0;
            }
            else
            {
                int xValue = 0;
                int yValue = 0;

                DateTime? endDate;
                if (loanbookRecord.RestructureIndicator && loanbookRecord.RestructureEndDate != null)
                {
                    endDate = DateTime.Parse(loanbookRecord.RestructureEndDate.ToString());
                }
                else
                {
                    if(loanbookRecord.ContractEndDate==null)
                    {
                        return 0;
                    }
                    endDate = DateTime.Parse(loanbookRecord.ContractEndDate.ToString());
                }
                var eomonth = ExcelFormulaUtil.EOMonth(endDate);
                var yearFrac = ExcelFormulaUtil.YearFrac(ECLNonStringConstants.i.reportingDate, eomonth);
                var round = Convert.ToInt32(Math.Round(yearFrac * 12, 0));

                xValue = endDate > ECLNonStringConstants.i.reportingDate ? round : 0;

                var maxx = Math.Max(expOdPerformacePastRepoting - round, 0);
                var prod = endDate < ECLNonStringConstants.i.reportingDate ? maxx : odPerformancePastExpiry;
                yValue = loanbookRecord.ProductType == ECLStringConstants.i._productType_card || loanbookRecord.ProductType == ECLStringConstants.i._productType_od ? prod : 0;

                //Financial.YearFrac()
                return xValue + yValue;
            }
        }
        protected DateTime? ComputeRestructureEndDatePerRecord(Loanbook_Data loanbookRecord)
        {
            var restructureEndDate = loanbookRecord.RestructureEndDate;
            if (restructureEndDate == null)
            {
                return null;
            }
            else
            {
                return restructureEndDate;
            }
        }
        protected int ComputeMaxClassificationScorePerRecord(PdMappings pdMappingWorkingRecord, List<PdMappings> pdMappingWorkings)
        {
            var r= pdMappingWorkings.Where(row => row.AccountNo == pdMappingWorkingRecord.AccountNo).Max(row => row.ClassificationScore);
            return r;
        }
        protected long ComputeMaxDpdPerRecord(Loanbook_Data loanbookRecord, List<Loanbook_Data> loanbook)
        {

            var temp = loanbook.Where(o => o.AccountNo == loanbookRecord.AccountNo).Max(p => p.DaysPastDue);
            return temp ?? 0;
        }
        protected int? ComputeClassificationScorePerRecord(Loanbook_Data loanbookRecord)
        {
            string classification = loanbookRecord.Classification.ToUpper();
            switch (classification)
            {
                case "P":
                    return 1;
                case "W":
                    return 2;
                case "S":
                    return 3;
                case "D":
                    return 4;
                case "L":
                    return 5;
                default:
                    return null;
            }
        }
        protected long ComputeRatingUsedPerRecord(Loanbook_Data loanbookRecord)
        {
            loanbookRecord.CurrentRating=loanbookRecord.CurrentRating ?? 0;
            var current_rating = loanbookRecord.CurrentRating.Value; ;
            return current_rating > 10 ? current_rating / 10 : current_rating;
        }

        internal List<PdMappings> GetPdMapping()
        {
            var qry = Queries.PdMapping(this._eclId, this._eclType);
            var _PdMapping = DataAccess.i.GetData(qry);

            var pdMapping = new List<PdMappings>();
            foreach (DataRow dr in _PdMapping.Rows)
            {
                pdMapping.Add(DataAccess.i.ParseDataToObject(new PdMappings(), dr));
            }
            return pdMapping;
        }
    }
}
