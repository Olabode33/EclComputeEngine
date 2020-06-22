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
using System.Linq;


namespace IFRS9_ECL.Core.FrameworkComputation
{
    public class LifetimeEadWorkings
    {

        Guid _eclId;
        protected IrFactorWorkings _irFactorWorkings;
        protected SicrInputWorkings _sicrInputs;
        protected EclType _eclType;
        ProcessECL_LGD _processECL_LGD;
        public LifetimeEadWorkings(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
            _irFactorWorkings = new IrFactorWorkings(_eclId, _eclType);
            _sicrInputs = new SicrInputWorkings(this._eclId, _eclType);
            _processECL_LGD = new ProcessECL_LGD(eclId, eclType);
        }


        public void Run()
        {
            //var dataTable = ComputeLifetimeEad();
            string stop = "Ma te";
        }

        public List<LifetimeEad> ComputeLifetimeEad(List<Loanbook_Data> loanbook)
        {
            var lifetimeEad = new List<LifetimeEad>();

            var eadInputs = GetTempEadInputData(loanbook);
            var sircInputs = GetSircInputResult();
            var contractData = _processECL_LGD.GetLgdContractData(loanbook);
            var marginalAccumulationFactor = GetMarginalAccumulationFactorResult();

            var refined_Raw_Data = GetRefinedLoanBookData(loanbook);

            var loanbook_contractNo = refined_Raw_Data.Select(o => o.contract_no).ToList();

            var contract_nos = eadInputs.Where(n=>loanbook_contractNo.Contains(n.Contract_no)).Select(o => o.Contract_no).Distinct().ToList();

            

            foreach (var contract_no in contract_nos)
            {
                var c_eadInputs = eadInputs.Where(c => c.Contract_no == contract_no).ToList();

                string contractId = contract_no;

                int cirIndex = 1;
                try { cirIndex = marginalAccumulationFactor.FirstOrDefault(o => o.CirGroup == c_eadInputs[0].Cir_Group).Rank; } catch { };
                string productType = refined_Raw_Data.FirstOrDefault(x => x.contract_no == contractId).product_type;
                var sirc = sircInputs.FirstOrDefault(x => x.ContractId == contractId);
                long? daysPastDue = sirc == null ? 0 : sirc.DaysPastDue;

                var month0Record = new LifetimeEad();
                month0Record.ContractId = contractId;
                month0Record.CirIndex = cirIndex;
                month0Record.ProductType = productType;
                month0Record.MonthsPastDue = (daysPastDue == null ? 0 : daysPastDue / 30) ?? 0;
                month0Record.ProjectionMonth = 0;
                month0Record.ProjectionValue = c_eadInputs[0].Value;
                lifetimeEad.Add(month0Record);

                for (int month = 1; month < FrameworkConstants.TempExcelVariable_LIM_MONTH; month++)
                {
                    var itm = new LifeTimeProjections();
                    if (c_eadInputs.Count > month)
                    {
                        itm = c_eadInputs[month - 1];
                    }
                    else
                    {
                        itm = c_eadInputs.LastOrDefault();
                    }
                    var newRecord = new LifetimeEad();
                    newRecord.ContractId = contractId;
                    newRecord.CirIndex = cirIndex;
                    newRecord.ProductType = productType;
                    newRecord.MonthsPastDue = (daysPastDue / 30) ?? 0;
                    newRecord.ProjectionMonth = month;
                    newRecord.ProjectionValue = ComputeLifetimeValue(c_eadInputs, itm, marginalAccumulationFactor, (long)daysPastDue / 30, month, cirIndex, productType);
                    lifetimeEad.Add(newRecord);
                }


            }
            //var contractIds = refined_Raw_Data.Select(o => o.contract_no).ToList();
            return lifetimeEad;//.Where(o=> contractIds.Contains(o.ContractId)).ToList();
        }

