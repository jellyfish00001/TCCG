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
    public class ProjectClosedController : ControllerBase
    {
        private IProjectClosedService service;

        public ProjectClosedController(IProjectClosedService service)
        {
            this.service = service;
        }


        /// <summary>
        /// 取得計畫結案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectFillCloseModel> GetProjectFillClose([FromBody] string PROJECT_NO)
             => await service.GetProjectFillClose(PROJECT_NO);

        /// <summary>
        /// 儲存計畫結案資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectFillClose(ProjectFillCloseModel model)
            => service.SaveProjectFillClose(model);


    }
}
