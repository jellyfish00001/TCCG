using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.APP.IPC.Models.ProjectAdjust;
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
    public class ProjectAdjustController : ControllerBase
    {
        private IProjectAdjustService service;

        public ProjectAdjustController(IProjectAdjustService service)
        {
            this.service = service;
        }

        /// <summary>
        /// 取得計畫調整撤銷清單
        /// </summary>
        /// <param name="model">篩選條件</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectAdjustListModel>> GetAdjustList(ProjectAdjustListQueryModel model)
        {
            return await service.GetAdjustList(model);
        } 

        /// <summary>
        /// 主辦取消調整
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <param name="AW_KIND">調整項目</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveExecCancel([FromForm] string PROJECT_NO, [FromForm] int PROJ_ADJ_ID, [FromForm] string AW_KIND)
        {
            return service.SaveExecCancel(PROJECT_NO, PROJ_ADJ_ID, AW_KIND);
        }

        /// <summary>
        /// 主辦申請調整撤銷原因
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveExecReason(AdjustReasonModel model)
        {
            return service.SaveExecReason(model);
        }

        /// <summary>
        /// 新增主辦申請調整計畫
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="AW_KIND">調整申請項目</param>
        /// <returns>調整檔流水號</returns>
        [HttpPost("[action]")]
        public int AddAdujustExec([FromForm] string PROJECT_NO, [FromForm] string AW_KIND)
        {
            return service.AddAdujustExec(PROJECT_NO, AW_KIND);
        }

        /// <summary>
        /// 取得主辦申請調整撤銷原因(for 調整基本資料、撤銷)
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <param name="AW_KIND">申請項目</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<AdjustReasonViewModel> GetExecReasonBasic([FromForm] int PROJ_ADJ_ID, [FromForm] string AW_KIND)
        {
            return await service.GetExecReasonBasic(PROJ_ADJ_ID, AW_KIND);
        }

        /// <summary>
        /// 取得調整計畫基本資料(含計畫基本資料調整、計畫經費來源調整、計畫建設類別調整、計畫協辦機關調整)
        /// </summary>
		/// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectBasicFillAdjustModel> GetProjectBasicAdj([FromForm] string PROJECT_NO, [FromForm] int PROJ_ADJ_ID)
        {
            return await service.GetProjectBasicAdj(PROJECT_NO, PROJ_ADJ_ID);
        }

        /// <summary>
        /// 儲存調整計畫基本資料(含計畫基本資料調整、計畫經費來源調整、計畫建設類別調整、計畫協辦機關調整)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SetProjectBasicAdj(ProjectBasicFillAdjustModel model)
        {
            return service.SetProjectBasicAdj(model);
        }

        /// <summary>
        /// 取得主辦申請調整期程原因
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<AdjustReasonScheduleViewModel> GetExecReasonSchedule([FromForm] int PROJ_ADJ_ID)
        {
            return await service.GetExecReasonSchedule(PROJ_ADJ_ID);
        }

        /// <summary>
        /// 取得檢核點調整
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水編號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<AdjustCheckPointModel> GetAdjustCheckPoint([FromBody] int PROJ_ADJ_ID)
        {
            return await service.GetAdjustCheckPoint(PROJ_ADJ_ID);
        }

        /// <summary>
        /// 儲存計劃檢核點設定
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SetAdjustCheckPoint(AdjustCheckPointModel model)
        {
            return service.SetAdjustCheckPoint(model);
        }

        /// <summary>
        /// 取得主辦調整檢核結果
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水編號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> GetAdjustChk([FromForm] string PROJECT_NO, [FromForm] int PROJ_ADJ_ID)
        {
            return await service.GetAdjustChk(PROJECT_NO, PROJ_ADJ_ID);
        }

        /// <summary>
        /// 主辦上傳准簽、已核章申請表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveScheAttach(AdjustReasonModel model)
        {
            return service.SaveScheAttach(model);
        }

        /// <summary>
        /// 主辦調整送審
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SendExecAdjust(AdjustReasonModel model)
        {
            return service.SendExecAdjust(model);
        }

        /// <summary>
        /// 管考取得主辦調整撤銷原因
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<AdjustAuditModel> GetExecReasonByAudit([FromForm] int PROJ_ADJ_ID, [FromForm] string AW_KIND)
        {
            return await service.GetExecReasonByAudit(PROJ_ADJ_ID, AW_KIND);
        }

        /// <summary>
        /// 儲存管考審核調整撤銷結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveAuditReview(AdjustAuditModel model)
        {
            return service.SaveAuditReview(model);
        }

        /// <summary>
        /// 下載調整撤銷佐證資料壓縮檔
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <param name="AW_KIND">申請項目</param>
        /// <returns>壓縮檔</returns>
        [HttpPost("[action]")]
        public async Task<FileContentResult> DownAdjustZip([FromForm] string PROJECT_NO, [FromForm] int PROJ_ADJ_ID, [FromForm] string AW_KIND)
        {
            var file = await service.DownAdjustZip(PROJECT_NO, PROJ_ADJ_ID, AW_KIND);
            return File(file.ms, file.contentType, file.fileName);
        }

    }
}
