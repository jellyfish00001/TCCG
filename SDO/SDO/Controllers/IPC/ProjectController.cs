using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDO.Attributes;
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
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService service;
        private readonly IProjectPrintService projectPrintService;
        private readonly IProjectChapterService projectChapterService;
        public ProjectController(IProjectService service, IProjectPrintService projectPrintService, IProjectChapterService projectChapterService)
        {
            this.service = service;
            this.projectPrintService = projectPrintService;
            this.projectChapterService = projectChapterService;
        }

        #region 計畫章節
        /// <summary>
        /// 取得計畫章節表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectChapterListModel>> GetProjectChapter(ProjectChapterQueryModel model)
        {
            return await projectChapterService.GetProjectChapter(model);
        }
        #endregion

        #region 計劃基本資料
        /// <summary>
        /// 取得計畫基本資料(含計畫基本資料、計畫經費來源、計畫建設類別、計畫協辦機關)
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectBasicFillModel> GetProjectBasicFill([FromBody] string PROJECT_NO)
        {
            return await service.GetProjectBasicFill(PROJECT_NO);
        }

        /// <summary>
        /// 儲存計畫基本資料(含計畫基本資料、計畫經費來源、計畫建設類別、計畫協辦機關)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectBasicAdd(ProjectBasicFillModel model)
        {
            return service.SaveProjectBasicAdd(model);
        }
        #endregion

        #region 計劃檢核點設定
        /// <summary>
        /// 取得計劃檢核點設定
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectCheckpointModel> GetProjectCheckpoint([FromBody] string PROJECT_NO)
            => await service.GetProjectCheckpoint(PROJECT_NO);

        /// <summary>
        /// 儲存計劃檢核點設定
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectCheckpoint(ProjectCheckpointModel model)
            => service.SaveProjectCheckpoint(model);
        #endregion

        #region 計劃送審
        /// <summary>
        /// 驗證計劃送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectSubmitResultModel> CheckProjectCanSubmit([FromBody] string PROJECT_NO)
            => await service.CheckProjectCanSubmit(PROJECT_NO);


        /// <summary>
        ///  儲存計劃立案送審
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectFillAddSubmit([FromBody] string PROJECT_NO)
            => service.SaveProjectFillAddSubmit(PROJECT_NO);
        #endregion

        #region 計劃審核
        /// <summary>
        /// 取得計劃審核資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectFillAddAuditModel> GetProjectFillAddAudit([FromBody] string PROJECT_NO)
            => await service.GetProjectFillAddAudit(PROJECT_NO);

        /// <summary>
        /// 儲存計劃審核資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectFillAddAudit(ProjectFillAddAuditModel model)
            => service.SaveProjectFillAddAudit(model);
        #endregion

        /// <summary>
        /// 取得計畫狀態
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<string> GetProjectStatus([FromBody] string PROJECT_NO)
        {
            return await service.GetProjectStatus(PROJECT_NO);
        }

        /// <summary>
        /// 取得計畫預覽資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectPrintModel> GetProjectPrint([FromForm] string PROJECT_NO, [FromForm] string type)
        {
            return await projectPrintService.GetProjectPrint(PROJECT_NO, type);
        }

        /// <summary>
        /// 取得是否使用國發會界接資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<bool> GetIsUserFtyData([FromBody] string PROJECT_NO)
        {
            return await service.GetIsUserFtyData(PROJECT_NO);
        }
    }
}
