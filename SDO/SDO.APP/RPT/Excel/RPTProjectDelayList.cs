using Aspose.Cells;
using Autofac;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Excel
{
    public class RPTProjectDelayList : XlsBuilder
    {
        private IStatisticsDac statisticsDac;
        private IProjectAdjustDac projAdjDac;
        private StatisticsModel statistics;
        private List<ProjectDelayListModel> delayDatas; // 落後資料
        private List<ProjectConferenceRPTModel> conferenceDatas; // 會議列管資料
        private List<ProjectCusCheckpointModel> checkItemDatas; // 檢核點資料
        private List<AdjustScheHistoryModel> adjScheDatas; // 期程調整資料

        public RPTProjectDelayList(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "ProjectDelayListRPT.xlsx";
            statisticsDac = coms.Resolve<IStatisticsDac>();
            projAdjDac = coms.Resolve<IProjectAdjustDac>();
        }

        protected override async Task GetData()
        {
            StatisticsModel model = Parameter.ObjectModel is null ? new() : (StatisticsModel)(Parameter.ObjectModel);
            statistics = model;
            delayDatas = await statisticsDac.GetProjectDelayList(model);
            delayDatas = delayDatas
                .Where(x => x.DATA_YEAR == model.STATISTICS_YEAR && x.DATA_MONTH == model.STATISTICS_MONTH)
                .OrderBy(x => x.OU_SORT_ORDER)
                .ThenBy(x => x.DELAY_KIND == "D2" ? 0
                    : x.DELAY_KIND == "D1" ? 1
                    : x.DELAY_KIND == "D3" ? 2
                    : 3)
                .ToList();
            if (!delayDatas.Any())
            {
                throw new Exception("無檔案可下載");
            }

            List<string> projectNos = delayDatas.Select(x => x.PROJECT_NO).ToList();
            conferenceDatas = await statisticsDac.GetProjectConferenceForRPT(projectNos);
            checkItemDatas = await statisticsDac.GetProjectCheckItemForRPT(projectNos);
            adjScheDatas = (await projAdjDac.GetProjAdjScheHistory(projectNos)).Where(x => x.SCHE_TYPE == "Y").ToList();
        }

        protected override void MakeContent()
        {
            string delayKindCnt = "";
            if (delayDatas.Any())
            {
                List<string> allDelayKind = delayDatas.Select(x => x.DELAY_KIND).Distinct().ToList();
                foreach (string delayKind in allDelayKind)
                {
                    string seperater = delayKind == allDelayKind.Last() ? "" : "、";
                    delayKindCnt += $"{delayKind.Trim()}-{delayDatas.Where(x => x.DELAY_KIND == delayKind).Count()}案" + $"{seperater}";
                }
            }

            CellReplaceByExcel(new
            {
                YEAR_MONTH = $"{statistics.STATISTICS_YEAR}年{statistics.STATISTICS_MONTH}月",
                TotalCnt = delayDatas.Count,
                DelayKindCnt = delayKindCnt,
                DATE = DateTime.Now.ToTwDateString()
            });

            Cells cells = Sheet.Cells;

            // 清單資料起始列
            int strRow = 3;
            int index = 0;
            foreach (ProjectDelayListModel item in delayDatas)
            {
                int row = strRow + index;
                if (index > 0)
                {
                    cells.InsertRow(row - 1);
                }
                // 工程進度資料
                decimal ipcResPrg = item.IPC_RES_PRG ?? 0;
                decimal ipcActPrg = item.IPC_ACT_PRG ?? 0;
                string engProgressData = item.CP_KIND == "1" || (item.CP_KIND == "0" && !item.ACTUAL_ENDDATE.HasValue)
                    ? "預定:\n實際:\n差異:"
                    : $"預定:{Math.Round(ipcResPrg, 1)}%\n實際:{Math.Round(ipcActPrg, 1)}%\n差異:{Math.Round(ipcActPrg - ipcResPrg, 1)}%";

                // 完整檢核點
                string checkpointString = GenFullCheckpointDataString(checkItemDatas.Where(x => x.PROJECT_NO == item.PROJECT_NO).ToList());
                // 總期程調整
                string adjScheString = GenAdjScheDataString(adjScheDatas.Where(x => x.PROJECT_NO == item.PROJECT_NO).ToList());
                // 會議資料 
                string conferenceString = GenConferenceString(conferenceDatas.Where(x => x.PROJECT_NO == item.PROJECT_NO).ToList());

                SetColumn(cells[$"A{row}"], index+1);
                SetColumn(cells[$"B{row}"], item.EXEC_ORGAN_NAME);
                SetColumn(cells[$"C{row}"], item.PROJECT_NAME);
                SetColumn(cells[$"D{row}"], item.BUDGET.ToString("N0"));
                SetColumn(cells[$"E{row}"], engProgressData);
                SetColumn(cells[$"F{row}"], item.EXECUTE_CONDITION ?? string.Empty);
                SetColumn(cells[$"G{row}"], checkpointString);
                SetColumn(cells[$"H{row}"], item.DELAY_KIND);
                SetColumn(cells[$"I{row}"], item.DELAY_CAUSAL);
                SetColumn(cells[$"J{row}"], adjScheString);
                SetColumn(cells[$"K{row}"], conferenceString);
                index++;
            }
        }

        /// <summary>
        /// 產生完整檢核點字串
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        private string GenFullCheckpointDataString(List<ProjectCusCheckpointModel> datas)
        {
            if (!datas.Any())
            {
                return string.Empty;
            }

            StringBuilder result = new();
            foreach (ProjectCusCheckpointModel data in datas)
            {
                result.Append($"{data.CHECKITEM_NAME}({DateTimeUtil.ToTwDateString(data.ESTIMATED_ENDDATE)};" +
                    $"{DateTimeUtil.ToTwDateString(data.ACTUAL_ENDDATE)})\n");
            }
            return result.ToString();
        }

        /// <summary>
        /// 產生總期程調整字串
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        private string GenAdjScheDataString(List<AdjustScheHistoryModel> datas)
        {
            if (!datas.Any())
            {
                return string.Empty;
            }

            StringBuilder result = new();
            foreach (AdjustScheHistoryModel data in datas)
            {
                result.Append($"第{data.SEQ}次:原定{DateTimeUtil.ToTwDateString(data.ORI_ESTIMATED_ENDDATE)}完成，" +
                    $"核准調整至{DateTimeUtil.ToTwDateString(data.ADJ_LAST_DATE)}。" +
                    $"(核准日期：{DateTimeUtil.ToTwDateString(data.APPRV_DATE)})\n");
            }
            return result.ToString();
        }

        /// <summary>
        /// 產生會議列管字串
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        private string GenConferenceString(List<ProjectConferenceRPTModel> datas)
        {
            if (!datas.Any())
            {
                return string.Empty;
            }

            StringBuilder result = new();
            foreach (ProjectConferenceRPTModel data in datas)
            {
                result.Append($"會議[種類:{data.CONFERENCE_NAME}、會次:{data.CONFERENCE_NUM}、" +
                    $"時間:{DateTimeUtil.ToTwDateString(data.CONFERENCE_TIME)}]\n");
            }
            return result.ToString();
        }
    }
}
