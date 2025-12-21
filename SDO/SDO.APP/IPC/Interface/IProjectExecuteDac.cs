using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IProjectExecuteDac : IDac
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
        /// 新增計畫工程進度
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        int InsertProjectEngineeringProgress(ProjectEngineeringProgressModel model);

        /// <summary>
        /// 修改計畫工程進度
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        void UpdateProjectEngineeringProgress(ProjectEngineeringProgressModel model);

        /// <summary>
        /// 比較與上一個月份執行情形比較
        /// </summary>
        /// <param name="model"></param>
        Task<float> ExecutionProgress(ProjectEngineeringProgressModel model);

        /// <summary>
        /// 新增計畫落後原因
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectDelayCausal(ProjectDelayCausalInsertModel model);

        /// <summary>
        /// 修改計畫落後原因
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectDelayCausal(ProjectDelayCausalInsertModel model);

        /// <summary>
        /// 刪除計畫落後原因
        /// </summary>
        /// <param name="model"></param>
        void DeleteProjectDelayCausal(ProjectDelayCausalInsertModel model);

        /// <summary>
        /// 檢查是否落後(根據控制點CTRL_POINT)
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="lastDay">填報月最後一天</param>
        /// <param name="ctrlPoint"></param>
        /// <returns></returns>
        Task<bool> CheckIsDelay(string projectNo,DateTime lastDay, string ctrlPoint = null);

        /// <summary>
        /// 取得執行類別，工程類or非工程類
        /// </summary>
        /// <param name="projectNo"></param>
        /// <returns></returns>
        Task<string> GetCpKind(string projectNo);
        #endregion

        #region 落後原因分析
        /// <summary>
        /// 取得計畫落後原因分析(單筆)
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        Task<ProjectDelayCausalModel> GetProjectDelayCausal(string PROJECT_NO, string SEQ = "");

        /// <summary>
        /// 取得計畫落後原因分析
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="DATA_TYPE">資料種類 0:預設近5個月 1:全部</param>
        /// <returns></returns>
        Task<List<ProjectDelayCausalModel>> GetProjectDelayCausalList(string PROJECT_NO, string DATA_TYPE);

        /// <summary>
        /// 修改計畫落後原因
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        void UpdateProjectDelayCausal(ProjectDelayCausalModel model);

        /// <summary>
        /// 刪除計畫落後原因
        /// </summary>
        /// <param name="SEQ"></param>
        /// <returns></returns>
        void DeleteProjectDelayCausal(string SEQ);
        #endregion

        #region 檢核點完成日期
        /// <summary>
        /// 取得檢核點完成日期
        /// </summary>
        /// <param name="projectNo">列管編號</param>
        /// <returns></returns>
        Task<ProjectFillCkptComModel> GetProjectFillCkptCom(string projectNo);

        /// <summary>
        /// 儲存檢核點完成日期 - 更新計畫基本資料 
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectBasicCkptCom(ProjectFillCkptComModel model);

        /// <summary>
        /// 更新檢核點完成日期 - 實際完成日期
        /// </summary>
        /// <param name="models"></param>
        void UpdateActualEndDate(List<ProjectCusCheckpointModel> models);

        /// <summary>
        /// 同步更新檢核點完成日期 - 實際完成日期
        /// </summary>
        /// <param name="model"></param>
        void UpdateActualEndDateSync(ProjectCusCheckpointModel model);

        /// <summary>
        /// 更新計畫聯繫資訊
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectFillContact(ProjectFillCkptComModel model);

        /// <summary>
        /// 清空當次週期已填報的檢核點完成日期、辦理情形及落後原因
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="projFillCycle"></param>
        void ClearCycleData(string PROJECT_NO, ProjectFillCycleModel projFillCycle);

        /// <summary>
        /// 檢查計畫檢核點項目最後一筆實際完成日期時否有填
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> ChkLastChkptActualEndDateIsFilled(string PROJECT_NO);

        /// <summary>
        /// 取得當期辦理情形資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectEngineeringProgressModel> GetCurrentProjectEngineeringProgress(string PROJECT_NO);
        #endregion

        #region 其他資料

        #region 招標情形
        /// <summary>
        /// 取得其他資料招標情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectBidModel>> GetProjectBid(string PROJECT_NO);

        /// <summary>
        /// 取得招標情形歷程
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectBidDetailModel>> GetProjectBidDetail(string PROJECT_NO);

        /// <summary>
        /// 新增其他資料招標情形
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectBid(List<ProjectBidModel> model);

        /// <summary>
        /// 更新其他資料招標情形
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectBid(List<ProjectBidModel> model);

        /// <summary>
        /// 新增招標情形歷程
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectBidDetail(List<ProjectBidDetailModel> model);

        /// <summary>
        /// 更新招標情形歷程
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectBidDetail(List<ProjectBidDetailModel> model);

        /// <summary>
        /// 刪除招標情形歷程
        /// </summary>
        /// <param name="model"></param>
        void DeleteProjectBidDetail(List<ProjectBidDetailModel> model);
        #endregion

        #region 相關活動
        /// <summary>
        /// 取得其他資料相關活動
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectActivityModel>> GetProjectActivity(string PROJECT_NO);
        /// <summary>
        /// 新增相關活動
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectActivity(List<ProjectActivityModel> model);

        /// <summary>
        /// 更新相關活動
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectActivity(List<ProjectActivityModel> model);

        /// <summary>
        /// 刪除相關活動
        /// </summary>
        /// <param name="model"></param>
        void DeleteProjectActivity(List<ProjectActivityModel> model);
        #endregion

        #region 相關審查
        /// <summary>
        /// 取得其他資料相關審查
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectReviewModel>> GetProjectReview(string PROJECT_NO);
        /// <summary>
        /// 新增相關審查
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectReview(List<ProjectReviewModel> model);

        /// <summary>
        /// 更新相關審查
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectReview(List<ProjectReviewModel> model);

        /// <summary>
        /// 刪除相關審查
        /// </summary>
        /// <param name="model"></param>
        void DeleteProjectReview(List<ProjectReviewModel> model);
        #endregion

        #region 廠商資訊
        /// <summary>
        /// 取得其他資料廠商資訊
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectTenderModel>> GetProjectTender(string PROJECT_NO);

        /// <summary>
        /// 新增廠商資訊
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectTender(List<ProjectTenderModel> model);

        /// <summary>
        /// 更新廠商資訊
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectTender(List<ProjectTenderModel> model);

        /// <summary>
        /// 刪除廠商資訊
        /// </summary>
        /// <param name="model"></param>
        void DeleteProjectTender(List<ProjectTenderModel> model);
        #endregion
        #endregion

        #region 執行情形送出
        /// <summary>
        /// 檢查計畫聯繫資訊必填欄位是否均填
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> CheckProjectContactValid(string PROJECT_NO);

        /// <summary>
        /// 檢查當期執行情形是否已送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> CheckProjectFillExecuteIsSend(string PROJECT_NO);

        /// <summary>
        /// 送出當期執行情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="YEAR"></param>
        /// <param name="MONTH"></param>
        void SendProjectFillExecute(string PROJECT_NO, string YEAR, string MONTH);
        /// <summary>
        /// 檢查計畫是否為工程類
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> CheckIsEngineeringType(string PROJECT_NO);

        /// <summary>
        /// 檢查是否辦理開工
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> CheckIsStartWork(string PROJECT_NO);

        /// <summary>
        /// 檢查是否辦理開工
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> CheckIsTycgProject(string PROJECT_NO);
        
        /// <summary>
        /// 檢查是否辦理竣工
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> CheckIsCompletedWork(string PROJECT_NO);

        /// <summary>
        /// 更新每月辦理情形-累計預定施工進度% & 累計實際施工進度%
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="prg"></param>
        void UpdateFillCycleIpcPrg(string PROJECT_NO, int? prg);
        #endregion

        #region 管考備註
        /// <summary>
        /// 取得計畫基本資料(管考備註需要的欄位)
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectBasicForProjectFillAuditModel> GetProjectBasic(string PROJECT_NO);

        /// <summary>
        /// 更新計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectBasic(ProjectBasicForProjectFillAuditModel model);

        /// <summary>
        /// 取得管考審核意見
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectEngineeringAuditOpinionModel>> GetProjectEngineeringAuditOpinion(string PROJECT_NO);

        /// <summary>
        /// 新增管考審核意見
        /// </summary>
        /// <param name="model"></param>
        int InsertProjectEngineeringAuditOpinion(ProjectEngineeringAuditOpinionModel model);

        /// <summary>
        /// 更新管考審核意見
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectEngineeringAuditOpinion(ProjectEngineeringAuditOpinionModel model);

        /// <summary>
        /// 刪除管考審核意見
        /// </summary>
        /// <param name="SEQ"></param>
        void DeleteProjectEngineeringAuditOpinion(int SEQ);

        /// <summary>
        /// 取得會議列管
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectConferenceModel>> GetProjectConference(string PROJECT_NO);

        /// <summary>
        /// 新增會議列管
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectConference(List<ProjectConferenceModel> model);

        /// <summary>
        /// 更新會議列管
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectConference(List<ProjectConferenceModel> model);

        /// <summary>
        /// 刪除會議列管
        /// </summary>
        /// <param name="model"></param>
        void DeleteProjectConference(List<ProjectConferenceModel> model);

        /// <summary>
        /// 取得逾期繳交填報紀錄
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectDelayfillModel>> GetProjectDelayfill(string PROJECT_NO);

        /// <summary>
        /// 新增逾期繳交填報紀錄
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectDelayfill(List<ProjectDelayfillModel> model);

        /// <summary>
        /// 更新逾期繳交填報紀錄
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectDelayfill(List<ProjectDelayfillModel> model);

        /// <summary>
        /// 刪除逾期繳交填報紀錄
        /// </summary>
        /// <param name="model"></param>
        void DeleteProjectDelayfill(List<ProjectDelayfillModel> model);

        /// <summary>
        /// 取得計畫分併案記錄檔
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectMergeLogModel>> GetProjectMergeLog(string PROJECT_NO);

        /// <summary>
        /// 取得計畫分併案記錄檔 分案次數
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<int> GetProjectMergeLogCount(string PROJECT_NO);

        /// <summary>
        /// 新增計畫分併案記錄檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        int InsertProjectMergeLog(ProjectMergeLogModel model);

        /// <summary>
        /// 更新計畫分併案記錄檔
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectMergeLog(ProjectMergeLogModel model);

        /// <summary>
        /// 刪除計畫分併案記錄檔
        /// </summary>
        /// <param name="SEQ"></param>
        void DeleteProjectMergeLog(int SEQ);

        /// <summary>
        /// 取得計畫撤銷資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<AdjustAuditModel> GetRevokeData(string PROJECT_NO);

        /// <summary>
        /// 取得實地查證情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectFactFindingModel>> GetProjectFactFinding(string PROJECT_NO);

        /// <summary>
        /// 新增實地查證情形
        /// </summary>
        /// <param name="model"></param>
        int InsertProjectFactFinding(ProjectFactFindingModel model);

        /// <summary>
        /// 更新實地查證情形
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectFactFinding(ProjectFactFindingModel model);

        /// <summary>
        /// 刪除實地查證情形
        /// </summary>
        /// <param name="SEQ"></param>
        void DeleteProjectFactFinding(int SEQ);

        /// <summary>
        /// 取得結案明細資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectCloseDetailsModel>> GetProjectCloseDetails(string PROJECT_NO);

        /// <summary>
        /// 新增結案明細資料
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectCloseDetails(List<ProjectCloseDetailsModel> model);

        /// <summary>
        /// 更新結案明細資料
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectCloseDetails(List<ProjectCloseDetailsModel> model);

        /// <summary>
        /// 刪除結案明細資料
        /// </summary>
        /// <param name="model"></param>
        void DeleteProjectCloseDetails(List<ProjectCloseDetailsModel> model);

        /// <summary>
        /// 取得結案意見
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectCloseMemoModel> GetProjectCloseMemo(string PROJECT_NO);

        /// <summary>
        /// 新增結案意見
        /// </summary>
        /// <param name="model"></param>
        void InsertProjectCloseMemo(ProjectCloseMemoModel model);

        /// <summary>
        /// 更新結案意見
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectCloseMemo(ProjectCloseMemoModel model);

        /// <summary>
        /// 取得未於期限內提出計畫調整資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectBasicAdjForDelayApply>> GetDelayApply(string PROJECT_NO);
        #endregion

        #region 預算執行情形
        /// <summary>
        /// 取得計畫預算執行情形
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectBudgetExecuteModel>> GetProjectFillBudgetExec(string PROJECT_NO);

        /// <summary>
        /// 新增計畫預算執行情形
        /// </summary>
        /// <param name="model"></param>
        int InsertProjectFillBudgetExec(ProjectBudgetExecuteModel model);

        /// <summary>
        /// 更新計畫預算執行情形
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectFillBudgetExec(ProjectBudgetExecuteModel model);

        /// <summary>
        /// 刪除計畫預算執行情形
        /// </summary>
        /// <param name="model"></param>
        void DeleteProjectFillBudgetExec(ProjectBudgetExecuteModel model);
        #endregion

        #region 共用
        /// <summary>
        /// 檢查當期執行情形是否未送出或未超過填報週期
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> CheckProjFillExeDataCanSave(string PROJECT_NO);

        /// <summary>
        /// 取消當期執行情形送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        void CancelSend(string PROJECT_NO);

        /// <summary>
        /// 取得計畫是否送出
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> GetIsSend(string PROJECT_NO);

        Task<ProjectFillCycleModel> GetProjFillCycle();
        #endregion
    }
}
