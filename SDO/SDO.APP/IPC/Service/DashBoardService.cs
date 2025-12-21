using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Newtonsoft.Json.Converters;
using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class DashBoardService : Service, IDashBoardService
    {
        private readonly IDashBoardDac dac;
        private readonly IStatisticsDac statisticsDac;
        private DateTime QueryDate;
        private string QueryOrgId;

        // 統計類別
        Dictionary<string, List<string>> StatProjectStatusTypes = new()
        {
            {"Summary", new List<string>(){ "4", "5","6","7","8" } },// 重要儀表板統計計畫類別 (執行中含結案、撤銷)
            {"InProgress", new List<string>(){ "4", "5", "6" } },    // 執行中列管情形資料(執行中 (不含結案、撤銷)
        };

        public DashBoardService(IDashBoardDac dac, IStatisticsDac statisticsDac)
        {
            this.dac = dac;
            this.statisticsDac = statisticsDac;
        }

        /// <summary>
        /// 寫入儀錶板資料
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task<bool> GenerateDashBoardData(string year, string month)
        {
            // 取得需寫入的資料
            List<DABProjectDataModel> dabProjectData = await dac.GetDABProjectData(year, month);
            // 取得連續落後資料
            List<DABDelayMonthModel> dabDelayMonthData = await GetDelayMonthData(year, month);
            // 計算行政區每月案件統計資料
            List<DABTownModel> dabTownData = CountDABTownData(dabProjectData, year, month);
            if (dabProjectData.Any())
            {
                // 歷年列管情形資料
                List<DABPastYearsModel> dabPastYearsData = await dac.GetDABPastYearsData();
                List<DABCompositeModel> compositeInsertData = new();
                // 計算執行中、執行中(含結案、撤銷)計畫統計資料
                foreach(KeyValuePair<string,List<string>> item in StatProjectStatusTypes)
                {
                    List<DABProjectDataModel> dabProjects = dabProjectData.Where(x => item.Value.Contains(x.PROJECT_STATUS)).ToList();
                    bool isInProgress = item.Key == "InProgress";
                    // 每月綜合排序 - 執行機關統計資料
                    compositeInsertData.AddRange(HandleDABStatistic(dabProjects, "A", isInProgress));
                    // 每月綜合排序 - 行政區統計資料
                    compositeInsertData.AddRange(HandleDABStatistic(dabProjects, "B", isInProgress));
                    // 每月綜合排序 - 建設別統計資料
                    compositeInsertData.AddRange(HandleDABStatistic(dabProjects, "C", isInProgress));
                }

                // 當年各執行機關總落後比率
                List<ExecOrgDelayModel> execOrgDelayRates = await CountExecDelayRate(year, month, HandleDABStatistic(dabProjectData, "A"));

                // 將統計資料寫入
                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // 寫入前先移除該月資料
                    await DeleteDABData(year, month);
                    // 基本資料明細檔
                    await dac.InsertDABProjectData(dabProjectData);
                    // 每月綜合排序資料
                    await dac.InsertDABComposite(compositeInsertData);
                    // 歷年列管情形 
                    await InsertDABPastYears(dabPastYearsData, execOrgDelayRates, year);
                    // 機關連續落後比率
                    await dac.InsertDABDelayMonth(dabDelayMonthData);
                    // 行政區每月案件統計
                    await dac.InsertDABTownData(dabTownData);

                    scope.Complete();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 取得重要儀表板相關資料
        /// </summary>
        /// <returns></returns>
        public async Task<DashBoardSummaryModel> GetDashBoardSummaryData(DashBoardQueryModel queryModel)
        {
            DashBoardSummaryModel result = new();
            var (DAB_YEAR_YYY, DAB_MONTH, EXEC_ORGAN_C,IS_IN_PROGRESS_DATA) = queryModel;

            this.QueryDate = $"{DAB_YEAR_YYY}/{DAB_MONTH}/01".ToDateTimeWithNull(true).Value;

            // 取得當年及前一年落後資料
            string lastYear = (Int32.Parse(DAB_YEAR_YYY) - 1).ToString();
            List<DABPastYearsModel> pastYearsData = await dac.GetDABPastYears(new List<string>() 
            {   DAB_YEAR_YYY,
                lastYear
            });
            // 全府平均落後比率
            result.AvgDelayRate = GetAverageDelayRate(pastYearsData.Where(x => x.DAB_YEAR_YYY == DAB_YEAR_YYY));
            // 前一年平均落後比率
            result.LastYearAvgDelayRate = GetAverageDelayRate(pastYearsData.Where(x => x.DAB_YEAR_YYY == lastYear));

            List<DABCompositeModel> compositeData = await dac.GetDABComposite(new DABCompositeQueryModel()
            {
                QueryPeriodSt = QueryDate,
                QueryPeriodEnd = QueryDate,
                IS_IN_PROGRESS_DATA = IS_IN_PROGRESS_DATA
            });

            // 落後排名資料
            result.OrgDelayRankData = compositeData.Where(x => x.DAB_KIND == "A").ToList();
            // 建設類別、行政區的件數及經費資料
            result.BudgetCountData = compositeData.Where(x => x.DAB_KIND == "B" || x.DAB_KIND == "C").ToList();
            return result;
        }

        /// <summary>
        /// 重要儀表板 - 取得機關統計資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        public async Task<OrgProjectSummaryModel> GetOrgProjectSummary(DashBoardQueryModel queryModel)
        {
            OrgProjectSummaryModel result = new ();
            var (DAB_YEAR_YYY, DAB_MONTH, EXEC_ORGAN_C, IS_IN_PROGRESS_DATA) = queryModel;
            this.QueryOrgId = EXEC_ORGAN_C;
            this.QueryDate = $"{DAB_YEAR_YYY}/{DAB_MONTH}/01".ToDateTimeWithNull(true).Value;
            // 近一年執行機關 列管件數、總經費、落後比率件數統計資料 
            List<DABCompositeModel> orgCompositeData = await dac.GetDABComposite(new DABCompositeQueryModel()
            {
                IsQueryFullYear = true,
                DabKinds = new List<string>() { "A" },
                DAB_YEAR_YYY = DAB_YEAR_YYY,
                DAB_MONTH = DAB_MONTH,
                IS_IN_PROGRESS_DATA = IS_IN_PROGRESS_DATA
            });
            // 近一年各機關落後比率
            result.AnnualOrgDelayRateData = orgCompositeData.OrderBy(x => x.DabDate).ToList();

            if(!string.IsNullOrEmpty(EXEC_ORGAN_C))
            orgCompositeData = orgCompositeData.Where(x=>x.SET_TYPE == EXEC_ORGAN_C).ToList();

            // 計算機關統計資料
            CountOrgCompositeData(orgCompositeData, ref result);

            // 各階段計畫落後件數
            result.ProjectCategoryDelayData = await dac.GetProjectDelayType(new DABCompositeQueryModel()
            {
                QueryPeriodSt = QueryDate,
                QueryPeriodEnd = QueryDate,
                SET_TYPE = EXEC_ORGAN_C                
            });

            // 機關落後資料(近兩個月)
            List<DABDelayMonthModel> orgDelayMonthData = await dac.GetOrgRecentDelayRates(QueryDate.AddMonths(-1),
                QueryDate, EXEC_ORGAN_C, IS_IN_PROGRESS_DATA);

            // 計算連續三個月落後案件比率
            CountDelayOver3MonthsData(orgDelayMonthData, ref result);
            // 行政區落後統計資料
            result.TownDelayData = await dac.GetTownDelayData(queryModel);

            return result;
        }

        /// <summary>
        /// 取得重大工程進度資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        public async Task<DABTownModel> GetDashBoardEngProgress(DashBoardQueryModel queryModel)
        {
            DABTownModel result = await dac.GetDashBoardEngProgress(queryModel);
            if(result!= null)
            {
                // 取得完工統計資料
                List<ProjectFinishModel> projectFinishModels = await dac.GetProjectFinishData(queryModel);
                result.LastTwoYearsFinishCnt = projectFinishModels.Count;
                result.EstimateFinishCnt = projectFinishModels.Where(x => (x.ESTIMATED_ENDDATE.Value.Year - 1911).ToString() == queryModel.DAB_YEAR_YYY).Count();
                result.ActualFinishCnt = projectFinishModels.Where(x =>
                    (x.ESTIMATED_ENDDATE.Value.Year - 1911).ToString() == queryModel.DAB_YEAR_YYY
                    && x.FINISH_DATE.HasValue).Count();
            }

            return result;
        }

        /// <summary>
        /// 件數及經費執行情形 - 頁面資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        public async Task<List<ProjectTotalDataModel>> GetDashBoardCountAndBudget(ProjectTotalQueryModel queryModel)
        {
            return await dac.GetDashBoardCountAndBudget(queryModel);
        }

        /// <summary>
        /// 取得歷年列管情形資料
        /// </summary>
        /// <param name="EXEC_ORGAN_C"></param>
        /// <returns></returns>
        public async Task<List<DABPastYearsModel>> GetPastYearsData(string EXEC_ORGAN_C)
        {
            List<DABPastYearsModel> result = await dac.GetPastYearsData(EXEC_ORGAN_C);
            // 桃園市政府須加總所有機關
            if (string.IsNullOrEmpty(EXEC_ORGAN_C))
            {
                return result.GroupBy(x => x.DAB_YEAR_YYY)
                    .OrderBy(group=>group.Key)
                    .Select(group => new DABPastYearsModel()
                    {
                        DAB_YEAR_YYY = group.Key,
                        TOTAL_EXS = group.Sum(y => y.TOTAL_EXS),
                        TOTAL_NUM = group.Sum(y =>y.TOTAL_NUM),
                        DELAY_RATE = Math.Round(group.Average(y=>y.DELAY_RATE),2)

                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 計算機關統計資料
        /// </summary>
        /// <param name="orgCompositeData"></param>
        /// <param name="result"></param>
        private void CountOrgCompositeData(List<DABCompositeModel> orgCompositeData, ref OrgProjectSummaryModel result)
        {
            // 若指定執行機關需篩選
            if (!string.IsNullOrEmpty(QueryOrgId))
            {
                orgCompositeData = orgCompositeData.Where(x => x.SET_TYPE == QueryOrgId).ToList();
            }
            // 查詢月資料
            List<DABCompositeModel> queryMonthData = orgCompositeData.Where(x => x.DabDate == QueryDate).ToList();
            // 查詢月 前一個月資料
            List<DABCompositeModel> previousMonthData = orgCompositeData.Where(x => x.DabDate == QueryDate.AddMonths(-1)).ToList();

            // 列管件數
            result.ProjectCount = queryMonthData.Sum(x => x.TOTAL_NUM);
            result.ProjectCountDiff = result.ProjectCount - previousMonthData.Sum(x => x.TOTAL_NUM);
            // 經費
            result.BudgetTotal = (decimal)Math.Round(queryMonthData.Sum(x => x.BUDGET_TOTAL)/100000000,2);
            result.BudgetTotalDiff = result.BudgetTotal - (decimal)Math.Round(previousMonthData.Sum(x => x.BUDGET_TOTAL) / 100000000);
            // 落後件數
            result.DelayCount = queryMonthData.Sum(x => x.DELAY_NUM);
            result.DelayCountDiff = result.DelayCount - previousMonthData.Sum(x => x.DELAY_NUM);
            // 落後比率
            if(queryMonthData.Any())
                result.DelayRate = Math.Round(queryMonthData.Average(x => x.DELAY_RATE),2);
            if(previousMonthData.Any())
                result.DelayRateDiff = result.DelayRate - Math.Round(previousMonthData.Average(x => x.DELAY_RATE),2);
            // 可能影響補助款件數
            result.AffectedSubsidyNum = orgCompositeData.Sum(x => x.AFFECTED_SUBSIDY_NUM);
        }

        /// <summary>
        /// 計算工程進度&檢核點落後統計件數
        /// </summary>
        /// <param name="dabProjects"></param>
        /// <param name="result"></param>
        private void CountOrgDelayDataFromDABProject(List<DABProjectDataModel> dabProjects , ref OrgProjectSummaryModel result)
        {
            // 工程進度落後資料統計
            result.DelayPrgBelow10Cnt = dabProjects.Where(x => x.DELAY_PRG < 10).Count();
            result.DelayPrgBelow20Cnt = dabProjects.Where(x => x.DELAY_PRG < 20 && x.DELAY_PRG >= 10).Count();
            result.DelayPrgOver20Cnt = dabProjects.Where(x => x.DELAY_PRG >= 20).Count();
            // 檢核點進度落後統計
            result.ChkPtDelayBelow3MonthsCnt = dabProjects.Where(x => x.CHKPT_DELAY_DAYS < 90).Count();
            result.ChkPtDelayBelow6MonthsCnt = dabProjects.Where(x => x.CHKPT_DELAY_DAYS >= 90 && x.CHKPT_DELAY_DAYS < 180).Count();
            result.ChkPtDelayOver6MonthsCnt = dabProjects.Where(x => x.CHKPT_DELAY_DAYS >= 180).Count();

        }

        /// <summary>
        /// 計算連續落後3個月以上比率資料
        /// </summary>
        /// <param name="delayData"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private void CountDelayOver3MonthsData(List<DABDelayMonthModel> delayData , ref OrgProjectSummaryModel result)
        {
            result.DelayOver3MonthsRate = delayData.Where(x => x.DabDate == QueryDate).Select(x => x.DELAY_NUM_3_RATE)
                .DefaultIfEmpty(0).Average();

            result.DelayOver3MonthsRateDiff = result.DelayOver3MonthsRate - delayData.Where(x => x.DabDate == QueryDate.AddMonths(-1))
                .Select(x => x.DELAY_NUM_3_RATE).DefaultIfEmpty(0).Average();
        }

        /// <summary>
        /// 刪除DAB相關該月統計資料
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        private async Task DeleteDABData(string year, string month)
        {
            await dac.DeleteDABProjectDataByMonth(year, month);
            await dac.DeleteDABCompositeByMonth(year, month);
            await dac.DeleteDABPastYears();
            await dac.DeleteDABDelayMonth(year, month);
            await dac.DeleteDABTown(year,month);
        }

        /// <summary>
        /// 統計各DAB表資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dabKind"></param>
        /// <param name="IsInProgress"></param>
        /// <returns></returns>
        private List<DABCompositeModel> HandleDABStatistic(List<DABProjectDataModel> data, string dabKind,bool IsInProgress = false)
        {
            // 統計標的資料(各執行機關、行政區、建設類別)
            Dictionary<string, string> statDict = new Dictionary<string, string>();
            List<DABCompositeModel> result = new List<DABCompositeModel>();
            switch (dabKind)
            {
                case "A":// 執行機關統計資料
                    statDict = data.Where(x => !string.IsNullOrEmpty(x.EXEC_ORGAN_C))
                        .Select(x => new { x.EXEC_ORGAN_C, x.EXEC_DEPT }).Distinct()
                        .ToDictionary(x => x.EXEC_ORGAN_C, y => y.EXEC_DEPT);
                    break;
                case "B":// 行政區統計資料
                    statDict = data.Where(x => !string.IsNullOrEmpty(x.TOWN_C))
                        .Select(x => new { x.TOWN_C, x.TOWNNAME }).Distinct()
                        .ToDictionary(x => x.TOWN_C, y => y.TOWNNAME);
                    break;
                case "C": // 建設別統計資料
                    statDict = data.Where(x => !string.IsNullOrEmpty(x.BUILD_KIND))
                        .Select(x => new { x.BUILD_KIND, x.BUILD_KIND_DESC }).Distinct()
                        .ToDictionary(x => x.BUILD_KIND, y => y.BUILD_KIND_DESC);
                    break;
            }
            // 將DABProjectData 統計並轉為 DABComposite
            result = CountDABProjectDataToComposite(data, dabKind, statDict,IsInProgress);
            RankStatisticalData(ref result);
            return result;
        }

        /// <summary>
        /// 將DABProjectData 統計並轉為 DABComposite
        /// </summary>
        /// <param name="models"></param>
        /// <param name="dabKind"></param>
        /// <param name="statDict"></param>
        /// <param name="IsInProgress"></param>
        /// <returns></returns>
        private List<DABCompositeModel> CountDABProjectDataToComposite(List<DABProjectDataModel> models, string dabKind, Dictionary<string, string> statDict, bool IsInProgress)
        {
            List<DABCompositeModel> result = new();
            if (!models.Any())
                return result;

            List<DABProjectDataModel> filterData = new();

            foreach (KeyValuePair<string, string> item in statDict)
            {
                DABCompositeModel comData = new();
                // 執行機關
                if (dabKind == "A")
                {
                    filterData = models.Where(x => x.EXEC_ORGAN_C == item.Key).ToList();
                }
                // 行政區
                else if (dabKind == "B")
                {
                    filterData = models.Where(x => x.TOWN_C == item.Key).ToList();
                }
                // 建設類別
                else if (dabKind == "C")
                {
                    filterData = models.Where(x => x.BUILD_KIND == item.Key).ToList();
                }

                if (filterData.Any())
                {
                    comData.DAB_YEAR_YYYY = models.First().DAB_YEAR_YYYY;
                    comData.DAB_YEAR_YYY = models.First().DAB_YEAR_YYY;
                    comData.DAB_MONTH = models.First().DAB_MONTH;
                    comData.DAB_KIND = dabKind;
                    comData.SET_TYPE = item.Key;
                    comData.SET_VALUE = item.Value;
                    comData.BUDGET_TOTAL = filterData.Sum(x => x.BUDGET_TOTAL);
                    comData.CLOSE_NUM = filterData.Where(x => x.STATUS == "B").Count();
                    comData.CONFORM_NUM = filterData.Where(x => x.STATUS == "C").Count();
                    comData.DELAY_NUM_D1 = filterData.Where(x => x.STATUS == "D1").Count();
                    comData.DELAY_NUM_D2 = filterData.Where(x => x.STATUS == "D2").Count();
                    comData.DELAY_NUM_D3 = filterData.Where(x => x.STATUS == "D3").Count();
                    comData.DELAY_NUM = comData.DELAY_NUM_D1 + comData.DELAY_NUM_D2 + comData.DELAY_NUM_D3;
                    comData.CANCEL_NUM = filterData.Where(x => x.STATUS == "E").Count();
                    comData.TOTAL_NUM = comData.CLOSE_NUM + comData.CONFORM_NUM + comData.DELAY_NUM + comData.CANCEL_NUM;
                    comData.DELAY_RATE = comData.TOTAL_NUM == 0 ? 0 : (decimal)Math.Round(comData.DELAY_NUM / comData.TOTAL_NUM * 100, 2);
                    comData.COMPLETE_RATE = comData.TOTAL_NUM == 0 ? 0 : (decimal)Math.Round(comData.CLOSE_NUM / comData.TOTAL_NUM * 100, 2);
                    comData.AFFECTED_SUBSIDY_NUM = filterData.Where(x => x.BUDGET_CENTRAL > 0).Count();
                    comData.IS_IN_PROGRESS_DATA = IsInProgress;
                    result.Add(comData);
                }
            }
            return result;
        }

        /// <summary>
        /// 排序統計資料
        /// </summary>
        /// <param name="source"></param>
        private void RankStatisticalData(ref List<DABCompositeModel> source)
        {
            // 依據 件數(D) 大到小排名 落後件數排序(F1)
            var dRankings = source.OrderByDescending(x => x.DELAY_NUM)
                .Select(x => x.DELAY_NUM).Distinct().Select((x, i) => new { DELAY_NUM = x, Rank = i + 1 });

            foreach (var ranking in dRankings)
            {
                List<DABCompositeModel> data = source.Where(x => x.DELAY_NUM == ranking.DELAY_NUM).ToList();
                data.ForEach(x =>
                {
                    x.F1 = ranking.Rank;
                });
            }

            // 依據 比率(D/A) 大到小排名 落後比率排序(F2)
            var daRankings = source.OrderByDescending(x => x.DELAY_RATE)
                .Select(x => x.DELAY_RATE).Distinct().Select((x, i) => new { DELAY_RATE = x, Rank = i + 1 });

            foreach (var ranking in daRankings)
            {
                List<DABCompositeModel> data = source.Where(x => x.DELAY_RATE == ranking.DELAY_RATE).ToList();
                data.ForEach(x =>
                {
                    x.F2 = ranking.Rank;
                });
            }

            // 依據 落後排序合計(F1+F2) 小到大排名 落後綜合排名
            var rankings = source.OrderBy(x => x.DELAY_F1F2)
                .Select(x => x.DELAY_F1F2).Distinct().Select((x, i) => new { DELAY_F1F2 = x, Rank = i + 1 });

            foreach (var ranking in rankings)
            {
                List<DABCompositeModel> data = source.Where(x => x.DELAY_F1F2 == ranking.DELAY_F1F2).ToList();
                data.ForEach(x =>
                {
                    x.DELAY_RANKING = ranking.Rank;
                });
            }
        }

        /// <summary>
        /// 計算當年執行機關總落後比率
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="execCompositeData"></param>
        /// <returns></returns>
        private async Task<List<ExecOrgDelayModel>> CountExecDelayRate(string year, string month, List<DABCompositeModel> execCompositeData)
        {
            List<ExecOrgDelayModel> result = new List<ExecOrgDelayModel>();
            // 當年度(不含當月)執行機關平均落後比率
            List<ExecOrgDelayModel> avgDelayRateOfYear = new();
            if (month != "01")
                avgDelayRateOfYear = await dac.GetExecAvgDelayRateOfYear(year, month);
            // 當月執行機關平均落後比率
            List<ExecOrgDelayModel> currentMonthDelayRate = execCompositeData.Any() ? execCompositeData.Select(x=> new ExecOrgDelayModel()
            {
                EXEC_ORGAN_C = x.SET_TYPE,
                DELAY_RATE = x.DELAY_RATE
            }).ToList() : new List<ExecOrgDelayModel>();

            // 當年度(不含當月)有落後比率機關
            List<string> avgDelayRateOfYearOrgIds = avgDelayRateOfYear.Select(x => x.EXEC_ORGAN_C).ToList();
            // 當月有落後比率的機關
            List<string> currentMonthDelayOrgIds = currentMonthDelayRate.Select(x => x.EXEC_ORGAN_C).ToList();
            // 列舉出所有有落後比率資料的機關
            List<string> orgIds = avgDelayRateOfYearOrgIds.Union(currentMonthDelayOrgIds).ToList() ;
            foreach(string orgId in orgIds)
            {
                ExecOrgDelayModel item = new () { EXEC_ORGAN_C = orgId};
                decimal avgOrgDelayRate = avgDelayRateOfYear.Where(x=>x.EXEC_ORGAN_C == orgId)?.Select(x=>x.DELAY_RATE).FirstOrDefault()??0;
                decimal currentMonthOrgDelayRate = currentMonthDelayRate.Where(x => x.EXEC_ORGAN_C == orgId)?.Select(x => x.DELAY_RATE).FirstOrDefault()??0;
                result.Add(new ExecOrgDelayModel()
                {
                    EXEC_ORGAN_C = orgId,
                    DELAY_RATE = Math.Round((avgOrgDelayRate + currentMonthOrgDelayRate) / 2, 2)
                });
            }

            return result;
        }

        /// <summary>
        /// 寫入歷年列管情形資料
        /// </summary>
        /// <param name="DABPastYearsData"></param>
        /// <param name="currentYearDelayRate"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private async Task InsertDABPastYears(List<DABPastYearsModel> DABPastYearsData, List<ExecOrgDelayModel> currentYearOrgDelayRate, string year)
        {
            foreach (DABPastYearsModel model in DABPastYearsData)
            {
                if (model.DAB_YEAR_YYY == year)
                {
                    model.DELAY_RATE = currentYearOrgDelayRate.Find(x => x.EXEC_ORGAN_C == model.EXEC_ORGAN_C)?.DELAY_RATE ?? 0;
                }
            }

            await dac.InsertDABPastYears(DABPastYearsData);
        }

        /// <summary>
        /// 取得連續落後資料
        /// </summary>
        /// <param name="year">民國年</param>
        /// <param name="month"></param>
        /// <returns></returns>
        private async Task<List<DABDelayMonthModel>> GetDelayMonthData(string year, string month)
        {
            List<DABDelayMonthModel> result = new List<DABDelayMonthModel>();

            if (!Int32.TryParse(month, out int monthInt) || !Int32.TryParse(year, out int yearInt) || monthInt == 0)
                return result;

            // 近三期年月
            var currYM = new { YEAR = year, MONTH = monthInt };
            var lastYM = new
            {
                YEAR = monthInt == 1 ? (yearInt - 1).ToString() : year,
                MONTH = monthInt == 1 ? 12 : monthInt - 1
            };
            var YMBeforeLast = new
            {
                YEAR = monthInt <= 2 ? (yearInt - 1).ToString() : year,
                MONTH = monthInt == 2 ? 12 : monthInt == 1 ? 11 : monthInt - 2
            };

            foreach(KeyValuePair<string, List<string>> item in StatProjectStatusTypes)
            {
                List<string> projStatuses = item.Value; 
                StatisticsModel queryModel = new()
                {
                    STATISTICS_YEAR = year,
                    PROJECT_YEAR = year,
                    STATISTICS_MONTH = monthInt,
                    PROJECT_YEAR_STATUS = "B",
                    ProjectStatuses = projStatuses
                };
                // 取得資料
                List<DelayStatisticsModel> models = await statisticsDac.GetDelayStatistics(queryModel);
                // 取得機關列管計畫件數
                List<OrgProjectCntModel> orgProjectCntModels = await statisticsDac.GetExecOrgProjectCnt(queryModel);
                if (models.Any())
                {
                    // 落後1個月資料
                    List<DelayStatisticsModel> delay1MnDatas = models.Where(x => x.DATA_YEAR.ToString() == currYM.YEAR
                        && (Convert.ToInt32(x.DATA_MONTH) == currYM.MONTH)).ToList();
                    // 連續落後兩個月資料
                    List<DelayStatisticsModel> delay2MnDatas = delay1MnDatas.Any()
                        ? models.Where(x => x.DATA_YEAR.ToString() == lastYM.YEAR
                            && Convert.ToInt32(x.DATA_MONTH) == lastYM.MONTH
                            && delay1MnDatas.Select(y => y.PROJECT_NO).Contains(x.PROJECT_NO)).ToList()
                        : new();
                    // 連續落後三個月的資料
                    List<DelayStatisticsModel> delay3MnDatas = delay2MnDatas.Any()
                        ? models.Where(x => x.DATA_YEAR.ToString() == YMBeforeLast.YEAR
                            && Convert.ToInt32(x.DATA_MONTH) == YMBeforeLast.MONTH
                            && delay2MnDatas.Select(y => y.PROJECT_NO).Contains(x.PROJECT_NO)).ToList()
                        : new();

                    // 落後一個月的資料中 排除連續落後兩個月
                    if (delay2MnDatas.Any())
                    {
                        delay1MnDatas = delay1MnDatas.Where(x => !delay2MnDatas.Select(x => x.PROJECT_NO).Contains(x.PROJECT_NO)).ToList();
                    }
                    // 落後兩個月的資料中，排除連續落後三個月的資料
                    if (delay3MnDatas.Any())
                    {
                        delay2MnDatas = delay2MnDatas.Where(x => !delay3MnDatas.Select(x => x.PROJECT_NO).Contains(x.PROJECT_NO)).ToList();
                    }

                    // 機關連續落後資料
                    foreach (OrgProjectCntModel model in orgProjectCntModels)
                    {
                        string org = model.OrgId;
                        // 機關列管件數
                        decimal orgCnt = model.ProjectCnt;
                        // 落後一個月件數
                        decimal delay1MnCnt = delay1MnDatas.Where(x => x.EXEC_ORGAN_C == org).Count();
                        // 連續落後兩個月件數
                        decimal delay2MnCnt = delay2MnDatas.Where(x => x.EXEC_ORGAN_C == org).Count();
                        // 連續落後三個月件數
                        decimal delay3MnCnt = delay3MnDatas.Where(x => x.EXEC_ORGAN_C == org).Count();
                        // 落後件數
                        decimal totalDelayCnt = delay1MnCnt + delay2MnCnt + delay3MnCnt;
                        result.Add(new DABDelayMonthModel()
                        {
                            DAB_YEAR_YYYY = (yearInt + 1911).ToString(),
                            DAB_YEAR_YYY = year,
                            DAB_MONTH = month,
                            EXEC_ORGAN_C = model.OrgId,
                            EXEC_DEPT = model.OrgName,
                            TOTAL_NUM = orgCnt,
                            DELAY_NUM_3 = delay3MnCnt,
                            DELAY_NUM_2 = delay2MnCnt,
                            DELAY_NUM_1 = delay1MnCnt,
                            IS_IN_PROGRESS_DATA = item.Key== "InProgress"
                        });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 計算行政區每月案件統計表
        /// </summary>
        /// <param name="dabProjects"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        private List<DABTownModel> CountDABTownData(List<DABProjectDataModel> dabProjects,string year,string month)
        {
            List<DABTownModel> result = new List<DABTownModel>();
            if (!dabProjects.Any())
                return result;

            // 找出所有行政區資料
            Dictionary<string, string> townData = dabProjects
                .Where(x => !string.IsNullOrEmpty(x.TOWN_C) 
                    && (x.TOWN_C != "H00" || x.TOWN_C != "H01"))
                .Select(x => new { x.TOWN_C, x.TOWNNAME })
                .Distinct()
                .ToDictionary(x => x.TOWN_C, y => y.TOWNNAME);

            // 找出所有機關資料
            List<string> orgIds = dabProjects.Select(x=>x.EXEC_ORGAN_C).Distinct().ToList();

            foreach(var town in townData)
            {
                foreach(string orgId in orgIds)
                {
                    List<DABProjectDataModel> orgTownData = dabProjects.Where(x => 
                        // 行政區篩選
                        (x.TOWN_C == town.Key
                            || x.TOWN_C == "H00"// 全市
                            || (x.TOWN_C == "H01" && !string.IsNullOrEmpty(x.TOWN_M) && x.TOWN_M.Contains(town.Key)))
                        // 機關篩選
                        && (x.EXEC_ORGAN_C == orgId))
                    .ToList();

                    result.Add(new DABTownModel()
                    {
                        DAB_YEAR_YYY = year,
                        DAB_MONTH = month,
                        TOWN = town.Key,
                        TOWN_NAME = town.Value,
                        TOTAL_NUM = orgTownData.Count,
                        EXEC_ORGAN_C = orgId,
                        STAGE_E2_NUM = orgTownData.Where(x => x.PROJECT_CATEGORY == "E2").Count(),
                        STAGE_E2_DELAY_NUM = orgTownData.Where(x => x.PROJECT_CATEGORY == "E2"
                            && !string.IsNullOrEmpty(x.DELAY_TYPE)).Count(),
                        STAGE_E1_NUM = orgTownData.Where(x => x.PROJECT_CATEGORY == "E1").Count(),
                        STAGE_E1_DELAY_NUM = orgTownData.Where(x => x.PROJECT_CATEGORY == "E1"
                            && !string.IsNullOrEmpty(x.DELAY_TYPE)).Count(),
                        STAGE_E3_NUM = orgTownData.Where(x => x.PROJECT_CATEGORY == "E3").Count(),
                        STAGE_E3_DELAY_NUM = orgTownData.Where(x => x.PROJECT_CATEGORY == "E3"
                            && !string.IsNullOrEmpty(x.DELAY_TYPE)).Count(),
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// 取得平均落後比率
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private decimal GetAverageDelayRate(IEnumerable<DABPastYearsModel> models)
        {
            if(models!=null && models.Any())
                return Math.Round(models.Average(x => x.DELAY_RATE),2);

            return 0;
        }
    }
}
