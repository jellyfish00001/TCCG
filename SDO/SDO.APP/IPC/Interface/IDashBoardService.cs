using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IDashBoardService
    {
        /// <summary>
        /// 寫入儀錶板資料
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        Task<bool> GenerateDashBoardData(string year, string month);

        /// <summary>
        /// 取得重要儀表板相關資料
        /// </summary>
        /// <returns></returns>
        Task<DashBoardSummaryModel> GetDashBoardSummaryData(DashBoardQueryModel queryModel);

        /// <summary>
        /// 重要儀表板 - 取得機關統計資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<OrgProjectSummaryModel> GetOrgProjectSummary(DashBoardQueryModel queryModel);

        /// <summary>
        /// 取得重大工程進度資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<DABTownModel> GetDashBoardEngProgress(DashBoardQueryModel queryModel);

        /// <summary>
        /// 件數及經費執行情形 - 頁面資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<List<ProjectTotalDataModel>> GetDashBoardCountAndBudget(ProjectTotalQueryModel queryModel);
        /// <summary>
        /// 取得歷年列管情形資料
        /// </summary>
        /// <param name="EXEC_ORGAN_C"></param>
        /// <returns></returns>
        Task<List<DABPastYearsModel>> GetPastYearsData(string EXEC_ORGAN_C);
    }
}
