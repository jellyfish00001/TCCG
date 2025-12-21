using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IPlanBasicBDac : IDac
    {
        /// <summary>
        /// 基本計畫資料存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task SavePlanBasicB(PlanBasicBModel model);

        /// <summary>
        /// 更新基本計畫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task UpdatePlanBasicB(PlanBasicBModel model);

        /// <summary>
        /// 取得基本計畫資料
        /// </summary>
        /// <param name="PLANNO"></param>
        Task<PlanBasicBModel> GetPlanBasicB(string PLANNO);

    }


}
