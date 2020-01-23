using IFRS9_ECL.Core.PDComputation;
using IFRS9_ECL.Data;
using IFRS9_ECL.Models.PD;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Core
{
    public class ProcessECL_PD
    {
        
        Guid _eclId;
        EclType _eclType;
        public ProcessECL_PD(Guid eclId, EclType eclType)
        {
            this._eclId = eclId;
            this._eclType = eclType;
        }
        public void ProcessTask()
        {
            // Compute Credit Index
            var crdIndx = new CreditIndex(this._eclId, this._eclType);
            crdIndx.Run();


            // Compute PD mapping
            var pDMapping = new PDMapping(this._eclId, this._eclType);
            pDMapping.Run();

            // Compute Scenario Life time Pd -- best
            var slt_Pd_b = new ScenarioLifetimePd(Util.ECL_Scenario.Best, this._eclId, this._eclType);
            slt_Pd_b.Run();

            // Compute Scenario Redefault Lifetime Pds  -- best
            var sRedefault_lt_pd_b = new ScenarioRedefaultLifetimePds(Util.ECL_Scenario.Best, this._eclId, this._eclType);
            sRedefault_lt_pd_b.Run();

            // Compute Scenario Marginal Pd -- best
            //var sMarginal_Pd_o = new ScenarioMarginalPd(Util.ECL_Scenario.Optimistic, this._eclId);
            //sMarginal_Pd_o.Run();

            // Compute Scenario Life time Pd -- Optimistic
            var slt_Pd_o = new ScenarioLifetimePd(Util.ECL_Scenario.Optimistic, this._eclId, this._eclType);
            slt_Pd_o.Run();

            // Compute Scenario Redefault Lifetime Pds  -- Optimistic
            var sRedefault_lt_pd_o = new ScenarioRedefaultLifetimePds(Util.ECL_Scenario.Optimistic, this._eclId, this._eclType);
            sRedefault_lt_pd_o.Run();


            // Compute Scenario Marginal Pd -- Optimistic
            //var sMarginal_Pd_de = new ScenarioMarginalPd(Util.ECL_Scenario.Downturn, this._eclId);
            //sMarginal_Pd_de.Run();

            // Compute Scenario Life time Pd -- Downturn
            var slt_Pd_de = new ScenarioLifetimePd(Util.ECL_Scenario.Downturn, this._eclId, this._eclType);
            slt_Pd_de.Run();

            // Compute Scenario Redefault Lifetime Pds  -- Downturn
            var sRedefault_lt_pd_de = new ScenarioRedefaultLifetimePds(Util.ECL_Scenario.Downturn, this._eclId, this._eclType);
            sRedefault_lt_pd_de.Run();



            // Compute Sicr Input Workings
            //var vasicekWorkings = new VasicekWorkings(this._eclId);
            //vasicekWorkings.Run();
        }




        public List<PDI_Assumptions> Get_PDI_Assumptions()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_pdInputAssumptionsQuery(this._eclId, this._eclType));
            var data = new List<PDI_Assumptions>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PDI_Assumptions(), dr);
                itm.PdGroup=PDInputs.GetPDAssumptionEnum(int.Parse(dr["PdGroup"].ToString()));
                data.Add(itm);
            }
            return data;
        }

        public List<PDI_MacroEconomicProjections> Get_PDI_MacroEconomicProjections()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_MacroEconomicProjections);
            var data = new List<PDI_MacroEconomicProjections>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PDI_MacroEconomicProjections(), dr);
                data.Add(itm);
            }
            return data;
        }

        public List<PDI_MacroEconomics> Get_PDI_MacroEconomics()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_macroEconomicsQuery(this._eclId, this._eclType));
            var data = new List<PDI_MacroEconomics>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PDI_MacroEconomics(), dr);
                data.Add(itm);
            }
            return data;
        }



        public List<PDI_HistoricIndex> Get_PDI_HistoricIndex()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_historicIndexQuery(this._eclId, this._eclType));
            var data = new List<PDI_HistoricIndex>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PDI_HistoricIndex(), dr);
                data.Add(itm);
            }
            return data;
        }

        //public List<PDI_MacroEcoBest> Get_PDI_MacroEcoBest()
        //{
        //    return new List<PDI_MacroEcoBest>();
        //}

        //public List<PDI_MacroEcoDownturn> Get_PDI_MacroEcoDownturn()
        //{
        //    return new List<PDI_MacroEcoDownturn>();
        //}

        //public List<PDI_MacroEcoOptimisit> Get_PDI_MacroEcoOptimisit()
        //{
        //    return new List<PDI_MacroEcoOptimisit>();
        //}

        public List<PdInputAssumptionNonInternalModels> Get_PDI_NonInternalModelInputs(int MonthId=0)
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_nonInternalmodelInputQuery(this._eclId, this._eclType, MonthId));//(this._eclId)); ;
            var data = new List<PdInputAssumptionNonInternalModels>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PdInputAssumptionNonInternalModels(), dr);
                data.Add(itm);
            }
            return data;
        }

        public List<PDI_SnPCummlativeDefaultRate> Get_PDI_SnPCummlativeDefaultRate()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_snpCummulativeDefaultRateQuery(this._eclId, this._eclType));
            var data = new List<PDI_SnPCummlativeDefaultRate>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PDI_SnPCummlativeDefaultRate(), dr);
                data.Add(itm);
            }
            return data;
        }

        public List<PDI_StatisticalInputs> Get_PDI_StatisticalInputs()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_statisticalInputsQuery(this._eclId, this._eclType));
            var data = new List<PDI_StatisticalInputs>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PDI_StatisticalInputs(), dr);
                data.Add(itm);
            }
            return data;
        }

        public List<PDI_ETI_NPL> Get_PDI_ETI_NPL()
        {
            var dt = DataAccess.i.GetData(PD_Queries.Get_etiNplQuery(this._eclId, this._eclType));
            var data = new List<PDI_ETI_NPL>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = DataAccess.i.ParseDataToObject(new PDI_ETI_NPL(), dr);
                data.Add(itm);
            }
            return data;
        }

        //public List<WholesalePdLifetimeBests> Get_WholesalePdLifetimeBests()
        //{
        //    var dt = DataAccess.i.GetData(PD_Queries.Get_statisticalInputsQuery(this._eclId));
        //    var data = new List<WholesalePdLifetimeBests>();
        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        var itm = DataAccess.i.ParseDataToObject(new WholesalePdLifetimeBests(), dr);
        //        data.Add(itm);
        //    }
        //    return data;
        //}

        //public List<WholesalePdLifetimeDownturns> Get_WholesalePdLifetimeDownturns()
        //{
        //    return new List<WholesalePdLifetimeDownturns>();
        //}

        //public List<WholesalePdLifetimeOptimistics> Get_WholesalePdLifetimeOptimistics()
        //{
        //    return new List<WholesalePdLifetimeOptimistics>();
        //}

        //public List<WholesalePdMappings> Get_WholesalePdMappings()
        //{
        //    return new List<WholesalePdMappings>();
        //}

        //public List<WholesalePdRedefaultLifetimeBests> Get_WholesalePdRedefaultLifetimeBests()
        //{
        //    return new List<WholesalePdRedefaultLifetimeBests>();
        //}

        //public List<WholesalePdRedefaultLifetimeDownturns> Get_WholesalePdRedefaultLifetimeDownturns()
        //{
        //    return new List<WholesalePdRedefaultLifetimeDownturns>();
        //}

        //public List<WholesalePdRedefaultLifetimeOptimistics> WholesalePdRedefaultLifetimeOptimistics()
        //{
        //    return new List<WholesalePdRedefaultLifetimeOptimistics>();
        //}
    }
}
