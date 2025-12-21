using Aspose.Cells;
using Autofac;
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
    /// <summary>
    /// 表11：年終考核案件成績表
    /// </summary>
    public class RPTIPCProjectFillYearAss : XlsBuilder
    {
        private readonly IStatisticsDac statisticsDac;
        private readonly IProjectCommonDac projectCommonDac;
        // 原始SQL資料
        List<IPCProjectFillYearAssModel> statisticsData;
        // 年終考核成績表資料
        List<IPCProjectFillYearAssModel> yearAssData = new();
        // 週期資料
        List<ProjectFillCycleModel> projectFillCycle;
        // 檢核點資料
        List<ProjectCusCheckpointModel> projectCheckItem;
        // 每月辦理情形進度資料
        List<ProjectEngineeringProgressGridModel> projectEngineeringProgress;
        // 落後資料
        List<ProjectDelayCausalModel> projectDelayCausal;
        StatisticsModel model;
        //匯出日期
        DateTime printDate = DateTime.Now;

        public RPTIPCProjectFillYearAss(IComponentContext coms) : base(coms)
        {
            TemplateFileName = "IPCProjectFillYearAssRPT.xlsx";
            statisticsDac = coms.Resolve<IStatisticsDac>();
            projectCommonDac = coms.Resolve<IProjectCommonDac>();
        }

        protected override async Task GetData()
        {
            model = (StatisticsModel)Parameter.ObjectModel;
            statisticsData = await statisticsDac.GetIPCProjectFillYearAss(model);
            projectFillCycle = await projectCommonDac.GetCycleData();
            projectCheckItem = await statisticsDac.GetProjectCusCheckpointList();
            projectEngineeringProgress = await statisticsDac.GetProjecFillExecuteList();
            projectDelayCausal = await statisticsDac.GetProjectDelayCausalList();

            // 組年終考核成績表資料
            foreach (IPCProjectFillYearAssModel item in statisticsData)
            {
                SetProgData(item);
                item.DELAY_MONTH = projectDelayCausal.Where(x => x.PROJECT_NO == item.PROJECT_NO).Count();
                item.MAX_DELAY_MONTH = MaxDelayMonth(item.PROJECT_NO);
                yearAssData.Add(item);
            }
        }

        /// <summary>
        /// 找管考實際進度、工程實際進度
        /// </summary>
        /// <param name="item"></param>
        private void SetProgData(IPCProjectFillYearAssModel item)
        {
            //該計畫的檢核點
            List<ProjectCusCheckpointModel> checkpoint = projectCheckItem.Where(x => x.PROJECT_NO == item.PROJECT_NO).OrderBy(x=>x.PROGRESS).ToList();
            //該計畫的預計結案日期
            DateTime? estimatedEndDateTemp = checkpoint.Select(x => x.ESTIMATED_ENDDATE).LastOrDefault();
            DateTime? estimatedEndDate = estimatedEndDateTemp != null ? estimatedEndDateTemp.Value.Date : null;
            //該計畫的每月辦理情形
            IEnumerable<ProjectEngineeringProgressGridModel> engineeringProgress = projectEngineeringProgress.Where(x => x.PROJECT_NO == item.PROJECT_NO);
            if (item.PROJECT_STATUS == "7")
            {
                //預計結案日期的填報週期資料
                ProjectFillCycleModel cycleData = projectFillCycle
                    .Where(x => x.PROJECT_YEAR == estimatedEndDate.ToTwDateString("yyy") 
                        && x.PROJECT_MONTH == estimatedEndDate.ToTwDateString("MM"))
                    .FirstOrDefault();
                //依據預計結案日期的填報週期(若沒有週期資料，以系統日)，找 填報日期小於等於預計結案日期的檢核點進度
                DateTime fillEndDate = cycleData == null ? printDate : cycleData.FILL_END_DATE;
                item.PROG_C = (double)checkpoint.Where(x => x.ACTUAL_ENDDATE <= fillEndDate).Select(x => x.PROGRESS).LastOrDefault();
                if (estimatedEndDate.HasValue)
                {
                    //以預訂結案日期找 小於等於預計結案日期的進度
                    item.PROG_P = (double)engineeringProgress
                        .Where(x => $"{x.YEAR}-{x.MONTH}".ToDateTimeWithNull().Value <= estimatedEndDate)
                        .Select(x => x.IPC_ACT_PRG ?? 0).FirstOrDefault();
                }
                
            }
            else
            {
                //匯出日期還沒到預計結案日期
                if (estimatedEndDate == null || printDate < estimatedEndDate)
                {
                    item.PROG_C = 100;
                    item.PROG_P = 100;
                }
                else
                {
                    //找最後一筆有實際完成日期的進度
                    double progress = (double)checkpoint.Where(x => x.ACTUAL_ENDDATE != null).Select(x => x.PROGRESS).LastOrDefault();
                    item.PROG_C = progress;
                    //找最新的施工進度
                    double ipcActPrg = (double)engineeringProgress.Select(x => x.IPC_ACT_PRG).FirstOrDefault();
                    item.PROG_P = ipcActPrg;
                }
            }
        }

        /// <summary>
        /// 找最大連續落後月數
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        private int MaxDelayMonth(string PROJECT_NO)
        {
            //找該計畫的落後資料
            List<DateTime> delayData = projectDelayCausal.Where(x => x.PROJECT_NO == PROJECT_NO).Select(x => $"{x.DATA_YEAR}-{x.DATA_MONTH}".ToDateTimeWithNull().Value).ToList();
            if (!delayData.Any())
            {
                return 0;
            }
            int count = 1;
            List<int> result = new List<int>();
            for (int i = 0; i < delayData.Count - 1; i++)
            {
                //當筆資料年月
                DateTime current = delayData[i];
                //下一筆資料年月
                DateTime next = delayData[i + 1];
                DateTime addMonth = current.AddMonths(1);
                //連續月份次數加一
                if (next == addMonth)
                {
                    count++;
                }
                //連續中斷紀錄次數
                else
                {
                    result.Add(count);
                    count = 1;
                }
            }
            result.Add(count);
            return result.Max();
        }

        protected override void MakeContent()
        {
            CellReplaceByExcel(new
            {
                DATE = printDate.ToTwDateString(),
            });

            // 成績明細表
            SetScoreDetails();
            // 權重明細表
            SetWeightDetails();
        }

        /// <summary>
        /// 成績明細表
        /// </summary>
        private void SetScoreDetails()
        {
            Sheet = Xls.Worksheets[0];
            Cells cells = Sheet.Cells;

            int strRow = 5;
            int index = 0;
            foreach (IPCProjectFillYearAssModel item in yearAssData)
            {
                int row = strRow + index;
                if (index > 0)
                {
                    cells.InsertRow(row - 1);
                }

                SetColumn(cells[$"A{row}"], index + 1);
                SetColumn(cells[$"B{row}"], item.PROJECT_NO);
                SetColumn(cells[$"C{row}"], item.PROJECT_NAME);
                SetColumn(cells[$"D{row}"], item.PROJECT_EXS);
                SetColumn(cells[$"E{row}"], item.EXEC_DEPT);
                SetColumn(cells[$"F{row}"], item.CP_KIND_NAME);
                SetColumn(cells[$"G{row}"], item.ACTUAL_ENDDATE.ToTwDateString());
                SetColumn(cells[$"H{row}"], item.FINISH_DATE.ToTwDateString());
                SetColumn(cells[$"I{row}"], item.DIFF_MONTH);
                SetColumn(cells[$"J{row}"], item.SA);
                SetColumn(cells[$"K{row}"], item.SB);
                SetColumn(cells[$"L{row}"], item.SC);
                SetColumn(cells[$"M{row}"], item.SD);
                SetColumn(cells[$"N{row}"], item.SE);
                SetColumn(cells[$"O{row}"], item.SF);
                SetColumn(cells[$"P{row}"], item.SG);
                SetColumn(cells[$"Q{row}"], item.SH);
                SetColumn(cells[$"R{row}"], item.DEDUCT_A);
                SetColumn(cells[$"S{row}"], item.DEDUCT_B);
                SetColumn(cells[$"T{row}"], item.DEDUCT_C);
                SetColumn(cells[$"U{row}"], item.DEDUCT_D);
                SetColumn(cells[$"V{row}"], item.SI);
                SetColumn(cells[$"W{row}"], item.SJ);

                index++;
            }

            SetRowHeight();
        }

        /// <summary>
        /// 權重明細表
        /// </summary>
        private void SetWeightDetails()
        {
            Sheet = Xls.Worksheets[1];
            Cells cells = Sheet.Cells;

            int strRow = 8;
            int index = 0;
            foreach (IPCProjectFillYearAssModel item in yearAssData)
            {
                int row = strRow + index;
                if (index > 0)
                {
                    cells.InsertRow(row - 1);
                }

                SetColumn(cells[$"A{row}"], index + 1);
                SetColumn(cells[$"B{row}"], item.PROJECT_NO);
                SetColumn(cells[$"C{row}"], item.PROJECT_NAME);
                SetColumn(cells[$"D{row}"], item.PROJECT_EXS);
                SetColumn(cells[$"E{row}"], item.EXEC_DEPT);
                SetColumn(cells[$"F{row}"], item.CP_KIND_NAME);
                SetColumn(cells[$"G{row}"], item.ACTUAL_ENDDATE.ToTwDateString());
                SetColumn(cells[$"H{row}"], item.FINISH_DATE.ToTwDateString());
                SetColumn(cells[$"I{row}"], item.DIFF_MONTH);
                // 基本資料
                SetColumn(cells[$"J{row}"], item.SCORE_A);
                SetColumn(cells[$"K{row}"], item.SA);
                // 報表填報
                SetColumn(cells[$"L{row}"], item.OVERDUE_DAY);
                SetColumn(cells[$"M{row}"], item.SB);
                // 報表品質
                SetColumn(cells[$"N{row}"], item.TYPE_1_CNT);
                SetColumn(cells[$"O{row}"], item.SC);
                // 計畫調整
                SetColumn(cells[$"P{row}"], item.SCHE_Y_TOTCNT);
                SetColumn(cells[$"Q{row}"], item.SCHE_Y_DELAY_CNT);
                SetColumn(cells[$"R{row}"], item.SCHE_Y_DELAY_CNT * 3);
                SetColumn(cells[$"S{row}"], item.SCHE_M_TOTCNT);
                SetColumn(cells[$"T{row}"], item.SCHE_M_DELAY_CNT);
                SetColumn(cells[$"U{row}"], item.SCHE_M_DELAY_CNT * 3);
                SetColumn(cells[$"V{row}"], item.SD);
                // 管考進度
                SetColumn(cells[$"W{row}"], 1);
                SetColumn(cells[$"X{row}"], item.PROG_C / 100);
                SetColumn(cells[$"Y{row}"], 1);
                SetColumn(cells[$"Z{row}"], item.PROG_P / 100);
                SetColumn(cells[$"AA{row}"], item.AVG_FFSCORE);
                SetColumn(cells[$"AB{row}"], item.SE);
                // 落後情形
                SetColumn(cells[$"AC{row}"], item.EX_MONTH);
                SetColumn(cells[$"AD{row}"], item.DELAY_MONTH);
                SetColumn(cells[$"AE{row}"], item.MAX_DELAY_MONTH);
                SetColumn(cells[$"AF{row}"], item.SF);
                // 經費執行
                SetColumn(cells[$"AG{row}"], item.BUDGET_ACHIEVE_RATE / 100);
                SetColumn(cells[$"AH{row}"], item.BUDGET_ACTUAL_RATE / 100);
                SetColumn(cells[$"AI{row}"], item.SG);
                // 合計得分
                SetColumn(cells[$"AJ{row}"], item.SH);
                // 未主動提報
                SetColumn(cells[$"AK{row}"], item.DEDUCT_A);
                // 逾期申請調整
                SetColumn(cells[$"AL{row}"], item.DELAY_APPLY_CNT);
                SetColumn(cells[$"AM{row}"], item.DEDUCT_B);
                // 填報不實
                SetColumn(cells[$"AN{row}"], item.TYPE_4_CNT);
                SetColumn(cells[$"AO{row}"], item.DEDUCT_C);
                // 分案列管
                SetColumn(cells[$"AP{row}"], item.DEDUCT_D);
                // 合計扣分
                SetColumn(cells[$"AQ{row}"], item.SI);
                // 初評分數
                SetColumn(cells[$"AR{row}"], item.SJ);

                index++;
            }

            SetRowHeight();
        }
    }
}
