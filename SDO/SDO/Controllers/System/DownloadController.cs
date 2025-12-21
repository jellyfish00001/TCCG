using Microsoft.AspNetCore.Mvc;
using SDO.Attributes;
using SDO.Models;
using SDO.Services;
using System.Threading.Tasks;

namespace SDO.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [AuthorizeLogin]
    public class DownloadController : ControllerBase
    {
        private readonly IDownloadService service;


        public DownloadController(IDownloadService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 下載範本檔
        /// </summary>
        /// <param name="tempFileName">範本檔名(含附檔名)</param>
        /// <param name="downFileName">下載結果檔名(不含附檔名)</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> DownTemplateFile ([FromForm]string tempFileName, [FromForm] string downFileName)
        {
            var (bytes, fileName, contentType) = await service.GetTemplateFile(tempFileName, downFileName);

            if (bytes == null)
            {
                return null;
            }
            return File(bytes, contentType,fileName);
        }

    }
}