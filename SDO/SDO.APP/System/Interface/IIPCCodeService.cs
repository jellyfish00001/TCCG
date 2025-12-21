using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IIPCCodeService
    {
        /// <summary>
        /// 取得執行方式
        /// </summary>
        /// <param name="CP_KIND"></param>
        /// <param name="isShowDel"></param>
        /// <returns></returns>
        Task<List<IPCCodeCheckpointModel>> GetCodeCheckpoint(string CP_KIND,bool isShowDel);

        /// <summary>
        /// 儲存執行方式
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        RtnResultModel SaveCodeCheckpoint(List<IPCCodeCheckpointModel> models);

        /// <summary>
        /// 取得自訂檢核點
        /// </summary>
        /// <param name="CHK_POINT_CLASS_ID"></param>
        /// <param name="needDelFlg"></param>
        /// <returns></returns>
        Task<List<IPCCusChkItemModel>> GetCusChkItem(int CHK_POINT_CLASS_ID, bool needDelFlg);

        /// <summary>
        /// 儲存自訂檢核點
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        RtnResultModel SaveCusChkItem(List<IPCCusChkItemModel> models);

        /// <summary>
        /// 取得落後原因類別
        /// </summary>
        /// <param name="DELAY_CLASS_ID"></param>
        /// <param name="DEL_FLG"></param>
        /// <returns></returns>
        Task<List<IPCCodeDelayClassModel>> GetCodeDelayClass(string DELAY_CLASS_ID, bool? DEL_FLG);

        /// <summary>
        /// 儲存落後原因類別
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task<RtnResultModel> SaveCodeDelayClass(List<IPCCodeDelayClassModel> models);

        /// <summary>
        /// 取得預算來源
        /// </summary>
        /// <param name="LEVEL_MARK">1:本府預算來源 2:中央預算來源</param>
        /// <param name="DEL_FLG"></param>
        /// <returns></returns>
        Task<List<IPCCodePlanItemModel>> GetCodePlanItem(string LEVEL_MARK,bool? DEL_FLG);

        /// <summary>
        /// 儲存預算來源
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task<RtnResultModel> SaveCodePlanItem(List<IPCCodePlanItemModel> models);

        /// <summary>
        /// 判斷有無該年度工作日
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        Task<int> GetWorkingDayCountByYear(int year);

        /// <summary>
        /// 取得工作日
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        Task<List<IPCWorkingDayModel>> GetWorkingDay(string startDate, string endDate);

        /// <summary>
        /// 產生年度工作日
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        Task<RtnResultModel> GenerateWorkingDay(int year);

        /// <summary>
        /// 儲存工作日
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        RtnResultModel SaveWorkingDay(List<IPCWorkingDayModel> models);

        /// <summary>
        /// 取得機關窗口維護
        /// </summary>
        /// <returns></returns>
        Task<List<IPCSetContactModel>> GetSetContact();

        /// <summary>
        /// 取得機關聯絡窗口
        /// </summary>
        /// <param name="ORGAN"></param>
        /// <returns></returns>
        Task<List<IPCDeptContactModel>> GetSetContactByOrgan(string ORGAN);

        /// <summary>
        /// 儲存機關窗口維護
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel SaveSetContact(IPCSetContactModel model);
    }
}
