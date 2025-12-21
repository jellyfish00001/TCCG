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

namespace SDO.APP.RPT.Word
{
    public class ProjectFillYearAss : WContentBuilder
    {
        private readonly IProjectDac projectDac;
        private readonly IProjectCommonDac projectCommonDac;
        private readonly IProjectExecuteDac projectExecuteDac;

        public ProjectFillYearAss(IComponentContext coms) : base(coms)
        {
            projectDac = coms.Resolve<IProjectDac>();
            projectCommonDac = coms.Resolve<IProjectCommonDac>();
            projectExecuteDac = coms.Resolve<IProjectExecuteDac>();
            TemplateFileName = "ProjectFillYearAssRPT.doc";
        }

        protected override async Task GetData()
        {
            ProjectFillYearAssModel projectFillYearAss = await projectCommonDac.GetProjectFillYearAss(Parameter.PROJECT_NO) ?? new();
            List<ProjectCusCheckpointModel> projectCheckItem = await projectDac.GetProjectCusCheckpoint(Parameter.PROJECT_NO);
            List<ProjectEngineeringProgressGridModel> projectEngineeringProgress = await projectExecuteDac.GetProjecFillExecuteList(Parameter.PROJECT_NO, "1");
            List<ProjectFactFindingModel> projectFactFinding = await projectExecuteDac.GetProjectFactFinding(Parameter.PROJECT_NO);
            List<DateTime> projectDelayCausal = (await projectExecuteDac.GetProjectDelayCausalList(Parameter.PROJECT_NO, "1")).Select(x => $"{x.DATA_YEAR}-{x.DATA_MONTH}".ToDateTimeWithNull().Value).Distinct().OrderBy(x => x).ToList();
            List<ProjectBudgetSourceGModel> projectBudgetSourceG = await projectDac.GetProjectBudgetSourceG(Parameter.PROJECT_NO);
            List<ProjectFillCycleModel> projectFillCycle = await projectCommonDac.GetCycleData();
            List<ProjectCloseDetailsModel> projectCloseDetails = await projectExecuteDac.GetProjectCloseDetails(Parameter.PROJECT_NO);
            List<ProjectMergeLogModel> projectMergeLog = await projectExecuteDac.GetProjectMergeLog(Parameter.PROJECT_NO);
            List<ProjectBasicAdjForDelayApply> delayApply = await projectExecuteDac.GetDelayApply(Parameter.PROJECT_NO);

            bool isClosed = projectFillYearAss.PROJECT_STATUS == "7";

            #region 執行狀況
            //管考進度
            double progC = 0;
            //工程進度
            double progP = 0;
            //管考進度-提示訊息
            string seDD = "";
            //預計結案日期
            DateTime? estimatedEndDate = projectCheckItem.Last().ESTIMATED_ENDDATE;
            //匯出日期
            DateTime printDate = DateTime.Now;

            if (isClosed)
            {
                //預計結案日期的填報週期資料
                ProjectFillCycleModel cycleData = projectFillCycle
                    .Where(x => x.PROJECT_YEAR == estimatedEndDate.ToTwDateString("yyy") &&
                        x.PROJECT_MONTH == estimatedEndDate.ToTwDateString("MM"))
                    .FirstOrDefault();
                //依據預計結案日期的填報週期(若沒有週期資料，以系統日)，找M3填報日期小於等於預計結案日期的檢核點進度
                DateTime fillEndDate = cycleData == null ? printDate : cycleData.FILL_END_DATE;
                progC = (double)projectCheckItem
                    .Where(x => x.ACTUAL_ENDDATE <= fillEndDate)
                    .OrderByDescending(x => x.PROGRESS).Select(x => x.PROGRESS).FirstOrDefault();
                //以預訂結案日期找M4小於等於預計結案日期的進度
                progP = Double.TryParse(projectEngineeringProgress
                    .Where(x => x.PROJECT_DATE <= estimatedEndDate)
                    .OrderByDescending(x => x.SEQ).Select(x => x.IPC_ACT_PRG).FirstOrDefault().ToString(), out progP)?progP:0 ;
            }
            else
            {
                //匯出日期還沒到預計結案日期
                if (estimatedEndDate == null || printDate < estimatedEndDate)
                {
                    progC = 100;
                    progP = 100;
                    seDD = "【進度以如期結案計算】";
                }
                else
                {
                    //找最後一筆有實際完成日期的進度
                    ProjectCusCheckpointModel checkItem = projectCheckItem.Where(x => x.ACTUAL_ENDDATE != null).OrderByDescending(x => x.PROGRESS).FirstOrDefault();
                    progC = checkItem == null ? 0 : (double)checkItem.PROGRESS;
                    //找最新的施工進度
                    ProjectEngineeringProgressGridModel progress = projectEngineeringProgress.OrderByDescending(x => x.SEQ).FirstOrDefault();
                    progP = progress == null || !progress.IPC_ACT_PRG.HasValue ? 0 : (double)progress.IPC_ACT_PRG;
                }
            }

            //查證成績 排除M5.FFSCORE沒有分數的平均
            double factFindingAA = projectFactFinding.Any()
                ? projectFactFinding.Average(x => x.FFSCORE) == null ? -1 : Math.Round((double)projectFactFinding.Average(x => x.FFSCORE), 1)
                : -1;
            //實地查證
            string factFinding = $"{GetCheckedYesNo(factFindingAA == -1)}本案無實地查證。\v" +
                $"{GetCheckedYesNo(factFindingAA != -1)}本案有實地查證。" +
                $"查證成績： { (factFindingAA != -1 ? factFindingAA : "____") } 分(E)。";
            //執行總期程 如PROJECT_STATUS是7，則實際結案日期-開始日期；不是則預定結案日期-開始日期
            int scheduleDD = isClosed
                ? (projectFillYearAss.FINISH_DATE.Value.Year - projectFillYearAss.CONTROL_DATE1.Value.Year) * 12 + projectFillYearAss.FINISH_DATE.Value.Month - projectFillYearAss.CONTROL_DATE1.Value.Month + 1
                : (projectFillYearAss.PROJECT_LAST_DATE.Value.Year - projectFillYearAss.CONTROL_DATE1.Value.Year) * 12 + projectFillYearAss.PROJECT_LAST_DATE.Value.Month - projectFillYearAss.CONTROL_DATE1.Value.Month + 1;

            //進度落後月數
            int scheduleEE = projectDelayCausal.Count;

            string schedule = $"計畫開始：{projectFillYearAss.CONTROL_DATE1.ToTwDateString("yyy年MM月")}；" +
                $"預定結案：{projectFillYearAss.PROJECT_LAST_DATE.ToTwDateString("yyy年MM月")}" +
                $"{(isClosed ? $"；實際結案時間：{ projectFillYearAss.FINISH_DATE.ToTwDateString("yyy年MM月")}。\v" : "。\v")}" +
                $"計畫{(isClosed ? "" : "預計")}執行期程總計 {scheduleDD} 個月(F)；" +
                $"進度落後月數：{scheduleEE}個月(G)";

            //計畫總經費
            long budgetAA = projectBudgetSourceG.Sum(x => x.BUDGET_CENTRAL + x.BUDGET_LOCAL);
            //最大年度
            int maxPlanYear = int.TryParse(projectBudgetSourceG.Max(x => x.PLAN_YEAR), out int max) ? max : 0;
            //最小年度
            int minPlanYear = int.TryParse(projectBudgetSourceG.Min(x => x.PLAN_YEAR), out int min) ? min : 0;
            //累計支用數 若PROJECT_STATUS不為7 直接給計畫總經費的值
            double budgetI = !isClosed ? budgetAA : (double)projectFillYearAss.ACTUAL_PAY;
            //應付未付數
            double budgetJ = (double)projectFillYearAss.UNPAY;
            //節餘數
            double budgetK = (double)projectFillYearAss.BALANCE;
            //預算達成率
            double budgetRateAA = Math.Round(budgetAA == 0 ? 100 : (double)(budgetI + budgetJ + budgetK) / budgetAA * 100, 1);
            //實際支用比
            double budgetRateBB = Math.Round(budgetAA == 0 ? 100 : (double)(budgetI / budgetAA) * 100, 1);
            #endregion

            #region 量化指標評分
            //基本資料-得分
            double sa = Math.Round(projectFillYearAss.SCORE_A * 0.2, 1);
            //報表填報-逾期天數
            int sbOverDueDay = projectEngineeringProgress.Sum(x => x.OVERDUE_DAY);
            //報表填報-得分
            int sb = (10 - sbOverDueDay < 0) ? 0 : (10 - sbOverDueDay);
            //報表品質-改善次數
            int closeDetailsCnt = projectCloseDetails.Where(x => x.CLOSE_DETAILS_TYPE == "1").Count();
            //報表品質-得分
            int sc = (5 - 3 * closeDetailsCnt < 0) ? 0 : (5 - 3 * closeDetailsCnt);
            //計畫調整-總計扣分
            int deductScoreD = (projectFillYearAss.SCHE_M_CNT != 0 ? (projectFillYearAss.SCHE_M_CNT - 1) * 1 : 0) + 
                (projectFillYearAss.SCHE_Y_CNT != 0 ? (projectFillYearAss.SCHE_Y_CNT - 1) * 3 : 0);
            //計畫調整-得分
            int sd = (10 - deductScoreD < 0) ? 0 : (10 - deductScoreD);

            #region 管考進度
            //管考進度-非工程案件分數
            double seAA = 0;
            //管考進度-工程案件分數
            double seBB = 0;
            //管考進度-有實地查證案件分數
            double seCC = 0;
            //管考進度-得分
            double se = 0;
            if (projectFillYearAss.CP_KIND == "1")
            {
                seAA = Math.Round(progC / 100, 2);
                se = Math.Round(seAA * 15, 1);
            }
            else if (projectFillYearAss.CP_KIND == "0" && !projectFactFinding.Where(x => x.FFSCORE != null).Any())
            {
                seBB = Math.Round((progC / 100) + (progP / 100), 2);
                se = Math.Round(seBB / 2 * 15, 1);
            }
            else if (projectFillYearAss.CP_KIND == "0" && projectFactFinding.Where(x => x.FFSCORE != null).Any())
            {
                seCC = Math.Round((progC / 100) + (progP / 100) + (factFindingAA / 100), 2);
                se = Math.Round(seCC / 3 * 15, 1);
            }
            //管考進度-文字
            string scoringMethodE =
                $"{GetCheckedYesNo(projectFillYearAss.CP_KIND == "1")}非工程類案件，" +
                $"(B/A= {(projectFillYearAss.CP_KIND == "1" ? seAA.ToString() : " ")}) × 15，即為本項得分。\v" +
                GetCheckedYesNo(projectFillYearAss.CP_KIND == "0" && !projectFactFinding.Where(x => x.FFSCORE != null).Any()) +
                "工程類案件，(B/A+D/C=" +
                (projectFillYearAss.CP_KIND == "0" && !projectFactFinding.Where(x => x.FFSCORE != null).Any()
                ? seBB.ToString()
                : " ") + ") /2 × 15，即為本項得分。\v" +
                GetCheckedYesNo(projectFillYearAss.CP_KIND == "0" && projectFactFinding.Where(x => x.FFSCORE != null).Any()) +
                "有實地查證案件，(B/A+D/C+E/100=" +
                (projectFillYearAss.CP_KIND == "0" && projectFactFinding.Where(x => x.FFSCORE != null).Any()
                ? seCC.ToString()
                : " ") + ") /3 × 15，即為本項得分。";
            #endregion

            //落後情形-最多連續落後月數
            int sfAA = MaxDelayMonth(projectDelayCausal);
            //落後情形-落後月數比例
            double sfBB = Math.Round(scheduleDD <= 0 ? 0 : ((double)scheduleEE) / ((double)scheduleDD), 2);
            //落後情形-得分
            double sf = Math.Round((1 - sfBB) * 20 - ((double)sfAA / 2), 1);
            sf = (sf < 0) ? 0 : sf;
            //經費執行-L
            double sgL = (budgetRateAA / 100 > 1) ? 1 : (budgetRateAA / 100);
            //經費執行-M
            double sgM = (budgetRateBB / 100 > 1) ? 1 : (budgetRateBB / 100);
            //經費執行-得分
            double sg = Math.Round((sgL + sgM) / 2 * 10, 1);
            //合計得分
            double sh = sa + sb + sc + sd + se + sf + sg;
            #endregion

            #region 特殊扣分
            int specDeductB1 = projectCloseDetails.Where(x => x.CLOSE_DETAILS_TYPE == "2").Any() ? 5 : 0;
            int specDeductA2 = delayApply.Count;
            int specDeductB2 = specDeductA2 * 5;
            int specDeductA3 = projectCloseDetails.Where(x => x.CLOSE_DETAILS_TYPE == "4").Count();
            int specDeductB3 = specDeductA3 * 3;
            int specDeductB4 = projectMergeLog.Where(x => x.MERGE_STATUS == "01").Any() ? 3 : 0;
            string specDeduct = GetCheckedYesNo(projectCloseDetails.Where(x => x.CLOSE_DETAILS_TYPE == "2").Any()) +
                $"符合列管標準，卻未依要點第4點規定，主動提報智發會列管，考核分數再扣減{specDeductB1}分\v" +
                GetCheckedYesNo(delayApply.Any()) +
                $"申請計畫調整，未依要點第9點規定於期限內提出，按次扣減考核分數5分。共{specDeductA2}次，扣減{specDeductB2}分。\v" +
                GetCheckedYesNo(projectCloseDetails.Where(x => x.CLOSE_DETAILS_TYPE == "4").Any()) +
                $"經查證填報不實者，按次扣減該計畫年終考核分數3分。共{specDeductA3}次，扣減{specDeductB3}分。\v" +
                GetCheckedYesNo(projectMergeLog.Where(x => x.MERGE_STATUS == "01").Any()) +
                $"計畫於管考期間，申請分案列管，考核分數再扣減{specDeductB4}分。";
            //合計扣分
            int si = specDeductB1 + specDeductB2 + specDeductB3 + specDeductB4;
            #endregion

            BasicData = new
            {
                TITLE = isClosed ? "" : "試算",
                SUB_TITLE = isClosed ? "" : "（本表僅提供參考，分數以實際結案時為準。）    ",
                DATE = isClosed
                    ? "填表日期：________________"
                    : $"資料日期：{ printDate.ToTwDateString("yyy年MM月dd日")}",
                PROJECT_NO = projectFillYearAss.PROJECT_NO,
                PROJECT_NAME = projectFillYearAss.PROJECT_NAME,
                PROJECT_YEAR = isClosed
                    ? projectFillYearAss.FINISH_DATE.ToTwDateString("yyy")
                    : projectFillYearAss.PROJECT_LAST_DATE.ToTwDateString("yyy"),
                PROJECT_RUNWAY_C = projectFillYearAss.CHECKPOINT_CLASS == null ? "" : projectFillYearAss.CHECKPOINT_CLASS,
                PROG_C = progC,
                PROG_P = projectFillYearAss.CP_KIND == "0" ? progP.ToString() : "",//CP_KIND 0：工程類 1：非工程類 免填
                FACT_FINDING = factFinding,
                SCHEDULE = schedule,
                BUDGET = $"計畫總經費：{budgetAA / 1000:N0}千元，" +
                    $"分{maxPlanYear - minPlanYear}年" +
                    $"（{projectBudgetSourceG.Min(x => x.PLAN_YEAR)}年至{projectBudgetSourceG.Max(x => x.PLAN_YEAR)}年）編列；",
                BUDGET_H = $"{budgetAA:N0}",
                BUDGET_I = budgetI > 0 ? $"{budgetI:N0}" : "-",
                BUDGET_J = budgetJ > 0 ? $"{budgetJ:N0}" : "-",
                BUDGET_K = budgetK > 0 ? $"{budgetK:N0}" : "-",
                BUDGET_RATE = $"預算達成率：(I+J+K)/(H)×100 = {budgetRateAA}%(L)。實際支用比：(I/H)×100 = {budgetRateBB}%(M)。",
                //量化指標評量
                SCORING_METHOD_A = $"基本資料審查成績：{projectFillYearAss.SCORE_A}分×20%，即為本項得分。",
                SA = sa,
                SCORING_METHOD_B = $"各項報表資料之填送時間，累計逾期 {sbOverDueDay}日。(占分10分，每延誤1日扣1分，最低以0分計。)",
                SB = sb,
                SCORING_METHOD_C = $"各項報表資料內容欠周詳，內容過於簡略，經智發會通知改善{closeDetailsCnt}次。\v" +
                    "(占分5分，每計1次扣3分，最低以0分計。)",
                SC = sc,
                SCORING_METHOD_D = $"申請調整計畫期程或預定進度、內容{projectFillYearAss.SCHE_M_CNT + projectFillYearAss.SCHE_Y_CNT}次，" +
                    $"總計扣分{deductScoreD}分。\v" +
                    "(占分10分，申請分月期程調整扣1分、申請總期程調整扣3分，惟第1次申請分月及總期程調整皆不扣分，最低以0分計。)",
                SD = sd,
                SCORING_METHOD_E_DD = seDD,
                SCORING_METHOD_E = scoringMethodE,
                SE = se,
                SCORING_METHOD_F_CC = isClosed ? "" : "（以目前落後月數計算。）\v",
                SCORING_METHOD_F = $"連續落後月數最長 {sfAA} 個月(O) ；落後月數比例： G/F= {sfBB} (P)，" +
                    $"(1-P)×20-O/2，即為本項得分。(占分20分，最低以0分計。)",
                SF = sf,
                SCORING_METHOD_G = isClosed ? "" : "；經費以全數支用計算",
                SG = sg,
                SH = Math.Round(sh, 1),
                //特殊扣分
                SPEC_DEDUCT = specDeduct,
                SI = si,
                SJ = Math.Round(sh - si, 1),
            };
        }

        /// <summary>
        /// 找最大連續落後月數
        /// </summary>
        /// <param name="projectDelayCausal"></param>
        /// <returns></returns>
        private static int MaxDelayMonth(List<DateTime> projectDelayCausal)
        {
            if (!projectDelayCausal.Any())
            {
                return 0;
            }

            int count = 1;
            List<int> result = new List<int>();
            for (int i = 0; i < projectDelayCausal.Count - 1; i++)
            {
                //當筆資料年月
                DateTime current = projectDelayCausal[i];
                //下一筆資料年月
                DateTime next = projectDelayCausal[i + 1];
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
    }
}
