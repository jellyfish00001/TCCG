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
    public class RDProjectAuditController : Controller
    {
        private readonly IRDProjectAuditService service;
        public RDProjectAuditController(IRDProjectAuditService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取得審核紀錄清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<RDAuditModel>> GetRDAuditList([FromForm] string MAIN_NO)
        {
            return await service.GetRDAuditList(MAIN_NO);
        }

        /// <summary>
        /// 儲存審核紀錄
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveRDAudit(RDAuditModel model)
        {
            string msg = "存檔成功";
            // 若是送審，改 msg 文字
            if (model != null && model.IS_SEND == 1)
            {
                msg = "審核成功";
            }
            await service.SaveRDAudit(model);
            return new RtnResultModel(true, msg);
        }
    }
}
