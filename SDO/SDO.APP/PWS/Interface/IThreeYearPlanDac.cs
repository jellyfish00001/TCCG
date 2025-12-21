using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IThreeYearPlanDac : IDac
    {
        /// <summary>
        /// 新增近三年相關研究
        /// </summary>
        /// <param name="model"></param>
        Task SaveThreeYearPlan(List<ThreeYearPlanModel> models);

        /// <summary>
        /// 刪除近三年相關研究
        /// </summary>
        /// <param name="model"></param>
        Task DeleteThreeYearPlan(string PLANNO);

        /// <summary>
        /// 取近三年相關研究
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task<List<ThreeYearPlanModel>> GetThreeYearPlan(string PLANNO);
    }


}
