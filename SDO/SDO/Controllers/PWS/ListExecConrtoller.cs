using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class ListExecController : ControllerBase
    {
        private readonly IListExecService service;
        public ListExecController(IListExecService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 查詢先期計畫資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public Task<List<ListExecModel>> GetPWSProjectList(ListExecQueryModel model)
            => service.GetPWSProjectList(model);

        /// <summary>
        /// 刪除計畫
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> DeleteProjectList([FromBody] List<string> PLANNO)
        {
            await service.DeleteProjectList(PLANNO);
            return new RtnResultModel(true, "刪除成功");
        }

        /// <summary>
        /// 查詢機關是否截止
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public Task<bool> GetOrgDeadline([FromBody] string OU_ID)
            => service.GetOrgDeadline(OU_ID);

    }
}
