using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectService
    {
        #region 計畫章節
        /// <summary>
        /// 取得計畫章節表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectChapterListModel>> GetProjectChapter(ProjectChapterQueryModel model);
        #endregion

        #region 計劃基本資料
        /// <summary>
        /// 取得計畫基本資料(含計畫基本資料、計畫經費來源、計畫建設類別、計畫協辦機關)
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="logId">取歷程檔</param>
        /// <returns></returns>
        Task<ProjectBasicFillModel> GetProjectBasicFill(string projectNo, int logId = 0);

        /// <summary>
        /// 儲存計畫基本資料(含計畫基本資料、計畫經費來源、計畫建設類別、計畫協辦機關)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectBasicAdd(ProjectBasicFillModel model);
        #endregion

        #region 計劃檢核點設定
        /// <summary>
        /// 取得計劃檢核點設定
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="LOG_ID">取歷程檔</param>
        /// <returns></returns>
        Task<ProjectCheckpointModel> GetProjectCheckpoint(string PROJECT_NO, int LOG_ID = 0);
        /// <summary>
        /// 儲存檢核點設定
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectCheckpoint(ProjectCheckpointModel model);

        /// <summary>
        /// 判斷計畫最後一個檢核點是否有填實際完成日期
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> IsLasttActualEnddate(string PROJECT_NO);
        #endregion

        #region 立案送審
        /// <summary>
        /// 驗證計劃送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectSubmitResultModel> CheckProjectCanSubmit(string PROJECT_NO);
        /// <summary>
        /// 儲存計劃立案送審
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectFillAddSubmit(string PROJECT_NO);
        #endregion

        #region 計劃審核
        /// <summary>
        /// 取得計劃審核資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectFillAddAuditModel> GetProjectFillAddAudit(string PROJECT_NO);
        /// <summary>
        /// 儲存計劃審核資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public RtnResultModel SaveProjectFillAddAudit(ProjectFillAddAuditModel model);
        #endregion

        #region 共用
        /// <summary>
        /// 產生計劃編號
        /// </summary>
        /// <param name="planYear">民國年</param>
        /// <param name="OU_ID">執行機關的機關代碼</param>
        /// <returns></returns>
        string GenProjectNo(string planYear, string OU_ID);

        /// <summary>
        /// 寫入計畫異動記錄檔
        /// </summary>
        /// <param name="PROJECT_NO">計畫編號</param>
        /// <param name="PROJECT_STAGE">作業階段</param>
        /// <param name="LOG_STATUS">異動狀態</param>
        /// <param name="MEMO"></param>
        int InsertProjectBasicLog(string PROJECT_NO, string PROJECT_STAGE, string LOG_STATUS, string MEMO = null);

        /// <summary>
        /// 新增計畫審查資料檔
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectAudit(ProjectAuditModel model);

        /// <summary>
        /// 取得計畫狀態
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<string> GetProjectStatus(string PROJECT_NO);

        /// <summary>
        /// 取得是否使用國發會界接資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> GetIsUserFtyData(string PROJECT_NO);
        #endregion
    }
}
