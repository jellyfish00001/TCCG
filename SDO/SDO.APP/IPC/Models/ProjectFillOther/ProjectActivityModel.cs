using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 相關活動
    /// </summary>
    public class ProjectActivityModel : DbEditor
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
        /// 相關活動種類代碼
        /// </summary>
        public string ACTIVITY_KIND { get; set; }

        /// <summary>
        /// 相關活動種類名稱
        /// </summary>
        public string ACTIVITY_KIND_NAME { get; set; }

        /// <summary>
        /// 是否有活動
        /// </summary>
        public bool? IS_ACTIVITY { get; set; }

        /// <summary>
        /// 活動日期
        /// </summary>
        public DateTime? ACTIVITY_DATE { get; set; }

        /// <summary>
        /// 活動名稱
        /// </summary>
        public string ACTIVITY_NAME { get; set; }
    }
}
