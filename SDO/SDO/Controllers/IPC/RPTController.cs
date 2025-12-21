using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using SDO.ReportBuilder.Models;
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
    public class RPTController : ControllerBase
    {
        private IRPTService rptService;

        public RPTController(IRPTService rptService)
        {
            this.rptService = rptService;
        }

        private ActionResult RtnFile(RtnRptModel model) => File(model.Bytes, model.Mime, model.OutputName);

        /// <summary>
        /// 匯出Grid共用
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> ExportGrid(ExportGridModel gridData)
        {
            return RtnFile(await rptService.ExportGrid(gridData));
        }

        /// <summary>
        /// Demo 匯出 word
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> ExportWord() => RtnFile(await rptService.ExportWord());

        /// <summary>
        /// 重大建設計畫B級管制案件機關統計表 TODO
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> RPTProjectDeptDetailed()
        {
            return RtnFile(await rptService.RPTProjectDeptDetailed());
        }

        /// <summary>
        /// 匯出調整期程申請表
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> RPTAdjustSchedule([FromForm] string PROJECT_NO, [FromForm] int PROJ_ADJ_ID)
        {
            return RtnFile(await rptService.RPTAdjustSchedule(PROJECT_NO, PROJ_ADJ_ID));
        }

        /// <summary>
        /// 產出計畫年終考核評分表
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> RPTProjectFillYearAss([FromBody] string PROJECT_NO)
        {
            return RtnFile(await rptService.RPTProjectFillYearAss(PROJECT_NO));
        }

        /// <summary>
        /// 計畫預覽列印-下載報表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> RPTProjectPrint(ProjectPrintQueryModel model)
        {
            return RtnFile(await rptService.ProjectPrint(model));
        }

        /// <summary>
        /// 統計報表
        /// </summary>
        /// <param name="model">統計報表model</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> RPTStatistics(StatisticsModel model)
        {
            return RtnFile(await rptService.RPTStatistics(model));
        }

        /// <summary>
        /// 取得屬於工程類的計畫 (用於統計報表 表10:選項列管案件計畫歷次調整審查表(簡表))
        /// </summary>
        /// <returns>屬於工程類的計畫清單(PROJECT_NO: 計畫編號、PROJ_ADJ_ID: 最近一次的調整流水號)</returns>
        [HttpPost("[action]")]
        public async Task<List<object>> GetEngineeringProjects()
        {
            return await rptService.GetEngineeringProjects();
        }

        /// <summary>
        /// 計畫調整內容比對結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> RPTProjectAdjustDiff(ProjectPrintQueryModel model)
        {
            return RtnFile(await rptService.ProjectAdjustDiff(model));
        }

        /// <summary>
        /// 綜合查詢
        /// </summary>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> RPTUnitingQuery(ExportGridModel gridData)
        {
            return RtnFile(await rptService.RPTUnitingQuery(gridData));
        }

        /// <summary>
        /// 檢核點歷程
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> ProjectScheduleOverview(ProjectScheOverviewQueryModel model)
        {
            return RtnFile(await rptService.ProjectScheduleOverview(model));
        }
    }
}
