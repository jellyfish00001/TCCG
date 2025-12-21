using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using Autofac.Features.Indexed;
using Newtonsoft.Json.Bson;
using SDO.APP.IPC.Models.Statistics;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 平時管考意見備註統計表
    /// </summary>
    public class ProjectComIpcMemo : WContentBuilder
    {
        #region 變數宣告
        private readonly IStatisticsDac statisticsDac;
        private readonly IIPCSetParamDac ipcSetparamDac;
        private readonly IDropDownDac dropDownDac;

        private StatisticsModel queryModel = new();                             // 查詢條件
        private Dictionary<int, List<string>> tbNeedToMergeDict = new();        // 需合併儲存格Table Index及替代資料 (表2~表11)
        private List<ProjectComIpcMemoModel> projectComIpcMemoData = new();     // 管考意見備註資料
        private List<DropDownListModel> allOrgData = new();                     // 所有機關清單
        private List<CheckpointExpiryModel> projectCheckpointData = new();      // 各計畫未完成的第一項檢核點
        private List<ProjectEngPrgRPTModel> projectEngPrgData = new();          // 近三個月計畫工程進度
        private List<OrgProjectCntModel> orgProjectCntModels = new();           // 機關列管計畫件數
        private List<IPCSetParamModel> comIpcMemoTypes = new();                 // 管考意見備註類型
        private readonly int tableCount = 11;                                   // 子表個數

        DateTime statDate; // 統計年月
        // 表1~表11資料集
        private Dictionary<string, List<Dictionary<string, object>>> tbDataSets = new();

        // 管考意見備註資料代碼對應表格編號
        readonly Dictionary<string, string> memoTypeMappingTableSeq = new()
        {
            {"02","2" }, {"01","3" },{"07","4" }, {"05","5" },{"03","6" },
            {"04","7" }, {"06","8" },{"08","9" }, {"09","10" },
        };

        // 表7工程進度替代資料
        private readonly string table7PrgTxtFormat = "預定：{0}%\n實際：{1}%\n差異：{2}%";

        #endregion 變數宣告
        public ProjectComIpcMemo(IComponentContext coms) : base(coms)
        {
            statisticsDac = coms.Resolve<IStatisticsDac>();
            this.ipcSetparamDac = coms.Resolve<IIPCSetParamDac>();
            this.dropDownDac = coms.Resolve<IDropDownDac>();
        }

        protected override void SetTemplateFileName()
        {
            TemplateFileName = "ProjectComIpcmemoRPT.doc";
        }

        /// <summary>
        /// 取資料
        /// </summary>
        /// <returns></returns>
        protected override async Task GetData()
        {
            queryModel = (StatisticsModel)Parameter.ObjectModel;
            allOrgData = await dropDownDac.GetOrganList();
            projectComIpcMemoData = await statisticsDac.GetProjectComIpcMemoList(queryModel);
            if(projectComIpcMemoData.Any())
            {
                List<string> projectNos = projectComIpcMemoData.Select(x => x.PROJECT_NO).ToList();
                projectCheckpointData = await statisticsDac.GetProjectFirstUnFilledCheckPoint(projectNos);
                projectEngPrgData = await statisticsDac.GetProjectProgressWithinThreeMonths(
                    Int32.Parse(queryModel.STATISTICS_YEAR),
                    queryModel.STATISTICS_MONTH, projectNos);
                comIpcMemoTypes = (await ipcSetparamDac.GetSysParams("COM_IPCMEMO", false)).ToList();
                orgProjectCntModels = await statisticsDac.GetExecOrgProjectCnt(queryModel);
            }
            
            SetContent();
        }

        /// <summary>
        /// 表格內容
        /// </summary>
        private void SetContent()
        {
            // 需合併儲存格Table Index及替代資料 (表2~表11)
            for (int i = 1; i < tableCount; i++)
                tbNeedToMergeDict.Add(i, new List<string>());

            // 統計年月
            statDate = new DateTime(Int32.Parse(queryModel.STATISTICS_YEAR) + 1911, queryModel.STATISTICS_MONTH, 1);
            #region 表1 加總欄位宣告
            // 表1 - 合計列資料宣告
            Dictionary<string, int> stat1MemoTypeCount = new Dictionary<string, int>() { { "Stat1ProjectSum", 0 } };
            foreach (IPCSetParamModel item in comIpcMemoTypes)
                stat1MemoTypeCount.Add($"Stat1Memo{item.SET_TYPE:D2}Sum", 0);
            stat1MemoTypeCount.Add($"Stat1AllMemoSum", 0);
            // 表1 - 機關總列管計畫數
            int orgProjectSum = 0;
            // 表1 - 機關各管考意見備註件數加總
            Dictionary<string, int> tb1OrgMemoTypesSum = comIpcMemoTypes.ToDictionary(x => x.SET_TYPE, y => 0);
            // 表1 - 備註總件數合計
            int memoProjectSum = 0;
            #endregion 表1 加總欄位宣告

            if (projectComIpcMemoData.Any())
            {
                // 表1~表11資料集
                for(int i = 1; i<= tableCount; i++)
                    tbDataSets.Add($"tb{i}",new List<Dictionary<string, object>>());

                // 執行機關資料
                Dictionary<string, string> dataOrgs = projectComIpcMemoData.DistinctBy(x => x.EXEC_ORGAN_C)
                    .ToDictionary(x => x.EXEC_ORGAN_C, y => y.EXEC_ORGAN_NAME);
                
                // 依據機關Loop
                foreach(DropDownListModel org in allOrgData) 
                {
                    // 管考意見備註機關資料
                    List<ProjectComIpcMemoModel> orgMemoData = projectComIpcMemoData.Where(x => x.EXEC_ORGAN_C == org.value).ToList();
                    // 已寫入表11計畫(表11是依據計畫Group)
                    List<string> table11ProjNos = new();
                    #region 表1 變數宣告
                    // 表1 機關各管考平時意見備註件數
                    Dictionary<string, int> tb1OrgMemoTypesCnt = comIpcMemoTypes.ToDictionary(x => x.SET_TYPE, y=> 0);
                    // 表1 替代資料
                    Dictionary<string, object> tb1ReplaceData = new Dictionary<string, object>();
                    #endregion 表1 變數宣告
                    foreach (ProjectComIpcMemoModel data in orgMemoData)
                    {
                        // 計算表1各備註件數
                        if (tb1OrgMemoTypesCnt.ContainsKey(data.COM_IPCMEMO))
                        {
                            tb1OrgMemoTypesCnt[data.COM_IPCMEMO] += 1;
                            tb1OrgMemoTypesSum[data.COM_IPCMEMO] += 1;
                        }

                        // 處理表2~表10替換資料
                        switch (data.COM_IPCMEMO)
                        {

                            // 表2,8,9,10
                            case "02":
                            case "06":
                            case "08":
                            case "09":
                                int orgCnt = orgMemoData.Where(x=>x.COM_IPCMEMO == data.COM_IPCMEMO).Count();
                                ProcessMemo(data,orgCnt); break;
                            // 表 3,4,5
                            case "01":
                            case "07":
                            case "05":
                                ProcessCheckpoint(data); break;
                            // 表6,7
                            case "03":
                            case "04":
                                ProcessEngineerProgress(data); break;
                        }
                        // 處理表11替換資料
                        if (!table11ProjNos.Contains(data.PROJECT_NO))
                        {
                            ProcessTable11(data.PROJECT_NO, data.PROJECT_NAME,orgMemoData);
                            table11ProjNos.Add(data.PROJECT_NO);
                        }
                    };

                    #region 處理表1替代資料
                    // 各備註件數資料
                    foreach (KeyValuePair<string, int> item in tb1OrgMemoTypesCnt)
                        tb1ReplaceData.Add($"Stat1Memo{item.Key}Cnt", item.Value.ToString());
                    // 執行機關
                    tb1ReplaceData.Add("Stat1Dept", org.text);
                    // 列管件數
                    int orgProjectCnt = orgProjectCntModels.Where(x => x.OrgId == org.value)
                        .FirstOrDefault()?.ProjectCnt ?? 0;
                    orgProjectSum += orgProjectCnt;
                    tb1ReplaceData.Add("Stat1ProjectCnt", orgProjectCnt.ToString()) ;
                    // 備註總件數(同筆計畫有多筆備註僅計算為一筆)
                    int orgProjectMemoCnt = orgMemoData.Select(x => x.PROJECT_NO).Distinct().Count();
                    memoProjectSum += orgProjectMemoCnt;
                    tb1ReplaceData.Add("Stat1AllMemoCnt", orgProjectMemoCnt);
                    tbDataSets[$"tb1"].Add(tb1ReplaceData);
                    #endregion 處理表1替代資料
                }

                #region 寫入ListData參數
                // 各表資料集Index
                int tbDataSetIndex = 0;
                foreach(KeyValuePair<string,List<Dictionary<string,object>>> tbDataSet in tbDataSets)
                {
                    if (tbDataSet.Value.Any())
                        ListData.Add(new WordTableData { LIST_DATA = tbDataSet.Value, TABLE_INDEX = tbDataSetIndex });
                    
                    tbDataSetIndex++;
                }
                #endregion 寫入ListData參數
            }

            #region 寫入BasicData 
            // 整份Word變數
            Dictionary<string, object> basicData = new()
            {
                {"TITLE", Parameter.FileName},
                {"YEAR_MONTH", $"{queryModel.STATISTICS_YEAR}年{queryModel.STATISTICS_MONTH}月"},
                {"DATE", DateTime.Now.ToTwDateString() },
                {"Stat7Y1",statDate.AddMonths(-2).Year-1911},
                { "Stat7Y2",statDate.AddMonths(-1).Year - 1911},
                { "Stat7Y3",statDate.Year - 1911},
                { "Stat7M1",statDate.AddMonths(-2).Month },
                { "Stat7M2",statDate.AddMonths(-1).Month },
                { "Stat7M3",statDate.Month},
                { "Stat1ProjectSum", orgProjectSum }, // 表1 列管件數
                { "Stat1AllMemoSum", memoProjectSum} // 表1 備註總件數合計
            };
            // 表1 合計資料
            foreach (KeyValuePair<string, int> memoSum in tb1OrgMemoTypesSum)
                basicData.Add($"Stat1Memo{memoSum.Key}Sum", memoSum.Value);

            BasicData = basicData;
            #endregion 寫入BasicData
        }

        /// <summary>
        /// 處理平時管考備註來源資料
        /// 表2, 8, 9 ,10
        /// </summary>
        private void ProcessMemo(ProjectComIpcMemoModel data, int orgCnt)
        {
            Dictionary<string, object> replaceData = new();
            // 管考意見備註代碼對應Table Seq
            string tbSeq = memoTypeMappingTableSeq[data.COM_IPCMEMO];
            SetCommonReplacement(data, Int32.Parse(tbSeq), orgCnt, ref replaceData);
            tbDataSets[$"tb{tbSeq}"].Add(replaceData);
        }

        /// <summary>
        /// 處理檢核點來源資料
        /// 表3 ,4 ,5
        /// </summary>
        private void ProcessCheckpoint(ProjectComIpcMemoModel data)
        {
            Dictionary<string, object> replaceData = new Dictionary<string, object>();
            List<CheckpointExpiryModel> orgCheckpoints = projectCheckpointData.Where(x => x.EXEC_ORGAN_C == data.EXEC_ORGAN_C).ToList() ;
            int orgCheckpointCnt = orgCheckpoints.Count;
            CheckpointExpiryModel checkpointData = projectCheckpointData.Where(x => x.PROJECT_NO == data.PROJECT_NO)
                .FirstOrDefault();
            if (checkpointData == null)
                return;
            // 表5 :流標3次明細資料 檢核點需為 「確定技術服務廠商」或「工程決標」，否則不顯示
            if (data.COM_IPCMEMO == "05")
            {
                if (checkpointData.CTRL_POINT == "D" || checkpointData.CTRL_POINT == "E")
                    orgCheckpointCnt = orgCheckpoints.Where(x => x.CTRL_POINT == "D" || x.CTRL_POINT == "E").Count();
                else
                    return;
            }

            // 管考意見備註代碼對應Table Seq
            string tbSeq = memoTypeMappingTableSeq[data.COM_IPCMEMO];
            SetCommonReplacement(data, Int32.Parse(tbSeq), orgCheckpointCnt, ref replaceData);

            replaceData.Add($"Stat{tbSeq}CheckItem", checkpointData.CHECKITEM_NAME);
            replaceData.Add($"Stat{tbSeq}EndDate", checkpointData.ESTIMATED_ENDDATE.ToTwDateString());

            tbDataSets[$"tb{tbSeq}"].Add(replaceData);
        }

        /// <summary>
        /// 處理工程進度來源資料
        /// 表6, 7
        /// </summary>
        private void ProcessEngineerProgress(ProjectComIpcMemoModel data)
        {
            Dictionary<string, object> replaceData = new();
            // 管考意見備註代碼對應Table Seq
            string tbSeq = memoTypeMappingTableSeq[data.COM_IPCMEMO];
            // 機關工程進度件數
            int orgEngPrgCnt = 0;
            
            // 表6 「工程進度落後10%以上」明細表 (僅統計當月)
            if (data.COM_IPCMEMO == "03")
            {
                ProjectEngPrgRPTModel prgData = projectEngPrgData.Where(x => x.YEAR == queryModel.STATISTICS_YEAR
                    && x.MONTH == $"{queryModel.STATISTICS_MONTH:D2}" 
                    && x.PROJECT_NO == data.PROJECT_NO).FirstOrDefault();
                if (prgData == null)
                    return;

                orgEngPrgCnt = projectEngPrgData.Where(x => x.YEAR == queryModel.STATISTICS_YEAR
                    && x.MONTH == $"{queryModel.STATISTICS_MONTH:D2}"
                    && x.EXEC_ORGAN_C == data.EXEC_ORGAN_C).Count();

                replaceData.Add($"Stat{tbSeq}ResPrg", FormatDecimal(prgData.IPC_RES_PRG)+ "%");
                replaceData.Add($"Stat{tbSeq}ActPrg", FormatDecimal(prgData.IPC_ACT_PRG) + "%");
                replaceData.Add($"Stat{tbSeq}DiffPrg", FormatDecimal(prgData.PRG_DIFF) + "%");
            }
            // 表7 「工程進度連續落後3個月以上」明細表
            else
            {
                // 二個月前工程進度資料
                ProjectEngPrgRPTModel twoMonthsAgoPrgData = projectEngPrgData.Where(x =>
                    x.YEAR == $"{statDate.AddMonths(-2).Year - 1911}" &&
                    x.MONTH == $"{statDate.AddMonths(-2).Month:D2}" && 
                    x.PROJECT_NO == data.PROJECT_NO).FirstOrDefault();
                // 上個月的工程進度資料
                ProjectEngPrgRPTModel lastMonthPrgData = projectEngPrgData.Where(x =>
                    x.YEAR == $"{statDate.AddMonths(-1).Year - 1911}" &&
                    x.MONTH == $"{statDate.AddMonths(-1).Month:D2}" &&
                    x.PROJECT_NO == data.PROJECT_NO).FirstOrDefault();
                // 統計當月工程進度資料
                ProjectEngPrgRPTModel statMonthPrgData = projectEngPrgData.Where(x =>
                    x.YEAR == $"{statDate.Year - 1911}" &&
                    x.MONTH == $"{statDate.Month:D2}" &&
                    x.PROJECT_NO == data.PROJECT_NO).FirstOrDefault();

                // 無工程進度資料時直接回傳
                if (twoMonthsAgoPrgData == null && lastMonthPrgData == null && statMonthPrgData == null)
                    return;

                orgEngPrgCnt = projectEngPrgData.Select(x=> new {x.EXEC_ORGAN_C,x.PROJECT_NO}).Distinct()
                    .GroupBy(x => new { x.EXEC_ORGAN_C })
                    .Select(x => new {x.Key.EXEC_ORGAN_C, orgProjectCnt = x.Count() })
                    .Where(x=>x.EXEC_ORGAN_C == data.EXEC_ORGAN_C).FirstOrDefault()?.orgProjectCnt ?? 0;

                if (twoMonthsAgoPrgData != null)
                {
                    replaceData.Add($"Stat{tbSeq}EngPrg1", string.Format(table7PrgTxtFormat,
                         FormatDecimal(twoMonthsAgoPrgData.IPC_RES_PRG),
                         FormatDecimal(twoMonthsAgoPrgData.IPC_ACT_PRG),
                         FormatDecimal(twoMonthsAgoPrgData.PRG_DIFF)));
                }
                    
                if (lastMonthPrgData != null)
                {
                    replaceData.Add($"Stat{tbSeq}EngPrg2", string.Format(table7PrgTxtFormat,
                         FormatDecimal(lastMonthPrgData.IPC_RES_PRG),
                         FormatDecimal(lastMonthPrgData.IPC_ACT_PRG),
                         FormatDecimal(lastMonthPrgData.PRG_DIFF)));
                }
                    
                if (statMonthPrgData != null)
                {
                    replaceData.Add($"Stat{tbSeq}EngPrg3", string.Format(table7PrgTxtFormat,
                        FormatDecimal(statMonthPrgData.IPC_RES_PRG),
                        FormatDecimal(statMonthPrgData.IPC_ACT_PRG),
                        FormatDecimal(statMonthPrgData.PRG_DIFF)));
                }
                    
            }

            SetCommonReplacement(data, Int32.Parse(tbSeq), orgEngPrgCnt, ref replaceData);
            tbDataSets[$"tb{tbSeq}"].Add(replaceData);
        }

        
        /// <summary>
        /// 格式小數點資料
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string FormatDecimal(decimal? value)
        {
            if (value.HasValue)
                return Math.Round(value.Value,2).ToString();
            
            return string.Empty;
        }

        /// <summary>
        /// 表11 資料 (計畫平時管考意見備註類型明細表)
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="projectName"></param>
        /// <param name="orgMemoData"></param>
        private void ProcessTable11(string projectNo, string projectName, List<ProjectComIpcMemoModel> orgMemoData)
        {
            Dictionary<string, object> replaceData = new();
            // 計畫管考意見備註代碼清單
            List<string> projectMemo = orgMemoData.Where(x => x.PROJECT_NO == projectNo)
                .Select(x => x.COM_IPCMEMO).ToList();
            // 機關名稱
            string orgName = orgMemoData.FirstOrDefault()?.EXEC_ORGAN_NAME ?? "";
            // 加入需垂直合併清單
            AddDataNeedtoMerge(10, new List<string>(){ orgName});

            foreach (string comIpcMemo in comIpcMemoTypes.Select(x=>x.SET_TYPE))
            {
                replaceData.Add($"Stat11Memo{comIpcMemo}Flg", projectMemo.Contains(comIpcMemo) ? "V" : "");    
            }
            replaceData.Add($"Stat11Dept", orgName);
            replaceData.Add($"Stat11ProjectName", projectName);

            tbDataSets[$"tb11"].Add(replaceData);
        }

        /// <summary>
        /// 各表共同替代資料
        /// </summary>
        /// <param name="model">資料</param>
        /// <param name="tbSeq">表號</param>
        /// <param name="orgCnt">機關件數資料</param>
        /// <param name="source">來源替代資料</param>
        private void SetCommonReplacement(ProjectComIpcMemoModel model, int tbSeq, int orgCnt, ref Dictionary<string, object> source)
        {
            // 加入需垂直合併清單
            AddDataNeedtoMerge(tbSeq-1, new List<string>(){
                    model.EXEC_ORGAN_NAME,$"{model.EXEC_ORGAN_NAME},{orgCnt}"});
            source.Add( $"Stat{tbSeq}Dept", model.EXEC_ORGAN_NAME);
            source.Add( $"Stat{tbSeq}ProjectCnt", $"{model.EXEC_ORGAN_NAME},{orgCnt}");
            source.Add( $"Stat{tbSeq}ProjectName", model.PROJECT_NAME);
        }

        /// <summary>
        /// 加入待垂直合併資料
        /// </summary>
        /// <param name="tbIndex"></param>
        /// <param name="mergeItems"></param>
        private void AddDataNeedtoMerge(int tbIndex, List<string> mergeItems)
        {
            foreach (string mergeItem in mergeItems)
            {
                if (tbNeedToMergeDict[tbIndex].IndexOf(mergeItem) == -1)
                    tbNeedToMergeDict[tbIndex].Add(mergeItem);
            }
        }

        /// <summary>
        /// 處理合併儲存格
        /// </summary>
        protected override void Other()
        {
            Dictionary<int, List<string>> targetDict = tbNeedToMergeDict;
            foreach (KeyValuePair<int, List<string>> dataNeedToMerge in targetDict)
            {
                int tbIndex = dataNeedToMerge.Key;
                if (dataNeedToMerge.Value.Any())
                {
                    Table targetTable = (Table)Doc.GetChildNodes(NodeType.Table, true)[tbIndex];
                    foreach (string targetData in dataNeedToMerge.Value)
                    {
                        var dataSet = targetData.Split(',');
                        List<Cell> targetCell = FindCell(targetTable, targetData);
                        VerticalMergeCells(targetCell, dataSet.Length == 2 ? dataSet[1] : targetData);
                    }
                }
            }
        }
    }
}
