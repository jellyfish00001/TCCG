using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class SuperiorController : ControllerBase
    {
        private readonly ISuperiorService superiorService;

        public SuperiorController(ISuperiorService superiorService)
        {
            this.superiorService = superiorService;
        }

        #region 重大建設分析
        /// <summary>
        /// 取得圖表統計
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<AnalysisModel> GetAnalysis(AnalysisQueryModel model)
        {
            return await superiorService.GetAnalysis(model);
        }

        /// <summary>
        /// 取得圓餅圖表統計
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ChartModel<int>> GetPieChartStat(AnalysisQueryModel model)
        {
            return await superiorService.GetPieChartStat(model);
        }

        /// <summary>
        /// 取得圖表統計明細
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<AnalysisDetailModel> GetAnalysisDetail(AnalysisQueryModel model)
        {
            return await superiorService.GetAnalysisDetail(model);
        }

        /// <summary>
        /// 取得重大建設計劃清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DecisionPlanModel>> GetAnalyzePlan(AnalysisQueryModel model)
        {
            return await superiorService.GetAnalyzePlan(model);
        }
        #endregion 重大建設分析

        #region 建設類別查詢
        /// <summary>
        /// 取得建設類別清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProgessModel>> GetProgesses(ProgessQueryModel model)
        {
            return await superiorService.GetProgesses(model);
        }
        #endregion 建設類別查詢

        #region 區域統計分析
        /// <summary>
        /// 取得區域統計清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ChartModel<int>>> GetRegions(RegionQueryModel model)
        {
            return await superiorService.GetRegions(model);
        }

        /// <summary>
        /// 取得區域統計計劃清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DecisionPlanModel>> GetRegionPlan(RegionQueryModel model)
        {
            return await superiorService.GetRegionPlan(model);
        }
        #endregion 區域統計分析

        #region 落後案件查詢
        /// <summary>
        /// 取得落後案件清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DelayPlanModel>> GetDelayPlans(DelayPlanQueryModel model)
        {
            return await superiorService.GetDelayPlans(model);
        }
        #endregion 落後案件查詢
    }
}
