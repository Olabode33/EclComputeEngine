using IFRS9_ECL.Data;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.PDComputation
{
    public class SicrInputWorkings
    {
        //protected ScenarioLifetimePd _scenarioLifetimePd;
        //protected ScenarioRedefaultLifetimePds _scenarioRedefaultLifetimePd;
        //protected PDMapping _pdMapping;

        Guid _eclId;
        EclType _eclType;
        public SicrInputWorkings(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
            //_scenarioLifetimePd = new ScenarioLifetimePd(ECL_Scenario.Best, this._eclId);
            //_scenarioRedefaultLifetimePd = new ScenarioRedefaultLifetimePds(ECL_Scenario.Best, this._eclId);
            //_pdMapping = new PDMapping(this._eclId);
        }

        //public void Run()
        //{
        //    var dataTable = ComputeSicrInput();

        //    string stop = "stop";
        //}

        //public List<SicrInputs> ComputeSicrInput()
        //{
        //    var sicrInput = new List<SicrInputs>();

        //    //string[] testAccounts = { "15033346", "15036347", "222017177" };

        //    var loanbookTable = GetLoanbookData().Where(x => x.ContractId.Substring(0, 3) != ECLStringConstants.i.ExpiredContractsPrefix).ToList();
        //    var lifetimePds = _scenarioLifetimePd.ComputeLifetimePd();
        //    var redefaultLifetimePds = _scenarioRedefaultLifetimePd.ComputeRedefaultLifetimePd();
        //    //var pdMapping = _pdMapping.ComputePdMappingTable();

        //    foreach (var loanbookRow in loanbookTable)
        //    {
        //        var contractPdMapping = pdMapping.FirstOrDefault(x => x.ContractId == loanbookRow.ContractId);
        //        if (contractPdMapping == null)
        //        {
        //            continue;
        //        }
        //        string contractPdGroup = contractPdMapping.PdGroup;
        //        int contractTtmMonths = contractPdMapping.TtmMonths;
        //        string impairedDate = null;
        //        if(loanbookRow.ImpairedDate != null)
        //        {
        //            impairedDate=loanbookRow.ImpairedDate.ToString().Contains("1900") ? null : loanbookRow.ImpairedDate.ToString();
        //        }
        //        string defaultDate = null;
        //        if(loanbookRow.DefaultDate!=null)
        //        {
        //            defaultDate = loanbookRow.DefaultDate.ToString().Contains("1900") ? null : loanbookRow.DefaultDate.ToString();
        //        }
                
        //        int maxClassification = contractPdMapping.MaxClassificationScore;
        //        long maxDpd = contractPdMapping.MaxDpd;

        //        var sicrRow = new SicrInputs();
        //        sicrRow.ContractId = loanbookRow.ContractId;
        //        sicrRow.Pd12Month = ComputeLifetimeAndRedefaultPds(lifetimePds, contractPdGroup, 12);
        //        sicrRow.LifetimePd = ComputeLifetimeAndRedefaultPds(lifetimePds, contractPdGroup, contractTtmMonths);
        //        sicrRow.RedefaultLifetimePd = ComputeLifetimeAndRedefaultPds(redefaultLifetimePds, contractPdGroup, contractTtmMonths);
        //        sicrRow.Stage1Transition = Math.Round(ComputeStageDaysPastDue(impairedDate));
        //        sicrRow.Stage2Transition = ComputeStageDaysPastDue(defaultDate);
        //        sicrRow.DaysPastDue = ComputeDaysPastDue(maxClassification, maxDpd);



        //        sicrInput.Add(sicrRow);
        //    }

        //    return sicrInput;
        //}

        public SicrInputs ComputeSICRInput(Loanbook_Data loanbookRow, WholesalePdMappings contractPdMapping, List<LifeTimeObject> lifetimePds, List<LifeTimeObject> redefaultLifetimePds)
        {
            string contractPdGroup = contractPdMapping.PdGroup;
            int contractTtmMonths = contractPdMapping.TtmMonths;
            string impairedDate = null;
            if (loanbookRow.ImpairedDate != null)
            {
                impairedDate = loanbookRow.ImpairedDate.ToString().Contains("1900") ? null : loanbookRow.ImpairedDate.ToString();
            }
            string defaultDate = null;
            if (loanbookRow.DefaultDate != null)
            {
                defaultDate = loanbookRow.DefaultDate.ToString().Contains("1900") ? null : loanbookRow.DefaultDate.ToString();
            }

            int maxClassification = contractPdMapping.MaxClassificationScore;
            long maxDpd = contractPdMapping.MaxDpd;

            var sicrRow = new SicrInputs();
            //sicrRow.ContractId = loanbookRow.ContractId;
            sicrRow.Pd12Month = ComputeLifetimeAndRedefaultPds(lifetimePds, contractPdGroup, 12);
            sicrRow.LifetimePd = ComputeLifetimeAndRedefaultPds(lifetimePds, contractPdGroup, contractTtmMonths);
            sicrRow.RedefaultLifetimePd = ComputeLifetimeAndRedefaultPds(redefaultLifetimePds, contractPdGroup, contractTtmMonths);
            sicrRow.Stage1Transition = int.Parse(Math.Round(ComputeStageDaysPastDue(impairedDate)).ToString());
            sicrRow.Stage2Transition = int.Parse(Math.Round(ComputeStageDaysPastDue(defaultDate)).ToString());
            sicrRow.DaysPastDue = ComputeDaysPastDue(maxClassification, maxDpd);

            return sicrRow;

        }


        public List<SicrInputs> GetSircInputResult()
        {
            var qry = Queries.PD_GetSIRCInputResult(this._eclId, _eclType);
            var dt = DataAccess.i.GetData(qry);
            var lifeTimeProjections = new List<SicrInputs>();

            foreach (DataRow dr in dt.Rows)
            {
                lifeTimeProjections.Add(DataAccess.i.ParseDataToObject(new SicrInputs(), dr));
            }

            return lifeTimeProjections;
        }



        protected int ComputeDaysPastDue(int maxClassification, long maxDpd)
        {
            if (maxClassification == 1 || maxClassification == 2)
            {
                return maxDpd < 30 ? 0 : 30;
            }
            else if (maxClassification == 3)
            {
                return 90;
            }
            else if (maxClassification == 4)
            {
                return 180;
            }
            else
            {
                return 360;
            }
        }
        protected double ComputeStageDaysPastDue(string date)
        {
            var r= date == null ? 0 : ExcelFormulaUtil.YearFrac(DateTime.Parse(date), ECLNonStringConstants.i.reportingDate) * 365;
            return r;
        }
        protected double ComputeLifetimeAndRedefaultPds(List<LifeTimeObject> lifetimePd, string contractPdMapping, int noOfMonths)
        {
            if (noOfMonths == 0)
            {
                return 1.0;
            }
            var monthPds = lifetimePd.AsEnumerable()
                                        .Where(row => row.PdGroup == contractPdMapping
                                                   && row.Month <= noOfMonths)
                                        .Select(row => row.Value).ToArray();
            return monthPds.Aggregate(0.0, (acc, x) => acc + x);
        }
        protected List<Loanbook_Data> GetLoanbookData()
        {
            var qry = Queries.Raw_Data(this._eclId, this._eclType);
            var _lstRaw = DataAccess.i.GetData(qry);

            var lstRaw = new List<Loanbook_Data>();
            foreach (DataRow dr in _lstRaw.Rows)
            {
                lstRaw.Add(DataAccess.i.ParseDataToObject(new Loanbook_Data(), dr));
            }
            return lstRaw;
        }
    }
}
