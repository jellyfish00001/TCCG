using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDO.Models
{
    /// <summary>
    /// 表2 本府委託研究計畫成果及運用情形調查列管表
    /// </summary>
    public class PlanResultModel
    {
        /// <summary>
        /// 委託機關
        /// </summary>
        public string OU_NAME { get; set; }

        /// <summary>
        /// 計畫編號
        /// </summary>
        public string PLAN_NO { get; set; }

        /// <summary>
        /// 計畫名稱
        /// </summary>
        public string PLAN_NAME { get; set; }

        /// <summary>
        /// 研究期程起
        /// </summary>
        public string PLAN_START_DATE { get; set; }

        /// <summary>
        /// 研究期程迄
        /// </summary>
        public string PLAN_END_DATE { get; set; }

        /// <summary>
        /// 研究經費(千元)
        /// </summary>
        public decimal SUM_MONEY { get; set; }

        /// <summary>
        /// 局處評核結果
        /// </summary>
        public string SITUATION_TYPE { get; set; }
    }
}
