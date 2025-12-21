using Autofac;
using SDO.Dac;
using SDO.Models;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Services;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.APP.RPT.Word
{
    /// <summary>
    /// 表1 案件統計表
    /// </summary>
    public class ProjectStatistics : WContentBuilder
    {
        private readonly IStatisticsDac statisticsDac;
        private StatisticsModel model;
        // 原始SQL資料
        protected List<ProjectStatisticsModel> statisticsData;
        // 報表結構資料
        protected List<ProjectStatisticsModel> execOrganData = new();

        public ProjectStatistics(IComponentContext coms) : base(coms)
        {
            statisticsDac = coms.Resolve<IStatisticsDac>();
            TemplateFileName = "IPCProjectStatisticsRPT.doc";
        }

        protected override async Task GetData()
        {
            model = (StatisticsModel)Parameter.ObjectModel;
            statisticsData = await statisticsDac.GetIPCProjectStatistics(model);

            SetExecDeptData();
            SetRankData();

            if (Parameter.Type == "Statistics")
            {
                SetStatisticsData();
            }
        }

        /// <summary>
        /// 組執行機關資料
        /// </summary>
        private void SetExecDeptData()
        {
            Dictionary<string, string> execOrganDic = statisticsData.Select(x => new { x.EXEC_ORGAN_C, x.EXEC_DEPT }).Distinct()
                .ToDictionary(x => x.EXEC_ORGAN_C, y => y.EXEC_DEPT);

            foreach (KeyValuePair<string, string> item in execOrganDic)
            {
                ProjectStatisticsModel model = new();
                // 取得 執行機關 = 此機關 的資料
                IEnumerable<ProjectStatisticsModel> details = statisticsData.Where(x => x.EXEC_ORGAN_C == item.Key);
                model.EXEC_DEPT = item.Value; //執行機關
                model.EXEC_DEPT_B = details.Where(x => x.STATUS == "B").Count(); //已結案件數(B)
                model.EXEC_DEPT_C = details.Where(x => x.STATUS == "C").Count(); //進度符合或超前(C)
                model.EXEC_DEPT_D1 = details.Where(x => x.STATUS == "D1").Count(); //開工前預定檢核點進度落後(D1)
                model.EXEC_DEPT_D2 = details.Where(x => x.STATUS == "D2").Count(); //施工進度落後(D2)
                model.EXEC_DEPT_D3 = details.Where(x => x.STATUS == "D3").Count(); //竣工後預定檢核點進度落後(D3)
                model.EXEC_DEPT_E = details.Where(x => x.STATUS == "E").Count(); //撤銷列管件數(E)
                model.EXEC_DEPT_F1_RATE = Math.Round((double)model.EXEC_DEPT_D / (double)model.EXEC_DEPT_A * 100, 1); //比率(D/A)
                model.EXEC_DEPT_BA = Math.Round((double)model.EXEC_DEPT_B / (double)model.EXEC_DEPT_A * 100, 1); //完成率(B/A)
                model.OU_SORT_ORDER = details.Select(x => x.OU_SORT_ORDER).FirstOrDefault(); //機關排序
                execOrganData.Add(model);
            }
        }

        /// <summary>
        /// 組所有機關落後排名
        /// </summary>
        private void SetRankData()
        {
            // 是否依機關群組
            if (model.SORT_GROUPBY_DEPT)
            {
                // 依據 件數(D) 大到小排名 落後件數排序(F1)
                List<(int EXEC_DEPT_D, int DeptType, int Rank)> dRankings = new();
                // 機關排名
                var dRankingsType1 = execOrganData.Where(x => x.DeptType == 1).OrderByDescending(x => x.EXEC_DEPT_D).Select(x => x.EXEC_DEPT_D).Distinct().Select((x, i) => (x, 1, i + 1));
                dRankings.AddRange(dRankingsType1);
                // 區公所排名
                var dRankingsType2 = execOrganData.Where(x => x.DeptType == 2).OrderByDescending(x => x.EXEC_DEPT_D).Select(x => x.EXEC_DEPT_D).Distinct().Select((x, i) => (x, 2, i + 1));
                dRankings.AddRange(dRankingsType2);

                foreach (var ranking in dRankings)
                {
                    List<ProjectStatisticsModel> data = execOrganData.Where(x => x.EXEC_DEPT_D == ranking.EXEC_DEPT_D && x.DeptType == ranking.DeptType).ToList();
                    data.ForEach(x =>
                    {
                        x.EXEC_DEPT_F1 = ranking.Rank;
                    });
                }

                // 依據 比率(D/A) 大到小排名 落後比率排序(F2)
                List<(double EXEC_DEPT_F1_RATE, int DeptType, int Rank)> daRankings = new();
                // 機關排名
                var daRankingsType1 = execOrganData.Where(x => x.DeptType == 1).OrderByDescending(x => x.EXEC_DEPT_F1_RATE).Select(x => x.EXEC_DEPT_F1_RATE).Distinct().Select((x, i) => (x, 1, i + 1));
                daRankings.AddRange(daRankingsType1);
                // 區公所排名
                var daRankingsType2 = execOrganData.Where(x => x.DeptType == 2).OrderByDescending(x => x.EXEC_DEPT_F1_RATE).Select(x => x.EXEC_DEPT_F1_RATE).Distinct().Select((x, i) => (x, 2, i + 1));
                daRankings.AddRange(daRankingsType2);

                foreach (var ranking in daRankings)
                {
                    List<ProjectStatisticsModel> data = execOrganData.Where(x => x.EXEC_DEPT_F1_RATE == ranking.EXEC_DEPT_F1_RATE && x.DeptType == ranking.DeptType).ToList();
                    data.ForEach(x =>
                    {
                        x.EXEC_DEPT_F2 = ranking.Rank;
                    });
                }

                // 依據 落後排序合計(F1+F2) 小到大排名 落後綜合排名
                List<(int EXEC_DEPT_F1F2, int DeptType, int Rank)> rankings = new();
                // 機關排名
                var rankingsType1 = execOrganData.Where(x => x.DeptType == 1).OrderBy(x => x.EXEC_DEPT_F1F2).Select(x => x.EXEC_DEPT_F1F2).Distinct().Select((x, i) => (x, 1, i + 1));
                rankings.AddRange(rankingsType1);
                // 區公所排名
                var rankingsType2 = execOrganData.Where(x => x.DeptType == 2).OrderBy(x => x.EXEC_DEPT_F1F2).Select(x => x.EXEC_DEPT_F1F2).Distinct().Select((x, i) => (x, 2, i + 1));
                rankings.AddRange(rankingsType2);

                foreach (var ranking in rankings)
                {
                    List<ProjectStatisticsModel> data = execOrganData.Where(x => x.EXEC_DEPT_F1F2 == ranking.EXEC_DEPT_F1F2 && x.DeptType == ranking.DeptType).ToList();
                    data.ForEach(x =>
                    {
                        x.EXEC_DEPT_RANKING = ranking.Rank;
                    });
                }
            }
            else
            {
                // 依據 件數(D) 大到小排名 落後件數排序(F1)
                var dRankings = execOrganData.OrderByDescending(x => x.EXEC_DEPT_D).Select(x => x.EXEC_DEPT_D).Distinct().Select((x, i) => new { EXEC_DEPT_D = x, Rank = i + 1 });

                foreach (var ranking in dRankings)
                {
                    List<ProjectStatisticsModel> data = execOrganData.Where(x => x.EXEC_DEPT_D == ranking.EXEC_DEPT_D).ToList();
                    data.ForEach(x =>
                    {
                        x.EXEC_DEPT_F1 = ranking.Rank;
                    });
                }

                // 依據 比率(D/A) 大到小排名 落後比率排序(F2)
                var daRankings = execOrganData.OrderByDescending(x => x.EXEC_DEPT_F1_RATE).Select(x => x.EXEC_DEPT_F1_RATE).Distinct().Select((x, i) => new { EXEC_DEPT_F1_RATE = x, Rank = i + 1 });

                foreach (var ranking in daRankings)
                {
                    List<ProjectStatisticsModel> data = execOrganData.Where(x => x.EXEC_DEPT_F1_RATE == ranking.EXEC_DEPT_F1_RATE).ToList();
                    data.ForEach(x =>
                    {
                        x.EXEC_DEPT_F2 = ranking.Rank;
                    });
                }

                // 依據 落後排序合計(F1+F2) 小到大排名 落後綜合排名
                var rankings = execOrganData.OrderBy(x => x.EXEC_DEPT_F1F2).Select(x => x.EXEC_DEPT_F1F2).Distinct().Select((x, i) => new { EXEC_DEPT_F1F2 = x, Rank = i + 1 });

                foreach (var ranking in rankings)
                {
                    List<ProjectStatisticsModel> data = execOrganData.Where(x => x.EXEC_DEPT_F1F2 == ranking.EXEC_DEPT_F1F2).ToList();
                    data.ForEach(x =>
                    {
                        x.EXEC_DEPT_RANKING = ranking.Rank;
                    });
                }
            }
        }

        /// <summary>
        /// 案件統計表資料
        /// </summary>
        private void SetStatisticsData()
        {
            int sumA = execOrganData.Sum(x => x.EXEC_DEPT_A);
            int sumB = execOrganData.Sum(x => x.EXEC_DEPT_B);
            int sumC = execOrganData.Sum(x => x.EXEC_DEPT_C);
            int sumD = execOrganData.Sum(x => x.EXEC_DEPT_D);
            int sumD1 = execOrganData.Sum(x => x.EXEC_DEPT_D1);
            int sumD2 = execOrganData.Sum(x => x.EXEC_DEPT_D2);
            int sumD3 = execOrganData.Sum(x => x.EXEC_DEPT_D3);
            int sumE = execOrganData.Sum(x => x.EXEC_DEPT_E);
            double rateB = Math.Round((double)sumB / (double)sumA * 100, 1);
            double rateC = Math.Round((double)sumC / (double)sumA * 100, 1);
            double rateD = Math.Round((double)sumD / (double)sumA * 100, 1);
            double rateE = Math.Round((double)sumE / (double)sumA * 100, 1);

            BasicData = new
            {
                TITLE = Parameter.FileName,
                SUBTITLE = Parameter.FileName.Replace("桃園市政府", "").Replace("統計表", ""),
                YEAR_MONTH = model.YEAR_MONTH_END.ToTwDateString("yyy年M月"),
                DATE = DateTime.Now.ToTwDateString(),
                YEAR = model.PROJECT_YEAR,
                TOTAL_NUM = statisticsData.Count,
                NEW_NUM = statisticsData.Where(x => x.PROJECT_YEAR == Int16.Parse(model.PROJECT_YEAR)).Count(),
                BEFORE_YEAE = Int16.Parse(model.PROJECT_YEAR) - 1,
                BEFORE_NUM = $"{(model.PROJECT_YEAR_STATUS == "B" ? "持續列管" : "")}{statisticsData.Where(x => x.PROJECT_YEAR <= Int16.Parse(model.PROJECT_YEAR) - 1).Count()}",
                CLOSE_NUM = sumB,
                CLOSE_RATE = rateB,
                CONFORM_NUM = sumC,
                CONFORM_RATE = rateC,
                DELAY_NUM = sumD,
                DELAY_RATE = rateD,
                DELAY_D1_NUM = sumD1,
                DELAY_D2_NUM = sumD2,
                DELAY_D3_NUM = sumD3,
                REVOKE_NUM = sumE,
                REVOKE_RATE = rateE,

                SUM_A = sumA,
                SUM_B = sumB,
                SUM_C = sumC,
                SUM_D = sumD,
                SUM_D1 = sumD1,
                SUM_D2 = sumD2,
                SUM_D3 = sumD3,
                SUM_E = sumE,
                SUM_BA = Math.Round((double)sumB / (double)sumA * 100, 1),
                RATE_A = Math.Round((double)sumA / (double)sumA * 100, 1),
                RATE_B = rateB,
                RATE_C = rateC,
                RATE_D = rateD,
                RATE_E = rateE
            };

            //排序
            SetOrder();

            ListData = new List<ITableData>();
            var item = execOrganData.Select(x => new
            {
                x.EXEC_DEPT,
                x.EXEC_DEPT_A,
                x.EXEC_DEPT_B,
                x.EXEC_DEPT_C,
                x.EXEC_DEPT_D,
                x.EXEC_DEPT_F1,
                x.EXEC_DEPT_F1_RATE,
                x.EXEC_DEPT_F2,
                x.EXEC_DEPT_D1,
                x.EXEC_DEPT_D2,
                x.EXEC_DEPT_D3,
                x.EXEC_DEPT_E,
                x.EXEC_DEPT_BA,
                x.EXEC_DEPT_F1F2,
                x.EXEC_DEPT_RANKING,
                // 依據不同排序條件決定哪個排序條件前3須加上網底
                BgColor = (model.SORT_TYPE_1 == "A" ? x.EXEC_DEPT_F1 :
                    model.SORT_TYPE_1 == "B" ? x.EXEC_DEPT_RANKING :
                    model.SORT_TYPE_1 == "C" ? x.EXEC_DEPT_F2 :
                    x.EXEC_DEPT_RANKING) <= 3 ? Color.FromArgb(255, 204, 153) : Color.White
            });
            if (item.Any())
            {
                ListData.Add(new WordTableData { LIST_DATA = item });
            }
        }

        /// <summary>
        /// 依條件排序資料
        /// </summary>
        private void SetOrder()
        {
            // 機關群組(依據OU_SORT_ORDER小到大) + 下拉選擇 + 執行機關(OU_SORT_ORDER小到大)
            switch (model.SORT_TYPE_1)
            {
                case "A": //落後件數
                    execOrganData = model.SORT_GROUPBY_DEPT
                        ?
                        execOrganData.OrderBy(x => x.DeptType).ThenBy(x => x.EXEC_DEPT_F1).ToList()
                        :
                        execOrganData.OrderBy(x => x.EXEC_DEPT_F1).ThenBy(x => x.OU_SORT_ORDER).ToList();
                    break;
                case "B": //依執行機關
                    execOrganData = execOrganData.OrderBy(x => x.OU_SORT_ORDER).ToList();
                    break;
                case "C": //落後比率
                    execOrganData = model.SORT_GROUPBY_DEPT
                        ?
                        execOrganData.OrderBy(x => x.DeptType).ThenBy(x => x.EXEC_DEPT_F2).ToList()
                        :
                        execOrganData.OrderBy(x => x.EXEC_DEPT_F2).ThenBy(x => x.OU_SORT_ORDER).ToList();
                    break;
                case "D": //綜合排序(落後件數及落後比率)
                    execOrganData = model.SORT_GROUPBY_DEPT
                        ?
                        execOrganData.OrderBy(x => x.DeptType).ThenBy(x => x.EXEC_DEPT_RANKING).ToList()
                        :
                        execOrganData.OrderBy(x => x.EXEC_DEPT_RANKING).ThenBy(x => x.OU_SORT_ORDER).ToList();
                    break;
            }
        }
    }
}
