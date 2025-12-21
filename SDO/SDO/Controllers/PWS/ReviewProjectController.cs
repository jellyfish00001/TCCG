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
    public class ReviewProjectController : ControllerBase
    {
        private readonly IReviewProjectService service;
        public ReviewProjectController(IReviewProjectService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取得審核資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ReviewProjectModel> GetReviewProject( [FromBody] string PLANNO )
            => await service.GetReviewProject(PLANNO);

        /// <summary>
        /// 審核作業
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SetReviewProject(ReviewProjectModel model)
        {
            await service.SetReviewProject(model);
            return new RtnResultModel(true, "存檔成功");
        }

        /// <summary>
        /// 取重大關聯資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<OtherProjectModel>> GetOtherProject()
          => await service.GetOtherProject();

        /// <summary>
        /// 退回計畫
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SendBackProject([FromBody] string PLANNO)
        {
            await service.SendBackProject(PLANNO);
            return new RtnResultModel(true, "已退回");
        }

    }
}
