using Aspose.Cells;
using Autofac;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Excel
{
    /// <summary>
    /// 重大建設系統介接公共工程雲端服務網資料一覽表
    /// </summary>
    class RPTProjectSyncLog : XlsBuilder
    {

        /*** 變數宣告 ***/

        /// <summary>
        /// 查詢條件
        /// </summary>
        private StatisticsModel queryModel = new();

        private IStatisticsDac statisticsDac;

        // 清單資料起始列，row 的 index 從 1 開始
        private int row = 5;

        

        /// <summary>
        /// 以上的介接計畫
        /// </summary>
        private Dictionary<List<ProjectSyncLogOverviewModel>, int> projectSyncLogDict = new();

        /*** 執行中計畫列管情形 ***/
        /// <summary>
        /// 界接正常資料數量
        /// </summary>
        private int syncNormalCnt;
        /// <summary>
        /// 無執行情形資料資料數量
        /// </summary>
        private int noExecuteDataCnt;
        /// <summary>
        /// 無工程進度資料資料數量
        /// </summary>
        private int noProgressDataCnt;
        /// <summary>
        /// 實際工進100%，無竣工日期
        /// </summary>
        private int noEndWorkDateCnt;
        /// <summary>
        /// 進度落後，無落後原因資料數量
        /// </summary>
        private int noReasonsForBackwardnessCnt;
        /// <summary>
        /// 進度正常，有落後原因資料數量
        /// </summary>
        private int reasonsForBackwardnessCnt;
        /// <summary>
        /// 工程會系統無落後，但重大建設系統落後資料數量
        /// </summary>
        private int ipcProgressBehindCnt;

        /// <summary>
        /// 介接狀態代號
        /// </summary>
        private List<IPCSetParamModel> syncTypes = new();

        IIPCSetParamDac ipcSetparamDac;


        public RPTProjectSyncLog(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "ProjectSyncLogRPT.xlsx";
            statisticsDac = coms.Resolve<IStatisticsDac>();
            ipcSetparamDac = coms.Resolve<IIPCSetParamDac>();
        }

        protected override async Task GetData()
        {
            // 界接正常代號
            const string syncNormal = "00";

            // 無執行情形資料代號
            const string noExecuteData = "01";

            // 無工程進度資料代號
            const string noProgressData = "02";

            // 實際工進100%，無竣工日期
            const string noWorkEndDate = "03";

            // 進度落後，無落後原因代號
            const string noReasonsForBackwardness = "04";

            // 進度正常，有落後原因代號
            const string reasonsForBackwardness = "05";

            // 工程會系統無落後，但重大建設系統落後代號
            const string ipcProgressBehind = "06";

            // Dac 撈回來的初始資料
            List<ProjectSyncLogOverviewModel> tempProjectSyncLogOverviewList = new();

            // 界接正常資料
            List<ProjectSyncLogOverviewModel> syncNormalList = new();

            // 無執行情形資料資料
            List<ProjectSyncLogOverviewModel> noExecuteDataList = new();

            // 無工程進度資料資料
            List<ProjectSyncLogOverviewModel> noProgressDataList = new();

            // 實際工進100%，無竣工日期
            List<ProjectSyncLogOverviewModel> noEndWorkDateList = new();

            // 進度落後，無落後原因資料
            List<ProjectSyncLogOverviewModel> noReasonsForBackwardnessList = new();

            // 進度正常，有落後原因資料
            List<ProjectSyncLogOverviewModel> reasonsForBackwardnessList = new();

            // 工程會系統無落後，但重大建設系統落後資料
            List<ProjectSyncLogOverviewModel> ipcProgressBehindList = new();

            queryModel = (StatisticsModel)Parameter.ObjectModel;
            tempProjectSyncLogOverviewList = await statisticsDac.GetProjectSyncLogOverviewList(queryModel);


            if (tempProjectSyncLogOverviewList.Any())
            {
                // 界接正常資料
                syncNormalList = tempProjectSyncLogOverviewList
                    .Where(x => x.GDB_ERROR_ITEM == syncNormal).ToList();
                syncNormalCnt = syncNormalList.Count();
                projectSyncLogDict[syncNormalList] = syncNormalCnt;

                // 無執行情形資料資料
                noExecuteDataList = tempProjectSyncLogOverviewList
                    .Where(x => x.GDB_ERROR_ITEM == noExecuteData).ToList();
                noExecuteDataCnt = noExecuteDataList.Count();
                projectSyncLogDict[noExecuteDataList] = noExecuteDataCnt;

                // 無工程進度資料資料
                noProgressDataList = tempProjectSyncLogOverviewList
                    .Where(x => x.GDB_ERROR_ITEM == noProgressData).ToList();
                noProgressDataCnt = noProgressDataList.Count();
                projectSyncLogDict[noProgressDataList] = noProgressDataCnt;

                // 實際工進100%，無竣工日期
                noEndWorkDateList = tempProjectSyncLogOverviewList
                    .Where(x => x.GDB_ERROR_ITEM == noWorkEndDate).ToList();
                noEndWorkDateCnt = noEndWorkDateList.Count();
                projectSyncLogDict[noEndWorkDateList] = noEndWorkDateCnt;

                // 進度落後，無落後原因資料
                noReasonsForBackwardnessList = tempProjectSyncLogOverviewList
                    .Where(x => x.GDB_ERROR_ITEM == noReasonsForBackwardness).ToList();
                noReasonsForBackwardnessCnt = noReasonsForBackwardnessList.Count();
                projectSyncLogDict[noReasonsForBackwardnessList] = noReasonsForBackwardnessCnt;

                // 進度正常，有落後原因資料
                reasonsForBackwardnessList = tempProjectSyncLogOverviewList
                    .Where(x => x.GDB_ERROR_ITEM == reasonsForBackwardness).ToList();
                reasonsForBackwardnessCnt = reasonsForBackwardnessList.Count();
                projectSyncLogDict[reasonsForBackwardnessList] = reasonsForBackwardnessCnt;

                // 工程會系統無落後，但重大建設系統落後資料
                ipcProgressBehindList = tempProjectSyncLogOverviewList
                    .Where(x => x.GDB_ERROR_ITEM == ipcProgressBehind).ToList();
                ipcProgressBehindCnt = ipcProgressBehindList.Count();
                projectSyncLogDict[ipcProgressBehindList] = ipcProgressBehindCnt;
            }
        }

        protected override void MakeContent()
        {

            // InsertRow(1) 在第一行之後插入
            // DeleteRow(1) 刪除第二行，index 由 0 開始算

            // 取代範本參數
            CellReplaceByExcel(new
            {
                YEAR = queryModel.STATISTICS_YEAR,
                MONTH = queryModel.STATISTICS_MONTH,
                DATE = DateTime.Now.ToTwDateString(),
                syncNormalCnt = syncNormalCnt,
                noExecuteDataCnt = noExecuteDataCnt,
                noProgressDataCnt = noProgressDataCnt,
                noEndWorkDateCnt= noEndWorkDateCnt,
                noReasonsForBackwardnessCnt = noReasonsForBackwardnessCnt,
                reasonsForBackwardnessCnt = reasonsForBackwardnessCnt,
                ipcProgressBehindCnt = ipcProgressBehindCnt,
            });

            Cells cells = Sheet.Cells;

            foreach(var item in projectSyncLogDict)
            {
                isCntGreaterThan0(cells, item.Value ,item.Key);
            }

        }

        /// <summary>
        /// 計畫數量是否大於0
        /// </summary>
        /// <param name="cells"></param>儲存格
        /// <param name="cnt"></param>計畫數量
        /// <param name="projectList"></param>計畫List
        private void isCntGreaterThan0(Cells cells, int cnt, List<ProjectSyncLogOverviewModel> projectList)
        {
            if(cnt > 0)
            {
                processProjectList(cells, projectList);
            }
            else
            {
                cells.DeleteRow(row - 1);
                row += 1;
            }
        }

        /// <summary>
        /// 處理計畫的 List，放入儲存格
        /// </summary>
        /// <param name="cells"></param>要放入的儲存格
        /// <param name="projectList"></param>計畫 List
        private void processProjectList(Cells cells, List<ProjectSyncLogOverviewModel> projectList)
        {
            int index = 1;

            foreach (var project in projectList)
            {
                SetColumn(cells[$"A{row}"], index);
                SetColumn(cells[$"B{row}"], project.PROJECT_NAME);
                SetColumn(cells[$"C{row}"], project.PROJECT_NO);
                SetColumn(cells[$"D{row}"], project.PCC_PROJECT_NO);
                SetColumn(cells[$"E{row}"], project.PCC_PROJECT_UID);
                SetColumn(cells[$"F{row}"], project.EXEC_ORGAN_NAME);
                SetColumn(cells[$"G{row}"], project.EXECUTE_CONDITION);
                SetColumn(cells[$"H{row}"], project.ASSISTANT_ITEM);
                SetColumn(cells[$"I{row}"], project.START_ESTIMATED_ENDDATE.ToTwDateString());
                SetColumn(cells[$"J{row}"], project.START_ACTUAL_ENDDATE.ToTwDateString());
                SetColumn(cells[$"K{row}"], project.COMPLETION_ESTIMATED_ENDDATE.ToTwDateString());
                SetColumn(cells[$"L{row}"], project.COMPLETION_ACTUAL_ENDDATE.ToTwDateString());
                SetColumn(cells[$"M{row}"], project.ACCEPT_ESTIMATED_ENDDATE.ToTwDateString());
                SetColumn(cells[$"N{row}"], project.ACCEPT_ACTUAL_ENDDATE.ToTwDateString());
                SetColumn(cells[$"O{row}"], project.START_PCC_ESTIMATED_ENDDATE.ToTwDateString());
                SetColumn(cells[$"P{row}"], project.START_PCC_ACTUAL_ENDDATE.ToTwDateString());
                SetColumn(cells[$"Q{row}"], project.COMPLETION_PCC_ESTIMATED_ENDDATE.ToTwDateString());
                SetColumn(cells[$"R{row}"], project.COMPLETION_PCC_ACTUAL_ENDDATE.ToTwDateString());

                SetColumn(cells[$"S{row}"],
                    $"預定：{ project.FormatProgress(project.IPC_RES_PRG) }\n" +
                    $"實際：{ project.FormatProgress(project.IPC_ACT_PRG) }\n" +
                    $"差異：{ project.FormatProgress(project.IPC_DIFF_PRG) }");

                SetColumn(cells[$"T{row}"],
                    $"預定：{ project.FormatProgress(project.TEN_RES_PRG) }\n" +
                    $"實際：{ project.FormatProgress(project.TEN_ACT_PRG) }\n" +
                    $"差異：{ project.FormatProgress(project.TEN_DIFF_PRG) }");

                SetColumn(cells[$"U{row}"], project.DELAY_KIND);
                SetColumn(cells[$"V{row}"], project.DELAY_CLASS_C);
                SetColumn(cells[$"W{row}"], project.DELAY_SUBCLASS_C);
                SetColumn(cells[$"X{row}"], project.DELAY_RESPON);
                SetColumn(cells[$"Y{row}"], project.DELAY_CAUSAL);
                SetColumn(cells[$"Z{row}"], project.SOLUTION);
                SetColumn(cells[$"AA{row}"], project.COORDINATION);
                SetColumn(cells[$"AB{row}"], project.DEADLINES);

                index += 1;
                cells.InsertRow(row);
                row += 1;

            }
            cells.DeleteRow(row - 1);
            row += 1;
        }
    }
}
