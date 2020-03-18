using IFRS9_ECL.Data;
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
    public class IrFactorWorkings
    {
        private const string EIR_TYPE = FrameworkConstants.EIR;
        private const string CIR_TYPE = FrameworkConstants.CIR;

        Guid _eclId;
        EclType _eclType;
        public IrFactorWorkings(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
        }

        public List<IrFactor> Run()
        {
            var dataTable = ComputeCummulativeDiscountFactor();
            string stop = "Ma te";
            return dataTable;
        }

        public List<IrFactor> ComputeCummulativeDiscountFactor()
        {
            var cummulativeDiscountFactor = new List<IrFactor>();

            var marginalDiscountFactor = ComputeMarginalDiscountFactor();

            foreach (var row in marginalDiscountFactor)
            {
                var dataRow = new IrFactor();
                dataRow.EirGroup = row.EirGroup;
                dataRow.Rank = row.Rank;
                dataRow.ProjectionMonth = row.ProjectionMonth;
                dataRow.ProjectionValue = ComputeCummulativeProjectionValue(marginalDiscountFactor, row.EirGroup, row.ProjectionMonth);

                cummulativeDiscountFactor.Add(dataRow);
            }

            return cummulativeDiscountFactor;
        }

        public List<IrFactor> ComputeMarginalDiscountFactor()
        {
            var marginalDiscountFactor = new List<IrFactor>();

            var eirProjection = GetEirProjectionData();

            var groups = eirProjection.Select(o => o.eir_group).Distinct();


            int rank = 1;
            double prevMonthValue = 0.0;


            foreach (var grp in groups)
            {
                var month0Record = new IrFactor();
                month0Record.EirGroup = grp;
                month0Record.Rank = rank;
                month0Record.ProjectionMonth = 0;
                month0Record.ProjectionValue = 1.0;
                marginalDiscountFactor.Add(month0Record);

                var _eirProjection = eirProjection.Where(o => o.eir_group == grp).OrderByDescending(p=>p.months).ToList();

                for (int month = 1; month < FrameworkConstants.MaxIrFactorProjectionMonths; month++)
                {
                    var row = new EIRProjections();
                    if (_eirProjection.Count>=month)
                    {
                        row = _eirProjection[month - 1];
                    }
                    else
                    {
                        row = _eirProjection.LastOrDefault();
                    }
                    

                    prevMonthValue = marginalDiscountFactor.FirstOrDefault(x => x.EirGroup == row.eir_group
                                                                                           && x.ProjectionMonth == row.months).ProjectionValue;


                    month0Record = new IrFactor();
                    month0Record.EirGroup = row.eir_group;
                    month0Record.Rank = rank;
                    month0Record.ProjectionMonth = month;
                    month0Record.ProjectionValue = ComputeProjectionValue(row.value, month, prevMonthValue, EIR_TYPE);
                    marginalDiscountFactor.Add(month0Record);

                    rank += 1;
                }

            }


            return marginalDiscountFactor;
        }

        protected double ComputeCummulativeProjectionValue(List<IrFactor> marginalDiscountFactor, string eirGroup, int month)
        {
            var range = marginalDiscountFactor.AsEnumerable()
                            .Where(x => x.EirGroup == eirGroup
                                                && x.ProjectionMonth <= month)
                            .Select(x => {
                                return x.ProjectionValue;
                            }).ToArray();
            var aggr = range.Aggregate(1.0, (acc, x) => acc * x);

            return aggr;
        }

        private List<EIRProjections> GetEirProjectionData()
        {
            var qry=Queries.EAD_GetEIRProjections(this._eclId, this._eclType);
            var dt=DataAccess.i.GetData(qry);
            var eIRProjections = new List<EIRProjections>();

            foreach (DataRow dr in dt.Rows)
            {
                eIRProjections.Add(DataAccess.i.ParseDataToObject(new EIRProjections(), dr));
            }

            return eIRProjections;
        }

        public double ComputeProjectionValue(double projectionValue, int month, double prevValue, string type = CIR_TYPE)
        {
            if (month > FrameworkConstants.TempExcelVariable_LIM_MONTH)
            {
                return prevValue;
            }
            else
            {
                return Math.Pow(1.0 + projectionValue, (type == EIR_TYPE ? -1.0 : 1.0) / 12.0);
            }
        }
    }
}
