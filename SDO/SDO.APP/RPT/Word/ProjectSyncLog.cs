using Aspose.Words;
using Aspose.Words.Tables;
using Autofac;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 重大建設系統介接公共工程雲端服務網資料統計表
    /// </summary>
    class ProjectSyncLog : WContentBuilder
    {
        // 常數宣告
        private readonly IStatisticsDac statisticsDac;
        private readonly IIPCSetParamDac ipcSetparamDac;

        // 8張子表
        private const int tableCount = 9;
        /// <summary>
        /// 界接正常
        /// </summary>
        private const string syncNormal = "00";
        /// <summary>
        /// 無執行情形資料
        /// </summary>
        private const string noExecuteData = "01";
        /// <summary>
        /// 無工程進度資料
        /// </summary>
        private const string noProgressData = "02";
        /// <summary>
        /// 實際工進100%，無竣工日期
        /// </summary>
        private const string noEndWorkDate = "03";
        /// <summary>
        /// 進度落後，無落後原因
        /// </summary>
        private const string noReasonsForBackwardness = "04";
        /// <summary>
        /// 進度正常，有落後原因
        /// </summary>
        private const string reasonsForBackwardness = "05";
        /// <summary>
        /// 工程會系統無落後，但重大建設系統落後
        /// </summary>
        private const string ipcProgressBehind = "06";

        /// <summary>
        /// 規劃中工程
        /// </summary>
        private const string planning = "E1";
        /// <summary>
        /// 施工中工程
        /// </summary>
        private const string engineering = "E2";
        /// <summary>
        /// 驗收中工程
        /// </summary>
        private const string accepting = "E3";
        /// <summary>
        /// 執行中非工程類計畫
        /// </summary>
        private const string nonEngineering = "S1";


        // 變數宣告
        // 查詢條件
        private StatisticsModel queryModel = new();
        // 需合併儲存格Table Index及替代資料
        private Dictionary<int, List<string>> tbNeedToMergeDict = new();
        // 需移除Table Index List
        private List<int> tbNeedToDelete = new();
        // 介接公共工程資料統計表
        private List<ProjectSyncLogModel> projectSyncLogData = new();
        // 執行中計畫介接計畫
        private List<ProjectSyncLogModel> projectSyncData = new();
        // 界接異常類型
        private List<IPCSetParamModel> syncAbnormalTypes = new();

        // 表1~表8資料集
        private Dictionary<string, List<Dictionary<string, object>>> tbDataSets = new();

        /*** 執行中計畫列管情形 ***/
        // 規劃中合計
        int planningProjectSum = 0;
        // 施工中界接合計
        int constructionSyncProjectSum = 0;
        // 施工中非界接合計
        int constructionNonSyncProjectSum = 0;
        // 驗收中界接合計
        int acceptingSyncProjectSum = 0;
        // 驗收中非界接合計
        int acceptingNonSyncProjectSum = 0;
        // 非工程類合計
        int nonEngineeringgProjectSum = 0;

        /*** 執行中計畫介接情形 ***/
        // 介接正常件數合計
        int stat1Sync00Sum = 0;
        // 無執行情形資料件數合計
        int stat1Sync01Sum = 0;
        // 無工程進度資料件數合計
        int stat1Sync02Sum = 0;
        // 實際工進100%，無竣工日期合計
        int stat1Sync03Sum = 0;
        // 進度落後，無落後原因合計
        int stat1Sync04Sum = 0;
        // 進度正常，有落後原因合計
        int stat1Sync05Sum = 0;
        // 工程會系統無落後，但重大建設系統落後 合計
        int stat1Sync06Sum = 0;



        public ProjectSyncLog(IComponentContext coms) : base(coms)
        {
            statisticsDac = coms.Resolve<IStatisticsDac>();
            this.ipcSetparamDac = coms.Resolve<IIPCSetParamDac>();
        }
        protected override void SetTemplateFileName()
        {
            TemplateFileName = "ProjectSyncLogRPT.docx";
        }

        protected override async Task GetData()
        {
            queryModel = (StatisticsModel)Parameter.ObjectModel;

            // 從 temp 撈取資料
            var tempProjectSyncLogList = await statisticsDac.GetProjectSyncLogList(queryModel);

            // 透過 PROJECT_NO 分組，再取每組第一筆，達到篩選 PROJECT_NO 重複值
            projectSyncLogData = tempProjectSyncLogList
                .GroupBy(x => x.PROJECT_NO)
                .Select(x => x.First())
                .ToList();

            projectSyncData = tempProjectSyncLogList.Where(x => x.IS_USER_FTY_DATA).ToList();
            syncAbnormalTypes = (await ipcSetparamDac.GetSysParams("GDB_ERROR_ITEM", false)).ToList();

            FillContent();

            // ListData 處理
            // 各表資料集Index
            int tbDataSetIndex = 0;
            foreach (KeyValuePair<string, List<Dictionary<string, object>>> tbDataSet in tbDataSets)
            {
                if (tbDataSet.Value.Any())
                    ListData.Add(new WordTableData { LIST_DATA = tbDataSet.Value, TABLE_INDEX = tbDataSetIndex });

                tbDataSetIndex++;
            }


            // BasicData 處理
            // 包含: 年、月、日、表1合計列資料
            Dictionary<string, object> basicData = new()
            {
                { "YEAR", queryModel.STATISTICS_YEAR },
                { "MONTH", queryModel.STATISTICS_MONTH },
                { "DATE", DateTime.Now.ToTwDateString() },
                { "Stat1PlanningSum", planningProjectSum },
                { "Stat1ConstructionSyncSum", constructionSyncProjectSum },
                { "Stat1ConstructionNonSyncSum", constructionNonSyncProjectSum },
                { "Stat1AcceptingSyncSum", acceptingSyncProjectSum },
                { "Stat1AcceptingNonSyncSum", acceptingNonSyncProjectSum },
                { "Stat1NonEngineeringSum", nonEngineeringgProjectSum },
                { "Stat1SyncSum", constructionSyncProjectSum + acceptingSyncProjectSum },
                { "Stat1Sync00Sum", stat1Sync00Sum },
                { "Stat1Sync01Sum", stat1Sync01Sum },
                { "Stat1Sync02Sum", stat1Sync02Sum },
                { "Stat1Sync03Sum", stat1Sync03Sum },
                { "Stat1Sync04Sum", stat1Sync04Sum },
                { "Stat1Sync05Sum", stat1Sync05Sum },
                { "Stat1Sync06Sum", stat1Sync06Sum }
            };

            // 加入Table Seq 替代資料 & 需移除Table清單
            HandleTableSeq(ref basicData);

            BasicData = basicData;
        }

        private void FillContent()
        {
            if (projectSyncLogData.Any())
            {
                // 表1~表9資料集
                for (int i = 1; i <= tableCount; i++)
                    tbDataSets.Add($"tb{i}", new List<Dictionary<string, object>>());

                // 需合併儲存格的 Table Index 及替代資料 (表2~表8)
                for (int i = 2; i <= tableCount; i++)
                    tbNeedToMergeDict.Add(i, new List<string>());
                

                // 執行機關資料
                List<string> orgs = projectSyncLogData.Select(x => x.EXEC_ORGAN_C).Distinct().ToList();
                Dictionary<string, string> dataOrgs = projectSyncLogData.DistinctBy(x => x.EXEC_ORGAN_C)
                    .ToDictionary(x => x.EXEC_ORGAN_C, y => y.EXEC_ORGAN_NAME);


                // 依據機關迴圈
                foreach (KeyValuePair<string, string> org in dataOrgs)
                {
                    // 處理表1替代資料
                    // 表1 替代資料
                    Dictionary<string, object> tb1ReplaceData = new Dictionary<string, object>();

                    // 執行機關
                    tb1ReplaceData.Add("Stat1Dept", org.Value);

                    /*** 執行中計畫列管情形 ***/
                    // 此執行機關 規劃中件數
                    int orgPlanningCnt = projectSyncLogData
                        .Count(x => x.PROJECT_STAGE == planning && x.EXEC_ORGAN_C == org.Key);
                    tb1ReplaceData.Add("PlanningCnt", orgPlanningCnt);

                    // 此執行機關 施工中介接填報件數
                    int orgConstructionSyncCnt = projectSyncLogData
                        .Count(x => x.PROJECT_STAGE == engineering && x.EXEC_ORGAN_C == org.Key && x.IS_USER_FTY_DATA);
                    tb1ReplaceData.Add("Stat1ConstructionSyncCnt", orgConstructionSyncCnt);

                    // 此執行機關 施工中非介接填報件數
                    int orgConstructionNonSyncCnt = projectSyncLogData
                        .Count(x => x.PROJECT_STAGE == engineering && x.EXEC_ORGAN_C == org.Key && !x.IS_USER_FTY_DATA);
                    tb1ReplaceData.Add("Stat1ConstructionNonSyncCnt", orgConstructionNonSyncCnt);

                    // 此執行機關 驗收中介接填報件數
                    int orgAcceptingSyncCnt = projectSyncLogData
                        .Count(x => x.PROJECT_STAGE == accepting && x.EXEC_ORGAN_C == org.Key && x.IS_USER_FTY_DATA);
                    tb1ReplaceData.Add("Stat1AcceptingSyncCnt", orgAcceptingSyncCnt);

                    // 此執行機關 驗收非中介接填報件數
                    int orgAcceptingNonSyncCnt = projectSyncLogData
                        .Count(x => x.PROJECT_STAGE == accepting && x.EXEC_ORGAN_C == org.Key && !x.IS_USER_FTY_DATA);
                    tb1ReplaceData.Add("Stat1AcceptingNonSyncCnt", orgAcceptingNonSyncCnt);

                    // 此執行機關 非工程類填報件數
                    int orgStat1NonEngineeringCnt = projectSyncLogData
                        .Count(x => x.PROJECT_STAGE == nonEngineering && x.EXEC_ORGAN_C == org.Key);
                    tb1ReplaceData.Add("Stat1NonEngineeringCnt", orgStat1NonEngineeringCnt);


                    /*** 執行中計畫介接情形 ***/
                    // 此執行機關 介接件數
                    int orgStat1SyncCnt = projectSyncData
                        .Count(x => x.IS_USER_FTY_DATA && x.EXEC_ORGAN_C == org.Key);
                    tb1ReplaceData.Add("Stat1SyncCnt", orgStat1SyncCnt);

                    // 此執行機關 介接正常件數
                    int orgStat1Sync00Cnt = projectSyncData
                        .Count(x => x.IS_USER_FTY_DATA && x.EXEC_ORGAN_C == org.Key && x.GDB_ERROR_ITEM == syncNormal);
                    tb1ReplaceData.Add("Stat1Sync00Cnt", orgStat1Sync00Cnt);

                    // 此執行機關 無執行情形資料件數
                    int orgStat1Sync01Cnt = projectSyncData
                        .Count(x => x.IS_USER_FTY_DATA && x.EXEC_ORGAN_C == org.Key && x.GDB_ERROR_ITEM == noExecuteData);
                    tb1ReplaceData.Add("Stat1Sync01Cnt", orgStat1Sync01Cnt);

                    // 此執行機關 無工程進度資料
                    int orgStat1Sync02Cnt = projectSyncData
                        .Count(x => x.IS_USER_FTY_DATA && x.EXEC_ORGAN_C == org.Key && x.GDB_ERROR_ITEM == noProgressData);
                    tb1ReplaceData.Add("Stat1Sync02Cnt", orgStat1Sync02Cnt);

                    // 實際工進100%，無竣工日期
                    int orgStat1Sync03Cnt = projectSyncData
                       .Count(x => x.IS_USER_FTY_DATA && x.EXEC_ORGAN_C == org.Key && x.GDB_ERROR_ITEM == noEndWorkDate);
                    tb1ReplaceData.Add("Stat1Sync03Cnt", orgStat1Sync03Cnt);

                    // 此執行機關 進度落後，無落後原因
                    int orgStat1Sync04Cnt = projectSyncData
                        .Count(x => x.IS_USER_FTY_DATA && x.EXEC_ORGAN_C == org.Key && x.GDB_ERROR_ITEM == noReasonsForBackwardness);
                    tb1ReplaceData.Add("Stat1Sync04Cnt", orgStat1Sync04Cnt);

                    // 此執行機關 進度正常，有落後原因
                    int orgStat1Sync05Cnt = projectSyncData
                        .Count(x => x.IS_USER_FTY_DATA && x.EXEC_ORGAN_C == org.Key && x.GDB_ERROR_ITEM == reasonsForBackwardness);
                    tb1ReplaceData.Add("Stat1Sync05Cnt", orgStat1Sync05Cnt);

                    // 此執行機關 工程會系統無落後，但重大建設系統落後
                    int orgStat1Sync06Cnt = projectSyncData
                        .Count(x => x.IS_USER_FTY_DATA && x.EXEC_ORGAN_C == org.Key && x.GDB_ERROR_ITEM == ipcProgressBehind);
                    tb1ReplaceData.Add("Stat1Sync06Cnt", orgStat1Sync06Cnt);

                    tbDataSets["tb1"].Add(tb1ReplaceData);



                    // 處理表1合計欄位
                    planningProjectSum += orgPlanningCnt;
                    constructionSyncProjectSum += orgConstructionSyncCnt;
                    constructionNonSyncProjectSum += orgConstructionNonSyncCnt;
                    acceptingSyncProjectSum += orgAcceptingSyncCnt;
                    acceptingNonSyncProjectSum += orgAcceptingNonSyncCnt;
                    nonEngineeringgProjectSum += orgStat1NonEngineeringCnt;

                    stat1Sync00Sum += orgStat1Sync00Cnt;
                    stat1Sync01Sum += orgStat1Sync01Cnt;
                    stat1Sync02Sum += orgStat1Sync02Cnt;
                    stat1Sync03Sum += orgStat1Sync03Cnt;
                    stat1Sync04Sum += orgStat1Sync04Cnt;
                    stat1Sync05Sum += orgStat1Sync05Cnt;
                    stat1Sync06Sum += orgStat1Sync06Cnt;

                    // 機關各異常狀態件數
                    Dictionary<string, int> orgErrorTypeCnt = new Dictionary<string, int>()
                    {
                        {syncNormal, orgStat1Sync00Cnt },
                        {noExecuteData, orgStat1Sync01Cnt },
                        {noProgressData, orgStat1Sync02Cnt }, 
                        {noEndWorkDate, orgStat1Sync03Cnt },   
                        {noReasonsForBackwardness, orgStat1Sync04Cnt },
                        {reasonsForBackwardness, orgStat1Sync05Cnt },
                        {ipcProgressBehind, orgStat1Sync06Cnt },
                    };

                    int tbIndex = 2;

                    foreach(KeyValuePair<string, int> data in orgErrorTypeCnt)
                    {
                        ProcessState2To8(tbIndex, org.Value, org.Key, data.Value, data.Key);
                        tbIndex++;
                    }

                    // 處理表8替代資料
                    // 排除 GDB_ERROR_ITEM 正常狀態及狀態是 null 的資料
                    // 只需要狀態是 01~06
                    List<ProjectSyncLogModel> projectOrgSyncData = projectSyncData
                        .Where(x => x.EXEC_ORGAN_C == org.Key
                         && x.GDB_ERROR_ITEM != syncNormal
                         && x.GDB_ERROR_ITEM != null)
                        .ToList();

                    var projectNoDict = new Dictionary<string, List<ProjectSyncLogModel>>();

                    foreach (ProjectSyncLogModel data in projectOrgSyncData)
                    {
                        // key: PROJECT_NO，value: 相同 PROJECT_NO 的資料 List
                        if (!projectNoDict.ContainsKey(data.PROJECT_NO))
                            projectNoDict.Add(data.PROJECT_NO, new List<ProjectSyncLogModel>());
                        projectNoDict[data.PROJECT_NO].Add(data);
                    }
                    foreach (KeyValuePair<string, List<ProjectSyncLogModel>> item in projectNoDict)
                    {
                        ProcessDetailReplaceData(org.Value, item.Key, item.Value);
                    }
                }
            }
            
        }


        /// <summary>
        /// 處理表 2 ~ 8 替代資料
        /// </summary>
        /// <param name="tbNo">第幾張表</param>
        /// <param name="orgName">機關名稱</param>
        /// <param name="orgName">機關編號</param>
        /// <param name="projectCnt">該機關計畫的件數</param>
        /// <param name="syncType">該機關計畫的界接狀態</param>
        private void ProcessState2To8(int tbNo, string orgName, string orgId, int projectCnt, string syncType)
        {
            if (projectCnt == 0)
                return;

            // 機關資料
            List<ProjectSyncLogModel> orgProjects = projectSyncData
                .Where(x => x.EXEC_ORGAN_C == orgId && x.GDB_ERROR_ITEM == syncType)
                .ToList();

            foreach (ProjectSyncLogModel orgProject in orgProjects)
            {
                Dictionary<string, object> tbReplaceData = new Dictionary<string, object>
                {
                    { $"Stat{tbNo}ProjectName", orgProject.PROJECT_NAME },
                    { $"Stat{tbNo}Dept", orgName },
                    { $"Stat{tbNo}ProjectCnt", $"{projectCnt},{orgName}" },
                    { $"Stat{tbNo}EngProgress", string.Format("預定：{0}\n實際：{1}\n差異：{2}",
                        orgProject.FormatProgress(orgProject.IPC_RES_PRG),
                        orgProject.FormatProgress(orgProject.IPC_ACT_PRG),
                        orgProject.FormatProgress(orgProject.IPC_DIFF_PRG))},
                    { $"Stat{tbNo}EndWorkDate", orgProject.COMPLETION_ACTUAL_ENDDATE }
                };

                tbDataSets[$"tb{tbNo}"].Add(tbReplaceData);
            }

            AddDataNeedtoMerge(tbNo, new List<string>() { orgName, $"{projectCnt},{orgName}" });
        }

        /// <summary>
        /// 處理 表9 界接異常類型明細 替代資料
        /// </summary>
        /// <param name="orgName">執行機關名稱</param>
        /// <param name="projectNo">計畫名稱</param>
        /// <param name="projectOrgSyncData">該計畫的計畫計畫異常集</param>
        private void ProcessDetailReplaceData(string orgName, string projectNo, List<ProjectSyncLogModel> projectOrgSyncData)
        {
            // 表9 替代資料
            Dictionary<string, object> tb9ReplaceData = new();
            foreach(string type in projectOrgSyncData.Select(x => x.GDB_ERROR_ITEM))
            {
                tb9ReplaceData.Add(type, "V");
            }

            string projectName = projectOrgSyncData
                .Where(x => x.PROJECT_NO == projectNo)
                .Select(x => x.PROJECT_NAME).ToList().FirstOrDefault();
            tb9ReplaceData.Add("Stat9Dept", orgName);
            tb9ReplaceData.Add("Stat9ProjectName", projectName);

            AddDataNeedtoMerge(9, new List<string>() {orgName});

            tbDataSets["tb9"].Add(tb9ReplaceData);
        }

        /// <summary>
        /// 處理表頭編號 & 判斷是否移除無資料Table
        /// </summary>
        /// <param name="basicData"></param>
        private void HandleTableSeq(ref Dictionary<string, object> basicData)
        {
            // 有資料 Table編號
            int tableSeqWithData = 2;
            // Table編號 : 明細表從表2開始
            int tableNum = 2;
            // Table 無資料的數量
            int tableWithNoDate = 0;

            foreach (string gdbErrorItem in syncAbnormalTypes.Select(x => x.SET_TYPE).ToList())
            {
                // 若無介接明細，需移除該範本表格
                if (Convert.ToInt32(basicData[$"Stat1Sync{gdbErrorItem}Sum"]) == 0)
                {
                    tbNeedToDelete.Add(tableNum);
                    tableWithNoDate++;
                }
                // 有資料才繼續編號
                else
                {
                    basicData.Add($"T{tableNum}Seq", tableSeqWithData);
                    tableSeqWithData++;
                }
                tableNum++;
            }
            // Table 無資料的數量跟介接異常數量一樣，代表每一張都沒有資料，最後一張表也要刪除
            if(tableWithNoDate == syncAbnormalTypes.Count)
            {
                tbNeedToDelete.Add(9);
            }
            basicData.Add($"T{tableNum}Seq", tableSeqWithData);
        } 

        /// <summary>
        /// 加入待垂直合併資料
        /// </summary>
        /// <param name="tbIndex">第幾張Table</param>
        /// <param name="mergeItems">要合併的資料</param>
        private void AddDataNeedtoMerge(int tbIndex, List<string> mergeItems)
        {
            foreach (string mergeItem in mergeItems)
            {
                // 不包含則回傳 -1
                if (tbNeedToMergeDict[tbIndex].IndexOf(mergeItem) == -1)
                    tbNeedToMergeDict[tbIndex].Add(mergeItem);
            }
        }

        /// <summary>
        /// 處理合併儲存格
        /// </summary>
        protected override void Other()
        {
            // 合併儲存格
            Dictionary<int, List<string>> targetDict = tbNeedToMergeDict;
            foreach (KeyValuePair<int, List<string>> dataNeedToMerge in targetDict)
            {
                int tbIndex = dataNeedToMerge.Key;
                if (dataNeedToMerge.Value.Any())
                {
                    // 取文件的所有 Table 節點，[] 裡面是抓第幾個 Table，索引從 0 開始
                    Table targetTable = (Table)Doc.GetChildNodes(NodeType.Table, true)[tbIndex - 1];
                    foreach (string targetData in dataNeedToMerge.Value)
                    {
                        // 判斷出現逗號就分隔，回傳成陣列
                        var dataSet = targetData.Split(',');
                        // 找到該 Table 有一樣的值，組合成一個 List
                        List<Cell> targetCell = FindCell(targetTable, targetData);
                        // 當 dataSet.Length == 2，代表 key 有兩個合成的（EX: 執行機關跟計數）
                        VerticalMergeCells(targetCell, dataSet.Length == 2 ? dataSet[0] : targetData);
                    }
                }
            }

            // 移除無資料表格
            List<Table> removedTable = new ();
            foreach(int tbNum in tbNeedToDelete)
                removedTable.Add((Table)Doc.GetChildNodes(NodeType.Table, true)[tbNum - 1]);
            
            foreach (Table targetTable in removedTable)
                targetTable.Remove();
        }
    }
}
