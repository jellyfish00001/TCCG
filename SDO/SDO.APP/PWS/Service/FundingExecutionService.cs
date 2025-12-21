using Microsoft.AspNetCore.Mvc;
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
    public class FundingExecutionService : Service, IFundingExecutionService
    {
        private readonly IFundingExecutionDac dac;
        private readonly IDAMTBDac DAMTBDac;
        private readonly IBudgetExecDac budgetExecDac;
        public FundingExecutionService(IFundingExecutionDac dac, IDAMTBDac DAMTBDac, IBudgetExecDac budgetExecDac)
        {
            this.dac = dac;
            this.DAMTBDac = DAMTBDac;
            this.budgetExecDac = budgetExecDac;
        }


        /// <summary>
        /// 存經費細項和執行情形
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveFundingExecution(FundingExecutionModel model)
        {
            using (TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                // 存經費需求細項
                await DAMTBDac.DeleteDAMTB(model.PLANNO);
                await DAMTBDac.SaveDAMTB(model.DAMTBListModel);

                // 存歷年執行經費
                if (model.budgetExecListModel != null && model.budgetExecListModel[0].editType == 1)
                {
                    await budgetExecDac.InsertBudgetExec(model.budgetExecListModel);
                }
                else
                {
                    await budgetExecDac.UpdateBudgetExec(model.budgetExecListModel);
                }
                scope.Complete();
            }
            return true;
        }

        /// <summary>
        /// 取機費細項和執行情形
        /// </summary>
        /// <param name="planNo"></param>
        /// <param name="planYear"></param>
        /// <returns></returns>
        public async Task<FundingExecutionModel> GetFundingExecution(string planNo, int planYear)
        {
            FundingExecutionModel model = new();
            model.DAMTBListModel = await DAMTBDac.GetDAMTB(planNo);
            model.budgetExecListModel = await budgetExecDac.GetBudgetExec(planNo);

            // 如果沒有資料，則創建畫面上固定的三筆資料回傳
            if (model.budgetExecListModel == null || !model.budgetExecListModel.Any())
            {
                for (int i = 1; i <= 3; i++)
                {
                    model.budgetExecListModel.Add(new BudgetExecModel
                    {
                        EXEYEAR = planYear - i,
                        GROWRATIO = 0,
                        RATIO = 0,
                        PUBLICMONEY = null,
                        EXECOUNT = null,
                        NOBUDGETYN = 0,
                        editType = 1
                    });
                }
            }
            //取計畫總經費
            model.TOTAL = await dac.GetFundingExecutionTOTAL(planNo);
            return model;
        }
    }
}