        private List<IrFactor> GetMarginalAccumulationFactorResult()
        {
            var marginalAccumulativeFactor = new List<IrFactor>();

            var cirProjections = GetCirProjectionData();
           
            var groups = cirProjections.Select(o => o.cir_group).Distinct();


            int rank = 1;
            double prevMonthValue = 0.0;


            foreach (var grp in groups)
            {
                var month0Record = new IrFactor();
                month0Record.EirGroup = grp;
                month0Record.Rank = rank;
                month0Record.ProjectionMonth = 0;
                month0Record.ProjectionValue = 1.0;
                marginalAccumulativeFactor.Add(month0Record);

                var _cirProjection = cirProjections.Where(o => o.cir_group == grp).OrderByDescending(p => p.months).ToList();

                for (int month = 1; month < FrameworkConstants.MaxIrFactorProjectionMonths; month++)
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
                    month0Record.ProjectionValue = _irFactorWorkings.ComputeProjectionValue(row.value, month, prevMonthValue, FrameworkConstants.CIR);
                    marginalAccumulativeFactor.Add(month0Record);

                    rank += 1;
                }

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
            return lifeTimeProjections.Where(o => lstContractId.Contains(o.Contract_no)).ToList();
        }

        public List<Refined_Raw_Retail_Wholesale> GetRefinedLoanBookData(List<Loanbook_Data> loanbook)
        {
            
            var qry = Queries.Raw_Data(this._eclId, this._eclType);
            
            var lstRaw = loanbook;

            if(lstRaw==null)
            {
                lstRaw = new List<Loanbook_Data>();
            }
            if(lstRaw.Count==0)
            {
                Console.WriteLine("Started");
                var _lstRaw = DataAccess.i.GetData(qry);
                Console.WriteLine("Selected Raw Data from table");

                foreach (DataRow dr in _lstRaw.Rows)
                {
                    lstRaw.Add(DataAccess.i.ParseDataToObject(new Loanbook_Data(), dr));
                }
            }
            
            Console.WriteLine("Completed pass raw data to object");

            var refined_lstRaw = new ECLTasks(this._eclId, this._eclType).GenerateContractIdandRefinedData(lstRaw);

            return refined_lstRaw.Where(o=>!o.contract_no.Contains("EXP")).ToList();
        }

        public List<Loanbook_Data> GetLoanBookData()
        {
            var qry = Queries.Raw_Data(this._eclId, this._eclType);
            Console.WriteLine("Started");
            var _lstRaw = DataAccess.i.GetData(qry);
            Console.WriteLine("Selected Raw Data from table");
            var lstRaw = new List<Loanbook_Data>();
            foreach (DataRow dr in _lstRaw.Rows)
            {
                lstRaw.Add(DataAccess.i.ParseDataToObject(new Loanbook_Data(), dr));
            }
           
            return lstRaw;
        }

        protected double ComputeLifetimeValue(List<LifeTimeProjections> eadInputRecords, LifeTimeProjections eadInputRecord, List<IrFactor> accumlationFactor, long monthsPastDue, int months, int cirIndex, string productType)
        {
            if (productType.ToLower() != "loan" && productType.ToLower() != "lease" && productType.ToLower() != "mortgage")
                return eadInputRecord.Month;
            else
            {
                double eadOffset = ComputeEadOffest(eadInputRecords, eadInputRecord, months, monthsPastDue);
                double multiplierValue = ComputeMultiplierValue(accumlationFactor, monthsPastDue, cirIndex, months);

                return eadOffset * multiplierValue;
            }

        }


        protected double ComputeEadOffest(List<LifeTimeProjections> eadInputRecords, LifeTimeProjections eadInputRecord, int month, long monthsPastDue)
        {
            int temp1 = FrameworkConstants.TempExcelVariable_MPD_DEFAULT_CRITERIA - (int)monthsPastDue;
            int temp2 = month - Math.Max(temp1, 0);
            int offestMonth = Math.Max(temp2, 0);


            var r= eadInputRecords.FirstOrDefault(o=>o.Month==offestMonth);
            return r==null?0:r.Value;
        }
        protected double ComputeMultiplierValue(List<IrFactor> accumlationFactor, long monthsPastDue, int cirIndex, int month)
        {
            int temp1 = Math.Min(FrameworkConstants.TempExcelVariable_MPD_DEFAULT_CRITERIA - (int)monthsPastDue, month);
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
            return monthsPastDue >= FrameworkConstants.TempExcelVariable_MPD_DEFAULT_CRITERIA ? 1 : product;
        }
    }
}
