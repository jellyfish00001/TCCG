using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SDO.Services;
using System.Collections.Generic;

namespace SDO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "handUser")]
    public class OfficialDocController : ControllerBase
    {
        private readonly IOfficialDocService officialDocService;

        public OfficialDocController(IOfficialDocService officialDocService)
        {
            this.officialDocService = officialDocService;
        }

        /// <summary>
        /// 公文編輯器 匯出PDF
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public ActionResult ExportHTMLToPDF(IDictionary<string, string> param)
        {
            if (param.TryGetValue("offDoc", out string offDoc))
                return File(officialDocService.ExportHTMLToPDF(offDoc), "application/pdf", "OffPdf.pdf");
            else
                return BadRequest();
        }

        /// <summary>
        /// 公文編輯器 匯出WORD
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public ActionResult ExportHTMLToWord(IDictionary<string, string> param)
        {
            if (param.TryGetValue("offDoc", out string offDoc))
                return File(officialDocService.ExportHTMLToWord(offDoc), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "OffDoc.docx");
            else
                return BadRequest();
        }
    }
}