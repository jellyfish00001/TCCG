using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Utils;
using SDO.Services;
using SDO.ReportBuilder.Models;
using SDO.ReportBuilder.Interface;
using System.Threading.Tasks;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "handUser")]
    public class AsposeExcelSetController : ControllerBase
    {
        private readonly IAsposeExcelSetService asposeExcelSetService;
        private readonly IUserProfile userProfile;
        public AsposeExcelSetController(IAsposeExcelSetService asposeExcelSetService,IReportFactory ReportFac, IUserProfile userProfile)
        {
            this.asposeExcelSetService = asposeExcelSetService;
            this.userProfile = userProfile;
        }
        
        /// <summary>
        /// 取得範例檔案
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<FileContentResult> GetSampleFile()
        {
            return File(await asposeExcelSetService.GetSampleFile(), "application/msword", "SampleReport.xls");
        }

        /// <summary>
        /// 動態生成欄位 (自動長欄位)
        /// </summary>
        /// <param name="saveFormat"></param>
        /// <returns></returns>
        [HttpGet("[action]/{saveFormat}")]
        public async Task<FileContentResult> ReportAuto(string saveFormat)
        {
            var report = await asposeExcelSetService.ReportAutoRPT(saveFormat);
            return File(report.Bytes, report.Mime, report.OutputName);
        }

        /// <summary>
        /// 動態生成欄位 (欄位預設數量)
        /// </summary>
        /// <param name="saveFormat"></param>
        /// <returns></returns>
        [HttpGet("[action]/{saveFormat}")]
        public async Task<FileContentResult> ReportDefault(string saveFormat)
        {
            return File(await asposeExcelSetService.ReportDefault(saveFormat), asposeExcelSetService.SetContentType(saveFormat), "ExcelExportExample." + saveFormat);
        }

        /// <summary>
        /// 生成單一頁面
        /// </summary>
        /// <param name="saveFormat"></param>
        /// <returns></returns>
        [HttpGet("[action]/{saveFormat}")]
        public async Task<FileContentResult> ReportSingle(string saveFormat)
        {
            return File(await asposeExcelSetService.ReportSingle(saveFormat), asposeExcelSetService.SetContentType(saveFormat), "ExcelExportExample." + saveFormat);
        }
    }
}