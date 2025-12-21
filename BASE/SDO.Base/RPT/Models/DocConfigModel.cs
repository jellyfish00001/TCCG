using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.ReportBuilder.Models
{
    /// <summary>
    /// 文件產生設定
    /// </summary>
    /// <remarks>
    ///     1. 利於後續擴充其它機制, 故建立 Config Model 以控管初始化設定行為
    /// </remarks>
    public class DocConfigModel
    {
        /// <summary>
        /// 是否顯示頁碼
        /// </summary>
        public bool IsShowPager { get; set; }

        /// <summary>
        /// 頁碼格式
        /// </summary>
        public string PagerNumberType { get; set; }

        /// <summary>
        /// 頁碼前的特殊字串
        /// </summary>
        public string PageNumberTitle { get; set; }

        /// <summary>
        /// 預設字型
        /// </summary>
        public string DefaultFontName { get; set; }

        /// <summary>
        /// 預設字型(中文、符號)
        /// </summary>
        public string DefaultFontNameForEast { get; set; }

    }
}
