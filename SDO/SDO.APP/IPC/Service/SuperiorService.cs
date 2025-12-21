using SDO.Base.Utils.Models;
using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class SuperiorService : Service, ISuperiorService
    {
        private readonly ISuperiorDac dac;
        private readonly IDropDownDac dropDownDac;

        public SuperiorService(ISuperiorDac superiorDac, IDropDownDac dropDownDac)
        {
            this.dac = superiorDac;
            this.dropDownDac = dropDownDac;
        }

        #region 重大建設分析
        /// <summary>
        /// 取得圖表統計
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AnalysisModel> GetAnalysis(AnalysisQueryModel model)
        {
            IEnumerable<ChartStatModel> chartStatData = await dac.GetChartStats(model);
            // 取得參數
            List<DropDownListModel> paramData = await GetParamData(model.CHART_TYPE);

            AnalysisModel result = new();
            paramData.ForEach(x =>
            {
                IEnumerable<ChartStatModel> chartStats = chartStatData.Where(y => y.Value == x.value);
                result.StatCnts.Add(new ChartModel<int>
                {
                    Key = x.value,
                    Text = x.text,
                    Values = new List<int>
                    {
                        chartStats.Where(y => y.PRG_OFFSET <= -5).Sum(y => y.Cnt), // 落後>=5%
                        chartStats.Where(y => -5 < y.PRG_OFFSET && y.PRG_OFFSET < 0).Sum(y => y.Cnt), // 落後<5%
                        chartStats.Where(y => y.PRG_OFFSET >= 0).Sum(y => y.Cnt), // 進度符合
                    }
                });

                // 落後件數比例
                decimal totalCnt = chartStats.Sum(y => y.Cnt);
                decimal delayPct = totalCnt > 0 ? chartStats.Count(y => y.PRG_OFFSET < 0)/totalCnt:0;

                // 落後比例代碼 A:無落後 B:落後<50% C:落後>=50%
                string delayPctType = delayPct == 0 ? "A": delayPct > 0 && delayPct * 100 < 50 ? "B" : "C";
                result.StatBudgetAmts.Add(new ChartModel<decimal>
                {
                    Key = x.value,
                    Text = x.text,
                    OtherData = delayPctType,
                    Values = new List<decimal>
                    {
                        chartStats.Sum(y => y.PROJ_BUDGET)
                    }
                });
            });

            result.StatCnts = result.StatCnts.OrderByDescending(x => x.Values[0]).ThenByDescending(x => x.Values[1]).ThenByDescending(x => x.Values[2]).ToList();
            result.StatBudgetAmts = result.StatBudgetAmts.OrderByDescending(x => x.Values[0]).ToList();

            return result;
        }

        /// <summary>
        /// 依照圖表類別取得參數
        /// </summary>
        /// <param name="chartType">圖表類別</param>
        /// <returns></returns>
        private async Task<List<DropDownListModel>> GetParamData(int chartType)
        {
            List<DropDownListModel> result = new();
            switch (chartType)
            {
                case 0:
                    result = await dropDownDac.GetComPlanKind();
                    break;
                case 1:
                    result = await dropDownDac.GetOrganList();
                    break;
                case 2:
                    result = await dropDownDac.GetCodeTownByCityId("H");
                    break;
            }
            return result;
        }

        /// <summary>
        /// 取得圓餅圖表統計
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ChartModel<int>> GetPieChartStat(AnalysisQueryModel model)
        {
            List<ChartStatModel> chartStatData = (await dac.GetChartStats(model)).ToList();
            ChartModel<int> result = new ChartModel<int>
            {
                Values = new List<int>
                {
                    chartStatData.Where(y => y.PRG_OFFSET >= 0).Sum(y => y.Cnt), // 符合
                    chartStatData.Where(y => y.PRG_OFFSET < 0).Sum(y => y.Cnt), // 落後
                }
            };

            return result;
        }

        /// <summary>
        /// 取得圖表統計明細
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AnalysisDetailModel> GetAnalysisDetail(AnalysisQueryModel model)
        {
            int chartType = model.CHART_TYPE;

            AnalysisDetailModel result = new();
            result.StatBuilds = chartType != 0 ? await GetChartData(model, 0) : new();
            result.StatMasterOrgans = chartType != 1 ? await GetChartData(model, 1) : new();
            result.StatTowns = chartType != 2 ? await GetChartData(model, 2) : new();

            if (model.IS_GET_MATCH == false)
            {
                await GetBehind(model, result);
            }
            return result;
        }

        /// <summary>
        /// 取得落後
        /// </summary>
        /// <param name="model"></param>
        /// <param name="rtnModel"></param>
        /// <returns></returns>
        private async Task GetBehind(AnalysisQueryModel model, AnalysisDetailModel rtnModel)
        {
            IEnumerable<ChartStatBehindModel> chartStatBehinds = await dac.GetChartStatBehinds(model);

            List<ChartStatBehindModel> delayKindNulls = chartStatBehinds.Where(x => string.IsNullOrEmpty(x.DELAY_KIND)).ToList();
            List<ChartStatBehindModel> delayKinds = chartStatBehinds.Where(x => !string.IsNullOrEmpty(x.DELAY_KIND)).ToList();
            rtnModel.StatCntBehind =  new ChartModel<int>
            {
                Values = new List<int>
                {
                    delayKinds.Where(x => x.DELAY_KIND.Trim() == "D1").Count(), // D1
                    delayKinds.Where(x => x.DELAY_KIND.Trim() == "D2").Count(), // D2
                    delayKinds.Where(x => x.DELAY_KIND.Trim() == "D3").Count(), // D3
                    delayKindNulls.Count(), // null
                }
            };

            rtnModel.BehindItems = chartStatBehinds.GroupBy(x => new { x.DELAY_SUBCLASS_C, x.DELAY_CLASS_SUB_ITEM }).Select(x => new ChartModel<int>
            {
                Key = x.Key.DELAY_SUBCLASS_C,
                Text = x.Key.DELAY_CLASS_SUB_ITEM,
                Values = new List<int> { x.Count() }
            }).ToList();
        }

        /// <summary>
        /// 取得圖表資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="chartType"></param>
        /// <returns></returns>
        private async Task<List<ChartModel<int>>> GetChartData(AnalysisQueryModel model, int chartType)
        {
            model.CHART_TYPE = chartType;

            IEnumerable<ChartStatModel> chartStatData = await dac.GetChartStats(model);
            // 取得參數
            List<DropDownListModel> paramData = await GetParamData(model.CHART_TYPE);

            List<ChartModel<int>> result = new();
            paramData.ForEach(x =>
            {
                IEnumerable<ChartStatModel> chartStats = chartStatData.Where(y => y.Value == x.value);
                if (chartStats.Any())
                {
                    result.Add(new ChartModel<int>
                    {
                        Key = x.value,
                        Text = x.text,
                        Values = new List<int> { chartStats.Sum(y => y.Cnt) }
                    });
                }
            });

            return result;
        }

        /// <summary>
        /// 取得分析計劃清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<DecisionPlanModel>> GetAnalyzePlan(AnalysisQueryModel model)
        {
            return await dac.GetAnalyzePlan(model);
        }
        #endregion 重大建設分析

        #region 建設類別查詢
        /// <summary>
        /// 取得建設類別清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ProgessModel>> GetProgesses(ProgessQueryModel model)
        {
            return await dac.GetProgesses(model);
        }
        #endregion 建設類別查詢

        #region 區域統計分析
        /// <summary>
        /// 取得區域統計清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<ChartModel<int>>> GetRegions(RegionQueryModel model)
        {
            IEnumerable<RegionModel> chartStatData = await dac.GetRegions(model);
            // 取得參數
            List<DropDownListModel> paramData = await dropDownDac.GetCodeTownByCityId("H");

            List<ChartModel<int>> result = new();
            paramData.ForEach(x =>
            {
                IEnumerable<RegionModel> chartStats = chartStatData.Where(y => y.TOWN_C == x.value);
                result.Add(new ChartModel<int>
                {
                    Key = x.value,
                    Text = x.text,
                    Values = new List<int>
                    {
                        chartStats.Where(y => y.PRG_OFFSET <= -5).Count(), // 落後>=5%
                        chartStats.Where(y => -5 < y.PRG_OFFSET && y.PRG_OFFSET < 0).Count(), // 落後<5%
                        chartStats.Where(y => y.PRG_OFFSET >= 0).Count(), // 進度符合
                    }
                });
            });

            result = result.OrderByDescending(x => x.Values[0]).ThenByDescending(x => x.Values[1]).ThenByDescending(x => x.Values[2]).ToList();

            return result;
        }

        /// <summary>
        /// 取得區域統計計劃清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<DecisionPlanModel>> GetRegionPlan(RegionQueryModel model)
        {
            return await dac.GetRegionPlan(model);
        }
        #endregion 區域統計分析

        #region 落後案件查詢
        /// <summary>
        /// 取得落後案件清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<List<DelayPlanModel>> GetDelayPlans(DelayPlanQueryModel model)
        {
            List<DelayPlanModel> result = await dac.GetDelayPlans(model);

            // 報表名稱
            switch (model.RPT_TYPE)
            {
                case "1": // 規劃階段工作進度落後案件
                    result = result.Where(x => x.DELAY_KIND == "D1").ToList();
                    break;
                case "2": // 施工階段工程進度落後案件
                    result = result.Where(x => x.DELAY_KIND == "D2").ToList();
                    break;
                case "3": // 驗收階段工作進度落後案件
                    result = result.Where(x => x.DELAY_KIND == "D3").ToList();
                    break;
                case "4": // 檢核點進度落後60天以上案件
                    List<string> dayOffsetProjNos = await dac.GetDayOffsetProjNos();
                    break;
                case "5": // 工程進度落後15%以上案件
                    result = result.Where(x => x.ENG_PRG_OFFSET >= 15).ToList();
                    break;
                case "6": // 連續3個月工程進度落後案件
                    // 計畫編號清單
                    List<string> projNos = result.Select(x => x.PROJECT_NO).Distinct().ToList();
                    // 落後清單
                    List<(string PROJECT_NO, int DATA_YEAR, int DATA_MONTH)> delayCausals = await dac.GetDelayCausals(projNos);
                    // 近三個月(不含本月)連續落後 計畫編號清單
                    List<string> delayProjNos = GetConsecutiveMonths(delayCausals);
                    result = result.Where(x => delayProjNos.Contains(x.PROJECT_NO)).ToList();
                    break;
            }

            return result.OrderByDescending(x => x.PIS_SELECT).ThenBy(x => x.PROJECT_NO).ToList();
        }

        /// <summary>
        /// 取得近三個月(不含本月)都落後的計畫編號清單
        /// </summary>
        /// <param name="delayCausalList">落後清單</param>
        /// <returns></returns>
        private List<string> GetConsecutiveMonths(List<(string PROJECT_NO, int DATA_YEAR, int DATA_MONTH)> delayCausalList)
        {
            List<string> result = new();
            DateTime currMonthDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date;
            // 比對項目(近三個月的月份第一天)
            List<DateTime> nearlyThreeMonthsCycle = new List<DateTime> { currMonthDate.AddMonths(-1), currMonthDate.AddMonths(-2), currMonthDate.AddMonths(-3) };

            foreach(string projNo in delayCausalList.Select(x => x.PROJECT_NO).Distinct())
            {
                // 計畫落後周期
                List<DateTime> projDelayCycle = delayCausalList.Where(x => x.PROJECT_NO == projNo).Select(x => new DateTime(x.DATA_YEAR + 1911, x.DATA_MONTH, 1).Date).ToList() ;

                if(projDelayCycle.Any() && projDelayCycle.Intersect(nearlyThreeMonthsCycle).Count() == 3)
                {
                    result.Add(projNo);
                }
            }
            return result;
        }
        #endregion 落後案件查詢
    }
}
