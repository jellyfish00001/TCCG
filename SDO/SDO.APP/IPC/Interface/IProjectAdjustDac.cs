using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IProjectAdjustDac : IDac
    {
        /// <summary>
        /// 取得計畫調整撤銷清單
        /// </summary>
        /// <param name="model">篩選條件</param>
        /// <returns></returns>
        Task<List<ProjectAdjustListModel>> GetAdjustList(ProjectAdjustListQueryModel model);

        /// <summary>
        /// 主辦申請撤銷計畫 (for 計畫撤銷)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        int InsertProjectBasicAdjForRevoke(AdjustReasonModel model);

        /// <summary>
        /// 修改計畫調整主檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns>回傳列管編號</returns>
        void UpdateProjectBasicAdj(AdjustReasonModel model);

        /// <summary>
		/// 刪除計畫調整檔 (修改 DEL_FLG = 1，並改 PROJECT_AW_STATUS)
		/// </summary>
		/// <param name="PROJ_ADJ_ID">調整檔流水號</param>
		/// <param name="PROJECT_AW_STATUS">計畫調整狀態</param>
		void DeleteProjBasicAdj(int PROJ_ADJ_ID, string PROJECT_AW_STATUS);

        /// <summary>
        /// 修改計畫主檔狀態
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJECT_AW_STATUS">調整撤銷狀態</param>
		/// <param name="PROJECT_STATUS">計畫狀態</param>
        void UpdateProjBasicStatuses(string PROJECT_NO, string PROJECT_AW_STATUS, string PROJECT_STATUS = null);

        /// <summary>
        /// 修改計畫調整檔 調整檔狀態
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <param name="PROJECT_AW_STATUS">調整撤銷狀態</param>
        void UpdateProjBasicAdjAwSatus(int PROJ_ADJ_ID, string PROJECT_AW_STATUS);

        /// <summary>
        /// 取得計畫狀態
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <returns>計畫狀態</returns>
        Task<string> GetProjectStatus(string PROJECT_NO);

        /// <summary>
        /// 取得計畫調整狀態
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <returns>計畫調整狀態 PROJECT_AW_STATUS</returns>
        Task<string> GetProjectAwStatus(string PROJECT_NO);

        /// <summary>
        /// 修改計畫調整檔 異動紀錄檔ID
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <param name="LOG_ID">異動紀錄檔ID</param>
        void UpdateProjBasicAdjLogId(int PROJ_ADJ_ID, int LOG_ID);

        /// <summary>
        /// 計畫主檔轉調整檔 PROJECT_BASIC => PROJECT_BASIC_ADJ
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="AW_KIND">調整申請項目</param>
        /// <param name="PROJECT_AW_STATUS">計畫調整狀態</param>
        /// <returns>調整檔ID</returns>
        int TransferProjectBasic(string PROJECT_NO, string AW_KIND, string PROJECT_AW_STATUS);

        /// <summary>
        /// 協辦機關/人員主檔轉調整檔 PROJECT_ASST_ORG => PROJECT_ASST_ORG_ADJ
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整檔ID</param>
        void TransferAsstOrg(string PROJECT_NO, int PROJ_ADJ_ID);

        /// <summary>
        /// 建設類別主檔轉調整檔 PROJECT_BUILD_KIND => PROJECT_BUILD_KIND_ADJ
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整檔ID</param>
        void TransferBuildKind(string PROJECT_NO, int PROJ_ADJ_ID);

        /// <summary>
        /// 自訂的計畫檢核點日期轉調整檔 PROJECT_CHECKITEM => PROJECT_CHECKITEM_ADJ
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整檔ID</param>
        void TransferCheckItem(string PROJECT_NO, int PROJ_ADJ_ID);

        /// <summary>
        /// 取得主辦申請調整基本資料原因
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        Task<AdjustReasonViewModel> GetExecReasonBasic(int PROJ_ADJ_ID);

        /// <summary>
        /// 取得計畫基本資料調整
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        Task<ProjectBasicAdjustModel> GetProjectBasicAdj(string PROJECT_NO, int PROJ_ADJ_ID);

        /// <summary>
        /// 取得計畫建設類別調整
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        Task<List<ProjectBuildKindAdjustModel>> GetProjectBuildKindAdj(int PROJ_ADJ_ID);

        /// <summary>
        /// 取得計畫協辦機關調整
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        Task<List<ProjectAsstOrgAdjustModel>> GetAdjustAsstOrg(int PROJ_ADJ_ID);

        /// <summary>
        /// 修改計畫調整檔 基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        void UpdateProjectBasicDataAdj(ProjectBasicAdjustModel model);

        /// <summary>
        /// 新增計畫建設類別
        /// </summary>
        /// <param name="model"></param>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水編號</param>
        void InsertProjectBuildKindAdj(List<ProjectBuildKindAdjustModel> model, string PROJECT_NO, int PROJ_ADJ_ID);

        /// <summary>
        /// 刪除計畫建設類別
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水編號</param>
        void DeleteProjectBuildKindAdj(int PROJ_ADJ_ID);

        /// <summary>
        /// 新增計畫協辦機關調整
        /// </summary>
        /// <param name="model"></param>
        void InsertAdjustAsstOrg(ProjectAsstOrgAdjustModel model);

        /// <summary>
        /// 修改計畫協辦機關
        /// </summary>
        /// <param name="model"></param>
        void UpdateAdjustProjectAsstOrg(ProjectAsstOrgAdjustModel model);

        /// <summary>
        /// 刪除計畫協辦機關調整
        /// </summary>
        /// <param name="ASST_ID">計畫協辦機關流水編號</param>
        void DeleteAdjustAsstOrg(int ASST_ID);

        /// <summary>
        /// 取得主辦申請調整期程原因
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        Task<AdjustReasonScheduleViewModel> GetExecReasonSchedule(int PROJ_ADJ_ID);

        /// <summary>
        /// 修改計畫調整檔 for 期程調整
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        void UpdateProjectBasicAdjForSchedule(AdjustReasonModel model);

        /// <summary>
        /// 取得計劃檢核點設定資料
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        Task<AdjustCheckPointModel> GetAdjustCheckPoint(int PROJ_ADJ_ID);

        /// <summary>
        /// 取得計劃自訂檢核點資料
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        Task<List<AdjustCusCheckPointModel>> GetAdjustCusCheckPoint(int PROJ_ADJ_ID);

        /// <summary>
        /// 更新檢核點調整 - 計畫調整檔
        /// </summary>
        /// <param name="model"></param>
        void UpdateAdjustCheckPoint(AdjustCheckPointModel model);

        /// <summary>
        /// 新增檢核點設定資料
        /// </summary>
        /// <param name="models"></param>
        void InsertAdjustCustomChkItem(List<AdjustCusCheckPointModel> models);

        /// <summary>
		/// 更新檢核點設定資料
		/// </summary>
		/// <param name="models"></param>
		void UpdateAdjustCustomChkItem(List<AdjustCusCheckPointModel> models);

        /// <summary>
		/// 刪除檢核點設定資料
		/// </summary>
		/// <param name="SEQs"></param>
		void DeleteAdjustCustomChkItem(List<int> SEQs);

        /// <summary>
		/// 刪除計畫所有檢核點
		/// </summary>
		/// <param name="PROJ_ADJ_ID"></param>
		void DeleteProjectAdjChkItem(int PROJ_ADJ_ID);

        /// <summary>
        /// 取得主辦調整檢核結果
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水編號</param>
        /// <returns></returns>
        Task<AdjustCheckModel> GetAdjustScheChk(int PROJ_ADJ_ID);

        /// <summary>
        /// 管考取得主辦調整原因
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        Task<AdjustAuditModel> GetExecReasonByAudit(int PROJ_ADJ_ID);

        /// <summary>
        /// 儲存管考審核調整撤銷結果
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectBasicAdjByAudit(AdjustAuditModel model);

        /// <summary>
        /// 計畫調整檔回寫主檔 PROJECT_BASIC_ADJ => PROJECT_BASIC 
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        void TransferProjectBasicByAudit(int PROJ_ADJ_ID);

        /// <summary>
        /// 刪除協辦機關/人員 依據PROJECT_NO
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        void DeleteAsstOrgByProjectNo(string PROJECT_NO);

        /// <summary>
        /// 協辦機關/人員調整 從調整檔新增到主檔
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        void InsertAsstOrgSelectAdjust(int PROJ_ADJ_ID);

        /// <summary>
        /// 經費來源調整檔轉主檔 PROJECT_BUDGET_SOURCE_G 刪除原資料、修改調整資料的PROJECT_NO
        /// </summary>
		/// <param name="IDENTITY_FIELDs">欲刪除的流水編號</param>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="originProjectNo">調整時用的列管編號</param>
        void TransferBudgetSourceGByAudit(List<int> IDENTITY_FIELDs, string PROJECT_NO, string originProjectNo);

        /// <summary>
        /// 修改經費來源檔案 PROJECT_ATTACHMENT 刪除原資料、修改調整資料
        /// </summary>
		/// <param name="IDENTITY_FIELDs">欲刪除的流水編號</param>
        /// <param name="model">檔案model(修改成的FILE_KIND、PROJECT_NO)</param>
        /// <param name="adjustProjectNo">調整時的列管編號</param>
        void TransferBudgetSourceAttByAudit(List<int> IDENTITY_FIELDs, ProjectAttachmentModel model, string adjustProjectNo);

        /// <summary>
        /// 建設類別調整檔轉主檔 PROJECT_BUILD_KIND_ADJ => PROJECT_BUILD_KIND
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        void TransferBuildKindByAudit(int PROJ_ADJ_ID);

        /// <summary>
        /// 計畫調整檔回寫主檔(for 期程調整) PROJECT_BASIC_ADJ => PROJECT_BASIC 
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        void TransferProjectBasicByAuditForSchedule(int PROJ_ADJ_ID);

        /// <summary>
        /// 計畫開始日期回寫主檔 PROJECT_BASIC_ADJ.CONTROL_DATE1 => PROJECT_CONTROL_EXECUTE.CONTROL_DATE1
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        void TransferControlDate1ByAudit(int PROJ_ADJ_ID);
        /// <summary>
		/// 取得檢核點調整後寫回資料
		/// </summary>
		/// <param name="PROJ_ADJ_ID"></param>
		/// <returns></returns>
		Task<List<ProjectCusCheckpointModel>> GetProjectCheckItemTransData(int PROJ_ADJ_ID);

        /// <summary>
		/// 自訂的計畫檢核點日期調整寫回主檔 
		/// </summary>
		/// <param name="models"></param>
		void TransferCheckItemByAudit(List<ProjectCusCheckpointModel> models);

        /// <summary>
        /// 計畫調整檔回寫主檔(for 撤銷) PROJECT_BASIC_ADJ => PROJECT_BASIC 
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        void TransferProjectBasicByAuditForRevoke(int PROJ_ADJ_ID);

        /// <summary>
		/// 取消當期執行情形送出
		/// </summary>
		/// <param name="PROJECT_NO"></param>
		void CancelLatestSendStatus(string PROJECT_NO);

        /// <summary>
        /// 取得期程調整申請表
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <returns></returns>
        Task<RPTAdjustScheduleModel> GetRPTAdjustSchedule(int PROJ_ADJ_ID);

        /// <summary>
        /// 取得期程調整歷程資料
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="REVIEW_RESULT">管考審核結果(Y: 審核通過、N: 審核不通過、R: 退回補正)</param>
        /// <returns>該計畫的所有期程調整歷程(含分月和總期程調整)</returns>
        Task<List<AdjustScheHistoryModel>> GetProjAdjScheHistory(List<string> PROJECT_NO, string REVIEW_RESULT = "Y");

        /// <summary>
        /// 取得期程調整 「調整中」及「調整歷程」資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<(List<ProjectScheOverviewModel>, List<ProjectScheOverviewModel>)> GetProjectScheAdjustList(string PROJECT_NO);
        /// <summary>
        /// 取得立案期程
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectScheOverviewModel>> GetProjectOriginSchedule(string PROJECT_NO);
    }
}
