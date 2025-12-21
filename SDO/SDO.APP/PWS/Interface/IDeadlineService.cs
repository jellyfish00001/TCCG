using SDO.APP.IPC.Models.ProjectAdjust;
using SDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IDeadlineService
    {
        /// <summary>
        /// 取計畫年度
        /// </summary>
        /// <returns></returns>
        Task<List<DeadlineModel>> GetPlanYear();
        /// <summary>
        /// 更新截止日期
        /// </summary>
        /// <returns></returns>
        Task<bool> SetDeadLine(DeadlineModel model);
        /// <summary>
        /// 取年度截止日
        /// </summary>
        /// <returns></returns>
        Task<DeadlineModel> GetPlanDeadline( string year);

    }
}
