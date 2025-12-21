using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 落後案件查詢
    /// </summary>
    public class DelayPlanQueryModel
    {
        /// <summary>
        /// 主管機關
        /// </summary>
        public string MASTER_DEPT { get; set; }

        /// <summary>
        /// 執行機關
        /// </summary>
        public string EXEC_DEPT { get; set; }

        /// <summary>
        /// 計畫年度
        /// </summary>
        public string PROJECT_YEAR { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PROJECT_NAME { get; set; }

        /// <summary>
        /// 報表名稱
        /// </summary>
        public string RPT_TYPE { get; set; }

        /// <summary>
        /// 是否只有主辦權限
        /// </summary>
        public bool IS_HAND_ROLE { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        public string CRT_USER { get; set; }
    }
}
