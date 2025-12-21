using SDO.Base.Utils.Models;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IProjectCommonDac : IDac
    {
        /// <summary>
        /// 取得計畫檔案資料
        /// </summary>
        /// <param name="model">計畫檔案查詢model</param>
        /// <returns>計畫檔案清單</returns>
        Task<List<ProjectAttachmentModel>> GetProjectAttachmentList(ProjectAttachmentQueryModel model);

        /// <summary>
        /// 取得非相關檔案上傳的檔案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<List<ProjectOtherAttachmentModel>> GetOtherProjectAttachmentList(string PROJECT_NO);

        /// <summary>
        /// 取得單一計畫檔案資料(下載檔案用)
        /// </summary>
        /// <param name="IDENTITY_FIELD"></param>
        /// <returns></returns>
        Task<ProjectAttachmentModel> GetProjectAttachment(int IDENTITY_FIELD ,int DBKEY);

        /// <summary>
        /// 取得多筆計畫檔案資料(下載zip檔案用)
        /// </summary>
        /// <param name="IDENTITY_FIELDs"></param>
        /// <returns></returns>
        Task<List<ProjectAttachmentModel>> GetProjectAttachment(List<int> IDENTITY_FIELDs);

        /// <summary>
        /// 新增計畫檔案資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        int InsertProjectAttachment(ProjectAttachmentModel model);

        /// <summary>
        /// 修改計畫檔案資料
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectAttachment(ProjectAttachmentModel model);

        /// <summary>
        /// 刪除計畫檔案資料
        /// </summary>
        /// <param name="IDENTITY_FIELD"></param>
        void DeleteProjectAttachment(int IDENTITY_FIELD,int DB);

        /// <summary>
        /// 刪除計畫檔案資料
        /// </summary>
        /// <param name="IDENTITY_FIELD"></param>
        void DeleteProjectAttachment(int IDENTITY_FIELD);

        /// <summary>
        /// 刪除計畫檔案資料
        /// </summary>
        /// <param name="model"></param>
        void DeleteProjectAttachmentAll(ProjectAttachmentModel model);

        /// <summary>
        /// 刪除再新增計畫檔案資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        int MdfProjectAttachment(ProjectAttachmentModel model);

        /// <summary>
        /// 取得參考資料
        /// </summary>
        /// <returns></returns>
        Task<List<ProjectAttachmentModel>> GetRefFile();

        #region 計畫參數值對應資料
        /// <summary>
        /// 取得計畫參數值對應資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SET_ITEM"></param>
        /// <param name="SOURCE_ID"></param>
        /// <returns></returns>
        Task<List<ProjectMappingDataModel>> GetProjectMappingData(string PROJECT_NO, string SET_ITEM, string SOURCE_ID = null);
        /// <summary>
        /// 刪除計畫參數值對應資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="SET_ITEM"></param>
        /// <param name="SOURCE_ID"></param>
        void DeleteProjectMappingData(string PROJECT_NO, string SET_ITEM, string SOURCE_ID);
        /// <summary>
        /// 新增計畫參數對應值資料
        /// </summary>
        /// <param name="models"></param>
        void InsertProjectMappingData(List<ProjectMappingDataModel> models);
        #endregion

        /// <summary>
        /// 取得當期填報周期資料
        /// </summary>
        /// <returns></returns>
        Task<ProjectFillCycleModel> GetCurrentCycleData();

        /// <summary>
        /// 取得計畫填報周期(多筆)
        /// </summary>
        /// <returns></returns>
        Task<List<ProjectFillCycleModel>> GetCycleData();

        /// <summary>
        /// 取得章節表前端頁面路徑
        /// </summary>
        /// <param name="CHAPTER_ID"></param>
        /// <returns></returns>
        Task<string> GetChapterPath(string CHAPTER_ID);

        #region 年終考核
        /// <summary>
        /// 年終考核取得計畫基本資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectFillYearAssModel> GetProjectFillYearAss(string PROJECT_NO);
        #endregion

        #region 檢查同一計畫編號相同FILE_KIND的PROJECT_ATTACHMENT檔名不能重複
        /// <summary>
        /// 取得同一計畫編號FILE_KIND所有檔案
        /// </summary>
        /// <param name="PROJECT_NO">計畫編號</param>
        /// <param name="FILE_KIND">檔案類型</param>
        /// <param name="IDENTITY_FIELD">排除不查的檔案ID</param>
        /// <param name="PROJ_ADJ_ID">調整撤銷流水號</param>
        /// <returns></returns>
        List<(string fileName, string fileKind)> GetFileNameByFileKind(string PROJECT_NO, List<string> FILE_KIND, List<int> IDENTITY_FIELD, int? PROJ_ADJ_NO, int DBKey);
        #endregion


        /// <summary>
        /// 取得已使用的代碼清單
        /// </summary>
        /// <param name="SET_ITEM">代碼類別</param>
        /// <returns></returns>
        Task<List<string>> GetUsedCode(string SET_ITEM);

        /// <summary>
        /// 取得計畫落後原因的落後項目代碼清單
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetDelayClasses();

        /// <summary>
        /// 取得計畫落後原因的落後項目清單
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetDelaySubClasses();

        /// <summary>
        /// 取得計畫經費來源的預算編號清單
        /// </summary>
        /// <param name="LEVEL_MARK">預算來源類別</param>
        /// <returns></returns>
        Task<List<string>> GetPlanItems(string LEVEL_MARK);

        /// <summary>
        /// 取得機關窗口資料
        /// </summary>
        /// <param name="orgIds"></param>
        /// <returns></returns>
        Task<List<DeptContactModel>> GetDeptContactByOrg(List<string> orgIds);

        /// <summary>
        /// 取得SCUser信箱資料
        /// </summary>
        /// <param name="UserIds"></param>
        /// <returns></returns>
        Task<List<SCContactModel>> GetSCContactData(List<string> UserIds);

        /// <summary>
        /// 取得郵件範本替換參數資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<MailTemplateParamModel> GetMailTemplateParam(string PROJECT_NO);

        /// <summary>
        /// 取得落後原因類型
        /// </summary>
        /// <param name="PROJECT_NO">計畫編號</param>
        /// <param name="FILL_END_DATE">填報週期迄</param>
        /// <returns></returns>
        Task<string> GetDelayKind(string PROJECT_NO, DateTime FILL_END_DATE);

        /// <summary>
        /// 移除當月工程進度
        /// </summary>
        /// <param name="projectNos"></param>
        void DeleteLatestProjectEngProgess(List<string> projectNos);
        /// <summary>
        /// 移除當月落後原因
        /// </summary>
        /// <param name="projectNos"></param>
        void DeleteLatestDelayCausal(List<string> projectNos);
    }
}
