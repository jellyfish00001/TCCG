using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 管考備註使用到的計畫基本資料
    /// </summary>
    public class ProjectBasicForProjectFillAuditModel : DbEditor
    {
        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PROJECT_NO { get; set; }

        /// <summary>
        /// 計畫狀態
        /// </summary>
        public string PROJECT_STATUS { get; set; }

        /// <summary>
        /// 結案日期
        /// </summary>
        public DateTime? FINISH_DATE { get; set; }

        /// <summary>
        /// 基本分數
        /// </summary>
        public int? SCORE_A { get; set; }

        /// <summary>
        /// 年度考核備註
        /// </summary>
        public string NOTES_FOR_BUDGET { get; set; }

        /// <summary>
        /// 其他管考備註
        /// </summary>
        public string NOTES_FOR_SCHEDULE { get; set; }
    }
}
