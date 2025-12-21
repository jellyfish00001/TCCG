using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.Services;
using SDO.Utils;
using SDO.ReportBuilder.Interface;
using SDO.ReportBuilder.Models;
namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportWordReportController : ControllerBase
    {
        private readonly IReportFactory ReportFac;
        private readonly IExportWordReportService exportWordReportService;
        private readonly IUserProfile userProfile;
        public ExportWordReportController(IReportFactory ReportFac, IExportWordReportService exportWordReportService, IUserProfile userProfile)
        {
            this.ReportFac = ReportFac;
            this.exportWordReportService = exportWordReportService;
            this.userProfile = userProfile;
        }

        [HttpPost]
        public async Task<ActionResult> ExportWord([FromForm] string saveFormat)
        {
            //DemoParameter parameter = new DemoParameter
            //{
            //    FileName = "demo",
            //    ReportId = "DemoReport" ,
            //    Extension = "docx",
            //    USER_ID = userProfile.GetLoginUser().USER_ID ,
            //    USER_NAME = userProfile.GetLoginUser().USER_NAME
            //};
            //var report = await ReportFac.CreateRPT(parameter);
            //return File(report.bytes, report.mime, report.outputName);
            return null;
        }
        [HttpPost("[action]")]
        public async Task<ActionResult> ExportPdf([FromForm] string saveFormat)
        {
            //DemoParameter parameter = new DemoParameter
            //{
            //    FileName = "demo",
            //    ReportId = "DemoReport" ,
            //    Extension = "pdf",
            //    USER_ID = userProfile.GetLoginUser().USER_ID ,
            //    USER_NAME = userProfile.GetLoginUser().USER_NAME
            //};
            //var report = await ReportFac.MergePDF(parameter);
            //return File(report.bytes, report.mime, report.outputName);
            return null;
        }
        [HttpGet]
        public async Task<ActionResult> DownloadDescriptionFile()
        {
            (byte[] bytes, string fileName, string contentType) = await exportWordReportService.DownloadDescriptionFile();
            return File(bytes, contentType, fileName);
        }
    }
}
