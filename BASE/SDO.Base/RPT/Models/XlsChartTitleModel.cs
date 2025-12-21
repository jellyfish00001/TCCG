using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Models
{
    /// <summary>
    /// Xls圖檔標題
    /// </summary>
    public class XlsChartTitleModel
    {
        /// <summary>
        /// 標題文字
        /// </summary>
        public string TitleText { get; set; }
        /// <summary>
        /// 標題是否粗體
        /// </summary>
        public bool TitleFontIsBold { get; set; }
        /// <summary>
        /// 標題字體大小
        /// </summary>
        public int TitleFontSize { get; set; }
    }
}
