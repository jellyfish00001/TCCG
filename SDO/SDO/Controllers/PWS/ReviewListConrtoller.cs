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
    public class ReviewListController : ControllerBase
    {
        private readonly IReviewListService service;
        public ReviewListController(IReviewListService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 查詢先期計畫資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ReviewListModel>> GetPWSReviewList(ReviewListQueryModel model)
            => await service.GetPWSReviewList(model);

        /// <summary>
        /// 退回先期計畫資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SetPWSProject(List<ReviewListModel> model)
        {
            await service.SetPWSProject(model);
            return new RtnResultModel(true, "退回成功");
        }

    }
}
