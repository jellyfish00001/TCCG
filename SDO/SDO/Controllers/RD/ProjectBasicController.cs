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
    public class ProjectBasicController : Controller
    {
        private readonly IProjectBasicService service;
        public ProjectBasicController(IProjectBasicService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 變更委託研究刪除與撤銷
        /// 執行類別 D 刪除 R 撤銷 L1 送出鎖定 L2 解除鎖定 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveRDBasicStatus(ProjectBasicStatusModel model)
        {
            string msg = "";
            switch(model.EXEC_KIND)
            {
                // 刪除
                case "D":
                    msg = "成功刪除計畫";
                    break;

                // 撤銷
                case "R":
                    msg = "成功撤銷計畫";
                    break;

                // 送出鎖定
                case "L1":
                    msg = "成功送出鎖定";
                    break;

                // 解除鎖定
                case "L2":
                    msg = "成功解除鎖定";
                    break;

                default:
                    break;
            }
            bool success = await service.SaveRDBasicStatus(model);
            return new RtnResultModel(success, msg);
        }

        /// <summary>
        /// 取得委託研究的基本資料
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ResearchBasicModel> GetRDResearchBasic([FromForm] string PLAN_NO)
        {
            return await service.GetRDResearchBasic(PLAN_NO);
        }

        /// <summary>
        /// 儲存委託研究基本資料（新增與編輯）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveRDResearchBasic(ResearchBasicModel model)
        {
            string msg = "存檔成功";
            // 若是送審，改 msg 文字
            if (model.AUDIT != null && model.AUDIT.IS_SEND == 1)
            {
                msg = "送審成功";
            }
            string planNo = await service.SaveRDResearchBasic(model);
            return new ObjectResultModel<string>(true) { message = msg, data = planNo };
        }
    }
}
