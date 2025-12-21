using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.ReportBuilder.Models;
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
    public class ProjectListController : ControllerBase
    {
        private IProjectListService service;

        public ProjectListController(IProjectListService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取得計畫列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectListModel>> GetProjectList(ProjectListQueryModel model)
            => await service.GetProjectList(model);

        /// <summary>
        /// 更新計畫釘選狀態
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="PIS_SELECT">釘選</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel AddDelFavoriateProject([FromForm] string PROJECT_NO, [FromForm] bool PIS_SELECT)
            => service.AddDelFavoriateProject(PROJECT_NO, PIS_SELECT);

        /// <summary>
        /// 刪除計畫
        /// </summary>
        /// <param name="projectNos"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectCanceled([FromBody] List<string> projectNos)
            => service.SaveProjectCanceled(projectNos);


        /// <summary>
        /// 取得計畫異動紀錄清單
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectLogListModel>> GetProjectLogList([FromBody] string PROJECT_NO)
            => await service.GetProjectLogList(PROJECT_NO);
    }
}
