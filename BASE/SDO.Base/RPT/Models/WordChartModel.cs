using Aspose.Words.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Models
{
    /// <summary>
    /// Word圖檔
    /// </summary>
    public class WordChartModel
    {
        /// <summary>
        /// 種類 (常用 Column 柱形圖、Bar 條形圖、Line 折線圖、Pie 餅狀圖)
        /// </summary>
        public ChartType ChartType { get; set; }

        /// <summary>
        /// 圖檔寬
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 圖檔高
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 圖檔標題
        /// </summary>
        public string TitleText { get; set; }

        /// <summary>
        /// 資料來源標題
        /// </summary>
        public string[] Categories { get; set; }

        /// <summary>
        /// Y軸是否隱藏
        /// </summary>
        public bool IsAxisYHidden { get; set; }

        /// <summary>
        /// X軸是否隱藏
        /// </summary>
        public bool IsAxisXHidden { get; set; }

        /// <summary>
        /// 圖例位置 預設右邊
        /// </summary>
        public LegendPosition LegendPosition { get; set; } = LegendPosition.Right;

        /// <summary>
        /// 圖例是否被覆蓋
        /// </summary>
        public bool LegendOverlay { get; set; }

        /// <summary>
        /// 資料來源
        /// </summary>
        public List<WordSeriesCollModel> SeriesColls { get; set; }
    }
}
