using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IProjectAdjustService
    {
        /// <summary>
        /// 取得計畫調整撤銷清單
        /// </summary>
        /// <param name="model">篩選條件</param>
        /// <returns></returns>
        Task<List<ProjectAdjustListModel>> GetAdjustList(ProjectAdjustListQueryModel model);

        /// <summary>
        /// 主辦取消調整
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <param name="AW_KIND">調整項目</param>
        /// <returns></returns>
        RtnResultModel SaveExecCancel(string PROJECT_NO, int PROJ_ADJ_ID, string AW_KIND);

        /// <summary>
        /// 主辦申請調整撤銷原因
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SaveExecReason(AdjustReasonModel model);

        /// <summary>
        /// 新增主辦申請調整計畫 (for 基本資料、期程)
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="AW_KIND">調整申請項目</param>
        /// <returns>調整檔流水號</returns>
        int AddAdujustExec(string PROJECT_NO, string AW_KIND);

        /// <summary>
        /// 取得主辦申請調整撤銷原因(for 調整基本資料、撤銷)
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <param name="AW_KIND">申請項目</param>
        /// <returns></returns>
        Task<AdjustReasonViewModel> GetExecReasonBasic(int PROJ_ADJ_ID, string AW_KIND);

        /// <summary>
        /// 取得調整計畫基本資料(含計畫基本資料調整、計畫經費來源調整、計畫建設類別調整、計畫協辦機關調整)
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        Task<ProjectBasicFillAdjustModel> GetProjectBasicAdj(string PROJECT_NO, int PROJ_ADJ_ID);

        /// <summary>
        /// 儲存調整計畫基本資料(含計畫基本資料調整、計畫經費來源調整、計畫建設類別調整、計畫協辦機關調整)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SetProjectBasicAdj(ProjectBasicFillAdjustModel model);

        /// <summary>
        /// 取得主辦申請調整期程原因
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <returns></returns>
        Task<AdjustReasonScheduleViewModel> GetExecReasonSchedule(int PROJ_ADJ_ID);

        /// <summary>
        /// 取得檢核點調整
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水編號</param>
        /// <returns></returns>
        Task<AdjustCheckPointModel> GetAdjustCheckPoint(int PROJ_ADJ_ID);

        /// <summary>
        /// 儲存計劃檢核點調整
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SetAdjustCheckPoint(AdjustCheckPointModel model);

        /// <summary>
        /// 取得主辦調整檢核結果
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水編號</param>
        /// <returns></returns>
        Task<RtnResultModel> GetAdjustChk(string PROJECT_NO, int PROJ_ADJ_ID);

        /// <summary>
        /// 主辦上傳准簽、已核章申請表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SaveScheAttach(AdjustReasonModel model);

        /// <summary>
        /// 主辦調整送審
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SendExecAdjust(AdjustReasonModel model);

        /// <summary>
        /// 管考取得主辦調整撤銷原因
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整檔流水號</param>
        /// <param name="AW_KIND">申請項目</param>
        /// <returns></returns>
        Task<AdjustAuditModel> GetExecReasonByAudit(int PROJ_ADJ_ID, string AW_KIND);

        /// <summary>
        /// 儲存管考審核調整撤銷結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SaveAuditReview(AdjustAuditModel model);

        /// <summary>
        /// 下載調整佐證資料壓縮檔
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <param name="AW_KIND">申請項目</param>
        /// <returns>壓縮檔</returns>
        Task<(byte[] ms, string contentType, string fileName)> DownAdjustZip(string PROJECT_NO, int PROJ_ADJ_ID, string AW_KIND);
        
        /// <summary>
        /// 取得期程調整申請表
        /// </summary>
        /// <param name="PROJ_ADJ_ID">調整流水號</param>
        /// <returns></returns>
        Task<RPTAdjustScheduleModel> GetRPTAdjustSchedule(int PROJ_ADJ_ID);
    }
}
