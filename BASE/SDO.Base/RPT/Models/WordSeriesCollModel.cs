using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Models
{
    /// <summary>
    /// 圖檔柱體
    /// </summary>
    public class WordSeriesCollModel
    {
        /// <summary>
        /// 柱體名稱
        /// </summary>
        public string SeriesName { get; set; }

        /// <summary>
        /// 資料
        /// </summary>
        public double[] Values { get; set; }

        /// <summary>
        /// 樣式
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 是否顯示數據標籤 預設True
        /// </summary>
        public bool HasDataLabels { get; set; } = true;

        /// <summary>
        /// 是否顯示值
        /// </summary>
        public bool ShowValue { get; set; }

        /// <summary>
        /// 顯示數據標籤前導線
        /// </summary>
        public bool ShowLeaderLines { get; set; }

        /// <summary>
        /// 顯示來源名稱
        /// </summary>
        public bool ShowSeriesName { get; set; }

        /// <summary>
        /// 顯示圖例鍵
        /// </summary>
        public bool ShowLegendKey { get; set; }

        /// <summary>
        /// 顯示柱體名稱
        /// </summary>
        public bool ShowCategoryName { get; set; }

        /// <summary>
        /// 顯示百分比
        /// </summary>
        public bool ShowPercentage { get; set; }

        /// <summary>
        /// 分隔符號 預設逗號
        /// </summary>
        public string Separator { get; set; } = ",";
    }
}
