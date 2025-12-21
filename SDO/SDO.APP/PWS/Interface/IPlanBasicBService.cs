using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IPlanBasicBService
    {
        /// <summary>
        /// 基本計畫資料存檔
        /// </summary>
        /// <returns></returns>
        Task<PlanBasicBModel> SavePlanBasicB(PlanBasicBModel model);

        /// <summary>
        /// 取得基本計畫資料
        /// </summary>
        /// <param name="PROJECT_NO"></param>
        /// <returns></returns>
        Task<PlanBasicBModel> GetPlanBasicB(string PROJECT_NO);
    }
}
