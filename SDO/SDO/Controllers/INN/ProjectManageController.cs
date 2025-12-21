using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.APP.INN.Models.ProjectManage;
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
    public class ProjectManageController : ControllerBase
    {
        private IProjectManageService service;
        public ProjectManageController(IProjectManageService service)
        {

            this.service = service;
        }

        /// <summary>
        /// 取得提案清單資料
        /// </summary>
        /// <param name="INN_YEAR"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectManageModel>> GetInnProjectManage(ProjectManageQueryModel model)
        {
            return await service.GetInnProjectManage(model);
        }


    }
}
