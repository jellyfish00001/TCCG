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
    public class DashBoardController : ControllerBase
    {
        private readonly IDashBoardService service;

        public DashBoardController(IDashBoardService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 寫入儀錶板資料
        /// </summary>
        /// <param name="year">民國年</param>
        /// <param name="month"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> GenerateDashBoardData([FromForm]string year, [FromForm] string month)
        {
            if (year.Length != 3 || month.Length != 2)
                return new RtnResultModel(false, "輸入資料格式有誤");

            bool isSucceed = await service.GenerateDashBoardData(year, month);
            return new RtnResultModel(isSucceed, isSucceed ? "資料寫入成功" : "該月無資料寫入");
        }

        /// <summary>
        /// 重要儀表板 - 取得頁面所需統計資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        /// <param name="queryModel"></param>
        public async Task<DashBoardSummaryModel> GetDashBoardSummaryData(DashBoardQueryModel queryModel)
            => await service.GetDashBoardSummaryData(queryModel);

        /// <summary>
        /// 重要儀表板 - 取得機關統計資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<OrgProjectSummaryModel> GetOrgProjectSummary(DashBoardQueryModel queryModel)
            => await service.GetOrgProjectSummary(queryModel);


        /// <summary>
        /// 取得重大工程進度資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<DABTownModel> GetDashBoardEngProgress(DashBoardQueryModel queryModel)
            => await service.GetDashBoardEngProgress(queryModel);

        /// <summary>
        /// 件數及經費執行情形 - 頁面資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectTotalDataModel>> GetDashBoardCountAndBudget(ProjectTotalQueryModel queryModel)
            => await service.GetDashBoardCountAndBudget(queryModel);

        /// <summary>
        /// 取得歷年列管情形資料
        /// </summary>
        /// <param name="EXEC_ORGAN_C"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DABPastYearsModel>> GetPastYearsData([FromBody]string EXEC_ORGAN_C)
            => await service.GetPastYearsData(EXEC_ORGAN_C);

       
    }
}
