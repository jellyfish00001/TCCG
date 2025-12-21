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
    public class FundingExecutionController : ControllerBase
    {
        private readonly IFundingExecutionService service;
        public FundingExecutionController(IFundingExecutionService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 經費需求細項和執行情形
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveFundingExecution(FundingExecutionModel model)
        { 
            await service.SaveFundingExecution(model);
            return new RtnResultModel(true, "存檔成功");
        }

        /// <summary>
        /// 取經費需求細項和執行情形
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public Task<FundingExecutionModel> GetFundingExecution([FromForm] string planNo, [FromForm] int planYear)
            => service.GetFundingExecution(planNo, planYear);

    }
}
