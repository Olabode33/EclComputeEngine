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

        List<IrFactor> cummulativeDiscountFactor = new List<IrFactor>();
        List<IrFactor> marginalDiscountFactor = new List<IrFactor>();
        public List<IrFactor> ComputeCummulativeDiscountFactor()
        {
            

            marginalDiscountFactor = ComputeMarginalDiscountFactor();




            var threads = marginalDiscountFactor.Count / 500;
            threads = threads + 1;

            var taskLst = new List<Task>();
            for (int i = 0; i < threads; i++)
            {
                var marg = marginalDiscountFactor.Skip(i * 500).Take(500).ToList();

                var task = Task.Run(() =>
                {
                    RunMarginalDiscountJob(marg);
                });
                taskLst.Add(task);
            }
            Log4Net.Log.Info($"Total Task : {taskLst.Count()}");

            var completedTask = taskLst.Where(o => o.IsCompleted).Count();
            


            var tskStatusLst = new List<TaskStatus> { TaskStatus.RanToCompletion, TaskStatus.Faulted };
            while (0 < 1)
            {
                if (taskLst.All(o => tskStatusLst.Contains(o.Status)))
                {
                    break;
                }
                //Do Nothing
            }

            //Task t = Task.WhenAll(taskLst);

            //try
            //{
            //    t.Wait();
            //}
            //catch (Exception ex)
            //{
            //    Log4Net.Log.Error(ex);
            //}
            //Log4Net.Log.Info($"All Task status: {t.Status}");

            //if (t.Status == TaskStatus.RanToCompletion)
            //{
            //    Log4Net.Log.Info($"All Task ran to completion");
            //}
            //if (t.Status == TaskStatus.Faulted)
            //{
            //    Log4Net.Log.Info($"All Task ran to fault");
            //}

            Log4Net.Log.Info($"Marginal Discount Task Completed: {completedTask}");

            return cummulativeDiscountFactor;
        }

        private void RunMarginalDiscountJob(List<IrFactor> sub_marginalDiscountFactor)
        {

            var itms = new List<IrFactor>();
            foreach (var row in sub_marginalDiscountFactor)
            {
                var dataRow = new IrFactor();
                dataRow.EirGroup = row.EirGroup;
                dataRow.Rank = row.Rank;
                dataRow.ProjectionMonth = row.ProjectionMonth;
                dataRow.ProjectionValue = ComputeCummulativeProjectionValue(marginalDiscountFactor, row.EirGroup, row.ProjectionMonth);

                itms.Add(dataRow);
            }
            lock(cummulativeDiscountFactor)
                cummulativeDiscountFactor.AddRange(itms);
        }

        public List<IrFactor> ComputeMarginalDiscountFactor()
        {
            var marginalDiscountFactor = new List<IrFactor>();
            try
            {
                
                var eirProjection = GetEirProjectionData();
                var eirProjectionCount = GetEirProjectionCount();

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

                    var _eirProjection = eirProjection.Where(o => o.eir_group == grp).OrderBy(p => p.months).ToList();
                    var maxMonth= eirProjectionCount + (eirProjectionCount * 0.5);
                    for (int month = 1; month < maxMonth; month++)
                    {
                        var row = new EIRProjections();
                        //if (_eirProjection.Count >= month)
                        //{
                        //    row = _eirProjection[month - 1];
                        //}
                        //else
                        //{
                        //    row = _eirProjection.LastOrDefault();
                        //}
                        row = _eirProjection.FirstOrDefault();

                        //********************************************************************
                        prevMonthValue = marginalDiscountFactor.FirstOrDefault(x => x.EirGroup == row.eir_group
                                                                                               && x.ProjectionMonth == month-1).ProjectionValue;
                        //&& x.ProjectionMonth == row.months).ProjectionValue;


                        month0Record = new IrFactor();
                        month0Record.EirGroup = row.eir_group;
                        month0Record.Rank = rank;
                        month0Record.ProjectionMonth = month;
                        month0Record.ProjectionValue = ComputeProjectionValue(row.value, month, prevMonthValue, EIR_TYPE, eirProjectionCount);
                        marginalDiscountFactor.Add(month0Record);

                        rank += 1;
                    }

                }
            }
            catch(Exception ex)
            {
                Log4Net.Log.Error(ex);
                var cc = ex;
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

        private int GetEirProjectionCount()
        {
            var qry = Queries.EAD_GetEIRProjectionsCount(this._eclId, this._eclType);
            var dt = DataAccess.i.GetData(qry);
            
            return Convert.ToInt32(dt.Rows[0][0]);
        }

        public double ComputeProjectionValue(double projectionValue, int month, double prevValue, string type = CIR_TYPE, double lim_months=1)
        {
            if (month > lim_months)
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
