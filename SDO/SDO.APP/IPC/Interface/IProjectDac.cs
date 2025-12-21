using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IProjectDac : IDac
    {
        #region 計畫章節
        /// <summary>
        /// 取得計畫章節表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectChapterListModel>> GetProjectChapter(ProjectChapterQueryModel model);
        #endregion

        #region 取得計劃基本資料
        /// <summary>
        /// 取得計畫基本資料 或 計畫基本資料歷程檔
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="logId">取歷程檔</param>
        /// <returns></returns>
        Task<ProjectBasicModel> GetProjectBasic(string projectNo, int logId = 0);

        /// <summary>
        /// 取得計畫經費來源
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="logId">取歷程檔</param>
        /// <returns></returns>
        Task<List<ProjectBudgetSourceGModel>> GetProjectBudgetSourceG(string projectNo, int logId = 0);

        /// <summary>
        /// 取得計畫建設類別
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="logId">取歷程檔</param>
        /// <returns></returns>
        Task<List<ProjectBuildKindModel>> GetProjectBuildKind(string projectNo, int logId = 0);

        /// <summary>
        /// 取得計畫協辦機關
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="logId">取歷程檔</param>
        /// <returns></returns>
        Task<List<ProjectAsstOrgModel>> GetProjectAsstOrg(string projectNo, int logId = 0);
        #endregion

        #region AddMdf 計畫基本資料、計畫經費來源、計畫建設類別、計畫協辦機關
        #region 計畫基本資料
        /// <summary>
        /// 新增計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns>回傳新增後，產生的列管編號</returns>
        void InsertProjectBasic(ProjectBasicModel model);

        /// <summary>
        /// 修改計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns>回傳列管編號</returns>
        void UpdateProjectBasic(ProjectBasicModel model);
        #endregion

        #region 計畫經費來源
        /// <summary>
        /// 新增計畫經費來源
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        int InsertProjectBudgetSourceG(ProjectBudgetSourceGModel model);

        /// <summary>
        /// 修改計畫經費來源
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectBudgetSourceG(ProjectBudgetSourceGModel model);

        /// <summary>
        /// 刪除計計畫經費來源
        /// </summary>
        /// <param name="identityField"></param>
        void DeleteProjectBudgetSourceG(int identityField);
        #endregion

        #region 計畫建設類別
        /// <summary>
        /// 新增計畫建設類別
        /// </summary>
        /// <param name="model"></param>
        /// <param name="projectNo"></param>
        void InsertProjectBuildKind(List<ProjectBuildKindModel> model, string projectNo);

        /// <summary>
        /// 刪除計畫建設類別
        /// </summary>
        /// <param name="projectNo"></param>
        void DeleteProjectBuildKind(string projectNo);
        #endregion

        #region 計畫協辦機關
        /// <summary>
        /// 新增計畫協辦機關
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectAsstOrg(ProjectAsstOrgModel model);

        /// <summary>
        /// 修改計畫協辦機關
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectAsstOrg(ProjectAsstOrgModel model);

        /// <summary>
        /// 刪除計畫協辦機關
        /// </summary>
        /// <param name="asstId"></param>
        void DeleteProjectAsstOrg(int asstId);
        #endregion
        #endregion

        #region 計劃檢核點設定
        /// <summary>
        /// 取得計劃檢核點設定
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="logId">取歷程檔</param>
        /// <returns></returns>
        Task<ProjectCheckpointModel> GetProjectCheckpoint(string PROJECT_NO, int LOG_ID = 0);
        /// <summary>
        /// 取得計劃自訂檢核點設定
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="LOG_ID">取歷程檔</param>
        /// <returns></returns>
        Task<List<ProjectCusCheckpointModel>> GetProjectCusCheckpoint(string PROJECT_NO, int LOG_ID = 0);
        /// <summary>
        /// 更新檢核點設定 - 計劃基本資料
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectCheckpoint(ProjectCheckpointModel model);
        /// <summary>
        /// 更新計畫預定實際期程
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectControlExecute(ProjectCheckpointModel model);
        /// <summary>
        /// 新增自訂檢核點設定資料
        /// </summary>
        /// <param name="models"></param>
        void InsertProjectCustomChkItemDate(List<ProjectCusCheckpointModel> models);
        /// <summary>
        /// 更新自訂檢核點設定資料
        /// </summary>
        /// <param name="models"></param>
        void UpdateProjectCustomChkItemDate(List<ProjectCusCheckpointModel> models);
        /// <summary>
        /// 刪除自訂檢核點設定資料
        /// </summary>
        /// <param name="models"></param>
        void DeleteProjectCustomChkItemDate(List<ProjectCusCheckpointModel> models);
        /// <summary>
        /// 刪除自訂檢核點設定資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        void DeleteProjectCustomChkItemDate(string PROJECT_NO);
        /// <summary>
        /// 判斷計畫最後一個檢核點是否有填實際完成日期
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> IsLasttActualEnddate(string PROJECT_NO);
        #endregion

        #region 計畫送審
        /// <summary>
        /// 是否在填報週期內
        /// </summary>
        /// <returns></returns>
        bool IsInTheFillCycle();
        /// <summary>
        /// 更新計劃狀態 & 管考備註
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="PROJECT_STATUS"></param>
        /// <param name="MEMO_EVALUATION"></param>
        void UpdateProjectStatusAndMemo(string PROJECT_NO, string PROJECT_STATUS, string MEMO_EVALUATION = "");
        /// <summary>
        /// 新增計畫工程進度 取PROJECT_FILL_CYCLE最新年月
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        void InsertProjectEngineeringProgress(string PROJECT_NO);
        /// <summary>
        /// 新增計畫基本資料異動紀錄檔
        /// </summary>
        /// <param name="model"></param>
        int InsertProjectBasicLog(ProjectBasicLogModel model);
        /// <summary>
        /// 新增計畫基本資料歷程檔
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="LOG_ID"></param>
        /// <returns></returns>
        int InsertProjectBasicHis(string PROJECT_NO, int LOG_ID);
        #endregion

        #region 共用
        /// <summary>
        /// 取得計劃編號流水號
        /// </summary>
        /// <param name="projectNoStart6Char">計劃編號前6碼</param>
        /// <returns></returns>
        string GetProjectNoSeq(string projectNoStart6Char);
        /// <summary>
        /// 取得審查資料檔
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="PLAN_REVIEW_TYPE"></param>
        /// <returns></returns>
        Task<ProjectAuditModel> GetProjectAudit(string PROJECT_NO, string PLAN_REVIEW_TYPE);
        /// <summary>
        /// 新增計畫審查資料檔
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectAudit(ProjectAuditModel model);
        /// <summary>
        /// 更新計畫審查資料檔
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectAudit(ProjectAuditModel model);
        /// <summary>
        /// 取得計畫狀態
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<string> GetProjectStatus(string PROJECT_NO);

        /// <summary>
        /// 新增計畫檢核點歷程檔
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectCheckpointHis(ProjectHisModel model);

        /// <summary>
        /// 新增計畫經費來源歷程檔
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectBudgetSourceGHis(ProjectHisModel model);
        /// <summary>
        /// 計畫建設類別歷程檔
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectBuildKindHis(ProjectHisModel model);
        /// <summary>
        /// 新增計畫協辦機關歷程檔
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectAsstOrgHis(ProjectHisModel model);
        /// <summary>
        /// 新增計畫預定實際期程歷程檔
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectControlExecuteHis(ProjectHisModel model);
        /// <summary>
        /// 取得是否使用國發會界接資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> GetIsUserFtyData(string PROJECT_NO);
        #endregion
    }
}
