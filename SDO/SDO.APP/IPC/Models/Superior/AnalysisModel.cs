using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 重大建設分析
    /// </summary>
    public class AnalysisModel
    {
        /// <summary>
        /// 計畫件數清單
        /// </summary>
        public List<ChartModel<int>> StatCnts { get; set; } = new List<ChartModel<int>>();

        /// <summary>
        /// 計畫金額清單
        /// </summary>
        public List<ChartModel<decimal>> StatBudgetAmts { get; set; } = new List<ChartModel<decimal>>();
    }
}
