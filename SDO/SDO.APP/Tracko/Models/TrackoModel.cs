using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    public class TrackoModel
    {
        /// <summary>
        /// 表單流水號
        /// </summary>
        public int ProjectMainId { get; set; }

        /// <summary>
        /// 表單編號
        /// </summary>
        public string ProjectNo { get; set; }

        /// <summary>
        /// 表單名稱
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 表單逾期種類
        /// </summary>
        public string WorkPeriod { get; set; }

        /// <summary>
        /// 代辦案件數量
        /// </summary>
        public int TotalAmount { get; set; }

        /// <summary>
        /// 系統別代碼（PTMS：議會案件，PTMS2：追蹤案件）
        /// </summary>
        public string AP_ID { get; set; }
    }
}
