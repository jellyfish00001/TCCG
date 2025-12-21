using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 其他資料招標情形
    /// </summary>
    public class ProjectBidModel : DbEditor
    {
        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 招標種類代碼
        /// </summary>
        public string BID_KIND { get; set; }

        /// <summary>
        /// 招標種類名稱
        /// </summary>
        public string BID_NAME { get; set; }

        /// <summary>
        /// 決標日期
        /// </summary>
        public DateTime? AWARD_BID_DATE { get; set; }

        /// <summary>
        /// 決標廠商
        /// </summary>
        public string BID_TENDER { get; set; }
    }
}
