using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.XlsReader.Models
{
    /// <summary>
    /// Excel 參數
    /// </summary>
    public class XlsParameter
    {
        /// <summary>
        /// 活頁名稱
        /// </summary>
        public string WorksheetName { get; set; }
        /// <summary>
        /// 活頁索引
        /// </summary>
        public int WorksheetIndex { get; set; }
        /// <summary>
        /// 表頭開始行
        /// </summary>
        public int HeaderStrRow { get; set; }
        /// <summary>
        /// 表頭開始欄
        /// </summary>
        public int HeaderStrColumn { get; set; }
        /// <summary>
        /// 內容開始行
        /// </summary>
        public int ContentStrRow { get; set; }
    }
}
