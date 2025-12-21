using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Services;

namespace SDO.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class SystemInfoController : ControllerBase
    {
        private readonly ISystemInfoService systemInfoService;

        public SystemInfoController(ISystemInfoService systemInfoService)
        {
            this.systemInfoService = systemInfoService;
        }

        /// <summary>
        /// 查詢版本號
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> GetVersionInfo()
        {
            
            return await systemInfoService.GetVersionInfo();
        }


        /// <summary>
        /// 快取保存時間
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public string GetCacheExpireTime()
        {
            return systemInfoService.GetCacheExpireTime();
        }
    }
}
