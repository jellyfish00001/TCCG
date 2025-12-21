using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Models
{
    /// <summary>
    /// Grid
    /// </summary>
    public class ExportGridModel
    {
        /// <summary>
        /// 表投
        /// </summary>
        public List<GridHead> Header { get; set; }
        /// <summary>
        /// 表身
        /// </summary>
        public List<Dictionary<string, object>> Data { get; set; }
        /// <summary>
        /// 輸出格式
        /// </summary>
        public string Format { get; set; }
        /// <summary>
        /// 下載檔名
        /// </summary>
        public string OutputName { get; set; }
    }

    /// <summary>
    /// 表頭
    /// </summary>
    public class GridHead
    {
        public string Title { get; set; }
        public string Field { get; set; }
    }

    public class ExportGridParameter : RptParameter
    {
        public ExportGridModel GridData { get; set; }
    }
}
