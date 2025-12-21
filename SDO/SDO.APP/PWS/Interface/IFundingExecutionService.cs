using Microsoft.AspNetCore.Mvc;
using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IFundingExecutionService
    {
        /// <summary>
        /// 存經費需求細項和執行情形
        /// </summary>
        /// <returns></returns>
        Task<bool> SaveFundingExecution(FundingExecutionModel model);


        /// <summary>
        /// 取得經費細項和執行情形
        /// </summary>
        /// <param name="planNo"></param>
        /// <param name="planYear"></param>
        /// <returns></returns>
        Task<FundingExecutionModel> GetFundingExecution(string planNo, int planYear);
    }
}
