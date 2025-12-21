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
    public class ProjectExecutionController : Controller
    {
        private readonly IProjectExecutionService service;
        public ProjectExecutionController(IProjectExecutionService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取得執行情形填報清單
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ResPolicyListQueryModel>> GetRDResPolicyList(ResPolicyListQueryModel model)
        {
            // 取得執行情形填報清單
            return await service.GetRDResPolicyList(model);
        }

        /// <summary>
        /// 取得執行情形填報明細
        /// </summary>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ResPolicyIndexModel> GetRDResPolicyIndex([FromForm] int SEQ)
        {
            // 取得執行情形填報明細
            return await service.GetRDResPolicyIndex(SEQ);
        }

        /// <summary>
        /// 儲存執行情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveRDResPolicyIndex(ResPolicyIndexModel model)
        {
            string msg = "存檔成功";
            // 若是送審，改 msg 文字
            if (model.AUDIT != null && model.AUDIT.IS_SEND == 1)
            {
                msg = "送審成功";
            }
            // 儲存執行情形
            await service.SaveRDResPolicyIndex(model);
            return new RtnResultModel(true, msg);
        }
    }
}
