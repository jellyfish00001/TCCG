using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class RDProjectManageController : Controller
    {
        private readonly IRDProjectManageService service;
        public RDProjectManageController(IRDProjectManageService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取得委託研究管理清單資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<RDProjectManageModel>> GetRDProjectManage(RDProjectManageQueryModel model)
        {
            return await service.GetRDProjectManage(model);
        }
    }
}
