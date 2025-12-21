using SDO.Models;
using SDO.ReportBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface IInnStatisticsDac
    {
        /// <summary>
        /// 取得各主題統計
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<DeptStatisticsModel>> GetDeptStatistics(InnStatisticsModel model);

        /// <summary>
        /// 取得各年度提案資料清冊
        /// </summary>
        /// <returns></returns>
        Task<List<PlanListModel>> GetPlanList(InnStatisticsModel model);

        /// <summary>
        /// 涉及其他主題
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<InnProjectProposalTypeModel>> GetPlanProposalTypeSub(InnStatisticsModel model);

        /// <summary>
        /// 各年度提案數統計表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<YearStatisticsModel>> GetYearStatistics(InnStatisticsModel model);
    } 
}
