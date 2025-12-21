using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface IBudgetExecDac : IDac
    {
        /// <summary>
        /// 歷年執行經費存檔
        /// </summary>
        /// <param name="model"></param>
        Task InsertBudgetExec(List<BudgetExecModel> models);

        /// <summary>
        /// 歷年執行經費更新
        /// </summary>
        /// <param name="model"></param>
        Task UpdateBudgetExec(List<BudgetExecModel> models);

        /// <summary>
        /// 取得歷年執行情形
        /// </summary>
        /// <param name="PLANNO"></param>
        /// <returns></returns>
        Task<List<BudgetExecModel>> GetBudgetExec(string PLANNO);
    }


}
