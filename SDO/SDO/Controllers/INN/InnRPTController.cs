using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class InnRPTController : ControllerBase
    {
        private IInnRPTService rptService;

        public InnRPTController(IInnRPTService rptService)
        {
            this.rptService = rptService;
        }

        private ActionResult RtnFile(RtnRptModel model) => File(model.Bytes, model.Mime, model.OutputName);

        /// <summary>
        /// 取得各年度提案資料清冊
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> RPTInnStatistics(InnStatisticsModel model)
        {
            return RtnFile(await rptService.RPTInnStatistics(model));
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> ProjectPrint(InnProjectPrintQueryModel model)
        {
            return RtnFile(await rptService.ProjectPrint(model));
        }

    }
}
