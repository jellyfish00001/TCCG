
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 重要儀表板相關資料
    /// </summary>
    public class DashBoardSummaryModel
    {
        /// <summary>
        /// 平均落後比率
        /// </summary>
        public decimal AvgDelayRate { get; set; }
        /// <summary>
        /// 前一年平均落後比率
        /// </summary>
        public decimal LastYearAvgDelayRate { get; set; }
        /// <summary>
        /// 所有機關落後排名資料
        /// </summary>
        public List<DABCompositeModel> OrgDelayRankData { get; set; }
        /// <summary>
        /// 建設類別、行政區的件數及經費資料
        /// </summary>
        public List<DABCompositeModel> BudgetCountData { get; set; }
    }
}
