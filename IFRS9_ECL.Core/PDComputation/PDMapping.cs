using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
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


        public string Run(List<Loanbook_Data> loanbook_Data)
        {
            var pdMappings = ComputePdMappingTable(loanbook_Data);
            return "";
        }

        public bool ComputePdMappingTable(List<Loanbook_Data> loanbook_Data)
        {
            var expireNoExpire = new CalibrationInput_EAD_Behavioural_Terms_Processor().GetBehaviouralData(this._eclId, this._eclType);
            var temp = new ProcessECL_PD(this._eclId, this._eclType).Get_PDI_Assumptions();
            //string[] testAccounts = { "103ABLD150330005", "15036347", "222017177" };
            //*****************************************************
            int expOdPerformacePastRepoting = 0;
            try { expOdPerformacePastRepoting=int.Parse(expireNoExpire.NonExpired.ToString()); } catch { }
            int odPerformancePastExpiry = 0;
            try { odPerformancePastExpiry = int.Parse(expireNoExpire.Expired.ToString()); } catch { }

            //Get Data Excel/Database
            //var qry = Queries.Raw_Data(this._eclId,this._eclType);
            //var _lstRaw = DataAccess.i.GetData(qry);

            var _NonExpLoanbook_data = loanbook_Data.Where(o => o.ContractId.Substring(0, 3) != ECLStringConstants.i.ExpiredContractsPrefix).ToList();


            var lifetimePds = _scenarioLifetimePd.ComputeLifetimePd();
            var redefaultLifetimePds = _scenarioRedefaultLifetimePd.ComputeRedefaultLifetimePd();

            var threads = _NonExpLoanbook_data.Count / 500;
            threads = threads + 1;

            var taskLst = new List<Task>();

            for (int i = 0; i < threads; i++)
            {
                var sub_LoanBook = _NonExpLoanbook_data.Skip(i * 500).Take(500).ToList();

                var task = Task.Run(() => {
                    RunPDMappingJob(sub_LoanBook, _eclId, _eclType, lifetimePds, redefaultLifetimePds, expOdPerformacePastRepoting, odPerformancePastExpiry);
                });
                taskLst.Add(task);
            }
            Log4Net.Log.Info($"Total Task : {taskLst.Count()}");

            var completedTask = taskLst.Where(o => o.IsCompleted).Count();
            Log4Net.Log.Info($"Task Completed: {completedTask}");

            //while (!taskLst.Any(o => o.IsCompleted))
            var tskStatusLst = new List<TaskStatus> { TaskStatus.RanToCompletion, TaskStatus.Faulted };
            while (0 < 1)
            {
                if (taskLst.All(o => tskStatusLst.Contains(o.Status)))
                {
                    break;
                }
                //Do Nothing
            }


            return true;


        }

        private string RunPDMappingJob(List<Loanbook_Data> sub_LoanBook, Guid eclId, EclType eclType, List<LifeTimeObject> lifetimePds, List<LifeTimeObject> redefaultLifetimePds, int expOdPerformacePastRepoting, int odPerformancePastExpiry)
        {

            var pdMappingTable = new List<PdMappings>();

            foreach (var loanbookRecord in sub_LoanBook)
            {
                var mappingRow = new PdMappings();

                try
                {
                    mappingRow.ContractId = loanbookRecord.ContractId;
                    mappingRow.AccountNo = loanbookRecord.AccountNo;
                    mappingRow.ProductType = loanbookRecord.ProductType;
                    mappingRow.RatingModel = loanbookRecord.RatingModel;
                    mappingRow.Segment = loanbookRecord.Segment;
                    mappingRow.RatingUsed = ComputeRatingUsedPerRecord(loanbookRecord);
                    mappingRow.ClassificationScore = ComputeClassificationScorePerRecord(loanbookRecord) ?? 0;
                    mappingRow.MaxDpd = Convert.ToInt32(Math.Round(ComputeMaxDpdPerRecord(loanbookRecord, sub_LoanBook)));
                    mappingRow.TtmMonths = ComputeTimeToMaturityMonthsPerRecord(loanbookRecord, expOdPerformacePastRepoting, odPerformancePastExpiry);
                    mappingRow.PdGroup = ComputePdGroupingPerRecord(mappingRow);

                }catch(Exception ex)
                {
                    var cc = ex;
                }

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
                var sicrinput = sicrInputWorking.ComputeSICRInput(sub_LoanBook[i], pdMappingTable[i], lifetimePds, redefaultLifetimePds);

                pdMappingTable[i].DaysPastDue = sicrinput.DaysPastDue;
                pdMappingTable[i].LifetimePd = sicrinput.LifetimePd;
                pdMappingTable[i].Pd12Month = sicrinput.Pd12Month;
                pdMappingTable[i].RedefaultLifetimePd = sicrinput.RedefaultLifetimePd;
                pdMappingTable[i].Stage1Transition = sicrinput.Stage1Transition;
                pdMappingTable[i].Stage2Transition = sicrinput.Stage2Transition;
            }

            var c = new PdMappings();

            Type myObjOriginalType = c.GetType();
            PropertyInfo[] myProps = myObjOriginalType.GetProperties();

            var dt = new DataTable();

            for (int i = 0; i < myProps.Length; i++)
            {
                dt.Columns.Add(myProps[i].Name, myProps[i].PropertyType);
            }
            //dt.Columns.Add($"{eclType.ToString()}EclId", typeof(Guid));
            dt.Columns.Remove("AccountNo");
            dt.Columns.Remove("ProductType");
            dt.Columns.Remove("RatingModel");
            dt.Columns.Remove("RatingUsed");
            dt.Columns.Remove("ClassificationScore");
            dt.Columns.Remove("Segment");

            foreach (var _d in pdMappingTable)
            {
                _d.Id = Guid.NewGuid();
                _d.WholesaleEclId = _eclId;

                dt.Rows.Add(new object[]
                    {
                            _d.Id, _d.ContractId, _d.PdGroup, _d.TtmMonths, _d.MaxDpd, _d.MaxClassificationScore, _d.Pd12Month, _d.LifetimePd, _d.RedefaultLifetimePd, _d.Stage1Transition, _d.Stage2Transition, _d.DaysPastDue, _d.WholesaleEclId
                    });
            }


            var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.PdMappings_Table(this._eclType));

            return r > 0 ? "" : $"Could not Bulk Insert [{ECLStringConstants.i.PdMappings_Table(this._eclType)}]";

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

                DateTime? endDate = new DateTime(1900, 01, 01);
                if (loanbookRecord.RestructureIndicator && loanbookRecord.RestructureEndDate != null)
                {
                    if (loanbookRecord.RestructureEndDate == null)
                        xValue = 0;
                    else
                        endDate = DateTime.Parse(loanbookRecord.RestructureEndDate.ToString());
                }
                else
                {
                    if (loanbookRecord.ContractEndDate == null)
                    {
                        xValue = 0;
                    }
                    else
                    {
                        endDate = DateTime.Parse(loanbookRecord.ContractEndDate.ToString());
                    }
                }

                var prod = 0;
                if (endDate!=null && endDate != new DateTime(1900, 01, 01))
                {
                    if(!endDate.ToString().Contains("001"))
                    {

                        var eomonth = ExcelFormulaUtil.EOMonth(endDate);
                        var yearFrac = ExcelFormulaUtil.YearFrac(GetReportingDate(_eclType, _eclId), eomonth);
                        var round = Convert.ToInt32(Math.Round(yearFrac * 12, 0));

                        xValue = endDate > GetReportingDate(_eclType, _eclId) ? round : 0;

                        var maxx = Math.Max(expOdPerformacePastRepoting - round, 0);
                        prod = endDate < GetReportingDate(_eclType, _eclId) ? maxx : odPerformancePastExpiry;
                    }
                }
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
        protected double ComputeMaxDpdPerRecord(Loanbook_Data loanbookRecord, List<Loanbook_Data> loanbook)
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
        protected int ComputeRatingUsedPerRecord(Loanbook_Data loanbookRecord)
        {
            loanbookRecord.CurrentRating=loanbookRecord.CurrentRating ?? 0;
            var current_rating = loanbookRecord.CurrentRating.Value; ;
            return current_rating > 10 ? int.Parse(current_rating.ToString().Substring(0,1)) : current_rating;
        }

        internal List<PdMappings> GetPdMapping()
        {
            var qry = Queries.PdMapping(this._eclId, this._eclType);
            var _PdMapping = DataAccess.i.GetData(qry);

            var pdMapping = new List<PdMappings>();
            foreach (DataRow dr in _PdMapping.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PdMappings(), dr);
                pdMapping.Add(itm);
            }
            return pdMapping;
        }
    }
}
