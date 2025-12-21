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
    public class ProjectExecuteController : ControllerBase
    {
        private IProjectExecuteService service;

        public ProjectExecuteController(IProjectExecuteService service)
        {
            this.service = service;
        }

        #region 每月辦理情形
        /// <summary>
        /// 取得計畫每月辦理情形(單筆)
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectEngineeringProgressTableModel> GetProjecFillExecute([FromForm] string PROJECT_NO, [FromForm] string? SEQ)
        {
            return await service.GetProjecFillExecute(PROJECT_NO, SEQ);
        }

        /// <summary>
        /// 取得計畫每月辦理情形清單
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="DATA_TYPE">資料種類 0:預設近5個月 1:全部</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectEngineeringProgressGridModel>> GetProjecFillExecuteList([FromForm] string PROJECT_NO, [FromForm] string DATA_TYPE)
        {
            return await service.GetProjecFillExecuteList(PROJECT_NO, DATA_TYPE);
        }

        /// <summary>
        /// 儲存計劃每月辦理情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public Task<ObjectResultModel<float>> SaveProjecFillExecute(ProjectEngineeringProgressModel model)
        {
            return service.SaveProjecFillExecute(model);
        }
        #endregion

        #region 落後原因分析
        /// <summary>
        /// 取得計畫落後原因分析(單筆)
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SEQ"></param>
        /// <param name="isRdecFun"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectDelayCausalModel> GetProjectFillDelay([FromForm] string PROJECT_NO, [FromForm] string? SEQ, [FromForm] bool isRdecFun)
        {
            return await service.GetProjectFillDelay(PROJECT_NO, SEQ, isRdecFun);
        }

        /// <summary>
        /// 取得計畫落後原因分析
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="DATA_TYPE">資料種類 0:預設近5個月 1:全部</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectDelayCausalModel>> GetProjectFillDelayList([FromForm] string PROJECT_NO, [FromForm] string DATA_TYPE)
        {
            return await service.GetProjectFillDelayList(PROJECT_NO, DATA_TYPE);
        }

        /// <summary>
        /// 儲存計畫落後原因
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectFillDelay(ProjectDelayCausalModel model)
        {
            return service.SaveProjectFillDelay(model);
        }

        /// <summary>
        /// 刪除計畫落後原因
        /// </summary>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel DeleteProjectDelayCausal([FromBody] string SEQ)
        {
            return service.DeleteProjectDelayCausal(SEQ);
        }
        #endregion

        #region 檢核點完成日期
        /// <summary>
        /// 取得檢核點完成日期
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectFillCkptComModel> GetProjectFillCkptCom([FromBody] string PROJECT_NO)
            => await service.GetProjectFillCkptCom(PROJECT_NO);

        /// <summary>
        /// 儲存檢核點完成日期
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectFillCkptCom([FromForm] ProjectFillCkptComModel model, [FromForm] IFormFile file)
            => service.SaveProjectFillCkptCom(model, file);

        /// <summary>
        /// 清空當次週期已填報的檢核點完成日期、辦理情形及落後原因
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel ClearCycleData([FromBody] string PROJECT_NO)
            => service.ClearCycleData(PROJECT_NO);
        #endregion

        #region 其他資料
        /// <summary>
        /// 取得其他資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectFillOtherModel> GetProjectFillOther([FromBody] string PROJECT_NO)
        {
            return await service.GetProjectFillOther(PROJECT_NO);
        }

        /// <summary>
        /// 儲存其他資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectFillOther(ProjectFillOtherModel model)
        {
            return service.SaveProjectFillOther(model);
        }
        #endregion

        /// <summary>
        /// 執行情形送出檢核
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectExecuteSubmitModel> CheckProjectFillExecuteSubmit([FromBody] string PROJECT_NO)
             => await service.CheckProjectFillExecuteSubmit(PROJECT_NO);

        /// <summary>
        /// 執行情形送出 送出/結案申請
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SaveType"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectFillExecuteSubmit([FromForm] string PROJECT_NO, [FromForm] int SaveType)
            => service.SaveProjectFillExecuteSubmit(PROJECT_NO, SaveType);

        #region 管考備註
        /// <summary>
        /// 管考意見寄信
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<RtnResultModel> SendProjectAuditOpinionMail(ProjectEngineeringAuditOpinionModel model)
        {
            return await service.SendProjectAuditOpinionMail(model);
        }

        /// <summary>
        /// 取得平時管考意見
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectFillAuditModel> GetProjectFillAudit([FromBody] string PROJECT_NO)
        {
            return await service.GetProjectFillAudit(PROJECT_NO);
        }

        /// <summary>
        /// 儲存平時管考意見
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectFillAudit(ProjectFillAuditModel model)
        {
            return service.SaveProjectFillAudit(model);
        }
        #endregion

        #region 實地查證情形
        /// <summary>
        /// 取得實地查證
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<List<ProjectFactFindingModel>> GetProjectFactFinding([FromBody] string PROJECT_NO)
        {
            return await service.GetProjectFactFinding(PROJECT_NO);
        }

        /// <summary>
        /// 儲存實地查證
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectFactFinding(List<ProjectFactFindingModel> model)
        {
            return service.SaveProjectFactFinding(model);
        }
        #endregion

        #region 預算執行情形
        /// <summary>
        /// 取得計畫預算執行情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ProjectFillBudgetExecModel> GetProjectFillBudgetExec([FromBody] string PROJECT_NO)
        {
            return await service.GetProjectFillBudgetExec(PROJECT_NO);
        }

        /// <summary>
        /// 儲存計畫預算執行情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public RtnResultModel SaveProjectFillBudgetExec(ProjectBudgetExecuteModel model)
        {
            return service.SaveProjectFillBudgetExec(model);
        }
        #endregion
    }
}
