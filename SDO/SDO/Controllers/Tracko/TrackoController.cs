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
    [Route("api/[controller]")]
    [ApiController]
    public class TrackoController : ControllerBase
    {
        private readonly ITrackoService trackoService;

        public TrackoController(ITrackoService risToDoListService)
        {
            this.trackoService = risToDoListService;
        }

        /// <summary>
        /// 取得本日到期或待辦案件資訊
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnTrackoModel> GetTrackoData()
        {
            return await trackoService.GetTrackoData();
        }
    }
}
