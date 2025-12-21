using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "handUser")]
    public class ApiLogController : ControllerBase
    {
        private readonly IApiLogService apiLogService;

        public ApiLogController(IApiLogService apiLogService)
        {
            this.apiLogService = apiLogService;
        }

        /// <summary>
        /// 查詢Api日誌
        /// 參數有日期、查詢頁碼、顯示筆數
        /// </summary>
        /// <param name="model">[FromQuery]:參數從url傳遞</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IList<ApiTraceModel>> GetApiLog([FromQuery] GridBasicQryModel model)
        {
            return await apiLogService.GetApiLog(model);
        }
    }
}