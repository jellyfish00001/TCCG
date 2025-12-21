using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 招標情形歷程
    /// </summary>
    public class ProjectBidDetailModel : DbEditor
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
        /// 標案類別
        /// </summary>
        public string BID_KIND { get; set; }

        /// <summary>
        /// 明細類別
        /// </summary>
        public int DETAIL_TYPE { get; set; }

        /// <summary>
        /// 明細日期
        /// </summary>
        public DateTime? DETAIL_DATE { get; set; }

        /// <summary>
        /// 明細原因
        /// </summary>
        public string DETAIL_REASON { get; set; }
    }
}
