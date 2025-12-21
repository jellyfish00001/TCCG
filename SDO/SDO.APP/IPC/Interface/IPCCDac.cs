using SDO.Base.Utils.Models;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IPCCDac : IDac
    {
        /// <summary>
        /// 取得 Pcc Table 欄位
        /// </summary>
        /// <param name="tableName">Pcc 資料表名稱</param>
        /// <returns></returns>
        Task<List<string>> GetPccmColumns(string tableName);

        /// <summary>
        /// 取得 Pcc 資料
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<T>> GetPccmData<T>(PccModel model);

        /// <summary>
        /// 取得 Pcc 資料 to Dict
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<Dictionary<string, object>>> GetPccmDataToDict(PccModel model);

        /// <summary>
        /// 取得 Pcc 基本檔
        /// </summary>
        /// <param name="DSNAME"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetPccmSchemaInfoDict(string DSNAME);

        /// <summary>
        /// 修改使用國發會介接資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="IS_USER_FTY_DATA"></param>
        /// <returns></returns>
        bool SaveProjectUsePCC(string PROJECT_NO, bool IS_USER_FTY_DATA);

        /// <summary>
        /// 關聯工程會標案資料 - 更新計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjectBasicByPCC(ProjectMapPCCModel model);

        /// <summary>
        /// 關聯工程會標案資料 - 調整計畫預定實際期程資料
        /// </summary>
        /// <param name="model"></param>
        void UpdateProjCtrlExeByPCC(ProjectMapPCCModel model);

        /// <summary>
        /// 調整特定控制點的工程會預定/實際 完成日期
        /// </summary>
        /// <param name="models"></param>
        void UpdateProjChkItemByPCC(List<PCCProjChkItemModel> models);

        /// <summary>
        /// 檢查計畫是否關聯工程會標案
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<bool> CheckProjIsAssociatePCC(string PROJECT_NO);

        /// <summary>
        /// 新增重新執行的計畫編號
        /// </summary>
        /// <param name="projNos"></param>
        /// <returns></returns>
        Task InsertProjBasicReloads(List<string> projNos);

        /// <summary>
        /// 取得重新執行的計畫編號
        /// </summary>
        /// <param name="cycleModel"></param>
        /// <returns></returns>
        Task<List<AssociatePccProjectModel>> GetReloadProjectNos(ProjectFillCycleModel cycleModel);

        /// <summary>
        /// 刪除重新執行的計畫編號
        /// </summary>
        /// <returns></returns>
        void DeleteReloadProjectNo();

        /// <summary>
        /// 修改使用國發會介接資料
        /// </summary>
        /// <param name="PROJECT_NOs"></param>
        /// <param name="isUserFtyData"></param>
        /// <returns></returns>
        void UpdateProjIsUserFtyData(List<string> PROJECT_NOs, bool isUserFtyData);

        /// <summary>
        /// 更新有關聯的工程會異動檢核點
        /// 且 CTRL_POINT 如果是 C，會同步更新 CHECKITEM_SEQ 大於 CTRL_POINT=C 的資料
        /// </summary>
        /// <param name="models"></param>
        void UpdatePccProjChkItemsByPcc(List<PCCProjChkItemModel> models);

        /// <summary>
        /// 更新工程會異動檢核點
        /// 且 CTRL_POINT 如果是 C，會同步更新 CHECKITEM_SEQ 大於 CTRL_POINT=C 的資料
        /// </summary>
        /// <param name="models"></param>
        void UpdatePccProjChkItems(List<PCCProjChkItemModel> models);

        /// <summary>
        /// 透過工程會API資料更新使用標案系統資料的計畫工程進度
        /// </summary>
        /// <param name="models"></param>
        void UpdateEngProgressUseFtyDataByPcc(List<ProjectEngProgressInsertModel> models);

        /// <summary>
        /// 更新使用標案系統資料的計畫工程進度
        /// </summary>
        /// <param name="models"></param>
        void UpdateEngProgressUseFtyData(List<ProjectEngProgressInsertModel> models);

        /// <summary>
        /// 取得落後原因類別
        /// </summary>
        /// <returns></returns>
        Task<List<CodeDelayClassModel>> GetCodeDelayClass();

        /// <summary>
        /// 增修 計畫落後原因
        /// </summary>
        /// <param name="models"></param>
        void AddMdfDelayCausal(List<ProjectDelayCausalModel> models);

        /// <summary>
        /// 取得工程進度資料清單
        /// </summary>
        /// <param name="projectNos"></param>
        /// <param name="YEAR"></param>
        /// <param name="MONTH"></param>
        /// <returns></returns>
        Task<List<ProjectEngineeringProgressModel>> GetProjEngProgresses(List<string> projectNos, int YEAR, string MONTH);

        /// <summary>
        /// 取得落後原因類型
        /// </summary>
        /// <param name="PROJECT_NO">計畫編號</param>
        /// <param name="FILL_END_DATE">填報週期迄</param>
        /// <returns></returns>
        string GetDelayKind(string PROJECT_NO, DateTime FILL_END_DATE);

        /// <summary>
        /// 更新IS_SEND SEND_DATE 
        /// </summary>
        /// <param name="projectNos"></param>
        /// <param name="model"></param>
        void UpdateSyncFlag(List<string> projectNos, ProjectFillCycleModel model);

        /// <summary>
        /// 取得工程標案資料集
        /// </summary>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetPccSrcTables();
    }
}
