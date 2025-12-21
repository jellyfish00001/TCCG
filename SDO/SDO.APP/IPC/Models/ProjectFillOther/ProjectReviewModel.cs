using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 相關審查
    /// </summary>
    public class ProjectReviewModel : DbEditor
    {
        /// <summary>
        /// 流水編號
        /// </summary>
        public int SEQ { get; set; }

        /// <summary>
        /// 列管編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 相關審查種類代碼
        /// </summary>
        public string REVIEW_KIND { get; set; }

        /// <summary>
        /// 相關審查種類名稱
        /// </summary>
        public string REVIEW_NAME { get; set; }

        /// <summary>
        /// 是否有審查
        /// </summary>
        public bool? IS_REVIEW { get; set; }

        /// <summary>
        /// 送件日期
        /// </summary>
        public DateTime? SEND_DATE { get; set; }

        /// <summary>
        /// 核定日期
        /// </summary>
        public DateTime? REVIEW_DATE { get; set; }

        /// <summary>
        /// 審查名稱
        /// </summary>
        public string OTH_RVWNAME { get; set; }
    }
}
