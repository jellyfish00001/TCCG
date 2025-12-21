using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class ThreeYearPlanService : Service, IThreeYearPlanService
    {
        private readonly IThreeYearPlanDac dac;

        public ThreeYearPlanService(IThreeYearPlanDac dac)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 編輯近三年相關研究
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveThreeYearPlan(ThreeYearPlanIDModel models)
        {
            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {

                await dac.DeleteThreeYearPlan(models.PLANNO);
                await dac.SaveThreeYearPlan(models.ThreeYearPlanList);

                scope.Complete();
            }
            return true;
        }

        /// <summary>
        /// 取得近三年相關研究
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task<List<ThreeYearPlanModel>> GetThreeYearPlan(string PLANNO)
        {
            var result = await dac.GetThreeYearPlan(PLANNO);
            return result;
        }
    }
}
