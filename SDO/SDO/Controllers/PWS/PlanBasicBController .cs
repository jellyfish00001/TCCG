using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class PlanBasicBController : ControllerBase
    {
        private readonly IPlanBasicBService service;
        public PlanBasicBController(IPlanBasicBService service)
        {
            this.service = service;
        }

        /// <summary>
        /// (委託B)計畫資料存檔
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ObjectResultModel<object>> SavePWSSDPlanMain(PlanBasicBModel model)
        {
            PlanBasicBModel PlanData = await service.SavePlanBasicB(model);

            return new ObjectResultModel<object>() { success = true, message = PlanData.Message, data = PlanData };
        }

        /// <summary>
        /// 取基本計畫資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<PlanBasicBModel> GetPWSSDPlanMain([FromBody] string PROJECT_NO)
            => await service.GetPlanBasicB(PROJECT_NO);

    }
}
