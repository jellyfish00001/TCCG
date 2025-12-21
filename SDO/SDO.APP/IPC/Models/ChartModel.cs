using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 圖檔資訊
    /// </summary>
    public class ChartModel<T>
    {
        /// <summary>
        /// Key值
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 顯示的文字
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 結果值清單
        /// </summary>
        public List<T> Values { get; set; }

        /// <summary>
        /// 其他參數
        /// </summary>
        public object OtherData { get; set; }
    }
}
