using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Base.Utils.Models;
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
    public class ProjectCommonController : ControllerBase
    {
        private readonly IProjectCommonService service;
        public ProjectCommonController(IProjectCommonService service)
        {
            this.service = service;
        }

        #region 相關檔案上傳
        /// <summary>
        /// 取得計畫檔案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="FILE_UP_SOURCE">01:相關檔案上傳、02:其他地方上傳</param>
        /// <param name="FILE_KIND">SET_PARAM.SET_ITEM ='FILE_KIND'</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectAttachmentModel>> GetProjectAttachment(UpLoadModel model)
        {
            return await service.GetProjectAttachment(model);
        }

        /// <summary>
        /// 取得非相關檔案上傳的檔案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectFillFileUpModel> GetProjectOtherAttachmentList([FromBody] string PROJECT_NO)
        {
            return await service.GetProjectOtherAttachmentList(PROJECT_NO);
        }

        /// <summary>
        /// 儲存計畫檔案資料
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectAttachment(List<ProjectAttachmentModel> models)
        {
            return service.SaveProjectAttachment(models);
        }

        /// <summary>
        /// 下載相關檔案
        /// </summary>
        /// <param name="IDENTITY_FIELD"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> DownProjectAttachment([FromForm] int IDENTITY_FIELD, [FromForm] int DBKEY)
        {
            var result = await service.DownProjectAttachment(IDENTITY_FIELD, DBKEY);
            if (result.bytes == null)
            {
                return null;
            }
            return File(result.bytes, result.contentType, result.fileName);
        }

        /// <summary>
        /// 下載附件壓縮檔
        /// </summary>
        /// <param name="IDENTITY_FIELDs"></param>
        /// <param name="title">壓縮檔檔名</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> DownProjectAttachmentZip([FromForm] List<int> IDENTITY_FIELDs, [FromForm] string title)
        {
            var result = await service.DownProjectAttachmentZip(IDENTITY_FIELDs, title);
            if (result.bytes == null)
            {
                return null;
            }
            return File(result.bytes, result.contentType, result.fileName);
        }
        #endregion

        #region 參考資料
        /// <summary>
        /// 取得參考資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectAttachmentModel>> GetRefFile()
        {
            return await service.GetRefFile();
        }

        /// <summary>
        /// 儲存參考資料
        /// </summary>
        /// <param name="EditFiles"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveRefFile(List<UploadTempFileModel> EditFiles)
        {
            return service.SaveRefFile(EditFiles);
        }
        #endregion

        /// <summary>
        /// 取得當期填報周期資料
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<ProjectFillCycleModel> GetCurrentCycleData() 
            =>await service.GetCurrentCycleData();

        /// <summary>
        /// 取得已使用的代碼清單
        /// </summary>
        /// <param name="SET_ITEM">代碼類別</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<string>> GetUsedCode([FromBody] string SET_ITEM)
            => await service.GetUsedCode(SET_ITEM);

        /// <summary>
        /// 取得計畫落後原因的落後項目代碼清單
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<string>> GetDelayClasses()
            => await service.GetDelayClasses();

        /// <summary>
        /// 取得計畫落後原因的落後項目清單
        /// </summary>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<string>> GetDelaySubClasses()
            => await service.GetDelaySubClasses();

        /// <summary>
        /// 取得計畫經費來源的預算編號清單
        /// </summary>
        /// <param name="LEVEL_MARK">預算來源類別</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<string>> GetPlanItems([FromForm] string LEVEL_MARK)
            => await service.GetPlanItems(LEVEL_MARK);
    }
}
