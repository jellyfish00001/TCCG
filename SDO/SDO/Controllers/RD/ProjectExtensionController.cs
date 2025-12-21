using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SDO.Services;
using SDO.Models;


namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectExtensionController : Controller
    {
        private readonly IProjectExtensionService service;

        public ProjectExtensionController(IProjectExtensionService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取得展延紀錄清單
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectExtensionModel>> GetRDExtensionList([FromForm] string PLAN_NO)
        {
            return await service.GetExtensionList(PLAN_NO);
        }

        /// <summary>
        /// 取得展延紀錄明細
        /// </summary>
        /// <param name="EXTENSION_NO">展延編號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectExtensionModel> GetRDExtension([FromForm] string EXTENSION_NO)
        {
            return await service.GetRDExtension(EXTENSION_NO);
        }

        /// <summary>
        /// 儲存展延紀錄
        /// </summary>
        /// <param name="model">展延紀錄 Model</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveRDExtension(ProjectExtensionModel model)
        {
            string msg = "操作成功";
            // 若是送審，改 msg 文字
            if (model.AUDIT != null && model.AUDIT.IS_SEND == 1)
            {
                msg = "送審成功";
            }
            // 回傳展延編號(EXTENSION_NO)
            string extensionNo = await service.SaveRDExtension(model);
            return new ObjectResultModel<string>(true) { message = msg, data = extensionNo };
        }
    }
}
