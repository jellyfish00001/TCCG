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
    public class SCApplicationController : ControllerBase
    {
        private ISCApplicationService scApplicationService;

        public SCApplicationController(ISCApplicationService scApplicationService)
        {
            this.scApplicationService = scApplicationService;
        }

        /// <summary>
        /// 取得Sc連結
        /// </summary>
        /// <param name="domainName">網域名稱</param>
        /// <param name="apId">系統代號</param>
        /// <returns></returns>
        [HttpGet("[action]/{domainName}/{apId}")]
        public async Task<RtnResultModel> GetScLink(string domainName, string apId)
        {
            return await scApplicationService.GetScLink(domainName, apId);
        }

        /// <summary>
        /// 取得 SC 資訊
        /// </summary>
        /// <param name="orgId">組織ID</param>
        /// <returns></returns>
        [HttpGet("[action]/{orgId}")]
        public async Task<SCApplicationModel> GetScData(string orgId)
        {
            return await scApplicationService.GetScData(orgId);
        }
    }
}
