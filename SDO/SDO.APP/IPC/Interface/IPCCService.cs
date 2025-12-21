using SDO.Base.Utils.Models;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IPCCService
    {
        /// <summary>
        /// 取得關聯工程會標案資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectMapPCCGridModel>> GetProjectMapPCC(PccFilterModel model);

        /// <summary>
        /// 取得工程會基本資料單筆
        /// </summary>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetPccmDs01(string PCC_PROJECT_UID);

        /// <summary>
        /// 取得標案系統執行進度資料
        /// </summary>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <returns></returns>
        Task<PCCExeProgressGridModel> GetPCCExeProgress(string PCC_PROJECT_UID);

        /// <summary>
        /// 工程標案工程概要資料
        /// </summary>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetPCCDs07(string PCC_PROJECT_UID);

        /// <summary>
        /// 工程標案決標資料
        /// </summary>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetPCCDs09(string PCC_PROJECT_UID);

        /// <summary>
        /// 關聯工程會標案資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="PCC_PROJECT_UID"></param>
        /// <param name="START_WORK"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectMapPCC(string PROJECT_NO, string PCC_PROJECT_UID, DateTime? START_WORK);

        /// <summary>
        /// 修改使用國發會介接資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <param name="IS_USER_FTY_DATA"></param>
        /// <returns></returns>
        RtnResultModel SaveProjectUsePCC(string PROJECT_NO, bool IS_USER_FTY_DATA);

        /// <summary>
        /// 工程標案同步
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<RtnResultModel> SyncPCCData(string PROJECT_NO);

        /// <summary>
        /// 工程標案 excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ObjectResultModel<PccmXlsGridModel>> GetPccmXls(PccXlsFilterModel model);

        /// <summary>
        /// 下載工程標案 excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<(byte[] bytes, string fileName, string contentType)> DownloadPccmXls(PccXlsFilterModel model);

        /// <summary>
        /// 取得工程標案資料集
        /// </summary>
        /// <returns></returns>
        Task<List<DropDownListModel>> GetPccSrcTables();
    }
}
