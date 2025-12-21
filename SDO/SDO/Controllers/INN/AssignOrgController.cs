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
    public class AssignOrgController : ControllerBase
    {
        private IAssignOrgService service;
        public AssignOrgController(IAssignOrgService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 創新提案局處截止時間
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<AssignOrgModel> GetInnAssignOrg([FromBody] string INN_YEAR)
        {
            return await service.GetInnAssignOrg(INN_YEAR);
        }

        /// <summary>
        /// 儲存創新提案局處截止時間
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveInnAssignOrg(AssignOrgModel model)
        {
            await service.SaveInnAssignOrg(model);

            return new RtnResultModel(true, "存檔成功");
        }

    }
}
