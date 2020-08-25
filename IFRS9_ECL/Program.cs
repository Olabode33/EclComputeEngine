using IFRS9_ECL.Core;
using IFRS9_ECL.Core.FrameworkComputation;
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

namespace IFRS9_ECL
{
    class Program
    {

        static void Main(string[] args)
        {
            //var eclId = "9ec1ae81-a13f-4c90-4bba-08d83eb190fd";
            //Read_EAD_Input(eclId);
            //Read_LGD_AccountData(eclId);
            //Read_LGD_CollateralData(eclId);
            //Read_PD_Mapping(eclId);
            //Read_EAD_Impairment(eclId);
            //Read_LGD_Impairment(eclId);
            //Read_PD_Impairment(eclId);
            //Console.ReadKey();


            //Log4Net.Log.Info($"End Time {DateTime.Now}");
            //Log4Net.Log.Info("Done Done Done");
            ////Console.ReadKey();
            // return;
        }

        public void AssumptionExtraction()
        {

        }

        private static void Read_PD_Mapping(string eclId)
        {
            var qry = $"SELECT ContractId,PdGroup,TtmMonths,MaxDpd,MaxClassificationScore,Pd12Month,LifetimePd,RedefaultLifetimePd,Stage1Transition,Stage2Transition,DaysPastDue from WholesalePdMappings where WholesaleEclId='{eclId}'";
            var dt = Data.DataAccess.i.GetData(qry);
            var basepath = AppDomain.CurrentDomain.BaseDirectory;
            var fpath = Path.Combine(basepath, $"PD_Mapping.csv");
            ToCSV(dt, fpath);
        }

        private static void Read_LGD_CollateralData(string eclId)
        {
            var qry = $"SELECT Month,CollateralProjectionType,Debenture,Cash,Inventory,Plant_And_Equipment,Residential_Property,Commercial_Property,Receivables,Shares,Vehicle FROM WholesaleLgdCollateralProjection where WholesaleEclId='{eclId}'";
            var dt = Data.DataAccess.i.GetData(qry);
            var basepath = AppDomain.CurrentDomain.BaseDirectory;
            var fpath = Path.Combine(basepath, $"LGD_CollateralData.csv");
            ToCSV(dt, fpath);
        }


        private static void Read_LGD_AccountData(string eclId)
        {
            var qry = $"SELECT CONTRACT_NO,TTR_YEARS,COST_OF_RECOVERY,GUARANTOR_PD,GUARANTOR_LGD,GUARANTEE_VALUE,GUARANTEE_LEVEL FROM WholesaleLGDAccountData where WholesaleEclId='{eclId}'";
            var dt = Data.DataAccess.i.GetData(qry);
            var basepath = AppDomain.CurrentDomain.BaseDirectory;
            var fpath = Path.Combine(basepath, $"LGD_AccountData.csv");
            ToCSV(dt, fpath);
        }

        public static void ToCSV(DataTable dtDataTable, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            //headers  
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }

        private static void Read_PD_Impairment(string eclId)
        {
            //var eclId = "9ec1ae81-a13f-4c90-4bba-08d83eb190fd";
            var basepath = AppDomain.CurrentDomain.BaseDirectory;

            for(int l=0; l<3; l++)
            {
                var sc = "";
                if(l==0)
                {
                    sc = "Bests";
                }
                if (l == 1)
                {
                    sc = "Optimistics";
                }
                if (l == 2)
                {
                    sc = "Downturns";
                }

                var qry = $"select PdGroup, Month, Value from WholesalePdLifetime{sc} where WholesaleEclId='{eclId}'";


                var dt = Data.DataAccess.i.GetData(qry);

                var eadInput = new List<LifeTimeObject>();
                foreach (DataRow dr in dt.Rows)
                {
                    eadInput.Add(Data.DataAccess.i.ParseDataToObject(new LifeTimeObject(), dr));
                }
                var maxMonth = eadInput.Max(o => o.Month);
                eadInput = eadInput.OrderBy(o => o.PdGroup).ThenBy(p => p.Month).ToList();
                var sb = new StringBuilder();
                var header = "Group,";

                for (int i = 1; i <= maxMonth; i++)
                {
                    header = $"{header}{i},";
                }
                header = header.Trim(',');
                header = $"{header}{Environment.NewLine}";

                sb.Append(header);
                var pdgroup = eadInput.Select(o => o.PdGroup).Distinct().ToList();

                var cnt = 1;
                foreach (var _pdgroup in pdgroup)
                {
                    Console.WriteLine($"{cnt} - {_pdgroup}");
                    var pdgroupData = eadInput.Where(o => o.PdGroup == _pdgroup).OrderBy(p => p.Month).ToList();

                    var pdgroupLine = $"{_pdgroup},";

                    foreach (var monthVal in pdgroupData)
                    {
                        Console.WriteLine($"{cnt} - {_pdgroup}-{monthVal.Month}");
                        pdgroupLine = $"{pdgroupLine}{monthVal.Value},";
                    }
                    pdgroupLine = pdgroupLine.Trim(',');
                    pdgroupLine = $"{pdgroupLine}{Environment.NewLine}";

                    sb.Append(pdgroupLine);
                    cnt = cnt + 1;
                }
                var fpath = Path.Combine(basepath, $"LifetimePD_{sc}.csv");
                File.WriteAllText(fpath, sb.ToString());



            }


            Console.WriteLine("Done Read_EAD_Input");
        }

