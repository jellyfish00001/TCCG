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
    public class BudgetExecController : ControllerBase
    {
        private readonly IBudgetExecService service;
        public BudgetExecController(IBudgetExecService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 歷年執行情形存檔
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SavePWSSDHISTORYEXE(List<BudgetExecModel> models)
        {
            await service.SaveBudgetExec(models);
            return new RtnResultModel(true, "存檔成功");
        }

        /// <summary>
        /// 取得歷年執行情形
        /// </summary>
        /// <param name="planNo"></param>
        /// <param name="planYear"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public Task<List<BudgetExecModel>> GetPWSSDHISTORYEXE([FromForm] string planNo, [FromForm] int planYear)
            => service.GetBudgetExec(planNo, planYear);
    }
}
