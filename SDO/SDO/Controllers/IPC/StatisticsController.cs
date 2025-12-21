using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService service;

        public StatisticsController(IStatisticsService service)
        {
            this.service = service;
        }

        #region 綜合查詢
        /// <summary>
        /// 取得自選欄位
        /// </summary>
        /// <param name="isRdec">檢查登入者是否有管考權限(管考角色)</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<OptionColumnModel>> GetOptionColumns([FromBody]bool isRdec)
        {
            return await service.GetOptionColumns(isRdec);
        }

        /// <summary>
        /// 取得綜合查詢結果
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<IDictionary<string, object>>> GetUnitingQuery(Object condition)
        {
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(condition.ToString());
            return await service.GetUnitingQuery(data);
        }
        #endregion
    }
}
