using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.APP.INN.Models.ProjectManage;
using SDO.Attributes;
using SDO.Models;
using SDO.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class InnProjectController : ControllerBase
    {
        private readonly IInnProjectService service;

        public InnProjectController(IInnProjectService service)
        {
            this.service = service;
        }


        #region 提案基本資料
        /// <summary>
        /// 取得提案基本資料
        /// </summary>
        /// <param name="PROJECT_NO">提案編號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<InnProjectBasicFillModel> GetInnBasic([FromBody] string PROJECT_NO)
        {
            return await service.GetInnBasic(PROJECT_NO);
        }

        /// <summary>
        /// 儲存創新提案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveInnBasic(InnProjectBasicFillModel model)
        {
            string INN_PLAN_NO = await service.SaveInnBasic(model);
            return new ObjectResultModel<string> { success = true, message = "存檔成功", data = INN_PLAN_NO };
        }
        #endregion

    }
}
