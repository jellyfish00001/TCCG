using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Models
{
    /// <summary>
    /// 資料來源
    /// </summary>
    public class XlsNSeriesModel
    {
        /// <summary>
        /// 資料來源範圍
        /// </summary>
        public string DataArea { get; set; }
        /// <summary>
        /// 資料來源類別
        /// </summary>
        public string CategoryData { get; set; }
        /// <summary>
        /// 資料名稱
        /// </summary>
        public string DataName { get; set; }
        /// <summary>
        /// 顯示值
        /// </summary>
        public bool ShowValue { get; set; }
    }
}
