using IFRS9_ECL.Core.Macro.Entities;
using IFRS9_ECL.Core.Macro.Input;
using IFRS9_ECL.Data;
using IFRS9_ECL.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace IFRS9_ECL.Core.Calibration
{
    public class Macro_Processor
    {
        List<AffiliateMacroEconomicVariableOffsets> affM = new List<AffiliateMacroEconomicVariableOffsets>();
        public bool ProcessMacro(int macroId, long affiliateId)
        {

            var qry = Queries.Affiliate_MacroeconomicVariable(affiliateId);
            var dt = DataAccess.i.GetData(qry);

            
            foreach (DataRow dr in dt.Rows)
            {
                affM.Add(DataAccess.i.ParseDataToObject(new AffiliateMacroEconomicVariableOffsets(), dr));
            }

            var lstMacroData = GeneratesaveMacroData(affiliateId, macroId);

            ProcessMacroAnalysis(affiliateId, macroId);

            // Read Eingen final to determine the comp to consider
            var EingenFinalPath = Path.Combine(AppSettings.MacroModelPath, affiliateId.ToString(), "ETI_Eingen_Final.csv");
            var all_Eingen = File.ReadAllLines(EingenFinalPath);

            var eIngenValues = new List<double>();
            for (int i = 1; i < all_Eingen.Count(); i++)
            {
                eIngenValues.Add(double.Parse(all_Eingen[i].Split(',')[1]));
                if (i == 5) break;
            }

            // Read loading final to determine the comp to consider
            var LoadingFinalPath = Path.Combine(AppSettings.MacroModelPath, affiliateId.ToString(), "ETI_Loadings_Final.csv");
            var all_loadingFinal = File.ReadAllLines(LoadingFinalPath);

            var dataLoaded = new List<List<double>>();
            var macvarCol = new List<string>();
            var colCount = 0;
            for (int i = 0; i < all_loadingFinal.Length; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                var splitted = all_loadingFinal[i].Split(',');
                macvarCol.Add(splitted[0]);

                splitted = splitted.Skip(1).ToArray();
                colCount = splitted.Count();
                var _joined = string.Join(",", splitted);
                dataLoaded.Add(_joined.Split(',').Select(r => Convert.ToDouble(r)).ToArray().ToList());
            }

            var loadingOutputResult = new List<List<double>>();
            var finalMaxIndex = new List<int>();

            var actual_macvar = new List<AffiliateMacroEconomicVariableOffsets>();
            for (int i = 0; i < colCount; i++)
            {
                var tempResult = new List<double>();
                foreach (var ln in dataLoaded)
                {
                    var val = ln[i];
                    if (val < 0)
                    {
                        val = val * -1;
                    }
                    tempResult.Add(val);
                }
                var _indx = tempResult.Select((n, j) => (Number: n, Index: j)).Max().Index;

                var tkVal = affM.Count >= 4 ? 4 : affM.Count;
                if (colCount < tkVal)
                    tkVal = colCount;
                if (!loadingOutputResult.Contains(dataLoaded[_indx].Take(tkVal).ToList()))
                {
                    var varTitle = macvarCol[_indx];
                    varTitle = varTitle.Replace(" ", "").Replace("\"", "");
                    var indexAndBackLag = varTitle.Replace("Var", "").Split('-');

                    var aff = affM.FirstOrDefault(); //**************************************
                    try { aff = affM[int.Parse(indexAndBackLag[0]) - 1]; } catch { };
                    aff.varTitle = varTitle.Split('-')[0].Trim();
                    aff.BackwardOffset = 0;
                    if (indexAndBackLag.Length>1)
                    {
                        aff.BackwardOffset = int.Parse(indexAndBackLag[1]);
                    }
                    else
                    {
                        aff.BackwardOffset = 0;
                    }

                    if(!actual_macvar.Contains(aff))
                    {
                        actual_macvar.Add(aff);
                        loadingOutputResult.Add(dataLoaded[_indx].Take(tkVal).ToList());
                    }
                    else
                    {
                        if (actual_macvar.Count == 3)
                            break;
                    }
                }
                if (loadingOutputResult.Count == tkVal)
                    break;
            }
            loadingOutputResult = loadingOutputResult.Distinct().ToList();
            var maxBackLag = actual_macvar.Max(o => o.BackwardOffset);

            var macrodataHeader = lstMacroData[0].Split(',').ToList();
            // find and pick columsn to consider


            var positionsToHold = new List<int>();
            for (int i = 0; i < actual_macvar.Count; i++)
            {
                for (int j = 0; j < macrodataHeader.Count(); j++)
                {
                    if (macrodataHeader[j] == actual_macvar[i].varTitle)
                    {
                        positionsToHold.Add(j);
                    }
                }
            }

            //Get the actualMacroData for Analysis sheet
            var firstPick = true;
            var allLineData = new List<List<string>>();
            var actual_filtered_lineData = new List<List<string>>();
            
            for (int i = 1; i < lstMacroData.Count; i++)
            {
                var _lineData = lstMacroData[i].Split(',');

                var lineData = new List<string>();

                lineData.Add(_lineData[0]);
                for (int m = 0; m < positionsToHold.Count; m++)
                {
                    lineData.Add(_lineData[positionsToHold[m]]);
                }
                var npl = _lineData.Last();
                lineData.Add(npl);
                allLineData.Add(lineData);

                if (!string.IsNullOrWhiteSpace(npl) && !string.IsNullOrEmpty(npl))
                {
                    try
                    {
                        //if (double.Parse(npl.Trim()) > 0)
                        //{

                            if (firstPick)
                            {
                                actual_filtered_lineData.AddRange(allLineData.Skip(i - maxBackLag-1).Take(maxBackLag).ToList());
                                firstPick = false;
                            }
                            actual_filtered_lineData.Add(lineData);

                        //}
                    }
                    catch { }
                }
            }
            ///i have gotten the data on sheet 1 actual_filtered_lineData


            var groupMacroData = GenerateGroupMacroData(actual_filtered_lineData);


            ///the principal component will come from the score final sheet

            var scoreFinalPath = Path.Combine(AppSettings.MacroModelPath, affiliateId.ToString(), "ETI_scores_Final.csv");
            var all_score = File.ReadAllLines(scoreFinalPath);



            var startPeriod = groupMacroData.FirstOrDefault(o => o.NPL > 0).period;
            var endPeriod = groupMacroData.LastOrDefault(o => o.NPL > 0).period;
            var scoreValues = new List<double>();

            var mcPrincipalComponent = new List<MacroResult_PrincipalComponent>();

            var started = false;

            var startPos = lstMacroData.Count()- all_score.Count() +1; // one is to exlude the header
//            var allDataStartPeriod = lstMacroData[startPos].Split(',')[0];
            for (int i = 1; i <all_score.Count(); i++)
            {
                var _singleLine = all_score[i].Split(',');

                var posData = lstMacroData[startPos].Split(',')[0];
                if (posData == startPeriod)//allDataStartPeriod 
                {
                    started = true;
                }
                


                if (started)
                {
                    var mp = new MacroResult_PrincipalComponent();
                    
                        try { mp.PrincipalComponent1 = double.Parse(_singleLine[1].Trim()); } catch { mp.PrincipalComponent1 = 0; }
                        try{mp.PrincipalComponent2 = double.Parse(_singleLine[2].Trim()); } catch { mp.PrincipalComponent2 = 0; }
                        try{mp.PrincipalComponent3 = double.Parse(_singleLine[3].Trim()); } catch { mp.PrincipalComponent3 = 0; }
                        try{mp.PrincipalComponent4 = double.Parse(_singleLine[4].Trim()); } catch { mp.PrincipalComponent4 = 0; }

                        mcPrincipalComponent.Add(mp);
                }
                if (posData == endPeriod)
                {
                    started = false;
                    break;
                }
                startPos++;
               // allDataStartPeriod = GetNextPeriod(allDataStartPeriod, i);
            }


            //Continue Principal Component
            //////////////////////////Remove IT///////////////////////////////
            mcPrincipalComponent = mcPrincipalComponent.Take(mcPrincipalComponent.Count - 2).ToList(); //removeit
            //////////////////////////Remove IT///////////////////////////////

            // Principal Component SUmmary result
            var groupDataStartPos = groupMacroData.IndexOf(groupMacroData.FirstOrDefault(o => o.period == startPeriod));
            
            var lstPrinSummary = new List<MacroResult_PrincipalComponentSummary>();
            for (int i = 0; i < loadingOutputResult.Count; i++)
            {
                var selectedVariable = actual_macvar[i];

                var _extractForPrinCompSummary = groupMacroData.Skip(groupDataStartPos- selectedVariable.BackwardOffset).Take(mcPrincipalComponent.Count).ToList();

                var sum = new MacroResult_PrincipalComponentSummary();
                sum.PrincipalComponentIdA = 1;
                sum.PrincipalComponentIdB = 4 + i;
                sum.PricipalComponentLabelA = "Mean";
                sum.PricipalComponentLabelB = $"PrinComp{i + 1}";
                sum.MacroId = macroId;              

                var sum1 = new MacroResult_PrincipalComponentSummary();
                sum1.PrincipalComponentIdA = 2;
                sum1.PrincipalComponentIdB = 4 + i;
                sum1.PricipalComponentLabelA = "std.Dev";
                sum1.PricipalComponentLabelB = $"PrinComp{i + 1}";
                sum1.MacroId = macroId;
                
                if (i == 0)
                {
                    sum.Value = _extractForPrinCompSummary.Average(o => o.MacroValue1);
                    sum1.Value = Computation.GetStandardDeviationS(_extractForPrinCompSummary.Select(o => o.MacroValue1));
                }
                if (i == 1)
                {
                    sum.Value = _extractForPrinCompSummary.Average(o => o.MacroValue2);
                    sum1.Value = Computation.GetStandardDeviationS(_extractForPrinCompSummary.Select(o => o.MacroValue2));
                }
                if (i == 2)
                {
                    sum.Value = _extractForPrinCompSummary.Average(o => o.MacroValue3);
                    sum1.Value = Computation.GetStandardDeviationS(_extractForPrinCompSummary.Select(o => o.MacroValue3));
                }
                if (i == 3)
                {
                    sum.Value = _extractForPrinCompSummary.Average(o => o.MacroValue4);
                    sum1.Value = Computation.GetStandardDeviationS(_extractForPrinCompSummary.Select(o => o.MacroValue4));
                }
                
                lstPrinSummary.Add(sum);
                lstPrinSummary.Add(sum1);


                sum = new MacroResult_PrincipalComponentSummary();
                sum.PrincipalComponentIdA = 3;
                sum.PrincipalComponentIdB = 4 + i;
                sum.PricipalComponentLabelA = "EigenValues";
                sum.PricipalComponentLabelB = $"PrinComp{i + 1}";
                sum.MacroId = macroId;
                sum.Value = eIngenValues[i];
                lstPrinSummary.Add(sum);

                for (int j = 0; j < loadingOutputResult[i].Count; j++)
                {
                    sum = new MacroResult_PrincipalComponentSummary();
                    sum.PrincipalComponentIdA = 4 + j;
                    sum.PrincipalComponentIdB = 4 + i;
                    sum.PricipalComponentLabelA = $"PrinComp{j + 1}";
                    sum.PricipalComponentLabelB = $"PrinComp{i + 1}";
                    sum.MacroId = macroId;
                    sum.Value = loadingOutputResult[i][j];
                    lstPrinSummary.Add(sum);
                }

            }


            // Get Statistical Data
            var statistics = new MacroResult_Statistics();
            try { statistics.IndexWeight1 = eIngenValues[0] < 1 ? 0 : eIngenValues[0]; } catch { statistics.IndexWeight1 = 0; }
            try{statistics.IndexWeight2 = eIngenValues[1] < 1 ? 0 : eIngenValues[1]; } catch { statistics.IndexWeight2 = 0; }
            try{statistics.IndexWeight3 = eIngenValues[2] < 1 ? 0 : eIngenValues[2]; } catch { statistics.IndexWeight3=0; }
             try{statistics.IndexWeight4 = eIngenValues[3] < 1 ? 0 : eIngenValues[3]; } catch { statistics.IndexWeight4 = 0; }

            // Get Index Data
            var indxData = new List<MacroResult_IndexData>();
            for (int i = 0; i < mcPrincipalComponent.Count; i++)
            {
                var mcp = mcPrincipalComponent[i];

                var indx = new MacroResult_IndexData();
                // if(extractForPrinCompSummary.Count>(i + maxBackLag))
                // {
                var extractForPrinCompSummary = groupMacroData.Skip(groupDataStartPos).Take(mcPrincipalComponent.Count).ToList();

                indx.MacroId = macroId;
                    indx.Period = extractForPrinCompSummary[i].period;
                    indx.BfNpl = extractForPrinCompSummary[i].NPL;
                mcPrincipalComponent[i].PrincipalComponent1 = mcPrincipalComponent[i].PrincipalComponent1 ?? 0;
                mcPrincipalComponent[i].PrincipalComponent2 = mcPrincipalComponent[i].PrincipalComponent2 ?? 0;
                mcPrincipalComponent[i].PrincipalComponent3 = mcPrincipalComponent[i].PrincipalComponent3 ?? 0;
                mcPrincipalComponent[i].PrincipalComponent4 = mcPrincipalComponent[i].PrincipalComponent4 ?? 0;

                statistics.IndexWeight1 = statistics.IndexWeight1 ?? 0;
                statistics.IndexWeight2 = statistics.IndexWeight2 ?? 0;
                statistics.IndexWeight3 = statistics.IndexWeight3 ?? 0;
                statistics.IndexWeight4 = statistics.IndexWeight4 ?? 0;


                indx.Index = (mcPrincipalComponent[i].PrincipalComponent1.Value * statistics.IndexWeight1.Value) + (mcPrincipalComponent[i].PrincipalComponent2.Value * statistics.IndexWeight2.Value) + (mcPrincipalComponent[i].PrincipalComponent3.Value * statistics.IndexWeight3.Value) + (mcPrincipalComponent[i].PrincipalComponent4.Value * statistics.IndexWeight4.Value);
                    indxData.Add(indx);
               // }
            }

            statistics.StandardDev = 0;
            statistics.Average = 0;
            statistics.Correlation = 0;
            statistics.TTC_PD = 0;


            try { statistics.StandardDev = Computation.GetStandardDeviationP(indxData.Select(o => o.Index).ToList()); } catch { }
            try{statistics.Average = indxData.Average(o => o.Index); } catch { }
            try{statistics.TTC_PD = indxData.Average(o => o.BfNpl); } catch { }

            for (int i = 0; i < indxData.Count; i++)
            {
                indxData[i].StandardIndex = 0;
                try {indxData[i].StandardIndex = (indxData[i].Index - statistics.Average.Value) / statistics.StandardDev.Value; } catch { }
            }

                var arry1 = indxData.Select(o => o.StandardIndex).ToArray();
            var arry2 = indxData.Select(o => o.StandardIndex).ToArray();
                var fitResult = new FitResult();
            statistics.Correlation=fitResult.ComputeCoeff(arry1, arry2);
              // Get CorMat
              var macV1 = groupMacroData.Select(o => o.MacroValue1).ToList();
            var macV2 = groupMacroData.Select(o => o.MacroValue2).ToList();
            var macV3 = groupMacroData.Select(o => o.MacroValue3).ToList();
            var macV4 = groupMacroData.Select(o => o.MacroValue4).ToList();
            var allMacV = new List<List<double>> { macV1, macV2, macV3, macV4 };

            var lstCorMat = new List<MacroResult_CorMat>();
            for (int i = 0; i < actual_macvar.Count; i++)
            {
                for (int j = 0; j < actual_macvar.Count; j++)
                {
                    var sum = new MacroResult_CorMat();
                    sum.MacroEconomicIdA = 0;
                    sum.MacroEconomicIdB = 0;
                    sum.MacroEconomicLabelA = "";
                    sum.MacroEconomicLabelB = "";
                    sum.Value = 0;
                    try {sum.MacroEconomicIdA = actual_macvar[i].MacroeconomicVariableId; } catch { }
                    try { sum.MacroEconomicIdB = actual_macvar[j].MacroeconomicVariableId; } catch { }
                    try { sum.MacroEconomicLabelA = $"{actual_macvar[i].varTitle}-{actual_macvar[i].BackwardOffset}"; } catch { }
                    try { sum.MacroEconomicLabelB = $"{actual_macvar[j].varTitle}-{actual_macvar[j].BackwardOffset}"; } catch { }
                    sum.MacroId = macroId;
                    try { sum.Value = MathNet.Numerics.Statistics.Correlation.Pearson(allMacV[i], allMacV[j]); } catch { }
                    lstCorMat.Add(sum);
                }
            }


            //Delete all affiliate macroData
            qry = Queries.DeleteAffiliateMacroData(macroId, affiliateId);
            DataAccess.i.ExecuteQuery(qry);

            var sb = new StringBuilder();
            // Save Principal Component Result to DB
            foreach (var prinC in mcPrincipalComponent)
            {
                sb.Append(Queries.MacroResult_PrinC(macroId, prinC.PrincipalComponent1, prinC.PrincipalComponent2, prinC.PrincipalComponent3, prinC.PrincipalComponent4));
            }
            qry = sb.ToString().Replace("NaN", "0");
            DataAccess.i.ExecuteQuery(qry);

            sb = new StringBuilder();
            // Save Index Result to DB
            foreach (var mId in indxData)
            {
                sb.Append(Queries.MacroResult_IndxResult(macroId, mId.Period, mId.Index, mId.StandardIndex, mId.BfNpl));
            }
            qry = sb.ToString().Replace("NaN", "0");
            DataAccess.i.ExecuteQuery(qry);

            sb = new StringBuilder();
            // Save Statistics Index Result to DB
            sb.Append(Queries.MacroResult_StatisticalIndex(macroId, statistics.IndexWeight1, statistics.IndexWeight2, statistics.IndexWeight3,
                statistics.IndexWeight4, statistics.StandardDev, statistics.Average, statistics.Correlation, statistics.TTC_PD));
            qry = sb.ToString().Replace("NaN", "0");
            DataAccess.i.ExecuteQuery(qry);

            sb = new StringBuilder();
            // Save Correlation Mat Index Result to DB
            foreach (var corMar in lstCorMat)
            {
                sb.Append(Queries.MacroResult_CorMat(macroId, corMar.MacroEconomicIdA, corMar.MacroEconomicIdB, corMar.MacroEconomicLabelA, corMar.MacroEconomicLabelB, corMar.Value));
            }
            qry = sb.ToString().Replace("NaN", "0");
            DataAccess.i.ExecuteQuery(qry);

            // Save Principal Component Result to DB
            sb = new StringBuilder();
            foreach (var pcs in lstPrinSummary)
            {
                sb.Append(Queries.MacroResult_PrincipalComponent(macroId, pcs.PrincipalComponentIdA, pcs.PrincipalComponentIdB, pcs.PricipalComponentLabelA, pcs.PricipalComponentLabelB, pcs.Value));
            }
            qry = sb.ToString().Replace("NaN", "0");
            DataAccess.i.ExecuteQuery(qry);

            //Actual Selected MacroEconomic Variable
            sb = new StringBuilder();
            foreach (var itm in actual_macvar)
            {
                sb.Append(Queries.MacroResult_SelectedMacroEconomicVariables(itm.BackwardOffset, itm.AffiliateId, itm.MacroeconomicVariableId));
            }
            qry = sb.ToString().Replace("NaN", "0");
            DataAccess.i.ExecuteQuery(qry);


            return true;


        }

        private string GetNextPeriod(string allDataStartPeriod, int i)
        {
            allDataStartPeriod = allDataStartPeriod.Trim();
            var s = allDataStartPeriod.Split(' ');
            if (i == 1)
            {
                return $"{s[0]} {int.Parse(s[1]) + 1}";
            }

            if(s[0]=="Q4")
            {
                return $"Q1 {int.Parse(s[1])+1}";
            }

            if (s[0] == "Q1")
            {
                return $"Q2 {s[1]}";
            }

            if (s[0] == "Q2")
            {
                return $"Q3 {s[1]}";
            }

            if (s[0] == "Q3")
            {
                return $"Q4 {s[1]}";
            }
            return allDataStartPeriod;

        }

        private List<GroupMacroData> GenerateGroupMacroData(List<List<string>> actual_filtered_lineData)
        {
            var data = new List<GroupMacroData>();
            for (int i = 0; i < actual_filtered_lineData.Count; i++)
            {
                var afl = actual_filtered_lineData[i];
                var itm = new GroupMacroData();
                itm.period = afl[0];
                try { itm.MacroValue1 = double.Parse(afl[1].Trim()); } catch { itm.MacroValue1 = 0; }

                if (afl.Count >= 4)
                    try { itm.MacroValue2 = double.Parse(afl[2].Trim()); } catch { itm.MacroValue2 = 0; }
                if (afl.Count >= 5)
                    try { itm.MacroValue3 = double.Parse(afl[3].Trim()); } catch { itm.MacroValue3 = 0; }
                if (afl.Count >= 6)
                    try { itm.MacroValue4 = double.Parse(afl[4].Trim()); } catch { itm.MacroValue4 = 0; }

                try { itm.NPL = double.Parse(afl.Last().Trim()); } catch { itm.NPL = 0; }
                data.Add(itm);
            }

            return data;
        }

        public void ProcessMacroAnalysis(long affiliateId, int macroId)
        {

            var affBasePath = Path.Combine(AppSettings.MacroModelPath, affiliateId.ToString());

            if (!Directory.Exists(affBasePath))
            {
                Directory.CreateDirectory(affBasePath);
            }



            var macro = Path.Combine(AppSettings.MacroModelPath, "macro.r");
            var macro_final = Path.Combine(AppSettings.MacroModelPath, "macro_final.r");

            var aff_macro = Path.Combine(affBasePath, "macro.r");
            var aff_macro_final = Path.Combine(affBasePath, "macro_final.r");

            var rscript = Path.Combine(AppSettings.RScriptPath, "rscript.exe");
            var loading_initial = Path.Combine(affBasePath, "ETI_Loadings_Initial.csv");

            var macro_text = File.ReadAllLines(macro);
            for (int i = 0; i < macro_text.Length; i++)
            {
                if (macro_text[i].Contains("[macrobasepath]"))
                {
                    var mPath = affBasePath.Replace(@"\", "/");
                    macro_text[i] = macro_text[i].Replace("[macrobasepath]", mPath);
                }
                if (macro_text[i].Contains("[NPLCol]"))
                {
                    macro_text[i] = macro_text[i].Replace("[NPLCol]", (affM.Count+2).ToString());
                }
                for (int j=1; j<=affM.Count; j++)
                {
                    macro_text[i] = macro_text[i].Replace($"#{j}", "");
                }
                
            }
            if (File.Exists(loading_initial))
            {
                File.Delete(loading_initial);
            }

            File.WriteAllLines(aff_macro, macro_text);

            System.Diagnostics.Process prs = null;
            try
            {
                prs = System.Diagnostics.Process.Start(rscript, aff_macro);
            }catch(Exception ex)
            {
                Log4Net.Log.Error(ex);
                throw ex;
            }
            while (!File.Exists(loading_initial))
            {
                //do nothing
            }
            Thread.Sleep(1000);
            try
            {
                if (!prs.HasExited)
                {
                    prs.Close();
                    prs.Dispose();
                    prs.Kill();
                }
            }
            catch(Exception ex) {
                Log4Net.Log.Error(ex);
            }


            var loadingData = File.ReadAllLines(loading_initial);

            var computationCount = 0;
            var pickedClosed = false;

            var dataLoaded = new List<List<double>>();
            for (int i = 0; i < loadingData.Length; i++)
            {
                if (i == 0)
                {
                    //loadingData[i] = $"{i},{loadingData[i]}";
                    continue;
                }
                var splitted = loadingData[i].Split(',');
                if (!splitted[0].Contains("-") && !pickedClosed)
                {
                    computationCount = i;
                }
                else
                {
                    pickedClosed = true;
                }

                splitted = splitted.Skip(1).ToArray();
                var _joined = string.Join(",", splitted);
                dataLoaded.Add(_joined.Split(',').Select(r => Convert.ToDouble(r)).ToArray().ToList());

                //loadingData[i] = $"{i},{loadingData[i]}";
            }
            File.Delete(loading_initial);
            File.WriteAllLines(loading_initial, loadingData);

            var finalMaxIndex = new List<int>();
            for (int i = 0; i < computationCount; i++)
            {
                var tempResult = new List<double>();
                foreach (var ln in dataLoaded)
                {
                    var val = ln[i];
                    if (val < 0)
                    {
                        val = val * -1;
                    }
                    tempResult.Add(val);
                }
                finalMaxIndex.Add(tempResult.Select((n, j) => (Number: n, Index: j)).Max().Index + 2);
            }

            finalMaxIndex = finalMaxIndex.Distinct().ToList();
            //finalMaxIndex = finalMaxIndex.Take(5).ToList();
            finalMaxIndex.Sort();

            var strFinal = string.Join(",", finalMaxIndex);

            var macro_final_text = File.ReadAllLines(macro_final);
            for (int i = 0; i < macro_final_text.Length; i++)
            {
                if (macro_final_text[i].Contains("[macrobasepath]"))
                {
                    var mPath = affBasePath.Replace(@"\", "/");
                    macro_final_text[i] = macro_final_text[i].Replace("[macrobasepath]", mPath);
                }
                if (macro_final_text[i].Contains("[NPLCol]"))
                {
                    macro_final_text[i] = macro_text[i].Replace("[NPLCol]", (affM.Count + 2).ToString());
                }
                if (macro_final_text[i].Contains("[Picked_Fields]"))
                {
                    macro_final_text[i] = macro_final_text[i].Replace("[Picked_Fields]", strFinal);
                }
                for (int j = 1; j <= affM.Count; j++)
                {
                    macro_final_text[i] = macro_final_text[i].Replace($"#{j}", "");
                }
            }
            File.Delete(aff_macro_final);
            File.WriteAllLines(aff_macro_final, macro_final_text);

            System.Diagnostics.Process prs1 = System.Diagnostics.Process.Start(rscript, aff_macro_final);
            while (!File.Exists(Path.Combine(affBasePath, "ETI_scores_Final.csv")))
            {

            }
            try
            {
                if (!prs1.HasExited)
                {
                    prs1.Close();
                    prs1.Dispose();
                    prs1.Kill();
                }
            }
            catch { }
            Thread.Sleep(1000);
        }

        public List<string> GeneratesaveMacroData(long affiliateId, int macroId)
        {
            var affBasePath = Path.Combine(AppSettings.MacroModelPath, affiliateId.ToString());

            if(!Directory.Exists(affBasePath))
            {
                Directory.CreateDirectory(affBasePath);
            }
            //Get MacroData
            #region Get MacroData

            var qry = Queries.Macro_Analysis(macroId);
            var dt = DataAccess.i.GetData(qry);

            var itms = new List<MacroData>();
            for (int i = 0; i < dt.Rows.Count; i++)// DataRow dr in dt.Rows)
            {
                //Log4Net.Log.Info(i);
                DataRow dr = dt.Rows[i];
                var itm = DataAccess.i.ParseDataToObject(new MacroData(), dr);
                itms.Add(itm);
            }
            var periods = itms.Select(o => o.Period).Distinct().OrderBy(p => p).ToList();

            var lstMacroData = new List<string>();
            var header = new List<string>();


            var macroIds = itms.Select(o => o.MacroeconomicId).ToList();

            //foreach (var itm in macroIds)
            //{
            //    if (itms.Where(p => p.MacroeconomicId == itm).Any(o => o.Value > 0))
            //    {
            //        // do nothing
            //    }
            //    else
            //    {
            //        var toRemoveItms = itms.Where(p => p.MacroeconomicId == itm).ToList();
            //        foreach (var _itm in toRemoveItms)
            //        {
            //            itms.Remove(_itm);
            //        }
            //        var rmvAff = affM.FirstOrDefault(o => o.MacroeconomicVariableId == itm);
            //        affM.Remove(rmvAff);
            //    }
            //}

            header.Add("Units");
            for (int i = 0; i < affM.Count; i++)
            {
                header.Add($"Var{i + 1}");
            }
            header.Add("Percentage");

            lstMacroData.Add(string.Join(",", header));           

            for (int i = 0; i < periods.Count; i++)
            {
                var pickPeriod = periods[i];

                var grpdata = new GroupMacroData();
                var period = GetPeriod(pickPeriod);

                var body = new List<string>();
                body.Add(period);
                for (int j = 0; j < affM.Count; j++)
                {
                    try { body.Add(itms.FirstOrDefault(o => o.Period == pickPeriod && o.MacroeconomicId == affM[j].MacroeconomicVariableId).Value.ToString()); } catch { body.Add("0"); };
                }

                try { body.Add(itms.FirstOrDefault(o => o.Period == pickPeriod && o.MacroeconomicId == -1).Value.ToString()); } catch { body.Add(""); };
                lstMacroData.Add(string.Join(",", body));
            }
            var add_macro_data = Path.Combine(affBasePath, "MacroData.csv");
            File.WriteAllLines(add_macro_data, lstMacroData.ToArray());
            #endregion

            return lstMacroData;
        }

        private string getPricipalComponentLabel(int id)
        {
            if (id == 1) return "Mean";
            if (id == 2) return "Std.Dev";
            if (id == 3) return "EigenValues";
            if (id == 4) return "PrinComp1";
            if (id == 5) return "PrinComp2";
            if (id == 6) return "PrinComp3";
            if (id == 7) return "PrinComp4";
            if (id == 8) return "PrinComp5";
            return "";
        }

        private string GetPeriod(DateTime period)
        {
            var mth = 0;
            if (period.Month == 1 || period.Month == 2 || period.Month == 3)
                mth = 1;
            if (period.Month == 4 || period.Month == 5 || period.Month == 6)
                mth = 2;
            if (period.Month == 7 || period.Month == 8 || period.Month == 9)
                mth = 3;
            if (period.Month == 10 || period.Month == 11 || period.Month == 12)
                mth = 4;

            return $"Q{mth} {period.Year}";
        }


        public List<MacroResult_IndexData> GetMacroResult_IndexData(Guid eclId, EclType eclType)
        {
            string qry = Queries.GetPDIndexData(eclId, eclType.ToString());
            var dt = DataAccess.i.GetData(qry);
            if (dt.Rows.Count == 0)
            {
                return new List<MacroResult_IndexData>();
            }

            var itms = new List<MacroResult_IndexData>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = new MacroResult_IndexData();
                try { itm.Period = dr["StandardIndex"].ToString(); } catch { itm.Period = ""; }
                try { itm.StandardIndex = double.Parse(dr["StandardIndex"].ToString().Trim()); } catch { itm.StandardIndex = 0; }
                try { itm.BfNpl = double.Parse(dr["BfNpl"].ToString().Trim()); } catch { itm.BfNpl = 0; }
                try { itm.Index = double.Parse(dr["Index"].ToString().Trim()); } catch { itm.Index = 0; }
                itms.Add(itm);
            }
            return itms;
        }

        public MacroResult_Statistics GetMacroResult_Statistics(Guid eclId, EclType eclType)
        {
            string qry = Queries.GetPDStatistics(eclId, eclType.ToString());
            var dt = DataAccess.i.GetData(qry);
            if (dt.Rows.Count == 0)
            {
                return new MacroResult_Statistics();
            }

            var itms = new MacroResult_IndexData();
            DataRow dr = dt.Rows[0];
            var itm = new MacroResult_Statistics();
            try { itm.IndexWeight1 = double.Parse(dr["IndexWeight1"].ToString().Trim()); } catch { itm.IndexWeight1 = 0; }
            try { itm.IndexWeight2 = double.Parse(dr["IndexWeight2"].ToString().Trim()); } catch { itm.IndexWeight2 = 0; }
            try { itm.IndexWeight3 = double.Parse(dr["IndexWeight3"].ToString().Trim()); } catch { itm.IndexWeight3 = 0; }
            try { itm.IndexWeight4 = double.Parse(dr["IndexWeight4"].ToString().Trim()); } catch { itm.IndexWeight4 = 0; }
            try { itm.Average = double.Parse(dr["Average"].ToString().Trim()); } catch { itm.Average = 0; }
            try { itm.StandardDev = double.Parse(dr["StandardDev"].ToString().Trim()); } catch { itm.StandardDev = 0; }

            return itm;
        }
        public List<MacroResult_SelectedMacroEconomicVariables> Get_MacroResult_SelectedMacroEconomicVariables(Guid eclId, string eclType)
        {
            string qry = Queries.GetSelectMacroVariables(eclId, eclType);
            var dt = DataAccess.i.GetData(qry);
            if (dt.Rows.Count == 0)
            {
                return new List<MacroResult_SelectedMacroEconomicVariables>();
            }

            var itms = new List<MacroResult_SelectedMacroEconomicVariables>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = new MacroResult_SelectedMacroEconomicVariables();
                try { itm.AffiliateId = long.Parse(dr["AffiliateId"].ToString().Trim()); } catch { itm.AffiliateId = 0; }
                try { itm.BackwardOffset = int.Parse(dr["BackwardOffset"].ToString()); } catch { itm.BackwardOffset = 0; }
                try { itm.MacroeconomicVariableId = int.Parse(dr["MacroeconomicVariableId"].ToString()); } catch { itm.MacroeconomicVariableId = 0; }
                try { itm.friendlyName = dr["Description"].ToString().Trim(); } catch { try { itm.friendlyName = dr["Name"].ToString().Trim(); } catch { } }

                itm.friendlyName = $"{itm.friendlyName}-{itm.BackwardOffset}";

                itms.Add(itm);
            }
            return itms;
        }
        public List<MacroResult_PrincipalComponentSummary> GetMacroResult_PCSummary(Guid eclId, EclType eclType)
        {
            string qry = Queries.GetPrincipalComponentSummary(eclId, eclType.ToString());
            var dt = DataAccess.i.GetData(qry);
            if (dt.Rows.Count == 0)
            {
                return new List<MacroResult_PrincipalComponentSummary>();
            }

            var itms = new List<MacroResult_PrincipalComponentSummary>();
            foreach (DataRow dr in dt.Rows)
            {
                var itm = new MacroResult_PrincipalComponentSummary();
                try { itm.PricipalComponentLabelA = dr["PricipalComponentLabelA"].ToString(); } catch { itm.PricipalComponentLabelA = ""; }
                try { itm.PricipalComponentLabelB = dr["PricipalComponentLabelB"].ToString(); } catch { itm.PricipalComponentLabelB = ""; }
                try { itm.Value = double.Parse(dr["Value"].ToString().Trim()); } catch { itm.Value = 0; }
                try { itm.PrincipalComponentIdA = int.Parse(dr["PrincipalComponentIdA"].ToString().Trim()); } catch { itm.PrincipalComponentIdA = 0; }
                try { itm.PrincipalComponentIdB = int.Parse(dr["PrincipalComponentIdB"].ToString().Trim()); } catch { itm.PrincipalComponentIdB = 0; }

                itms.Add(itm);
            }
            return itms;
        }
    }

}
