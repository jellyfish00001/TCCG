using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IIPCCodeDac : IDac
    {
        /// <summary>
        /// 取得執行方式
        /// </summary>
        /// <param name="CP_KIND"></param>
        /// <param name="isShowDel"></param>
        /// <returns></returns>
        Task<List<IPCCodeCheckpointModel>> GetCodeCheckpoint(string CP_KIND, bool isShowDel);

        /// <summary>
        /// 新增執行方式
        /// </summary>
        /// <param name="model"></param>
        void InsertCodeCheckpoint(List<IPCCodeCheckpointModel> model);

        /// <summary>
        /// 修改執行方式
        /// </summary>
        /// <param name="model"></param>
        void UpdateCodeCheckpoint(List<IPCCodeCheckpointModel> model);

        /// <summary>
        /// 取得自訂檢核點
        /// </summary>
        /// <param name="CHK_POINT_CLASS_ID"></param>
        /// <param name="forSettings"></param>
        /// <returns></returns>
        Task<List<IPCCusChkItemModel>> GetCusChkItem(int CHK_POINT_CLASS_ID, bool forSettings);

        /// <summary>
        /// 新增自訂檢核點
        /// </summary>
        /// <param name="model"></param>
        void InsertCusChkItem(List<IPCCusChkItemModel> model);

        /// <summary>
        /// 修改自訂檢核點
        /// </summary>
        /// <param name="model"></param>
        void UpdateCusChkItem(List<IPCCusChkItemModel> model);

        /// <summary>
        /// 取得落後原因類別
        /// </summary>
        /// <param name="DELAY_CLASS_ID"></param>
        /// <param name="DEL_FLG"></param>
        /// <returns></returns>
        Task<List<IPCCodeDelayClassModel>> GetCodeDelayClass(string DELAY_CLASS_ID, bool? DEL_FLG);

        /// <summary>
        /// 取得已存在的落後項目代碼
        /// </summary>
        /// <param name="delayClassSubIds"></param>
        /// <returns></returns>
        Task<List<string>> GetExistDelayClassSubId(string[] delayClassSubIds);

        /// <summary>
        /// 取得已存在的落後項目
        /// </summary>
        /// <param name="delayClassSubItems"></param>
        /// <returns></returns>
        Task<List<string>> GetExistDelayClassSubItem(string[] delayClassSubItems);

        /// <summary>
        /// 新增落後原因類別
        /// </summary>
        /// <param name="model"></param>
        void InsertCodeDelayClass(List<IPCCodeDelayClassModel> model);

        /// <summary>
        /// 修改落後原因類別
        /// </summary>
        /// <param name="model"></param>
        void UpdateCodeDelayClass(List<IPCCodeDelayClassModel> model);

        /// <summary>
        /// 修改落後原因類別
        /// </summary>
        /// <param name="model"></param>
        void UpdateCodeDelayClass(List<IPCSetParamModel> model);

        /// <summary>
        /// 取得預算來源
        /// </summary>
        /// <param name="LEVEL_MARK"></param>
        /// <param name="DEL_FLG"></param>
        /// <returns></returns>
        Task<List<IPCCodePlanItemModel>> GetCodePlanItem(string LEVEL_MARK,bool? DEL_FLG);

        /// <summary>
        /// 新增預算來源
        /// </summary>
        /// <param name="model"></param>
        void InsertCodePlanItem(List<IPCCodePlanItemModel> model);

        /// <summary>
        /// 修改預算來源
        /// </summary>
        /// <param name="model"></param>
        void UpdateCodePlanItem(List<IPCCodePlanItemModel> model);

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
        /// <param name="model"></param>
        void InsertWorkingDay(List<IPCWorkingDayModel> model);

        /// <summary>
        /// 修改工作日
        /// </summary>
        /// <param name="model"></param>
        void UpdateWorkingDay(List<IPCWorkingDayModel> model);

        /// <summary>
        /// 取得機關窗口維護
        /// </summary>
        /// <returns></returns>
        Task<List<IPCDeptContactModel>> GetDeptContact();

        /// <summary>
        /// 取得機關聯絡窗口
        /// </summary>
        /// <param name="ORGAN"></param>
        /// <returns></returns>
        Task<List<IPCDeptContactModel>> GetDeptContactByOrgan(string ORGAN);

        /// <summary>
        /// 新增機關窗口維護
        /// </summary>
        /// <param name="model"></param>
        void InsertDeptContact(List<IPCDeptContactModel> model);

        /// <summary>
        /// 刪除機關窗口維護
        /// </summary>
        /// <param name="ORGAN"></param>
        void DeleteDeptContact(string ORGAN);
    }
}
