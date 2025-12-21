using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.APP.RD.Models.Report;
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
    public class RDRPTController : ControllerBase
    {
        private IRDRPTService rdRPTService;

        public RDRPTController(IRDRPTService rptService)
        {
            this.rdRPTService = rptService;
        }

        private ActionResult RtnFile(RtnRptModel model) => File(model.Bytes, model.Mime, model.OutputName);

        /// <summary>
        /// 報表列印
        /// </summary>
        /// <param name="model">報表列印 model</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> RDReport(ReportQueryModel model)
        {
            return RtnFile(await rdRPTService.RDReport(model));
        }
    }
}
