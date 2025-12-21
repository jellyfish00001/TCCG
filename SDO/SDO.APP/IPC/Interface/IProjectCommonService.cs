using SDO.Base.Utils.Models;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectCommonService
    {
        /// <summary>
        /// 取得計畫檔案資料
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="fileUpSource">01:相關檔案上傳、02:其他地方上傳</param>
        /// <param name="fileKind">SET_PARAM.SET_ITEM ='FILE_KIND'</param>
        /// <returns></returns>
        Task<List<ProjectAttachmentModel>> GetProjectAttachment(UpLoadModel model);

        /// <summary>
        /// 取得計畫檔案資料
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="fileUpSource">01:相關檔案上傳、02:其他地方上傳</param>
        /// <param name="fileKinds">SET_PARAM.SET_ITEM ='FILE_KIND'</param>
        /// <returns></returns>
        Task<List<ProjectAttachmentModel>> GetProjectAttachment(string projectNo, string fileUpSource, List<string> fileKinds);

        /// <summary>
        /// 取得非相關檔案上傳的檔案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<ProjectFillFileUpModel> GetProjectOtherAttachmentList(string PROJECT_NO);

        /// <summary>
        /// 儲存計畫檔案資料
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectAttachment(List<ProjectAttachmentModel> models, int datafrom = 0);

        /// <summary>
        /// 儲存計畫檔案資料
        /// </summary>
        /// <param name="model"></param>
        void MdfProjectAttachment(ProjectAttachmentModel model);

        /// <summary>
        /// 儲存相關檔案
        /// </summary>
        /// <param name="model"></param>
        void SaveFile(ProjectAttachmentModel model);

        /// <summary>
        /// 下載相關檔案
        /// </summary>
        /// <param name="IDENTITY_FIELD"></param>
        /// <returns></returns>
        Task<(byte[] bytes, string fileName, string contentType)> DownProjectAttachment(int IDENTITY_FIELD, int DB);

        /// <summary>
        /// 下載附件壓縮檔
        /// </summary>
        /// <param name="IDENTITY_FIELDs"></param>
        /// <param name="title">壓縮檔檔名</param>
        /// <returns></returns>
        Task<(byte[] bytes, string contentType, string fileName)> DownProjectAttachmentZip(List<int> IDENTITY_FIELDs, string title);

        /// <summary>
        /// 取得參考資料
        /// </summary>
        /// <returns></returns>
        Task<List<ProjectAttachmentModel>> GetRefFile();

        /// <summary>
        /// 儲存參考資料
        /// </summary>
        /// <param name="EditFiles"></param>
        /// <returns></returns>
        RtnResultModel SaveRefFile(List<UploadTempFileModel> EditFiles);

        /// <summary>
        /// 儲存計畫附加檔案(針對單一計畫多檔上傳)
        /// </summary>
        /// <param name="model"></param>
        void SaveProjectFiles(ProjectAttachmentModel model);

        /// <summary>
        /// 取得當期填報周期資料
        /// </summary>
        /// <returns></returns>
        Task<ProjectFillCycleModel> GetCurrentCycleData();


        /// <summary>
        /// 驗證Model必填欄位
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataModel"></param>
        /// <returns></returns>
         List<string> CheckModelRequiredField<T>(T dataModel);
        /// <summary>
        /// 驗證Model必填欄位 List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        List<List<string>> CheckModelRequiredField<T>(List<T> dataModel);

        /// <summary>
        /// 檢查檔名
        /// </summary>
        /// <param name="models"></param>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        List<string> CheckFileName(List<ProjectAttachmentModel> models, string PROJECT_NO = "", int? PROJ_ADJ_NO = null, int DBKey = 0);

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
        /// 取得郵件範本替換參數資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<MailTemplateParamModel> GetMailTemplateParam(string PROJECT_NO);

        /// <summary>
        /// 透過機關取得相關人員信箱資料
        /// </summary>
        /// <param name="queryData"></param>
        /// <returns></returns>
        Task<List<RecipientModel>> GetDeptContactRcvData(List<DeptContactRcvQueryModel> queryData);

        /// <summary>
        /// 透過機關取得相關人員信箱資料
        /// </summary>
        /// <param name="orgModels"></param>
        /// <returns></returns>
        Task<List<OrgContactModel>> GetDeptContactDatas(List<ProjectOrgContactModel> orgModels);
        /// <summary>
        /// 取得SCUser收件者資料
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mailType">郵件類型</param>
        /// <returns></returns>
        Task<RecipientModel> GetSCContactRcvData(string userId, string mailType = "1");

        /// <summary>
        /// 檢核點設定檢核 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        string CheckProjectCheckpointValid(ProjectCheckpointModel model);
    }
}
