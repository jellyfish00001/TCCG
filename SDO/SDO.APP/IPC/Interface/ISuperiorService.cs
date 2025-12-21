using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface ISuperiorService
    {
        #region 重大建設分析
        /// <summary>
        /// 取得圖表統計
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<AnalysisModel> GetAnalysis(AnalysisQueryModel model);

        /// <summary>
        /// 取得圓餅圖表統計
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ChartModel<int>> GetPieChartStat(AnalysisQueryModel model);

        /// <summary>
        /// 取得圖表統計明細
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<AnalysisDetailModel> GetAnalysisDetail(AnalysisQueryModel model);

        /// <summary>
        /// 取得分析計劃清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<DecisionPlanModel>> GetAnalyzePlan(AnalysisQueryModel model);
        #endregion 重大建設分析

        #region 建設類別查詢
        /// <summary>
        /// 取得建設類別清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProgessModel>> GetProgesses(ProgessQueryModel model);
        #endregion 建設類別查詢

        #region 區域統計分析
        /// <summary>
        /// 取得區域統計清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ChartModel<int>>> GetRegions(RegionQueryModel model);

        /// <summary>
        /// 取得區域統計計劃清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<DecisionPlanModel>> GetRegionPlan(RegionQueryModel model);
        #endregion 區域統計分析

        #region 落後案件查詢
        /// <summary>
        /// 取得落後案件清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<DelayPlanModel>> GetDelayPlans(DelayPlanQueryModel model);
        #endregion 落後案件查詢
    }
}