        public static void Read_EAD_Input(string eclId)
        {

            var basepath = AppDomain.CurrentDomain.BaseDirectory;
            var qry = $"select Contract_no, Month, Value, Eir_Group, Cir_Group from WholesaleEadLifetimeProjections where WholesaleEclId='{eclId}'";

            var dt = Data.DataAccess.i.GetData(qry);

            var eadInput = new List<LifeTimeProjections>();
            foreach (DataRow dr in dt.Rows)
            {
                eadInput.Add(Data.DataAccess.i.ParseDataToObject(new LifeTimeProjections(), dr));
            }
            var maxMonth = eadInput.Max(o => o.Month);
            eadInput = eadInput.OrderBy(o => o.Contract_no).ThenBy(p => p.Month).ThenBy(q => q.Value).ToList();
            var sb = new StringBuilder();
            var header = "Contract_No,CIR,EIR,";

            for (int i = 0; i <= maxMonth; i++)
            {
                header = $"{header}{i},";
            }
            header = header.Trim(',');
            header = $"{header}{Environment.NewLine}";

            sb.Append(header);
            var distinctCOntracts = eadInput.Select(o => o.Contract_no).Distinct().ToList();

            var cnt = 1;
            foreach (var contract in distinctCOntracts)
            {
                Console.WriteLine($"{cnt} - {contract}");
                var contractData = eadInput.Where(o => o.Contract_no == contract).OrderBy(p => p.Month).ToList();
                var cir = contractData.FirstOrDefault().Cir_Group;
                var eir = contractData.FirstOrDefault().Eir_Group;
                var contractLine = $"{contract},{cir},{eir},";

                foreach (var monthVal in contractData)
                {
                    Console.WriteLine($"{cnt} - {contract}-{monthVal.Month}");
                    contractLine = $"{contractLine}{monthVal.Value},";
                }
                contractLine = contractLine.Trim(',');
                contractLine = $"{contractLine}{Environment.NewLine}";

                sb.Append(contractLine);
                cnt = cnt + 1;
            }
            var fpath = Path.Combine(basepath, $"EAD_Input_LifeTimeEAD.csv");
            File.WriteAllText(fpath, sb.ToString());



            Console.WriteLine("Done Read_EAD_Input");
        }


        public static void Read_EAD_Impairment(string eclId)
        {
            
            var basepath = AppDomain.CurrentDomain.BaseDirectory;
           // var eadPathCsv = @"C:\PwC\Projects\SourceCode\Firs_9_ECL\Code\IFRS_Test1\bin\Debug\EADOutput.csv";
            var eadPathCsv = Path.Combine(basepath, $"EADOutput.csv");
            var csvrows = File.ReadAllLines(eadPathCsv);

            var lifetimeEad = new List<LifetimeEad>();
            //sb.Append($"{itm.ContractId},{itm.ProjectionMonth},{itm.ProjectionValue},{Environment.NewLine}");
            for (int i=1; i< csvrows.Length; i++)
            {
                var itmArry=csvrows[i].Split(',');
                lifetimeEad.Add(new LifetimeEad {ContractId= itmArry[0],ProjectionMonth= int.Parse(itmArry[1]), ProjectionValue = double.Parse(itmArry[2]) });
            }
            var maxMonth = lifetimeEad.Max(o => o.ProjectionMonth);
            lifetimeEad = lifetimeEad.OrderBy(o => o.ContractId).ThenBy(p => p.ProjectionMonth).ThenBy(q => q.ProjectionValue).ToList();
            var sb = new StringBuilder();
            var header = "Contract_No,";

            for (int i = 0; i <= maxMonth; i++)
            {
                header = $"{header}{i},";
            }
            header = header.Trim(',');
            header = $"{header}{Environment.NewLine}";

            sb.Append(header);
            var distinctCOntracts = lifetimeEad.Select(o => o.ContractId).Distinct().ToList();

            var cnt = 1;
            foreach (var contract in distinctCOntracts)
            {
                Console.WriteLine($"{cnt} - {contract}");
                var contractData = lifetimeEad.Where(o => o.ContractId == contract).OrderBy(p => p.ProjectionMonth).ToList();
                var contractLine = $"{contract},";

                foreach (var monthVal in contractData)
                {
                    Console.WriteLine($"{cnt} - {contract}-{monthVal.ProjectionMonth}");
                    contractLine = $"{contractLine}{monthVal.ProjectionValue},";
                }
                contractLine = contractLine.Trim(',');
                contractLine = $"{contractLine}{Environment.NewLine}";

                sb.Append(contractLine);
                cnt = cnt + 1;
            }
            var fpath = Path.Combine(basepath, $"EAD_Lifetime_LifeTimeEAD.csv");
            File.WriteAllText(fpath, sb.ToString());

            Console.WriteLine("Done Read_EAD_Lifetime");
        }


