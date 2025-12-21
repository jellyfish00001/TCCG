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
    public class ProjectSituationController : Controller
    {
        private readonly IProjectSituationService service;

        public ProjectSituationController(IProjectSituationService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取得參採情形/結案成果填報結果
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ResSituationModel> GetRDResSituation([FromForm] string PLAN_NO)
        {
            return await service.GetRDResSituation(PLAN_NO);
        }

        /// <summary>
        /// 取得續列管一年內參採情形
        /// </summary>
        /// <param name="PLAN_NO">計畫編號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ResSituationModel> GetRDResSituaContinue([FromForm] string PLAN_NO)
        {
            return await service.GetRDResSituaContinue(PLAN_NO);
        }

        /// <summary>
        /// 儲存參採情形/結案成果填報結果
        /// </summary>
        /// <param name="model">結案成果填報 Model</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveRDResSituation(ResSituationModel model)
        {
            string msg = "存檔成功";
            // 若是送審，改 msg 文字
            if (model.AUDIT != null && model.AUDIT.IS_SEND == 1)
            {
                msg = "送審成功";
            }
            await service.SaveRDResSituation(model);
            return new RtnResultModel(true, msg);
        }

        /// <summary>
        /// 儲存續列管一年內參採情形
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveRDResSituaContinue(ResSituationModel model)
        {
            string msg = "存檔成功";
            // 若是送審，改 msg 文字
            if (model.AUDIT != null && model.AUDIT.IS_SEND == 1)
            {
                msg = "送審成功";
            }
            await service.SaveRDResSituaContinue(model);
            return new RtnResultModel(true, msg);
        }
    }
}
