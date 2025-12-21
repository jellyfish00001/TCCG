using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class ThreeYearPlanController : ControllerBase
    {
        private readonly IThreeYearPlanService service;
        public ThreeYearPlanController(IThreeYearPlanService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 編輯近三年相關研究
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveThreeYearPlan(ThreeYearPlanIDModel models)
        {
            await service.SaveThreeYearPlan(models);
            return new RtnResultModel(true, "存檔成功");
        }

        /// <summary>
        /// 取近三年相關研究
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public Task<List<ThreeYearPlanModel>> GetThreeYearPlan([FromBody] string PLANNO )
            => service.GetThreeYearPlan(PLANNO);

    }
}
