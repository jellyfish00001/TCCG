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
    public class AdjustNumberController : ControllerBase
    {
        private readonly IAdjustNumberService service;
        public AdjustNumberController(IAdjustNumberService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取計畫資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<AdjustNumberModel>> GetProjectList(AdjustNumberQueryModel model)
            => await service.GetAdjustNumber(model);

        /// <summary>
        /// 計畫資料優先順序送審
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SetAdjustNumber(AdjustNumberQueryModel model)
        {
            string Message = await service.SetAdjustNumber(model);
            return new RtnResultModel(true, Message);
        }

    }
}