        public static void Read_LGD_Impairment(string eclId)
        {
            //var eclId = "9ec1ae81-a13f-4c90-4bba-08d83eb190fd";
            var basepath = AppDomain.CurrentDomain.BaseDirectory;

            var eadPathCsv = Path.Combine(basepath, $"LGDOutput.csv");
            //var eadPathCsv = @"C:\PwC\Projects\SourceCode\Firs_9_ECL\Code\IFRS_Test1\bin\Debug\LGDOutput.csv";
            var csvrows = File.ReadAllLines(eadPathCsv);

            var lifetimeLgd = new List<List<LifetimeLgd>>();
            lifetimeLgd.Add(new List<LifetimeLgd>());
            lifetimeLgd.Add(new List<LifetimeLgd>());
            lifetimeLgd.Add(new List<LifetimeLgd>());
            //sb.Append($"{itm.ContractId},{itm.Month},{itm.Ecl_Scenerio.ToString()},{itm.Value},{Environment.NewLine}");
            for (int i = 1; i < csvrows.Length; i++)
            {
                var itmArry = csvrows[i].Split(',');
                var snr = itmArry[2] == ECL_Scenario.Best.ToString() ? ECL_Scenario.Best : (itmArry[2] == ECL_Scenario.Optimistic.ToString() ? ECL_Scenario.Optimistic : ECL_Scenario.Downturn);

                if(snr== ECL_Scenario.Best)
                {
                    lifetimeLgd[0].Add(new LifetimeLgd { ContractId = itmArry[0], Month = int.Parse(itmArry[1]), Ecl_Scenerio = snr, Value = double.Parse(itmArry[3]) });
                }
                if (snr == ECL_Scenario.Optimistic)
                {
                    lifetimeLgd[1].Add(new LifetimeLgd { ContractId = itmArry[0], Month = int.Parse(itmArry[1]), Ecl_Scenerio = snr, Value = double.Parse(itmArry[3]) });
                }
                if (snr == ECL_Scenario.Downturn)
                {
                    lifetimeLgd[2].Add(new LifetimeLgd { ContractId = itmArry[0], Month = int.Parse(itmArry[1]), Ecl_Scenerio = snr, Value = double.Parse(itmArry[3]) });
                }

            }


            for (int i=0; i< lifetimeLgd.Count; i++)
            {
                var _lifetimeLgd = lifetimeLgd[i];
                var maxMonth = _lifetimeLgd.Max(o => o.Month);
                _lifetimeLgd = _lifetimeLgd.OrderBy(o => o.ContractId).ThenBy(p => p.Month).ThenBy(q => q.Value).ToList();

                var sb = new StringBuilder();
                var header = "Contract_No,";

                for (int j = 0; j <= maxMonth; j++)
                {
                    header = $"{header}{j},";
                }
                header = header.Trim(',');
                header = $"{header}{Environment.NewLine}";

                sb.Append(header);
                var distinctCOntracts = _lifetimeLgd.Select(o => o.ContractId).Distinct().ToList();

                var cnt = 1;
                foreach (var contract in distinctCOntracts)
                {
                    Console.WriteLine($"{cnt} - {contract}");
                    var contractData = _lifetimeLgd.Where(o => o.ContractId == contract).OrderBy(p => p.Month).ToList();
                    var contractLine = $"{contract},";

                    foreach (var monthVal in contractData)
                    {
                        Console.WriteLine($"{cnt} - {contract}-{monthVal.Month}");
                        contractLine = $"{contractLine}{monthVal.Value},";
                    }
                    contractLine = contractLine.Trim(',');
                    contractLine = $"{contractLine}{Environment.NewLine}";

                    sb.Append(contractLine);
                    cnt = cnt + 1;
                }

                var sc = "";
                if (i == 0)
                {
                    sc = "Bests";
                }
                if (i == 1)
                {
                    sc = "Optimistics";
                }
                if (i == 2)
                {
                    sc = "Downturns";
                }

                var fpath = Path.Combine(basepath, $"LGD_Lifetime_LifeTimeLGD_{sc}.csv");
                File.WriteAllText(fpath, sb.ToString());


            }


            Console.WriteLine("Done Read_LGD_Lifetime");
        }

    }
}