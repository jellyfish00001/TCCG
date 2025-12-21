using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// Pcc 篩選
    /// </summary>
    public class PccFilterModel
    {
        /// <summary>
        /// 標案UID
        /// </summary>
        public string PCC_PROJECT_UID { get; set; }

        /// <summary>
        /// 標案編號
        /// </summary>
        public string PCC_PROJECT_NO { get; set; }

        /// <summary>
        /// 標案名稱
        /// </summary>
        public string PCC_PROJECT_NAME { get; set; }

        /// <summary>
        /// 執行機關
        /// </summary>
        public string PCC_EXEC_ORG_NAME { get; set; }

        /// <summary>
        /// 年度
        /// </summary>
        public int? PCC_PROJECT_YEAR { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public string PCC_PROJECT_MONTH { get; set; }
    }
}
