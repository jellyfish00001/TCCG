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
    public class ImportProjectController : ControllerBase
    {
        private readonly IImportProjectService service;
        
        public ImportProjectController(IImportProjectService service)
        {
            this.service = service;
        }
        /// <summary>
        /// 取得先期計畫資料列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<PWSSDPlanGridModel>> QueryPWSSDPlanList(ImportProjectQueryModel model)
            => await service.QueryPWSSDPlanList(model);

        /// <summary>
        /// 匯入計畫基本資訊
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel ImportGeneralProject([FromForm]string PlanYear, [FromForm] IFormFile ImportFile)
            => service.ImportGeneralProject(PlanYear,ImportFile);

        /// <summary>
        /// 匯入先期計畫資訊
        /// </summary>
        /// <param name="PlanIds"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel ImportPWSSDPlans(List<int> PlanIds)
            => service.ImportPWSSDPlans(PlanIds);
    }
}
