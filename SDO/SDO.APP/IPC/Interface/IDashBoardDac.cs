using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IDashBoardDac:IDac
    {
        /// <summary>
        /// 刪除該月基本資料明細檔
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        Task DeleteDABProjectDataByMonth(string year, string month);
        /// <summary>
        /// 刪除每月綜合排序資料
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        Task DeleteDABCompositeByMonth(string year, string month);
        /// <summary>
        /// 刪除歷年列管情形
        /// </summary>
        /// <returns></returns>
        Task DeleteDABPastYears();
        /// <summary>
        /// 刪除機關連續落後比率
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        Task DeleteDABDelayMonth(string year, string month);

        /// <summary>
        /// 刪除行政區每月案件統計
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        Task DeleteDABTown(string year, string month);
        /// <summary>
        /// 取得當期基本資料明細檔
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        Task<List<DABProjectDataModel>> GetDABProjectData(string year, string month);
        /// <summary>
        /// 新增基本資料明細檔
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task InsertDABProjectData(List<DABProjectDataModel> data);
        /// <summary>
        /// 新增每月綜合排序資料
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        Task InsertDABComposite(List<DABCompositeModel> data);

        /// <summary>
        /// 取得年度(不含當月)執行機關總落後比率
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        Task<List<ExecOrgDelayModel>> GetExecAvgDelayRateOfYear(string year, string month);

        /// <summary>
        /// 新增歷年列管情形資料
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task InsertDABPastYears(List<DABPastYearsModel> models);

        /// <summary>
        /// 取得歷年列管情形資料(不含當年)
        /// </summary>
        /// <returns></returns>
        Task<List<DABPastYearsModel>> GetDABPastYearsData();

        /// <summary>
        /// 新增機關連續落後比率
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task InsertDABDelayMonth(List<DABDelayMonthModel> models);

        /// <summary>
        /// 新增行政區每月案件統計
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task InsertDABTownData(List<DABTownModel> models);

        /// <summary>
        /// 取得統計項目的列管件數、總經費、落後比率資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<List<DABCompositeModel>> GetDABComposite(DABCompositeQueryModel queryModel);

        /// <summary>
        /// 取得計畫落後資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<DABProjectDataModel>> GetProjectDelayType(DABCompositeQueryModel model);

        /// <summary>
        /// 取得全府平均落後比率
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<List<DABCompositeModel>> GetAnnualOrgAvgDelayRate(DABCompositeQueryModel queryModel);

        /// <summary>
        /// 取得機關連續3個月落後比率
        /// </summary>
        /// <param name="queryPeriodSt"></param>
        /// <param name="queryPeriodEnd"></param>
        /// <param name="EXEC_ORGAN_C"></param>
        /// <param name="IS_IN_PROGRESS_DATA"></param>
        /// <returns></returns>
        Task<List<DABDelayMonthModel>> GetOrgRecentDelayRates(DateTime queryPeriodSt, DateTime queryPeriodEnd,
            string EXEC_ORGAN_C, bool IS_IN_PROGRESS_DATA);

        /// <summary>
        /// 取得年度統計資料
        /// </summary>
        /// <param name="years"></param>
        /// <returns></returns>
        Task<List<DABPastYearsModel>> GetDABPastYears(List<string> years);

        /// <summary>
        /// 重要儀表板 - 取得建設類別、行政區 件數及經費資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<List<ProjectTotalDataModel>> GetProjectTotalData(ProjectTotalQueryModel queryModel);

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

        /// <summary>
        /// 取得重大工程進度資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<DABTownModel> GetDashBoardEngProgress(DashBoardQueryModel model);

        /// <summary>
        /// 取得近兩年預計完工計畫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<ProjectFinishModel>> GetProjectFinishData(DashBoardQueryModel model);

        /// <summary>
        /// 取得行政區落後情形統計資料
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<List<TownDelayModel>> GetTownDelayData(DashBoardQueryModel queryModel);
    }
}
