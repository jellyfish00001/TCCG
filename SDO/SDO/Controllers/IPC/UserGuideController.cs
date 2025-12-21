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
    public class UserGuideController : ControllerBase
    {
        private readonly IUserGuideService service;

        public UserGuideController(IUserGuideService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取得操作手冊清單
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public List<UserGuideModel> GetUserGuides()
        {
            return service.GetUserGuides();
        }

        /// <summary>
        /// 下載操作手冊
        /// </summary>
        /// <param name="groupName">群組名稱</param>
        /// <param name="fileName">檔案名稱</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public ActionResult DownloadFile([FromForm] string groupName, [FromForm] string fileName)
        {
            var result = service.DownloadFile(groupName, fileName);
            if (result.bytes == null)
            {
                return null;
            }
            return File(result.bytes, result.contentType, result.fileName);
        }
    }
}
