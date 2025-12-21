using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class MailLogController : ControllerBase
    {
        private readonly IMailLogService mailLogService;
        
        public MailLogController(IMailLogService mailLogService)
        {
            this.mailLogService = mailLogService;
        }

        /// <summary>
        /// 查詢MAIL日誌
        /// </summary>
        /// <param name="model">[FromQuery]:參數從url接</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IList<MailLogModel>> GetMailLog([FromQuery] GridBasicQryModel model)
        {
            return await mailLogService.GetMailLog(model);
        }
    }
}