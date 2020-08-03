using Excel.FinancialFunctions;
using IFRS9_ECL.Core.Calibration;
using IFRS9_ECL.Core.PDComputation;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models;
using IFRS9_ECL.Models.Framework;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Models.Raw;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core.FrameworkComputation
{
    public class LifetimeEadWorkings
    {

        Guid _eclId;
        protected IrFactorWorkings _irFactorWorkings;
        protected SicrInputWorkings _sicrInputs;
        protected EclType _eclType;
        ProcessECL_LGD _processECL_LGD;
        int MPD_Default_Criteria = 3;
        DateTime reportingDate;
        public LifetimeEadWorkings(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
            _irFactorWorkings = new IrFactorWorkings(_eclId, _eclType);
            _sicrInputs = new SicrInputWorkings(this._eclId, _eclType);
            _processECL_LGD = new ProcessECL_LGD(eclId, eclType);
            var eclFrameworkAssumptions=GetECLFrameworkAssumptions();
            var itm = eclFrameworkAssumptions.FirstOrDefault(o => o.Key == ImpairmentRowKeys.ForwardTransitionStage2to3);
            try
            {
                if (itm != null)
                {
                    MPD_Default_Criteria = int.Parse(itm.Value) / 30;
                }
            }
            catch { }
            reportingDate = GetReportingDate(eclType, eclId);
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

        List<LifeTimeProjections> eadInputs = new List<LifeTimeProjections>();
        List<SicrInputs> sircInputs = new List<SicrInputs>();
        List<IrFactor> marginalAccumulationFactor = new List<IrFactor>();
        List<Refined_Raw_Retail_Wholesale> refined_Raw_Data = new List<Refined_Raw_Retail_Wholesale>();
        List<LifetimeEad> lifetimeEad = new List<LifetimeEad>();
        public List<LifetimeEad> ComputeLifetimeEad(List<Loanbook_Data> loanbook, List<LifeTimeProjections> eadInputs)
        {


            this.eadInputs = eadInputs;// GetTempEadInputData(loanbook);
            sircInputs = GetSircInputResult();
            Console.WriteLine($"Got EAD_StatgeClassification");
            marginalAccumulationFactor = GetMarginalAccumulationFactorResult();
            Console.WriteLine($"Got marginalAccumulationFactor");
            refined_Raw_Data = GetRefinedLoanBookData(loanbook);
            Console.WriteLine($"Got refined_Raw_Data");
            var contractData = _processECL_LGD.GetLgdContractData(loanbook);
            var loanbook_contractNo = refined_Raw_Data.Select(o => o.contract_no).ToList();

            var contract_nos = eadInputs.Where(n=>loanbook_contractNo.Contains(n.Contract_no)).Select(o => o.Contract_no).Distinct().ToList();


            if (1!=1)//loanbook.Count <= 1000)
            {
                RunEADJob(contract_nos);
                return lifetimeEad;
            }
            //var checker = loanbook.Count / 60;

            var threads = loanbook.Count / 500;

            
            
            threads = threads + 1;

            var taskLst = new List<Task>();
            for (int i = 0; i < threads; i++)
            {
                var sub_contract_nos = contract_nos.Skip(i * 500).Take(500).ToList();

                var task = Task.Run(() =>
                {
                    RunEADJob(sub_contract_nos);
                });
                taskLst.Add(task);
            }
            Log4Net.Log.Info($"Total Task : {taskLst.Count()}");

            var completedTask = taskLst.Where(o => o.IsCompleted).Count();
            Log4Net.Log.Info($"Task Completed: {completedTask}");


            var tskStatusLst = new List<TaskStatus> { TaskStatus.RanToCompletion, TaskStatus.Faulted };
            while (0 < 1)
            {
                if (taskLst.All(o => tskStatusLst.Contains(o.Status)))
                {
                    break;
                }
                //Do Nothing
            }

            //StringBuilder sb = new StringBuilder();
            //sb.Append($"COntractID,Month,Value,{Environment.NewLine}");
            //foreach (var itm in lifetimeEad)
            //{
            //    sb.Append($"{itm.ContractId},{itm.ProjectionMonth},{itm.ProjectionValue},{Environment.NewLine}");
            //}
            //File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "EADOutput.csv"), sb.ToString());

            Log4Net.Log.Info("Completed ComputeLifetimeEad");
            return lifetimeEad;//.Where(o=> contractIds.Contains(o.ContractId)).ToList();
        }

        private void RunEADJob(List<string> contractNo)
        {

            var sub_lifetimeEad = new List<LifetimeEad>();
            foreach (var contract_no in contractNo)
            {
                if(contract_no.Contains("9CRLA142680001"))//005IELA143560002

                {
                    var cc = 0;
                }

                Console.WriteLine($"FEAD - {contract_no}");
                var c_eadInputs = eadInputs.Where(c => c.Contract_no == contract_no).OrderBy(o=>o.Month).ToList();

                string contractId = contract_no;

                int cirIndex = 1;
                try { cirIndex = marginalAccumulationFactor.FirstOrDefault(o => o.EirGroup == c_eadInputs[0].Cir_Group).Rank; } catch { };

                var loanRec = refined_Raw_Data.FirstOrDefault(x => x.contract_no == contractId);
                string productType = loanRec.product_type;
                var sirc = sircInputs.FirstOrDefault(x => x.ContractId == contractId);
                long? daysPastDue = sirc == null ? 0 : sirc.DaysPastDue;

                var month0Record = new LifetimeEad();
                month0Record.ContractId = contractId;
                month0Record.CirIndex = cirIndex;
                month0Record.ProductType = productType;
                month0Record.MonthsPastDue = (daysPastDue == null ? 0 : daysPastDue / 30) ?? 0;
                month0Record.ProjectionMonth = 0;
                month0Record.ProjectionValue = c_eadInputs.FirstOrDefault(o=>o.Month==0).Value;
                sub_lifetimeEad.Add(month0Record);

                var noOfMonths = 150;// loanRec.LIM_MONTH + 1; //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                //for (int month = 1; month < noOfMonths; month++)
                //{

                var month = 1;
                while (0 == 0)
                {
                    var eadInputRecord = 0.0;
                    if (c_eadInputs.Count > month)
                    {
                        eadInputRecord = c_eadInputs[month].Value;
                    }

                    var newRecord = new LifetimeEad();
                    newRecord.ContractId = contractId;
                    newRecord.CirIndex = cirIndex;
                    newRecord.ProductType = productType;
                    newRecord.MonthsPastDue = (daysPastDue / 30) ?? 0;
                    newRecord.ProjectionMonth = month;
                    newRecord.ProjectionValue = ComputeLifetimeValue(c_eadInputs, eadInputRecord, marginalAccumulationFactor, (long)daysPastDue / 30, month, cirIndex, productType);

                    if (newRecord.ProjectionValue == 0 || month==240)
                    {
                        break;
                    }
                    sub_lifetimeEad.Add(newRecord);
                    month++;
                }
                    ////Do computation
                    //var itm = new LifeTimeProjections();
                    //if (c_eadInputs.Count > month)
                    //{
                    //    itm = c_eadInputs[month];

                    //    var newRecord = new LifetimeEad();
                    //    newRecord.ContractId = contractId;
                    //    newRecord.CirIndex = cirIndex;
                    //    newRecord.ProductType = productType;
                    //    newRecord.MonthsPastDue = (daysPastDue / 30) ?? 0;
                    //    newRecord.ProjectionMonth = month;
                    //    newRecord.ProjectionValue = ComputeLifetimeValue(c_eadInputs, itm, marginalAccumulationFactor, (long)daysPastDue / 30, month, cirIndex, productType);
                    //    sub_lifetimeEad.Add(newRecord);
                    //}
                    //else
                    //{
                    //    //itm = c_eadInputs.LastOrDefault();
                    //}
                    
                //}
            }
            lifetimeEad.AddRange(sub_lifetimeEad);
        }

        private List<IrFactor> GetMarginalAccumulationFactorResult()
        {
            var marginalAccumulativeFactor = new List<IrFactor>();

            var cirProjections = GetCirProjectionData();
           
            var groups = cirProjections.Select(o => o.cir_group).Distinct().ToList();


            int rank = 1;
            double prevMonthValue = 0.0;

            groups.Sort();
            foreach (var grp in groups)
            {
                var month0Record = new IrFactor();
                month0Record.EirGroup = grp;
                month0Record.Rank = rank;
                month0Record.ProjectionMonth = 0;
                month0Record.ProjectionValue = 1.0;
                marginalAccumulativeFactor.Add(month0Record);

                var _cirProjection = cirProjections.Where(o => o.cir_group == grp).OrderByDescending(p => p.months).ToList();

                var maxMonth = _cirProjection.Count + (_cirProjection.Count * 0.5);
                for (int month = 1; month < maxMonth; month++)
                {
                    var row = new CIRProjections();
                    if (_cirProjection.Count > month)
                    {
                        row = _cirProjection[month - 1];
                    }
                    else
                    {
                        row = _cirProjection.LastOrDefault();
                    }


                    prevMonthValue = marginalAccumulativeFactor.FirstOrDefault(x => x.EirGroup == row.cir_group
                                                                                           && x.ProjectionMonth == month - 1).ProjectionValue;


                    month0Record = new IrFactor();
                    month0Record.EirGroup = row.cir_group;
                    month0Record.Rank = rank;
                    month0Record.ProjectionMonth = month;
                    month0Record.ProjectionValue = _irFactorWorkings.ComputeProjectionValue(row.value, month, prevMonthValue, FrameworkConstants.CIR, _cirProjection.Count);
                    marginalAccumulativeFactor.Add(month0Record);

                    
                }
                rank += 1;
            }
            return marginalAccumulativeFactor;
        }

        public List<CIRProjections> GetCirProjectionData()
        {
            var qry = Queries.EadCirProjections(this._eclId, this._eclType);
            var dt = DataAccess.i.GetData(qry);
            var cirProjectionData = new List<CIRProjections>();

            foreach (DataRow dr in dt.Rows)
            {
                cirProjectionData.Add(DataAccess.i.ParseDataToObject(new CIRProjections(), dr));
            }
            return cirProjectionData;
        }

        

        private List<SicrInputs> GetSircInputResult()
        {
            return _sicrInputs.GetSircInputResult();
        }

        public List<LifeTimeProjections> GetTempEadInputData(List<Loanbook_Data> loanbook)
        {
            var qry = Queries.EAD_GetLifeTimeProjections(this._eclId, this._eclType);
            var dt = DataAccess.i.GetData(qry);
            var lifeTimeProjections = new List<LifeTimeProjections>();

            foreach (DataRow dr in dt.Rows)
            {
                lifeTimeProjections.Add(DataAccess.i.ParseDataToObject(new LifeTimeProjections(), dr));
            }
            var lstContractId = loanbook.Select(o => o.ContractId).ToList();
            Log4Net.Log.Info("Completed GetTempEadInputData");
            return lifeTimeProjections.Where(o => lstContractId.Contains(o.Contract_no)).ToList();
        }

        public List<Refined_Raw_Retail_Wholesale> GetRefinedLoanBookData(List<Loanbook_Data> loanbook)
        {
            
            //var qry = Queries.Raw_Data(this._eclId, this._eclType);
            
            var lstRaw = loanbook;

            if(lstRaw==null)
            {
                lstRaw = new List<Loanbook_Data>();
            }
            //if(lstRaw.Count==0)
            //{
            //    Log4Net.Log.Info("Started");
            //    var _lstRaw = DataAccess.i.GetData(qry);
            //    Log4Net.Log.Info("Selected Raw Data from table");

            //    foreach (DataRow dr in _lstRaw.Rows)
            //    {
            //        lstRaw.Add(DataAccess.i.ParseDataToObject(new Loanbook_Data(), dr));
            //    }
            //}
            
            

            var refined_lstRaw = new ECLTasks(this._eclId, this._eclType).GenerateContractIdandRefinedData(lstRaw);

            return refined_lstRaw.Where(o=>!o.contract_no.Contains("EXP")).ToList();
        }

        private DateTime EndOfMonth(DateTime myDate, int numberOfMonths)
        {
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

        public List<Loanbook_Data> GetLoanBookData()
        {
            var qry = Queries.Raw_Data(this._eclId, this._eclType);
            Log4Net.Log.Info("Started");
            var _lstRaw = DataAccess.i.GetData(qry);
            Log4Net.Log.Info("Selected Raw Data from table");
            var lstRaw = new List<Loanbook_Data>();

            var bt_ead = new CalibrationInput_EAD_Behavioural_Terms_Processor();
            var bt_ead_data=bt_ead.GetBehaviouralData(this._eclId, this._eclType);
            foreach (DataRow dr in _lstRaw.Rows)
            {
                var loanRec = DataAccess.i.ParseDataToObject(new Loanbook_Data(), dr);

                loanRec.ContractId = loanRec.ContractId ?? "";
                loanRec.AccountNo = loanRec.AccountNo ?? "";

                loanRec.ContractId = loanRec.ContractId.Trim();
                loanRec.AccountNo = loanRec.AccountNo.Trim();

                double noOfMonths = 0;

                try
                {
                    if (loanRec.ContractEndDate != null)
                    {
                        var tmpEndMonth = loanRec.ContractEndDate;
                        var _EXP_EOMWithExpiryCalibration = EndOfMonth(tmpEndMonth.Value, int.Parse(Math.Ceiling(bt_ead_data.Expired).ToString()));
                        var EOMWithExpiryCalibration = EndOfMonth(tmpEndMonth.Value, int.Parse(Math.Ceiling(bt_ead_data.NonExpired).ToString()));

                        var EOM = EndOfMonth(tmpEndMonth.Value, 0);
                        if (loanRec.ContractEndDate < reportingDate && (loanRec.ProductType == ECLStringConstants.i._productType_od || loanRec.ProductType == ECLStringConstants.i._productType_card))
                        {
                            if (reportingDate == _EXP_EOMWithExpiryCalibration)
                            {
                                noOfMonths = 0;
                            }
                            else
                            {
                                if (reportingDate > _EXP_EOMWithExpiryCalibration)
                                    noOfMonths = Math.Round(Financial.YearFrac(_EXP_EOMWithExpiryCalibration,reportingDate, DayCountBasis.ActualActual) * 12, 0);
                                else
                                    noOfMonths = Math.Round(Financial.YearFrac(reportingDate,_EXP_EOMWithExpiryCalibration, DayCountBasis.ActualActual) * 12, 0);
                            }
                        }
                        else
                        {
                            if (loanRec.ProductType == ECLStringConstants.i._productType_od || loanRec.ProductType == ECLStringConstants.i._productType_card)
                            {
                                if (reportingDate == EOMWithExpiryCalibration)
                                    noOfMonths = 1;
                                else
                                {
                                    if (reportingDate > EOMWithExpiryCalibration)
                                        noOfMonths = Math.Round(Financial.YearFrac(EOMWithExpiryCalibration, reportingDate, DayCountBasis.ActualActual) * 12, 0);
                                    else
                                        noOfMonths = Math.Round(Financial.YearFrac(reportingDate,EOMWithExpiryCalibration, DayCountBasis.ActualActual) * 12, 0);
                                }
                            }
                            else
                            {
                                if (reportingDate == EOM)
                                    noOfMonths = 1;
                                else
                                {
                                    if (reportingDate > EOM)
                                        noOfMonths = Math.Round(Financial.YearFrac(EOM,reportingDate, DayCountBasis.ActualActual) * 12, 0);
                                    else
                                    {
                                        noOfMonths = Math.Round(Financial.YearFrac(reportingDate, EOM, DayCountBasis.ActualActual) * 12, 0);
                                    }
                                }

                            }
                            if(noOfMonths < 1.0)
                            {
                                noOfMonths = 0;
                            }
                        }
                    }
                }catch(Exception ex)
                {
                    var kk = ex;
                }

                //if (loanRec.RestructureEndDate != null && loanRec.RestructureIndicator)
                //{
                //    try
                //    {
                //        double noOfDays = (loanRec.RestructureEndDate.Value - reportingDate).Days;
                //        noOfMonths = Math.Ceiling(noOfDays * 12 / 365);
                //    }
                //    catch (Exception ex)
                //    {
                //        noOfMonths = 1;
                //        Log4Net.Log.Error(ex);
                //        //Log4Net.Log.Error(ex.ToString());
                //    }
                //}
                //else
                //{
                //    try
                //    {
                //        double noOfDays = (loanRec.ContractEndDate.Value - reportingDate).Days;
                //        noOfMonths = Math.Ceiling(noOfDays * 12 / 365);
                //    }
                //    catch (Exception ex)
                //    {
                //        noOfMonths = 1;
                //        Log4Net.Log.Error(ex);
                //        //Log4Net.Log.Error(ex.ToString());
                //    }
                //}

                //if(loanRec.ContractNo=="")
                loanRec.LIM_MONTH = noOfMonths;
                lstRaw.Add(loanRec);
            }
            lstRaw = lstRaw.OrderBy(o => o.AccountNo).ToList();
            return lstRaw;
        }

        protected double ComputeLifetimeValue(List<LifeTimeProjections> eadInputRecords, double eadInputRecord, List<IrFactor> accumlationFactor, long monthsPastDue, int months, int cirIndex, string productType)
        {
            if (productType.ToLower() != "loan" && productType.ToLower() != "lease" && productType.ToLower() != "mortgage")
                return eadInputRecord;
            else
            {
                double eadOffset = ComputeEadOffest(eadInputRecords, months, monthsPastDue);
                double multiplierValue = ComputeMultiplierValue(accumlationFactor, monthsPastDue, cirIndex, months);

                return eadOffset * multiplierValue;
            }

        }


        protected double ComputeEadOffest(List<LifeTimeProjections> eadInputRecords, int month, long monthsPastDue)
        {
            int temp1 = MPD_Default_Criteria - (int)monthsPastDue;
            int temp2 = month - Math.Max(temp1, 0);
            int offestMonth = Math.Max(temp2, 0);


            var r= eadInputRecords.FirstOrDefault(o=>o.Month==offestMonth);
            return r==null?0:r.Value;
        }
        protected double ComputeMultiplierValue(List<IrFactor> accumlationFactor, long monthsPastDue, int cirIndex, int month)
        {
            int temp1 = Math.Min(MPD_Default_Criteria - (int)monthsPastDue, month);
            int temp2 = Math.Abs(Math.Max(temp1, 1));
            int tempRow = cirIndex;
            int tempColumn = month;
            int tempHeight = temp2;
            var offsetvalues = accumlationFactor.Where(x => x.Rank == cirIndex
                                                         && (x.ProjectionMonth > 0 && x.ProjectionMonth <= temp2))
                                                .Select(x =>
                                                {
                                                    return x.ProjectionValue;
                                                }).ToArray();
            var product = offsetvalues.Aggregate(1.0, (acc, x) => acc * x);
            return monthsPastDue >= MPD_Default_Criteria ? 1 : product;
        }

        public List<EclAssumptions> GetECLFrameworkAssumptions()
        {
            var qry = Queries.eclFrameworkAssumptions(this._eclId, this._eclType);
            var dt = DataAccess.i.GetData(qry);
            var eclAssumptions = new List<EclAssumptions>();

            foreach (DataRow dr in dt.Rows)
            {
                eclAssumptions.Add(DataAccess.i.ParseDataToObject(new EclAssumptions(), dr));
            }

            return eclAssumptions;
        }
    }
}
