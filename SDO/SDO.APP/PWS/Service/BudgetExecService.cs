using SDO.Dac;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SDO.Services
{
    public class BudgetExecService : Service, IBudgetExecService
    {
        private readonly IBudgetExecDac dac;

        public BudgetExecService(IBudgetExecDac dac)
        {
            this.dac = dac;
        }

        /// <summary>
        /// 歷年執行經費存檔
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveBudgetExec(List<BudgetExecModel> models)
        {
            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (models[0].editType == 1)
                {
                    await dac.InsertBudgetExec(models);
                }
                else {
                    await dac.UpdateBudgetExec(models);
                }
                scope.Complete();
            }
            return true;
        }

        /// <summary>
        /// 取得歷年執行情形
        /// </summary>
        /// <param name="planNo"></param>
        /// <param name="planYear"></param>
        /// <returns></returns>
        public async Task<List<BudgetExecModel>> GetBudgetExec(string planNo, int planYear)
        {
            var result = await dac.GetBudgetExec(planNo);
            // 如果沒有資料，則創建畫面上固定的三筆資料回傳
            if (result == null || !result.Any())
            {
                result = new List<BudgetExecModel>
                {
                    new BudgetExecModel { EXEYEAR = planYear - 1, GROWRATIO = 0, RATIO = 0, PUBLICMONEY = null, EXECOUNT = null, NOBUDGETYN = 0, editType = 1},
                    new BudgetExecModel { EXEYEAR = planYear - 2, GROWRATIO = 0, RATIO = 0, PUBLICMONEY = null, EXECOUNT = null, NOBUDGETYN = 0, editType = 1 },
                    new BudgetExecModel { EXEYEAR = planYear - 3, GROWRATIO = 0, RATIO = 0, PUBLICMONEY = null, EXECOUNT = null, NOBUDGETYN = 0, editType = 1 }
                };
            }
            return result;
        }
            

    }
}
