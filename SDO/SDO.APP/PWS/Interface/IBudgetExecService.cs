using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IBudgetExecService
    {
        /// <summary>
        /// 歷年執行情形存檔
        /// </summary>
        /// <returns></returns>
        Task<bool> SaveBudgetExec(List<BudgetExecModel> model);

        /// <summary>
        /// 取得歷年執行情形經費
        /// </summary>
        /// <param name="planNo"></param>
        /// <param name="planYear"></param>
        /// <returns></returns>
        Task<List<BudgetExecModel>> GetBudgetExec(string planNo, int planYear);
    }
}
