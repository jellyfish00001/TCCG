using SDO.APP.IPC.Models.Statistics;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IStatisticsService
    {
        #region 綜合查詢
        /// <summary>
        /// 取得自選欄位
        /// </summary>
        /// <param name="isRdec">檢查登入者是否有管考權限(管考角色)</param>
        /// <returns></returns>
        Task<List<OptionColumnModel>> GetOptionColumns(bool isRdec);

        /// <summary>
        /// 取得綜合查詢結果
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        Task<List<IDictionary<string, object>>> GetUnitingQuery(Dictionary<string, object> condition);
        #endregion

        #region 表10: 選項列管案件計畫歷次調整審查表

        /// <summary>
        /// 取得 表10選項列管案件計畫歷次調整審查表(詳版) 資料
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <returns></returns>
        Task<List<ProjectAdjustDetailedModel>> GetProjAdjDetailed(string PROJECT_NO);

        /// <summary>
        /// 取得屬於工程類的計畫
        /// </summary>
        /// <returns>屬於工程類的計畫清單(PROJECT_NO: 計畫編號、PROJ_ADJ_ID: 最近一次的調整流水號)</returns>
        Task<List<object>> GetEngineeringProjects();

        /// <summary>
        /// 取得 表10:簡版 資料 (檢核點只取CTRL_POINT = A ~ F的資料)
        /// </summary>
        /// <param name="PROJECT_NO">列管編號</param>
        /// <returns></returns>
        Task<List<ProjectAdjustDetailedModel>> GetProjAdjShort(string PROJECT_NO);
        #endregion 表10: 選項列管案件計畫歷次調整審查表

    }
}
