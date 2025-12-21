using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Threading.Tasks;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class SubmitController : ControllerBase
    {
        private readonly ISubmitService service;
        public SubmitController(ISubmitService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 執行情形檢核
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public Task<SubmitModel> CheckProjectFillSubmit([FromForm] string PROJECT_NO, [FromForm] int PLANKIND)
            => service.CheckProjectFillSubmit(PROJECT_NO, PLANKIND);

        /// <summary>
        /// 執行情形送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SavePlanFillAddSubmit([FromBody] string PROJECT_NO)
        { 
            await service.ProjectFillSubmit(PROJECT_NO);
            return new RtnResultModel(true, "送出成功");
        }
    }
}
