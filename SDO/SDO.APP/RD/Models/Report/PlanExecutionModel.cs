using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表3 委託研究計畫執行情形調查表
    /// </summary>
    public class PlanExecutionModel
    {
        /// <summary>
        /// 年度
        /// </summary>
        public string PLAN_YEAR { get; set; }  

        /// <summary>
        /// 委託機關
        /// </summary>
        public string OU_NAME { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLAN_NAME { get; set; }

        /// <summary>
        /// 受委託人
        /// </summary>
        public string ENTRUST_UNIT_NAME { get; set; }

        /// <summary>
        /// 研究經費(千元)
        /// </summary>
        public decimal SUM_MONEY { get; set; }

        /// <summary>
        /// 研究期程起
        /// </summary>
        public string PLAN_START_DATE { get; set; }

        /// <summary>
        /// 研究期程迄
        /// </summary>
        public string PLAN_END_DATE { get; set; }

        /// <summary>
        /// 期中報告
        /// </summary>
        public string MID_REPORT_YM { get; set; }

        /// <summary>
        /// 期末報告
        /// </summary>
        public string FINAL_REPORT_YM { get; set; }

    }
}
