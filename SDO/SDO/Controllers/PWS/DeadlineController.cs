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
    public class DeadlineController : ControllerBase
    {
        private readonly IDeadlineService service;
        public DeadlineController(IDeadlineService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取年度下拉選單
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<DeadlineModel>> GetPlanYear()
            => await service.GetPlanYear();

        /// <summary>
        /// 取年度截止日期
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<DeadlineModel> GetPWSSDAssignment([FromBody] string year)
            => await service.GetPlanDeadline(year);

        /// <summary>
        /// 更新截止日期
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SavePWSSDAssignment(DeadlineModel model)
        {
            await service.SetDeadLine(model);
            return new RtnResultModel(true, "存檔成功");
        }

    }
}
