using Aspose.Cells.Charts;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 統計報表model
    /// </summary>
    public class LineChartModel
    {
        
        /// <summary>
        /// 圖表欄
        /// </summary>
        public string ChartColumn { get; set; }
        /// <summary>
        /// 圖表欄名稱
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 圖表欄初始範圍
        /// </summary>
        public int ColumnStart { get; set; }
        /// <summary>
        /// 圖表欄結束範圍
        /// </summary>
        public int ColumnEnd { get; set; }
        /// <summary>
        /// 圖表列
        /// </summary>
        public List<string> ChartRow { get; set; }
        /// <summary>
        /// 圖表列名稱
        /// </summary>
        public string RowName { get; set; }
        /// <summary>
        /// 圖表值1
        /// </summary>
        public List<decimal> ChartValue1 { get; set; }
        /// <summary>
        /// 圖表值2
        /// </summary>
        public List<decimal> ChartValue2 { get; set; }
        /// <summary>
        /// 圖表值3
        /// </summary>
        public List<decimal> ChartValue3 { get; set; }
        /// <summary>
        /// 圖表類型
        /// </summary>
        public string ChartType { get; set; }

    }
}
