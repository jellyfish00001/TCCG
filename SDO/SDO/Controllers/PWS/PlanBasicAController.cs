using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet.Messages.Authentication;
using SDO.Models;
using SDO.Services;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class PlanBasicAController : ControllerBase
    {
        private readonly IPlanBasicAService service;
        public PlanBasicAController(IPlanBasicAService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 基本計畫資料存檔
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ObjectResultModel<object>> SavePWSSDPlanMain(PlanBasicAModel model)
        {
            PlanBasicAModel PlanData = await service.SavePlanBasicA(model);
            return new ObjectResultModel<object>() { success = true, message = PlanData.Message, data = PlanData };
        }

        /// <summary>
        /// 取基本計畫資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public Task<PlanBasicAModel> GetPWSSDPlanMain([FromBody] string PROJECT_NO)
            => service.GetPlanBasicA(PROJECT_NO);

        /// <summary>
        /// 複製計畫
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> CopyProject([FromBody] string PLANNO)
        {
            object PlanNo = await service.copyProject(PLANNO);
            return new ObjectResultModel<object>() { success = true, message = "複製成功", data = PlanNo };
        }

    }
}
