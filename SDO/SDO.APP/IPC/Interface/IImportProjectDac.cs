using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDO.Base.Utils.Models;
using SDO.Models;

namespace SDO.Dac
{
    public interface IImportProjectDac : IDac
    {
        /// <summary>
        /// 取得先期計畫資料列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<PWSSDPlanGridModel>> QueryPWSSDPlanList(ImportProjectQueryModel model);
        /// <summary>
        /// 取得先期計畫資料
        /// </summary>
        /// <param name="PlanId"></param>
        /// <returns></returns>
        Task<PWSSDPlanMainModel> GetPWSSDPlan(int PlanId);

        /// <summary>
        /// 取得預定完成期限
        /// </summary>
        /// <param name="PlanId"></param>
        /// <returns></returns>
        Task<DateTime?> GetProjectLastDate(int PlanId);

        /// <summary>
        /// 匯入先期計畫檢核點日期
        /// </summary>
        /// <param name="PlanId"></param>
        /// <param name="ProjectNo"></param>
        void InsertCustomCheckItemDateByPlanId(int PlanId, string ProjectNo);

        /// <summary>
        /// 先期計畫匯入 - 新增計畫基本資料
        /// </summary>
        /// <param name="ProjectNo"></param>
        /// <param name="ProjectLastDate"></param>
        /// <param name="model"></param>
        void InsertProjectBasicByPWSSD(string ProjectNo, DateTime? ProjectLastDate, PWSSDPlanMainModel model);

        /// <summary>
        /// 先期計畫匯入 - 新增計畫經費來源
        /// </summary>
        /// <param name="ProjectNo"></param>
        /// <param name="PlanId"></param>
        void InsertBudgetSourceGByPWSSD(string ProjectNo, int PlanId);

        /// <summary>
        /// 取得縣市代碼
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        string GetCityGovId(string cityName);

        /// <summary>
        /// 新增計畫基本資料
        /// </summary>
        /// <param name="models"></param>
        void InsertProjectBasicByExcel(ImportGeneralProjectModel model);
        /// <summary>
        /// 新增計畫經費來源
        /// </summary>
        /// <param name="models"></param>
        void InsertProjecBudgetSourceGByExcel(ImportGeneralProjectModel model);
    }
}
