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
    public class MaintainYearController : ControllerBase
    {
        private readonly IMaintainYearService service;
        public MaintainYearController(IMaintainYearService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取得維護年度
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<MaintainYearModel>> GetPWSSDYearSet()
            => await service.GetMaintainYear();

        /// <summary>
        /// 更新維護年度
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SavePWSSDYearSet(List<MaintainYearModel> model)
        {
            await service.SetMaintainYear(model);
            return new RtnResultModel(true, "存檔成功");
        }

    }
}
