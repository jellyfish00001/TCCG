using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SDO.Base.Utils.Models;
using SDO.Models;

namespace SDO.Services
{
    public interface IImportProjectService
    {
        /// <summary>
        /// 取得先期計畫資料列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<PWSSDPlanGridModel>> QueryPWSSDPlanList(ImportProjectQueryModel model);

        /// <summary>
        /// 匯入計畫基本資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        RtnResultModel ImportGeneralProject(string PlanYear,IFormFile file);

        /// <summary>
        /// 匯入先期計畫資料
        /// </summary>
        /// <param name="PlanIds"></param>
        /// <returns></returns>
        RtnResultModel ImportPWSSDPlans(List<int> PlanIds);
    }
}
