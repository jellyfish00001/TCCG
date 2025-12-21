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
    public class DAMTBController : ControllerBase
    {
        private readonly IDAMTBService service;
        public DAMTBController(IDAMTBService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 經費需求細項
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SavePWSSDPLANFUND(DAMTBIDModel model)
        { 
            await service.SaveDAMTB(model);
            return new RtnResultModel(true, "存檔成功");
        }

        /// <summary>
        /// 取經費需求細項
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public Task<DAMTBIDModel> GetPWSSDPLANFUND([FromBody] string PLANNO)
            => service.GetDAMTB(PLANNO);

    }
}
