using Aspose.Words;
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
    public class PWSRPTController : ControllerBase
    {
        private IPWSRPTService rptService;

        public PWSRPTController(IPWSRPTService rptService)
        {
            this.rptService = rptService;
        }

        /// <summary>
        /// 回傳Word檔案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private ActionResult RtnFile(RtnRptModel model)
        {
            return File(model.Bytes, model.Mime, model.OutputName);
        }

        /// <summary>
        /// 取審查結果彙整表
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> RPTPWSReport(PWSReportModel model)
        {
            return RtnFile(await rptService.RPTPWSReport(model));
        }

    }
}
