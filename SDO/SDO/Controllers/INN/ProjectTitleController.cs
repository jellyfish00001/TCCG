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
    public class ProjectTitleController : ControllerBase
    {
        private IProjectTitleService service;
        public ProjectTitleController(IProjectTitleService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取得維護專題
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<MaintainInnProjectTitleModel> GetInnProjectTitle([FromBody] string INN_YEAR)
        {
            return await service.GetMaintainInnProjectTitle(INN_YEAR);
        }

        /// <summary>
        /// 儲存維護專題
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SaveInnProjectTitle(MaintainInnProjectTitleModel model)
        {
            await service.SaveInnProjectTitle(model);

            return new RtnResultModel(true, "存檔成功");
        }
    }
}
