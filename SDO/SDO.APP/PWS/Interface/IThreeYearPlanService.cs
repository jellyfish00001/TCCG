using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IThreeYearPlanService
    {
        /// <summary>
        /// 編輯近三年相關研究
        /// </summary>
        /// <returns></returns>
        Task<bool> SaveThreeYearPlan(ThreeYearPlanIDModel models);
        /// <summary>
        /// 取得近三年相關研究
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task<List<ThreeYearPlanModel>> GetThreeYearPlan( string PLANNO);
    }
}
