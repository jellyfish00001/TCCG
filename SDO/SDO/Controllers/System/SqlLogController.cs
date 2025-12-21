using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "handUser")]
    public class SqlLogController : ControllerBase
    {
        private readonly ISqlLogService sqlTraceService;

        public SqlLogController(ISqlLogService sqlTraceService)
        {
            this.sqlTraceService = sqlTraceService;
        }

        /// <summary>
        /// 查詢SQL日誌
        /// 參數有日期、查詢頁碼、顯示筆數
        /// </summary>
        /// <param name="model">[FromQuery]:參數從url傳遞</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IList<SqlTraceGridModel>> GetSqlLog([FromQuery] GridBasicQryModel model)
        {
            return await sqlTraceService.ReadSqlLog(model);
        }
    }
}