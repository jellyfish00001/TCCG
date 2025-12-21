using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SDO.LOG.Services;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "handUser")]
    public class ErrorLogController : ControllerBase
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public ErrorLogController(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// 查詢Api日誌
        /// 參數有日期、查詢頁碼、顯示筆數
        /// </summary>
        /// <param name="model">[FromQuery]:參數從url傳遞</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<string> GetErrorLog([FromQuery] DateTime LogDate)
        {
            LogService ls = new(webHostEnvironment);
            var log = ls.GetErrorLog(LogDate);
            return await Task.FromResult(log);
        }
    }
}