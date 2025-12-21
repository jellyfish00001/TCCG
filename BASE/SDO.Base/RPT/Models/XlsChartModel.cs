using Aspose.Cells.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Models
{
    /// <summary>
    /// Xls圖檔
    /// </summary>
    public class XlsChartModel
    {
        /// <summary>
        /// 種類
        /// (常用 Column 柱形圖、Bar 條形圖、Line 折線圖、Pie 餅狀圖)
        /// </summary>
        public ChartType ChartType { get; set; }

        /// <summary>
        /// 開始列位置
        /// </summary>
        public int StartRow { get; set; }

        /// <summary>
        /// 開始欄位置
        /// </summary>
        public int StartColumn { get; set; }

        /// <summary>
        /// 結束列位置
        /// </summary>
        public int EndRow { get; set; }

        /// <summary>
        /// 結束欄位置
        /// </summary>
        public int EndColumn { get; set; }

        /// <summary>
        /// 圖檔標題
        /// </summary>
        public XlsChartTitleModel Title { get; set; }

        /// <summary>
        /// 資料來源清單
        /// </summary>
        public List<XlsNSeriesModel> NSerieses { get; set; }
    }
}
