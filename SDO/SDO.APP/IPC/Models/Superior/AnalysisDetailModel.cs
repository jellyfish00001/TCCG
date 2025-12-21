using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 重大建設分析明細
    /// </summary>
    public class AnalysisDetailModel
    {
        /// <summary>
        /// 建設清單
        /// </summary>
        public List<ChartModel<int>> StatBuilds { get; set; } = new();

        /// <summary>
        /// 機關清單
        /// </summary>
        public List<ChartModel<int>> StatMasterOrgans { get; set; } = new();

        /// <summary>
        /// 辦理地點清單
        /// </summary>
        public List<ChartModel<int>> StatTowns { get; set; } = new();

        /// <summary>
        /// 落後圓餅圖清單
        /// </summary>
        public ChartModel<int> StatCntBehind { get; set; } = new();

        /// <summary>
        /// 落後項目清單
        /// </summary>
        public List<ChartModel<int>> BehindItems { get; set; } = new();
    }
}
