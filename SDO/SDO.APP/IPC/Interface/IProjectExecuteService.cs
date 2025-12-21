using Microsoft.AspNetCore.Http;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectExecuteService
    {
        #region 每月辦理情形
        /// <summary>
        /// 取得計畫每月辦理情形(單筆)
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        Task<ProjectEngineeringProgressTableModel> GetProjecFillExecute(string PROJECT_NO, string SEQ);

        /// <summary>
        /// 取得計畫每月辦理情形清單
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="DATA_TYPE">資料種類 0:預設近5個月 1:全部</param>
        /// <returns></returns>
        Task<List<ProjectEngineeringProgressGridModel>> GetProjecFillExecuteList(string PROJECT_NO, string DATA_TYPE);

        /// <summary>
        /// 儲存計劃每月辦理情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ObjectResultModel<float>> SaveProjecFillExecute(ProjectEngineeringProgressModel model);
        #endregion

        #region 落後原因分析
        /// <summary>
        /// 取得計畫落後原因分析(單筆)
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SEQ"></param>
        /// <param name="isRdecFun"></param>
        /// <returns></returns>
        Task<ProjectDelayCausalModel> GetProjectFillDelay(string PROJECT_NO, string SEQ, bool isRdecFun);

        /// <summary>
        /// 取得計畫落後原因分析
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="DATA_TYPE">資料種類 0:預設近5個月 1:全部</param>
        /// <returns></returns>
        Task<List<ProjectDelayCausalModel>> GetProjectFillDelayList(string PROJECT_NO, string DATA_TYPE);

        /// <summary>
        /// 儲存計畫落後原因
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectFillDelay(ProjectDelayCausalModel model);

        /// <summary>
        /// 刪除計畫落後原因
        /// </summary>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        RtnResultModel DeleteProjectDelayCausal(string SEQ);
        #endregion

        #region 檢核點完成日期
        /// <summary>
        /// 取得檢核點完成日期
        /// </summary>
        /// <param name="projectNo">列管編號</param>
        /// <returns></returns>
        Task<ProjectFillCkptComModel> GetProjectFillCkptCom(string projectNo);

        /// <summary>
        /// 儲存檢核點完成日期
        /// </summary>
        /// <param name="model"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectFillCkptCom(ProjectFillCkptComModel model, IFormFile file);

        /// <summary>
        /// 取得 落後原因類型
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        Task<string> GetDelayKind(string PROJECT_NO);

        /// <summary>
        /// 清空當次週期已填報的檢核點完成日期、辦理情形及落後原因
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        RtnResultModel ClearCycleData(string PROJECT_NO);

        /// <summary>
        /// 檢核落後項目-增刪修計畫落後原因
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        void CheckDelayKind(string PROJECT_NO);
        #endregion

        #region 其他資料
        /// <summary>
        /// 取得其他資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectFillOtherModel> GetProjectFillOther(string PROJECT_NO);

        /// <summary>
        /// 儲存其他資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectFillOther(ProjectFillOtherModel model);

        /// <summary>
        /// 取得其他資料招標情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectFillOtherModel> GetProjectBid(string PROJECT_NO);

        /// <summary>
        /// 儲存其他資料招標情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        void SaveProjectBid(ProjectFillOtherModel model);

        /// <summary>
        /// 取得其他資料相關活動
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectActivityModel>> GetProjectActivity(string PROJECT_NO);

        /// <summary>
        /// 儲存其他資料相關活動
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        void SaveProjectActivity(List<ProjectActivityModel> model);

        /// <summary>
        /// 取得其他資料相關審查
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectFillOtherModel> GetProjectReview(string PROJECT_NO);

        /// <summary>
        /// 儲存其他資料相關審查
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        void SaveProjectReview(List<ProjectReviewModel> model);

        /// <summary>
        /// 取得其他資料廠商資訊
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectTenderModel>> GetProjectTender(string PROJECT_NO);

        /// <summary>
        /// 儲存其他資料廠商資訊
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        void SaveProjectTender(List<ProjectTenderModel> model);
        #endregion

        #region 執行情形送出
        /// <summary>
        /// 驗證計畫執行情形可否送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectExecuteSubmitModel> CheckProjectFillExecuteSubmit(string PROJECT_NO);
        /// <summary>
        /// 執行情形送出 送出/結案申請
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SaveType"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectFillExecuteSubmit(string PROJECT_NO, int SaveType);
        #endregion

        #region 管考備註
        /// <summary>
        /// 管考意見寄信
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RtnResultModel> SendProjectAuditOpinionMail(ProjectEngineeringAuditOpinionModel model);

        /// <summary>
        /// 取得平時管考意見
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectFillAuditModel> GetProjectFillAudit(string PROJECT_NO);

        /// <summary>
        /// 儲存平時管考意見
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectFillAudit(ProjectFillAuditModel model);

        /// <summary>
        /// 計算特殊扣分
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        void CalculateScore(string PROJECT_NO);

        /// <summary>
        /// 儲存管考審核意見
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        void SaveProjectAuditOpinion(List<ProjectEngineeringAuditOpinionModel> model);
        #endregion

        #region 實地查證情形
        /// <summary>
        /// 取得實地查證
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectFactFindingModel>> GetProjectFactFinding(string PROJECT_NO);

        /// <summary>
        /// 儲存實地查證
        /// </summary>
        RtnResultModel SaveProjectFactFinding(List<ProjectFactFindingModel> model);
        #endregion

        #region 預算執行情形
        /// <summary>
        /// 取得計畫預算執行情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectFillBudgetExecModel> GetProjectFillBudgetExec(string PROJECT_NO);

        /// <summary>
        /// 儲存計畫預算執行情形
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectFillBudgetExec(ProjectBudgetExecuteModel model);
        #endregion
    }
}
