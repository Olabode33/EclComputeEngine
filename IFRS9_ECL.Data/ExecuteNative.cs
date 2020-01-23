using IFRS9_ECL.Models;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace IFRS9_ECL.Data
{
    public static class ExecuteNative
    {

        public static string SaveEIRProjections(List<EIRProjections> d, Guid master_G, EclType eclType)
        {
            //truncate table
            //var qry = $"truncate table {ECLStringConstants.i.EadEirProjections_Table(eclType)}";
            //var tR = DataAccess.i.ExecuteQuery(qry);

            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(Guid));
            dt.Columns.Add("EIR_Group");
            dt.Columns.Add("Month", typeof(int));
            dt.Columns.Add("Value", typeof(float));
            dt.Columns.Add("WholesaleEclId", typeof(Guid));

            foreach (var _d in d)
            {
                var g = Guid.NewGuid();
                dt.Rows.Add(new object[]
                    {
                            g,_d.eir_group, _d.months, _d.value, master_G.ToString()
                    });
            }
            var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.EadEirProjections_Table(eclType));

            return r > 0 ? "" : $"Could not Bulk Insert [{ECLStringConstants.i.EadEirProjections_Table(eclType)}]";

        }

        public static string SaveLGDAccountdata(List<LGDAccountData> d, Guid masterGuid, EclType eclType)
        {
            //truncate table
            //var qry = $"truncate table {ECLStringConstants.i.LGDAccountData_Table(eclType)}";
            //var tR = DataAccess.i.ExecuteQuery(qry);

            //if (tR >= 0)
            //{
                var c = new LGDAccountData();

                Type myObjOriginalType = c.GetType();
                PropertyInfo[] myProps = myObjOriginalType.GetProperties();

                var dt = new DataTable();
                for (int i = 0; i < myProps.Length; i++)
                {
                    dt.Columns.Add(myProps[i].Name, myProps[i].PropertyType);
                }
                dt.Columns.Add("WholesaleEclId", typeof(Guid));


                foreach (var _d in d)
                {
                    _d.Id = Guid.NewGuid();
                    dt.Rows.Add(new object[]
                        {
                            _d.Id, _d.CONTRACT_NO, _d.TTR_YEARS, _d.COST_OF_RECOVERY, _d.GUARANTOR_PD, _d.GUARANTOR_LGD, _d.GUARANTEE_VALUE, _d.GUARANTEE_LEVEL, masterGuid
                        });
                }
                var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.LGDAccountData_Table(eclType));

                return r > 0 ? "" : $"Could not Bulk Insert [{ECLStringConstants.i.LGDAccountData_Table(eclType)}]";
            //}

            //return $"Could not Truncate Table [{ECLStringConstants.i.LGDAccountData_Table(eclType)}]";
        }

        public static string SaveLGDCollaterals(List<LGDCollateralData> d, Guid masterGuid, EclType eclType)
        {
            //truncate table
            //var qry = $"truncate table {ECLStringConstants.i.LGDCollateral_Table(eclType)}";
            //var tR = DataAccess.i.ExecuteQuery(qry);

            //if (tR >= 0)
            //{
                var c = new LGDCollateralData();

                Type myObjOriginalType = c.GetType();
                PropertyInfo[] myProps = myObjOriginalType.GetProperties();

                var dt = new DataTable();

                for (int i = 0; i < myProps.Length; i++)
                {
                    dt.Columns.Add(myProps[i].Name, myProps[i].PropertyType);
                }

                dt.Columns.Add("WholesaleEclId", typeof(Guid));


                foreach (var _d in d)
                {
                    _d.Id = Guid.NewGuid();
                    dt.Rows.Add(new object[]
                        {
                            _d.Id, _d.contract_no, _d.customer_no, _d.debenture_omv, _d.cash_omv, _d.inventory_omv, _d.plant_and_equipment_omv, _d.residential_property_omv, _d.commercial_property_omv, _d.receivables_omv, _d.shares_omv, _d.vehicle_omv, _d.total_omv, _d.debenture_fsv
                            ,_d.cash_fsv, _d.inventory_fsv, _d.plant_and_equipment_fsv, _d.residential_property_fsv, _d.commercial_property_fsv, _d.receivables_fsv, _d.shares_fsv, _d.vehicle_fsv, masterGuid
                        });
                }
                var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.LGDCollateral_Table(eclType));

                return r > 0 ? "" : $"Could not Bulk Insert [{ECLStringConstants.i.LGDCollateral_Table(eclType)}]";
            //}

            //return $"Could not Truncate Table [{ECLStringConstants.i.LGDCollateral_Table(eclType)}]";
        }

        public static string SaveCIRProjections(List<CIRProjections> d, Guid master_G, EclType eclType)
        {
            //truncate table
            //var qry = $"truncate table {ECLStringConstants.i.EadCirProjections_Table(eclType)}";
            //var tR = DataAccess.i.ExecuteQuery(qry);

            //if (tR >= 0)
            //{

                var dt = new DataTable();
                dt.Columns.Add("Id", typeof(Guid));
                dt.Columns.Add("CIR_Group");
                dt.Columns.Add("Month", typeof(int));
                dt.Columns.Add("Value", typeof(float));
                dt.Columns.Add("CIR_EFFECTIVE", typeof(float));
                dt.Columns.Add("WholesaleEclId", typeof(Guid));

                foreach (var _d in d)
                {
                    var g = Guid.NewGuid();
                    dt.Rows.Add(new object[]
                        {
                            g, _d.cir_group, _d.months, _d.value, _d.cir_effective, master_G.ToString()
                        });
                }
                var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.EadCirProjections_Table(eclType));

                return r > 0 ? "" : $"Could not Bulk Insert [{ECLStringConstants.i.EadCirProjections_Table(eclType)}]";
        //}

        //    return $"Could not Truncate Table [{ECLStringConstants.i.EadCirProjections_Table(eclType)}]";
        }

        public static string SaveLifeTimeProjections(List<LifeTimeProjections> d, Guid master_G, EclType eclType)
        {
            //truncate table
            //var qry = $"truncate table {ECLStringConstants.i.EadLifetimeProjections_Table(eclType)}";
            //var tR = DataAccess.i.ExecuteQuery(qry);

            //if (tR >= 0)
            //{
                var dt = new DataTable();
                dt.Columns.Add("Id", typeof(Guid));
                dt.Columns.Add("Contract_no");
                dt.Columns.Add("Eir_Group");
                dt.Columns.Add("Cir_Group");
                dt.Columns.Add("Month", typeof(int));
                dt.Columns.Add("Value", typeof(float));
                dt.Columns.Add("WholesaleEclId", typeof(Guid));

                foreach (var _d in d)
                {
                    var g = Guid.NewGuid();
                    dt.Rows.Add(new object[]
                        {
                            g, _d.Contract_no,_d.Eir_Group,_d.Cir_Group, _d.Month, _d.Value, master_G.ToString()
                        });
                }
                var r = DataAccess.i.ExecuteBulkCopy(dt, ECLStringConstants.i.EadLifetimeProjections_Table(eclType));

                return r > 0 ? "" : $"Could not Bulk Insert [{ECLStringConstants.i.EadLifetimeProjections_Table(eclType)}]";
            //}

            //return $"Could not Truncate Table [{ECLStringConstants.i.EadLifetimeProjections_Table(eclType)}]";
        }
    }
}
